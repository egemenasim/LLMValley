using UnityEngine;
using LLMValley.Items;
using LLMValley.UI;
using LLMValley.Components.Animation;

namespace LLMValley.Player
{
    public class RodToolAction : MonoBehaviour, IToolAction
    {
        public ItemType ItemType => ItemType.Rod;

        [Tooltip("The item to give when fishing succeeds.")]
        [SerializeField] private ItemData fishReward;

        private PlayerAnimationManager _animationManager;

        private void Awake()
        {
            _animationManager = GetComponent<PlayerAnimationManager>();
            if (_animationManager == null)
            {
                _animationManager = GetComponentInParent<PlayerAnimationManager>();
            }
        }

        public bool CanUse() => true;

        public void Use()
        {
            if (_animationManager == null) return;

            Vector2 direction = _animationManager.FacingDirection;
            Vector2 targetPos = (Vector2)transform.position + direction * 3f;

            // Check for water layer by checking Tilemaps directly
            // This is more robust than Physics2D for tiles that might not have physics shapes generated.
            bool waterFound = false;
            foreach (var tilemap in FindObjectsByType<UnityEngine.Tilemaps.Tilemap>(FindObjectsSortMode.None))
            {
                if (tilemap.gameObject.layer == LayerMask.NameToLayer("Water"))
                {
                    Vector3Int cellPos = tilemap.WorldToCell(targetPos);
                    if (tilemap.HasTile(cellPos))
                    {
                        waterFound = true;
                        break;
                    }
                }
            }

            if (waterFound)
            {
                Debug.Log("[RodToolAction] Hit water! Starting fishing minigame...");
                
                // Ensure UI is spawned if not already present
                if (FishingMinigameUI.Instance == null)
                {
                    GameObject uiObj = new GameObject("FishingMinigameUI");
                    uiObj.AddComponent<FishingMinigameUI>();
                }
                
                // Disable player input during fishing
                PlayerInputLock.Lock();

                FishingMinigameUI.Instance.OnMinigameEnded += OnMinigameEnded;
                FishingMinigameUI.Instance.StartMinigame();
            }
            else
            {
                Debug.Log($"[RodToolAction] No water found at {targetPos}.");
            }
        }

        private void OnMinigameEnded(bool success)
        {
            FishingMinigameUI.Instance.OnMinigameEnded -= OnMinigameEnded;
            
            // Re-enable player input
            PlayerInputLock.Unlock();
            
            if (_animationManager != null)
            {
                _animationManager.ToggleFishing(); // Stop fishing animation
            }

            if (success)
            {
                Debug.Log("[RodToolAction] Fishing successful!");
                if (fishReward != null)
                {
                    PlayerInventory.Instance.CollectItem(fishReward, 1);
                }
                else
                {
                    Debug.LogWarning("[RodToolAction] No fishReward assigned in inspector.");
                }
            }
            else
            {
                Debug.Log("[RodToolAction] Fishing failed.");
            }
        }
    }
}
