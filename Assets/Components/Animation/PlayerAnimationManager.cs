using LLMValley.Player;
using LLMValley.Items;
using UnityEngine;

namespace LLMValley.Components.Animation
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimationManager : MonoBehaviour
    {
        private Animator animator;

        private Vector2 lastDirection = Vector2.down; //son yönü alması için 

        private static readonly int XInput = Animator.StringToHash("xInput");
        private static readonly int YInput = Animator.StringToHash("yInput");
        private static readonly int IsMoving = Animator.StringToHash("isMoving");

        private static readonly int WaterTrigger = Animator.StringToHash("waterTrigger");
        private static readonly int HoeTrigger = Animator.StringToHash("hoeTrigger");
        private static readonly int FishingTrigger = Animator.StringToHash("fishingTrigger");
        private static readonly int PlantTrigger = Animator.StringToHash("plantTrigger");

        private void Awake()
        {
            animator = GetComponent<Animator>();
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

        public void PlayToolAnimation(ItemType itemType)
        {
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
                    animator.SetTrigger(FishingTrigger);
                    break;

                case ItemType.Seed:
                    animator.SetTrigger(PlantTrigger);
                    break;
            }
        }
    }
}