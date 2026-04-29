using UnityEngine;

namespace LLMValley.Components.Animation
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimationManager : MonoBehaviour
    {
        private Animator animator;

        // Son bakılan yönü saklıyoruz
        private Vector2 lastDirection = Vector2.down;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }


        public void SetMovement(Vector2 direction)
        {
            if (animator == null) return;

            bool isMoving = direction.sqrMagnitude > 0.01f;

            // Eğer hareket varsa yönü güncelle
            if (isMoving)
            {
                lastDirection = direction.normalized;
            }

            // Animator parametrelerini set et
            animator.SetFloat("xInput", lastDirection.x);
            animator.SetFloat("yInput", lastDirection.y);
            animator.SetBool("isMoving", isMoving);
        }
    }
}