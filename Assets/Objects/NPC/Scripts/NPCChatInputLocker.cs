using UnityEngine;
using UnityEngine.InputSystem;

namespace LLMValley.NPCChat
{
    public class NPCChatInputLocker : MonoBehaviour
    {
        [SerializeField] private PlayerInput playerInput;

        private CursorLockMode previousLockMode;
        private bool previousCursorVisible;
        private bool wasInputEnabled;

        public void LockForChat()
        {
            if (playerInput == null)
            {
                playerInput = FindFirstObjectByType<PlayerInput>();
            }

            previousLockMode = Cursor.lockState;
            previousCursorVisible = Cursor.visible;

            if (playerInput != null)
            {
                wasInputEnabled = playerInput.enabled;
                if (wasInputEnabled)
                {
                    playerInput.DeactivateInput();
                }
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void UnlockAfterChat()
        {
            if (playerInput == null)
            {
                playerInput = FindFirstObjectByType<PlayerInput>();
            }

            if (playerInput != null && wasInputEnabled)
            {
                playerInput.ActivateInput();
            }

            Cursor.lockState = previousLockMode;
            Cursor.visible = previousCursorVisible;
        }
    }
}
