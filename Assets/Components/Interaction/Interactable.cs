using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Base interactable component.
///
/// • When the Player enters the trigger area, spawns BUTTON_Interact as a child of
///   the scene's Canvas and tracks it in screen-space over this object every frame.
/// • When the Player exits, the button is destroyed.
/// • While the button is active, pressing [E] on the keyboard OR clicking the button
///   calls interact_event().
/// • Subclasses (e.g. Sleepable) override interact_event() for custom behaviour.
/// </summary>
[AddComponentMenu("Interaction/Interactable")]
public class Interactable : MonoBehaviour
{
    // ──────────────────────────────────────────────────────────
    //  Inspector
    // ──────────────────────────────────────────────────────────

    [Header("Trigger Area")]
    [Tooltip("Collider2D that defines the interactable area. Must have 'Is Trigger' enabled.")]
    [SerializeField] private Collider2D triggerCollider;

    [Header("Interact Button")]
    [Tooltip("The BUTTON_Interact UI prefab to spawn when the player is in range.")]
    [SerializeField] private GameObject interactButtonPrefab;

    [Tooltip("World-space offset from this object's position where the button appears.")]
    [SerializeField] private Vector3 buttonWorldOffset = new Vector3(0f, 1.5f, 0f);

    [Header("Events")]
    [Tooltip("Raised when the Player enters the trigger area.")]
    public UnityEvent onPlayerEnteredArea;

    [Tooltip("Raised when the Player exits the trigger area.")]
    public UnityEvent onPlayerExitedArea;

    [Tooltip("Raised when the player triggers an interaction (E key or button click).")]
    public UnityEvent onInteract;

    // ──────────────────────────────────────────────────────────
    //  Private state
    // ──────────────────────────────────────────────────────────

    private const string PlayerTag = "Player";

    private GameObject _spawnedButton;
    private RectTransform _buttonRect;
    private Canvas _targetCanvas;
    private RectTransform _canvasRect;
    private Camera _cam;
    private bool _playerInRange;

    // ──────────────────────────────────────────────────────────
    //  Unity callbacks
    // ──────────────────────────────────────────────────────────

    private void Awake()
    {
        _cam = Camera.main;
        FindTargetCanvas();
    }

    private void OnValidate()
    {
        if (triggerCollider != null && !triggerCollider.isTrigger)
            Debug.LogWarning("[Interactable] Collider2D is not marked as a trigger on: " + gameObject.name, this);
    }

    private void Update()
    {
        if (_spawnedButton != null && _buttonRect != null)
            UpdateButtonPosition();

        // Don't allow interaction while player is sleeping
        if (_playerInRange && _spawnedButton != null && !Sleepable.IsPlayerSleeping && Input.GetKeyDown(KeyCode.E))
            interact_event();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(PlayerTag)) return;
        _playerInRange = true;
        player_entered_area();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(PlayerTag)) return;
        _playerInRange = false;
        player_exited_area();
    }

    private void OnDestroy()
    {
        DestroyButton();
    }

    // ──────────────────────────────────────────────────────────
    //  Virtual methods — override in subclasses
    // ──────────────────────────────────────────────────────────

    /// <summary>Called when the Player enters the trigger area.</summary>
    public virtual void player_entered_area()
    {
        Debug.Log("[Interactable] Player entered area of: " + gameObject.name);
        SpawnButton();
        onPlayerEnteredArea?.Invoke();
    }

    /// <summary>Called when the Player exits the trigger area.</summary>
    public virtual void player_exited_area()
    {
        Debug.Log("[Interactable] Player exited area of: " + gameObject.name);
        DestroyButton();
        onPlayerExitedArea?.Invoke();
    }

    /// <summary>
    /// Called when the player presses [E] or clicks the interact button.
    /// Override in subclasses to implement the actual interaction.
    /// </summary>
    public virtual void interact_event()
    {
        Debug.Log("[Interactable] interact_event fired on: " + gameObject.name);
        onInteract?.Invoke();
    }

    // ──────────────────────────────────────────────────────────
    //  Button helpers
    // ──────────────────────────────────────────────────────────

    private void FindTargetCanvas()
    {
        // Try to find a canvas tagged "MainCanvas" first
        GameObject mainCanvasGO = GameObject.FindWithTag("MainCanvas");
        if (mainCanvasGO != null)
        {
            _targetCanvas = mainCanvasGO.GetComponent<Canvas>();
            Debug.Log("[Interactable] Canvas found with tag 'MainCanvas' for: " + gameObject.name, this);
        }
        
        // Fallback to any canvas if not found by tag
        if (_targetCanvas == null)
        {
            _targetCanvas = FindFirstObjectByType<Canvas>();
            Debug.Log("[Interactable] Canvas found with FindFirstObjectByType in scene for: " + gameObject.name, this);
        }

        if (_targetCanvas != null)
        {
            _canvasRect = _targetCanvas.GetComponent<RectTransform>();
            Debug.Log("[Interactable] Canvas found in scene for: " + gameObject.name, this);
        }
        else
        {
            Debug.LogWarning("[Interactable] No Canvas found in scene for: " + gameObject.name, this);
        }
    }

    private void SpawnButton()
    {
        if (_spawnedButton != null) return;

        if (interactButtonPrefab == null)
        {
            Debug.LogWarning("[Interactable] interactButtonPrefab is not assigned on: " + gameObject.name, this);
            return;
        }

        if (_targetCanvas == null)
        {
            FindTargetCanvas();
            if (_targetCanvas == null) return;
        }

        // Spawn as a child of the Canvas so UI renders correctly
        _spawnedButton = Instantiate(interactButtonPrefab, _targetCanvas.transform);
        _buttonRect = _spawnedButton.GetComponent<RectTransform>();

        // Snap to position immediately (no one-frame pop)
        UpdateButtonPosition();

        // Wire Button.onClick → interact_event
        Button btn = _spawnedButton.GetComponentInChildren<Button>();
        if (btn != null)
            btn.onClick.AddListener(interact_event);
        else
            Debug.LogWarning("[Interactable] BUTTON_Interact prefab has no Button component.", this);
    }

    private void DestroyButton()
    {
        if (_spawnedButton == null) return;

        Button btn = _spawnedButton.GetComponentInChildren<Button>();
        if (btn != null)
            btn.onClick.RemoveListener(interact_event);

        Destroy(_spawnedButton);
        _spawnedButton = null;
        _buttonRect = null;
    }

    /// <summary>
    /// Converts this object's world position (+offset) to the Canvas's local coordinate
    /// system so the button always floats directly above the interactable in screen-space.
    /// </summary>
    private void UpdateButtonPosition()
    {
        if (_cam == null || _canvasRect == null || _buttonRect == null) return;

        // World point → screen point
        Vector3 worldPos = transform.position + buttonWorldOffset;
        Vector2 screenPoint = _cam.WorldToScreenPoint(worldPos);

        // Screen point → Canvas local point
        bool isOverlay = _targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRect,
            screenPoint,
            isOverlay ? null : _cam,
            out Vector2 localPoint);

        _buttonRect.localPosition = localPoint;
    }
}
