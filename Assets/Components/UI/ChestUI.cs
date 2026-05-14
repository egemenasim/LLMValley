using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LLMValley.Items;
using LLMValley.Player;
using LLMValley.Interaction;

namespace LLMValley.UI
{
    public class ChestUI : MonoBehaviour
    {
        [Header("Slot References")]
        [Tooltip("All InventorySlotUI components for this chest, in order.")]
        [SerializeField] private InventorySlotUI[] slots;

        [Header("UI Components")]
        [SerializeField] private GameObject panelRoot;

        private Chest _currentChest;
        private CanvasGroup _canvasGroup;

        public bool IsOpen => panelRoot != null && panelRoot.activeSelf;

        private void Awake()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null) continue;
                int captured = i;
                slots[i].SetSlotIndex(captured);
                slots[i].OnSlotClicked.AddListener(OnChestSlotClicked);
            }
        }



        public void Open(Chest chest)
        {
            _currentChest = chest;
            if (panelRoot != null) panelRoot.SetActive(true);
            Refresh();

            // Make sure player inventory is visible
            if (PlayerInventory.Instance != null && PlayerInventory.Instance.InventoryUI != null)
            {
                PlayerInventory.Instance.InventoryUI.Show();
                PlayerInventory.Instance.InventoryUI.OnSlotClickedEvent -= OnPlayerSlotClicked;
                PlayerInventory.Instance.InventoryUI.OnSlotClickedEvent += OnPlayerSlotClicked;
            }

            PlayerInputLock.Lock();
        }

        public void Close()
        {
            if (!IsOpen) return;

            _currentChest = null;
            if (panelRoot != null) panelRoot.SetActive(false);
            
            PlayerInputLock.Unlock();
            
            if (PlayerInventory.Instance != null && PlayerInventory.Instance.InventoryUI != null)
            {
                PlayerInventory.Instance.InventoryUI.OnSlotClickedEvent -= OnPlayerSlotClicked;
            }
        }

        public void Refresh()
        {
            if (_currentChest == null) return;

            var items = _currentChest.Items;
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null) continue;

                if (i < items.Count && items[i] != null && items[i].IsValid)
                {
                    slots[i].SetItem(items[i].item, items[i].quantity);
                }
                else
                {
                    slots[i].SetEmpty();
                }
            }
        }

        private void OnChestSlotClicked(int index)
        {
            Debug.Log($"[ChestUI] OnChestSlotClicked({index}) called. IsOpen={IsOpen}, _currentChest={_currentChest}");
            if (!IsOpen || _currentChest == null) return;
            if (index < 0 || index >= _currentChest.Items.Count) return;

            var stack = _currentChest.Items[index];
            if (stack == null || !stack.IsValid)
            {
                Debug.Log($"[ChestUI] OnChestSlotClicked({index}) - invalid stack.");
                return;
            }

            Debug.Log($"[ChestUI] Attempting to transfer {stack.quantity}x {stack.item.itemName} to Player.");
            // Transfer from Chest to Player
            if (PlayerInventory.Instance.TryCollectItem(stack.item, stack.quantity))
            {
                Debug.Log($"[ChestUI] Transfer successful.");
                _currentChest.RemoveAt(index, stack.quantity);
                Refresh();
            }
            else
            {
                Debug.Log($"[ChestUI] Transfer failed (Player Inventory full?).");
            }
        }

        private void OnPlayerSlotClicked(int index)
        {
            Debug.Log($"[ChestUI] OnPlayerSlotClicked({index}) called. IsOpen={IsOpen}, _currentChest={_currentChest}");
            if (!IsOpen || _currentChest == null) return;

            var items = PlayerInventory.Instance.Items;
            if (index < 0 || index >= items.Count) return;

            var stack = items[index];
            if (stack == null || !stack.IsValid)
            {
                Debug.Log($"[ChestUI] OnPlayerSlotClicked({index}) - invalid stack.");
                return;
            }

            Debug.Log($"[ChestUI] Attempting to transfer {stack.quantity}x {stack.item.itemName} to Chest.");
            // Transfer from Player to Chest
            if (_currentChest.TryCollectItem(stack.item, stack.quantity))
            {
                Debug.Log($"[ChestUI] Transfer successful.");
                PlayerInventory.Instance.RemoveAt(index, stack.quantity);
                Refresh();
            }
            else
            {
                Debug.Log($"[ChestUI] Transfer failed (Chest full?).");
            }
        }
    }
}
