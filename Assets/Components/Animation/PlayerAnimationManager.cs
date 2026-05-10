using LLMValley.Player;
using LLMValley.Items;
using System.Linq;
using UnityEngine;

namespace LLMValley.Components.Animation
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimationManager : MonoBehaviour
    {
        private Animator animator;

        private Vector2 lastDirection = Vector2.down; //son yönü alması için 
        [SerializeField] private PlayerMove playerMove;

        private static readonly int XInput = Animator.StringToHash("xInput");
        private static readonly int YInput = Animator.StringToHash("yInput");
        private static readonly int IsMoving = Animator.StringToHash("isMoving");

        private static readonly int WaterTrigger = Animator.StringToHash("waterTrigger");
        private static readonly int HoeTrigger = Animator.StringToHash("hoeTrigger");
        private static readonly int IsFishing = Animator.StringToHash("isFishing");
        private static readonly int PlantTrigger = Animator.StringToHash("plantTrigger");
        private bool isFishingActive = false;
        private void Awake()
        {
            animator = GetComponent<Animator>();

            // If this GameObject has an Animator but no controller assigned,
            // it is very likely the real character Animator lives on a child.
            if (animator == null || animator.runtimeAnimatorController == null)
            {
                var animators = GetComponentsInChildren<Animator>(true);
                for (int i = 0; i < animators.Length; i++)
                {
                    var candidate = animators[i];
                    if (candidate != null && candidate.runtimeAnimatorController != null)
                    {
                        animator = candidate;
                        break;
                    }
                }
            }

            if (playerMove == null)
                playerMove = GetComponent<PlayerMove>();
        }

        public void SetMovement(Vector2 direction)
        {
            if (animator == null) return;

            bool isMoving = direction.sqrMagnitude > 0.01f;

            if (isMoving)
            {
                lastDirection = direction.normalized; //harektte son yönü güncelleştir
            }

            animator.SetFloat(XInput, lastDirection.x);
            animator.SetFloat(YInput, lastDirection.y);
            animator.SetBool(IsMoving, isMoving);
        }

        public void ToggleFishing()
        {
            isFishingActive = !isFishingActive;

            animator.SetBool(IsFishing, isFishingActive);

            if (playerMove != null)
                playerMove.SetMoveState(!isFishingActive);

            if (isFishingActive)
            {
                animator.SetBool(IsMoving, false);
                animator.SetFloat(XInput, lastDirection.x);
                animator.SetFloat(YInput, lastDirection.y);
            }
        }

        public void PlayToolAnimation(ItemType itemType)
        {
            if (animator == null)
            {
                Debug.LogWarning("[PlayerAnimationManager] Animator reference is null.");
                return;
            }

            var controllerName = animator.runtimeAnimatorController != null ? animator.runtimeAnimatorController.name : "(no controller)";
            Debug.Log($"[PlayerAnimationManager] PlayToolAnimation -> {itemType} | AnimatorGO: {animator.gameObject.name} | Controller: {controllerName}");

            switch (itemType)
            {
                case ItemType.WaterCan:
                    animator.SetTrigger(WaterTrigger);
                    Debug.Log("water can selected");
                    break;

                case ItemType.Hoe:
                    animator.SetTrigger(HoeTrigger);
                    break;

                case ItemType.Rod:
                    ToggleFishing();
                    break;

                case ItemType.Seed:
                    if (!animator.parameters.Any(p => p.type == AnimatorControllerParameterType.Trigger && p.nameHash == PlantTrigger))
                    {
                        Debug.LogWarning("[PlayerAnimationManager] plantTrigger not found on Animator. Check parameter name matches exactly: 'plantTrigger'");
                    }
                    animator.SetTrigger(PlantTrigger);
                    break;
            }
        }
    }
}