using System.Collections.Generic;
using UnityEngine;
using LLMValley.Items;
using LLMValley.SaveSystem;
using LLMValley.UI;
using UnityEngine.SceneManagement;

namespace LLMValley.Player
{
    /// <summary>
    /// Sits on the Player GameObject.
    /// Implements IItemCollector so CollectibleItem can hand items off,
    /// stores the inventory data as a List of ItemStacks, and keeps
    /// InventoryUI in sync after every change.
    /// </summary>
    public class PlayerInventory : MonoBehaviour, IItemCollector, IInventoryKeyProvider
    {
        public static PlayerInventory Instance { get; private set; }
        // ─── Inspector ────────────────────────────────────────────────────────────

        [Header("Inventory")]
        [Tooltip("Maximum number of distinct item slots.")]
        [SerializeField] private int slotCount = 10;

        [Header("UI")]
        [Tooltip("Drag the InventoryUI instance from the Hierarchy here.")]
        [SerializeField] private InventoryUI inventoryUI;

        // ─── State ────────────────────────────────────────────────────────────────

        private readonly List<ItemStack> _items = new();

        public IReadOnlyList<ItemStack> Items => _items;

        public InventoryUI InventoryUI => inventoryUI;

        public bool HasItemType(ItemType itemType)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                ItemStack stack = _items[i];
                if (stack == null || !stack.IsValid)
                    continue;

                if (stack.item.itemType == itemType)
                    return true;
            }

            return false;
        }

        public bool HasKey(string keyId)
        {
            if (string.IsNullOrWhiteSpace(keyId))
            {
                return false;
            }

            for (int i = 0; i < _items.Count; i++)
            {
                var stack = _items[i];
                if (stack == null || !stack.IsValid)
                {
                    continue;
                }

                if (stack.item is LLMValley.Items.KeyItemData keyItem && keyItem.keyId == keyId)
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasItemById(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return false;
            }

            for (int i = 0; i < _items.Count; i++)
            {
                var stack = _items[i];
                if (stack == null || !stack.IsValid)
                {
                    continue;
                }

                if (stack.item.name == itemId || stack.item.itemID.ToString() == itemId)
                {
                    return true;
                }
            }

            return false;
        }

        // ─── Unity Lifecycle ──────────────────────────────────────────────────────
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Start()
        {
            RefreshUI();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RefreshUI();
        }

        // ─── IItemCollector ───────────────────────────────────────────────────────

        /// <summary>
        /// Called automatically by CollectibleItem when the player's collider
        /// enters the item's trigger. Adds the item to the first available slot,
        /// stacks it if possible, then refreshes the hotbar UI.
        /// </summary>
        public void CollectItem(ItemData item, int quantity)
        {
            if (item == null || quantity <= 0) return;

            // Try to stack onto an existing slot first.
            if (item.isStackable)
            {
                foreach (var stack in _items)
                {
                    if (stack.item == item && stack.quantity < item.maxStackSize)
                    {
                        int space = item.maxStackSize - stack.quantity;
                        int toAdd = Mathf.Min(space, quantity);
                        stack.quantity += toAdd;
                        quantity -= toAdd;
                        if (quantity <= 0) break;
                    }
                }
            }

            // Place remaining quantity into empty slots.
            while (quantity > 0 && _items.Count < slotCount)
            {
                int toAdd = item.isStackable ? Mathf.Min(quantity, item.maxStackSize) : 1;
                _items.Add(new ItemStack(item, toAdd));
                quantity -= toAdd;
            }

            if (quantity > 0)
                Debug.LogWarning($"[PlayerInventory] Inventory full — could not add {quantity}x {item.itemName}.");

            RefreshUI();
            SaveInventoryChange();
        }

        /// <summary>
        /// Returns true if the inventory can fit the full quantity without dropping any items.
        /// Used by shop purchases so players are not charged when their inventory cannot accept the item.
        /// </summary>
        public bool CanCollect(ItemData item, int quantity)
        {
            if (item == null || quantity <= 0)
            {
                return false;
            }

            var remainingQuantity = quantity;
            var freeSlots = slotCount - _items.Count;

            if (item.isStackable)
            {
                foreach (var stack in _items)
                {
                    if (stack.item != item || stack.quantity >= item.maxStackSize)
                    {
                        continue;
                    }

                    var availableSpace = item.maxStackSize - stack.quantity;
                    remainingQuantity -= Mathf.Min(availableSpace, remainingQuantity);
                    if (remainingQuantity <= 0)
                    {
                        return true;
                    }
                }
            }

            while (remainingQuantity > 0 && freeSlots > 0)
            {
                var amountInNewSlot = item.isStackable ? Mathf.Min(remainingQuantity, item.maxStackSize) : 1;
                remainingQuantity -= amountInNewSlot;
                freeSlots--;
            }

            return remainingQuantity <= 0;
        }

        /// <summary>
        /// Atomically attempts to add the full quantity to the inventory.
        /// Returns false if there is not enough room.
        /// </summary>
        public bool TryCollectItem(ItemData item, int quantity)
        {
            if (!CanCollect(item, quantity))
            {
                return false;
            }

            CollectItem(item, quantity);
            return true;
        }

        // ─── Public Helpers ───────────────────────────────────────────────────────

        /// <summary>Removes one unit from the slot at index. Removes the slot if empty.</summary>
        public void RemoveAt(int slotIndex, int amount = 1)
        {
            if (slotIndex < 0 || slotIndex >= _items.Count) return;

            _items[slotIndex].quantity -= amount;
            if (_items[slotIndex].quantity <= 0)
                _items.RemoveAt(slotIndex);

            RefreshUI();
            SaveInventoryChange();
        }

        /// <summary>Adds an item to the inventory (used for loading saves).</summary>
        public void AddItem(ItemData item, int quantity)
        {
            CollectItem(item, quantity);
        }

        /// <summary>Attempts to remove the requested quantity of a specific item from the inventory.</summary>
        public bool TryRemoveItem(ItemData item, int quantity = 1)
        {
            if (item == null || quantity <= 0)
            {
                return false;
            }

            var remaining = quantity;
            for (var index = _items.Count - 1; index >= 0 && remaining > 0; index--)
            {
                var stack = _items[index];
                if (stack == null || stack.item != item || stack.quantity <= 0)
                {
                    continue;
                }

                var removedAmount = Mathf.Min(stack.quantity, remaining);
                stack.quantity -= removedAmount;
                remaining -= removedAmount;

                if (stack.quantity <= 0)
                {
                    _items.RemoveAt(index);
                }
            }

            if (remaining > 0)
            {
                return false;
            }

            RefreshUI();
            SaveInventoryChange();
            return true;
        }

        /// <summary>Clears all items from the inventory (used for loading saves).</summary>
        public void ClearInventory()
        {
            _items.Clear();
            RefreshUI();
            SaveInventoryChange();
        }

        // ─── Private ──────────────────────────────────────────────────────────────

        private void RefreshUI()
        {
            if (inventoryUI == null)
            {
                inventoryUI = Object.FindAnyObjectByType<InventoryUI>(FindObjectsInactive.Include);
                if (inventoryUI != null)
                {
                    Debug.Log($"[PlayerInventory] Dynamically found InventoryUI in scene: {SceneManager.GetActiveScene().name}");
                }
            }

            if (inventoryUI != null)
            {
                inventoryUI.Refresh(_items);
            }
        }

        private void SaveInventoryChange()
        {
            if (SaveManager.IsApplyingSaveData)
            {
                return;
            }

            SaveManager.SaveGame();
        }
    }
}
