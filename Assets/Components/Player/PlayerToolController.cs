using System.Collections.Generic;
using UnityEngine;

namespace LLMValley.Player
{
    public class PlayerToolController : MonoBehaviour
    {
        [Header("Test Selection")]
        [SerializeField] private ToolType selectedTool = ToolType.None;

        private readonly Dictionary<ToolType, IToolAction> _toolActions = new();
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

                if (_toolActions.ContainsKey(action.ToolType))
                {
                    Debug.LogWarning($"[PlayerToolController] Duplicate tool action found: {action.ToolType}");
                    continue;
                }

                _toolActions.Add(action.ToolType, action);
            }
        }

        private void UseSelectedTool()
        {
            if (selectedTool == ToolType.None)
                return;

            if (!_toolActions.TryGetValue(selectedTool, out IToolAction action))
            {
                Debug.LogWarning($"[PlayerToolController] No action found for selected tool: {selectedTool}");
                return;
            }

            if (!action.CanUse())
                return;

            action.Use();
        }

        public void SetSelectedTool(ToolType toolType)
        {
            selectedTool = toolType;
        }

        public ToolType GetSelectedTool()
        {
            return selectedTool;
        }
    }
}