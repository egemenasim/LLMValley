using System.Collections.Generic;
using UnityEngine;
using LLMValley.Items;

namespace LLMValley.Player
{
    public class PlayerToolController : MonoBehaviour
    {
        [Header("Test Selection")]
        [SerializeField] private ItemType selectedTool = ItemType.Misc;

        private readonly Dictionary<ItemType, IToolAction> _toolActions = new();
        //toola göre script değişecek bu şuanlık test sadece

        private void Awake()
        {
            RegisterToolActions();
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
            if (!_toolActions.TryGetValue(selectedTool, out IToolAction action))
            {
                Debug.LogWarning($"[PlayerToolController] No action found for selected tool: {selectedTool}");
                return;
            }

            if (!action.CanUse())
                return;

            action.Use();
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