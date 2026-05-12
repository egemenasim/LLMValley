using System.Collections;
using UnityEngine;
using LLMValley.Items;

namespace LLMValley.Items
{
    /// <summary>
    /// A world-space item that is picked up when the Player enters its trigger.
    ///
    /// Works with:
    ///   • 2D physics  (OnTriggerEnter2D — Collider2D + Rigidbody2D on player)
    ///   • 3D physics  (OnTriggerEnter   — Collider   + Rigidbody  on player)
    ///   • Direct transform movement (distance check in Update — no physics needed)
    ///
    /// Requirements:
    ///   • Player GameObject must be tagged "Player"
    ///   • Player must have PlayerInventory (IItemCollector) to receive the item
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
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

        [Header("Pickup")]
        [Tooltip("Fallback collection radius used when trigger events don't fire " +
                 "(e.g. direct transform movement without Rigidbody).")]
        public float pickupRadius = 0.6f;

        // ── Private ───────────────────────────────────────────────────────────────

        private SpriteRenderer _spriteRenderer;
        private Vector3        _originPosition;
        private bool           _collected;
        private Transform      _playerTransform;   // cached for the proximity check

        // ── Unity lifecycle ───────────────────────────────────────────────────────

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _originPosition = transform.position;
            ApplyIcon();
        }

        private void Start()
        {
            ApplyIcon();
            StartCoroutine(BobRoutine());

            // Cache the player transform once so Update doesn't call
            // FindGameObjectWithTag every frame.
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                _playerTransform = player.transform;
        }

        private void Update()
        {
            // Proximity fallback — works even when trigger events don't fire
            // because the player moves via direct transform assignment.
            if (_collected || _playerTransform == null) return;

            if (Vector3.Distance(transform.position, _playerTransform.position) <= pickupRadius)
                Collect(_playerTransform.gameObject);
        }

        // ── Trigger detection (2D) ────────────────────────────────────────────────

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_collected) return;
            if (!other.CompareTag("Player")) return;
            Collect(other.gameObject);
        }

        // ── Trigger detection (3D) ────────────────────────────────────────────────

        private void OnTriggerEnter(Collider other)
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
