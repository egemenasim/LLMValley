using UnityEngine;
using LLMValley.Components.Animation;

namespace LLMValley.Player
{
    public class PlayerToolController : MonoBehaviour
    {
        [SerializeField] private ToolType selectedTool = ToolType.None;

        private PlayerAnimationManager _animationManager;

        private void Awake()
        {
            _animationManager = GetComponent<PlayerAnimationManager>();
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

        private void UseSelectedTool()
        {
            if (selectedTool == ToolType.None)
                return;

            if (_animationManager == null)
                return;

            _animationManager.PlayToolAnimation(selectedTool);
        }
    }
}