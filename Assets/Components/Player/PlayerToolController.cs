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
        private float _discardHoldTime = 0f;

        private void Awake()
        {
            EnsureCoreToolActionComponents();

            RegisterToolActions();

            // Fallback registrations so core tool ItemTypes are treated as usable
            // even if no IToolAction components are configured in the scene.
            RegisterFallbackToolActions();

            if (animationManager == null)
            {
                animationManager = GetComponent<PlayerAnimationManager>();
                if (animationManager == null)
                    animationManager = GetComponentInChildren<PlayerAnimationManager>(true);
            }

            if (playerInventory == null)
            {
                playerInventory = GetComponent<PlayerInventory>();
                if (playerInventory == null)
                    playerInventory = GetComponentInChildren<PlayerInventory>(true);
            }
        }

        private void EnsureCoreToolActionComponents()
        {
            EnsureToolActionComponent<HoeToolAction>(ItemType.Hoe);
            EnsureToolActionComponent<WaterCanToolAction>(ItemType.WaterCan);
            EnsureToolActionComponent<RodToolAction>(ItemType.Rod);
            EnsureToolActionComponent<SeedToolAction>(ItemType.Seed);
        }

        private void EnsureToolActionComponent<T>(ItemType itemType) where T : MonoBehaviour, IToolAction
        {
            // If one already exists anywhere under the Player, don't add another.
            if (GetComponentInChildren<T>(true) != null)
                return;

            // Add to the same GameObject as PlayerToolController so it is guaranteed to be found.
            gameObject.AddComponent<T>();
            Debug.Log($"[PlayerToolController] Added missing tool action component: {typeof(T).Name} for {itemType}");
        }

        private void Update()
        {
            if (PlayerInputLock.IsLocked)
                return;

            if (Input.GetMouseButtonDown(0))
            {
                if (UnityEngine.EventSystems.EventSystem.current == null || !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                {
                    UseSelectedInventoryItem();
                }
            }

            HandleDiscardInput();
        }

        private void HandleDiscardInput()
        {
            ItemStack stack = GetSelectedStackInternal();
            if (stack == null || !stack.IsValid)
            {
                _discardHoldTime = 0f;
                return;
            }

            if (Input.GetKey(KeyCode.Z))
            {
                // If it's negative, it means we recently discarded and are waiting for the user to release the key.
                if (_discardHoldTime >= 0f)
                {
                    _discardHoldTime += Time.deltaTime;
                    if (_discardHoldTime >= 0.3f)
                    {
                        Debug.Log($"[PlayerToolController] Discarding {stack.quantity}x {stack.item.itemName}");
                        ConsumeSelectedStackItem(stack.quantity);
                        _discardHoldTime = -100f; 
                    }
                }
            }
            else
            {
                _discardHoldTime = 0f;
            }
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
            Debug.LogWarning($"[PlayerToolController] Registered fallback tool action for: {itemType} (no-op Use)");
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

            Debug.Log($"[PlayerToolController] Use item: {selectedItem.itemName} (ItemType: {selectedItemType}) -> Action: {action.GetType().Name}");

            if (!action.CanUse())
            {
                Debug.LogWarning($"[PlayerToolController] Tool use blocked by CanUse(): {selectedItemType}");
                return;
            }

            Debug.Log($"[PlayerToolController] CanUse() ok -> PlayToolAnimation({selectedItemType})");

            if (animationManager != null)
                animationManager.PlayToolAnimation(selectedItemType);
            else
                Debug.LogWarning($"[PlayerToolController] No PlayerAnimationManager found for: {selectedItemType}");

            action.Use();
        }

        public ItemStack GetSelectedStack() => GetSelectedStackInternal();

        public void ConsumeSelectedStackItem(int amount = 1)
        {
            if (amount <= 0 || playerInventory == null)
                return;

            InventoryUI ui = playerInventory.InventoryUI;
            if (ui == null)
                return;

            int index = ui.SelectedIndex;
            if (index < 0)
                return;

            playerInventory.RemoveAt(index, amount);
        }

        public bool TryGetTargetFarmableArea(out FarmableArea area)
        {
            area = null;

            Camera cam = Camera.main;
            if (cam == null)
                cam = Object.FindAnyObjectByType<Camera>();

            if (cam == null)
            {
                Debug.LogWarning("[PlayerToolController] No Camera found (Camera.main is null). Ensure your active camera is tagged as MainCamera.");
                return false;
            }

            Vector3 world = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 point = new Vector2(world.x, world.y);

            Collider2D hit = Physics2D.OverlapPoint(point);
            if (hit == null)
                return false;

            area = hit.GetComponentInParent<FarmableArea>();
            return area != null;
        }

        public Plantable FindPlantPrefabForSeed(ItemData seedItem)
        {
            if (seedItem == null)
                return null;

            // Simple mapping strategy: find any Plantable prefab/instance in loaded resources
            // whose PlantData's itemID matches the seed's itemID + 1000 (see ItemDataGenerator).
            int targetPlantId = seedItem.itemID + 1000;

            Plantable[] allPlantables = Resources.FindObjectsOfTypeAll<Plantable>();
            for (int i = 0; i < allPlantables.Length; i++)
            {
                Plantable p = allPlantables[i];
                if (p == null)
                    continue;

                ItemData data = p.PlantData;
                if (data != null && data.itemID == targetPlantId)
                    return p;
            }

            return null;
        }

        private ItemStack GetSelectedStackInternal()
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