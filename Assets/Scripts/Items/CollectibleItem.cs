using System.Collections;
using UnityEngine;
using LLMValley.Items;

namespace LLMValley.Items
{
    /// <summary>
    /// A world-space item that floats in place and is picked up when the Player
    /// enters its trigger collider.
    ///
    /// Dependencies:
    ///   • Requires a <see cref="SpriteRenderer"/> on the same GameObject.
    ///   • Requires a trigger <see cref="Collider2D"/> on the same GameObject.
    ///   • The Player GameObject must have the tag "Player" and implement
    ///     <see cref="IItemCollector"/> to receive the pickup callback.
    ///     If no collector is found the item is still destroyed (silent pickup).
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Collider2D))]
    public class CollectibleItem : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────────

        [Header("Item Data")]
        [Tooltip("The ScriptableObject that describes this item.")]
        public ItemData itemData;

        [Tooltip("How many units of the item this pickup contains.")]
        [Min(1)]
        public int quantity = 1;

        [Header("Audio")]
        [Tooltip("Played when the player picks up the item. Optional.")]
        public AudioClip pickupSound;

        [Header("Bob Animation")]
        [Tooltip("Vertical distance (units) the item drifts up and down.")]
        public float bobAmplitude = 0.1f;

        [Tooltip("Seconds for one full bob cycle.")]
        public float bobCycle = 1f;

        // ── Private ───────────────────────────────────────────────────────────────

        private SpriteRenderer _spriteRenderer;
        private Vector3        _originPosition;
        private bool           _collected;

        // ── Unity lifecycle ───────────────────────────────────────────────────────

        private void Awake()
        {
            _spriteRenderer  = GetComponent<SpriteRenderer>();
            _originPosition  = transform.position;

            ApplyIcon();
        }

        private void Start()
        {
            StartCoroutine(BobRoutine());
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_collected) return;
            if (!other.CompareTag("Player")) return;

            Collect(other.gameObject);
        }

        // ── Core logic ────────────────────────────────────────────────────────────

        private void ApplyIcon()
        {
            if (itemData != null && itemData.icon != null)
                _spriteRenderer.sprite = itemData.icon;
        }

        private void Collect(GameObject player)
        {
            _collected = true;

            // Try to hand the item to an inventory component on the player.
            // IItemCollector is intentionally lightweight so the inventory system
            // can be built independently.
            IItemCollector collector = player.GetComponent<IItemCollector>();
            collector?.CollectItem(itemData, quantity);

            PlayPickupSound();
            Destroy(gameObject);
        }

        private void PlayPickupSound()
        {
            if (pickupSound == null) return;

            // Spawn a temporary AudioSource so the clip finishes even after this
            // GameObject is destroyed.
            GameObject soundHost = new GameObject("PickupSound");
            soundHost.transform.position = transform.position;

            AudioSource src = soundHost.AddComponent<AudioSource>();
            src.clip        = pickupSound;
            src.spatialBlend = 0f; // 2-D / UI sound
            src.Play();

            Destroy(soundHost, pickupSound.length + 0.1f);
        }

        // ── Bob coroutine ─────────────────────────────────────────────────────────

        /// <summary>
        /// Smoothly oscillates the item on the Y axis using a sine wave.
        /// Runs until the object is destroyed.
        /// </summary>
        private IEnumerator BobRoutine()
        {
            float elapsed = 0f;

            while (true)
            {
                elapsed += Time.deltaTime;

                float offset = Mathf.Sin((elapsed / bobCycle) * Mathf.PI * 2f) * bobAmplitude;
                transform.position = _originPosition + new Vector3(0f, offset, 0f);

                yield return null;
            }
        }
    }
}
