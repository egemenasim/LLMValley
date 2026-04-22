using UnityEngine;

namespace LLMValley.Components.Animation
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimationManager : MonoBehaviour
    {
        public enum Direction
        {
            Idle,
            Up,
            Down,
            Left,
            Right
        }

        private Animator animator;
        private Direction currentDirection = Direction.Idle;

        [Header("Animation State Names")]
        [Tooltip("Animator içindeki state isimlerini buraya yazın.")]
        [SerializeField] private string walkUpAnim = "MainRunUp";
        [SerializeField] private string walkDownAnim = "MainRunDown";
        [SerializeField] private string walkLeftAnim = "MainRunLeft";
        [SerializeField] private string walkRightAnim = "MainRunRight";
        [SerializeField] private string idleAnim = "MainIdle";

        private void Awake()
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogWarning("Animator component missing on " + gameObject.name);
            }
        }

        /// <summary>
        /// Sadece yönü vererek animasyonu değiştiren ana fonksiyon
        /// </summary>
        public void PlayAnimation(Direction newDirection)
        {
            // Eğer aynı animasyondaysak tekrar play etmesini engelliyoruz
            if (currentDirection == newDirection) return;

            currentDirection = newDirection;
            
            if (animator == null) return;
            
            // Animator Controller atanmamışsa veya obje kapalıysa hata vermesin
            if (animator.runtimeAnimatorController == null || !animator.isActiveAndEnabled)
            {
                return;
            }

            // Transition olmadığı için doğrudan Play ile state'e geçiş yapıyoruz. 
            // 0 parametresi Base Layer'ı temsil eder. -1 hatasını önler.
            switch (currentDirection)
            {
                case Direction.Up:
                    animator.Play(walkUpAnim, 0);
                    break;
                case Direction.Down:
                    animator.Play(walkDownAnim, 0);
                    break;
                case Direction.Left:
                    animator.Play(walkLeftAnim, 0);
                    break;
                case Direction.Right:
                    animator.Play(walkRightAnim, 0);
                    break;
                case Direction.Idle:
                    animator.Play(idleAnim, 0);
                    break;
            }
        }

        /// <summary>
        /// Movement scripti içerisinden Vector2 hızı göndererek otomatik yön bulmasını sağlayabilirsiniz
        /// </summary>
        public void UpdateDirectionFromVelocity(Vector2 velocity)
        {
            if (velocity.sqrMagnitude < 0.01f)
            {
                PlayAnimation(Direction.Idle);
                return;
            }

            // X eksenindeki hız Y ekseninden büyükse yatay, değilse dikey hareket
            if (Mathf.Abs(velocity.x) > Mathf.Abs(velocity.y))
            {
                PlayAnimation(velocity.x > 0 ? Direction.Right : Direction.Left);
            }
            else
            {
                PlayAnimation(velocity.y > 0 ? Direction.Up : Direction.Down);
            }
        }
    }
}
