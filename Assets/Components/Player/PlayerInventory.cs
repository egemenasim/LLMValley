using System.Collections.Generic;
using UnityEngine;
using LLMValley.Items;
using LLMValley.UI;

namespace LLMValley.Player
{
    /// <summary>
    /// Sits on the Player GameObject.
    /// Implements IItemCollector so CollectibleItem can hand items off,
    /// stores the inventory data as a List of ItemStacks, and keeps
    /// InventoryUI in sync after every change.
    /// </summary>
    public class PlayerInventory : MonoBehaviour, IItemCollector
    {
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

        // ─── Unity Lifecycle ──────────────────────────────────────────────────────

        private void Start()
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
        }

        /// <summary>Adds an item to the inventory (used for loading saves).</summary>
        public void AddItem(ItemData item, int quantity)
        {
            CollectItem(item, quantity);
        }

        /// <summary>Clears all items from the inventory (used for loading saves).</summary>
        public void ClearInventory()
        {
            _items.Clear();
            RefreshUI();
        }

        // ─── Private ──────────────────────────────────────────────────────────────

        private void RefreshUI()
        {
            inventoryUI?.Refresh(_items);
        }
    }
}
