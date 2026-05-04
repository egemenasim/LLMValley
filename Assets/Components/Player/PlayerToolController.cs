using System.Collections.Generic;
using UnityEngine;
using LLMValley.Items;
using LLMValley.Components.Animation;
using LLMValley.UI;

namespace LLMValley.Player
{
    public class PlayerToolController : MonoBehaviour
    {
        [Header("Test Selection")]
        [SerializeField] private ItemType selectedTool = ItemType.Misc;

        [Header("Animation")]
        [SerializeField] private PlayerAnimationManager animationManager;

        [Header("Inventory Gate")]
        [SerializeField] private PlayerInventory playerInventory;

        private readonly Dictionary<ItemType, IToolAction> _toolActions = new();
        //toola göre script değişecek bu şuanlık test sadece

        private void Awake()
        {
            RegisterToolActions();

            if (animationManager == null)
            {
                animationManager = GetComponent<PlayerAnimationManager>();
                if (animationManager == null)
                    animationManager = GetComponentInChildren<PlayerAnimationManager>();
            }

            if (playerInventory == null)
            {
                playerInventory = GetComponent<PlayerInventory>();
                if (playerInventory == null)
                    playerInventory = GetComponentInChildren<PlayerInventory>();
            }
        }

        private void Update()
        {
            if (PlayerInputLock.IsLocked)
                return;

            if (Input.GetMouseButtonDown(0))
            {
                UseSelectedTool();
            }
        }

        private void RegisterToolActions()
        {
            _toolActions.Clear();

            IToolAction[] actions = GetComponents<IToolAction>();

            foreach (IToolAction action in actions)
            {
                if (action == null)
                    continue;

                if (_toolActions.ContainsKey(action.ItemType))
                {
                    Debug.LogWarning($"[PlayerToolController] Duplicate tool action found: {action.ItemType}");
                    continue;
                }

                _toolActions.Add(action.ItemType, action);
            }
        }

        private void UseSelectedTool()
        {
            SyncSelectedToolFromUI();

            if (!IsSelectedToolInInventory())
            {
                Debug.LogWarning($"[PlayerToolController] Selected tool not in inventory hotbar slot: {selectedTool}");
                return;
            }

            if (animationManager != null)
                animationManager.PlayToolAnimation(selectedTool);
            else
                Debug.LogWarning($"[PlayerToolController] No PlayerAnimationManager found for tool animation: {selectedTool}");

            if (!_toolActions.TryGetValue(selectedTool, out IToolAction action))
            {
                Debug.LogWarning($"[PlayerToolController] No action found for selected tool: {selectedTool}");
                return;
            }

            if (!action.CanUse())
            {
                Debug.LogWarning($"[PlayerToolController] Tool use blocked by CanUse(): {selectedTool}");
                return;
            }

            action.Use();
        }

        private void SyncSelectedToolFromUI()
        {
            InventoryUI ui = playerInventory?.InventoryUI;
            if (ui == null)
                return;

            ItemStack selectedStack = ui.GetSelectedStack();
            if (selectedStack != null && selectedStack.IsValid)
                selectedTool = selectedStack.item.itemType;
        }

        private bool IsSelectedToolInInventory()
        {
            if (playerInventory == null)
                return false;

            // Prefer hotbar selection (number keys / UI click) when available.
            if (playerInventory.InventoryUI != null)
            {
                ItemStack selectedStack = playerInventory.InventoryUI.GetSelectedStack();
                if (selectedStack != null && selectedStack.IsValid)
                    return selectedStack.item.itemType == selectedTool;
            }

            // Fallback: allow tool use if the item exists anywhere in inventory.
            return playerInventory.HasItemType(selectedTool);
        }

        public void SetSelectedTool(ItemType itemType)
        {
            selectedTool = itemType;
        }

        public ItemType GetSelectedTool()
        {
            return selectedTool;
        }
    }
}