using System.Collections.Generic;
using UnityEngine;
using LLMValley.Items;
using LLMValley.Components.Animation;
using LLMValley.UI;

namespace LLMValley.Player
{
    public class PlayerToolController : MonoBehaviour
    {
        [Header("Animation")]
        [SerializeField] private PlayerAnimationManager animationManager;

        [Header("Inventory")]
        [SerializeField] private PlayerInventory playerInventory;

        private readonly Dictionary<ItemType, IToolAction> _toolActions = new();

        private void Awake()
        {
            RegisterToolActions();

            // Fallback registrations so core tool ItemTypes are treated as usable
            // even if no IToolAction components are configured in the scene.
            RegisterFallbackToolActions();

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
                UseSelectedInventoryItem();
        }

        private void RegisterToolActions()
        {
            _toolActions.Clear();

            var behaviours = GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var behaviour in behaviours)
            {
                if (behaviour == null)
                    continue;

                if (behaviour is not IToolAction action)
                    continue;

                if (_toolActions.ContainsKey(action.ItemType))
                {
                    Debug.LogWarning($"[PlayerToolController] Duplicate tool action found: {action.ItemType}");
                    continue;
                }

                _toolActions.Add(action.ItemType, action);
            }
        }

        private void RegisterFallbackToolActions()
        {
            RegisterFallback(ItemType.Hoe);
            RegisterFallback(ItemType.WaterCan);
            RegisterFallback(ItemType.Rod);
            RegisterFallback(ItemType.Seed);
        }

        private void RegisterFallback(ItemType itemType)
        {
            if (_toolActions.ContainsKey(itemType))
                return;

            _toolActions.Add(itemType, new FallbackToolAction(itemType));
        }

        private sealed class FallbackToolAction : IToolAction
        {
            public ItemType ItemType { get; }

            public FallbackToolAction(ItemType itemType)
            {
                ItemType = itemType;
            }

            public bool CanUse() => true;

            public void Use()
            {
            }
        }

        private void UseSelectedInventoryItem()
        {
            ItemStack selectedStack = GetSelectedStack();

            if (selectedStack == null || !selectedStack.IsValid)
            {
                Debug.LogWarning("[PlayerToolController] No valid item selected.");
                return;
            }

            ItemData selectedItem = selectedStack.item;
            ItemType selectedItemType = selectedItem.itemType;

            if (!_toolActions.TryGetValue(selectedItemType, out IToolAction action))
            {
                Debug.LogWarning($"[PlayerToolController] Selected item is not usable as a tool: {selectedItem.itemName} (ItemType: {selectedItemType})");
                return;
            }

            if (!action.CanUse())
            {
                Debug.LogWarning($"[PlayerToolController] Tool use blocked by CanUse(): {selectedItemType}");
                return;
            }

            if (animationManager != null)
                animationManager.PlayToolAnimation(selectedItemType);
            else
                Debug.LogWarning($"[PlayerToolController] No PlayerAnimationManager found for: {selectedItemType}");

            action.Use();
        }

        private ItemStack GetSelectedStack()
        {
            if (playerInventory == null)
                return null;

            InventoryUI ui = playerInventory.InventoryUI;
            if (ui == null)
                return null;

            return ui.GetSelectedStack();
        }

    }
}