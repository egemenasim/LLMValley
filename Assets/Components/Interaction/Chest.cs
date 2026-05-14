using System;
using System.Collections.Generic;
using UnityEngine;
using LLMValley.Items;
using LLMValley.SaveSystem;
using LLMValley.UI;
using LLMValley.Player;

namespace LLMValley.Interaction
{
    public class Chest : Interactable
    {
        [Header("Chest Settings")]
        [Tooltip("Unique ID for saving. Must be unique for every chest in the world.")]
        public string chestId = "Chest_01";
        
        [Tooltip("Maximum number of distinct item slots in this chest.")]
        public int slotCount = 30;

        private List<ItemStack> _items = new List<ItemStack>();
        public IReadOnlyList<ItemStack> Items => _items;

        public bool IsOpen { get; private set; }

        private void Start()
        {
            LoadChestData();
        }

        public override void interact_event()
        {
            base.interact_event();
            
            if (IsOpen)
            {
                CloseChest();
            }
            else
            {
                OpenChest();
            }
        }

        private void OpenChest()
        {
            IsOpen = true;
            ChestUI chestUI = FindFirstObjectByType<ChestUI>(FindObjectsInactive.Include);
            if (chestUI != null)
            {
                chestUI.Open(this);
            }
            else
            {
                Debug.LogError("[Chest] Could not find ChestUI in the scene.");
            }
        }

        public void CloseChest()
        {
            IsOpen = false;
            ChestUI chestUI = FindFirstObjectByType<ChestUI>(FindObjectsInactive.Include);
            if (chestUI != null && chestUI.IsOpen)
            {
                chestUI.Close();
            }
        }

        public void SaveChestData()
        {
            var saveData = new SaveManager.InventorySaveData
            {
                items = new SaveManager.InventorySaveData.InventoryItemData[_items.Count]
            };

            for (int i = 0; i < _items.Count; i++)
            {
                saveData.items[i] = new SaveManager.InventorySaveData.InventoryItemData
                {
                    itemId = _items[i].item.itemID,
                    quantity = _items[i].quantity
                };
            }

            SaveManager.SaveChest(chestId, saveData);
        }

        private void LoadChestData()
        {
            var saveData = SaveManager.LoadChest(chestId);
            if (saveData != null && saveData.items != null)
            {
                _items.Clear();
                foreach (var itemData in saveData.items)
                {
                    var item = SaveManager.GetItemById(itemData.itemId);
                    if (item != null)
                    {
                        _items.Add(new ItemStack(item, itemData.quantity));
                    }
                }
            }
        }

        public bool TryCollectItem(ItemData item, int quantity)
        {
            if (item == null || quantity <= 0) return false;

            int remainingQuantity = quantity;

            // Stack onto existing
            if (item.isStackable)
            {
                foreach (var stack in _items)
                {
                    if (stack.item == item && stack.quantity < item.maxStackSize)
                    {
                        int space = item.maxStackSize - stack.quantity;
                        int toAdd = Mathf.Min(space, remainingQuantity);
                        stack.quantity += toAdd;
                        remainingQuantity -= toAdd;
                        if (remainingQuantity <= 0) break;
                    }
                }
            }

            // Empty slots
            while (remainingQuantity > 0 && _items.Count < slotCount)
            {
                int toAdd = item.isStackable ? Mathf.Min(remainingQuantity, item.maxStackSize) : 1;
                _items.Add(new ItemStack(item, toAdd));
                remainingQuantity -= toAdd;
            }

            if (remainingQuantity < quantity)
            {
                SaveChestData();
                return true; // Partially or fully added
            }

            return false;
        }

        public void RemoveAt(int index, int amount = 1)
        {
            if (index < 0 || index >= _items.Count) return;

            _items[index].quantity -= amount;
            if (_items[index].quantity <= 0)
            {
                _items.RemoveAt(index);
            }
            SaveChestData();
        }
    }
}
