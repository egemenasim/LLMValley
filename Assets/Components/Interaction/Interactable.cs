using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Attach this component to any GameObject to detect when the Player enters or exits
/// the assigned Collider2D trigger area.
///
/// Usage:
///   1. Add this component to a GameObject.
///   2. Assign a Collider2D (with 'Is Trigger' enabled) to the Trigger Collider field.
///   3. Override player_entered_area() / player_exited_area() in a subclass, OR
///      subscribe callbacks via the UnityEvents in the Inspector.
/// </summary>
[AddComponentMenu("Interaction/Interactable")]
public class Interactable : MonoBehaviour
{
    [Header("Trigger Area")]
    [Tooltip("The 2D collider that defines the interactable area. Must have 'Is Trigger' enabled.")]
    [SerializeField] private Collider2D triggerCollider;

    [Header("Events")]
    [Tooltip("Raised when the Player enters the trigger area.")]
    public UnityEvent onPlayerEnteredArea;

    [Tooltip("Raised when the Player exits the trigger area.")]
    public UnityEvent onPlayerExitedArea;

    private const string PlayerTag = "Player";

    private void OnValidate()
    {
        if (triggerCollider != null && !triggerCollider.isTrigger)
        {
            Debug.LogWarning(
                $"[Interactable] Collider2D on '{triggerCollider.name}' is not set as a trigger. " +
                "Enable 'Is Trigger' on it for area detection to work correctly.", this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(PlayerTag)) return;

        player_entered_area();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(PlayerTag)) return;

        player_exited_area();
    }

    /// <summary>
    /// Called when the Player enters the trigger area.
    /// Override in a derived class to add custom behaviour, or wire up
    /// callbacks through the onPlayerEnteredArea UnityEvent in the Inspector.
    /// </summary>
    public virtual void player_entered_area()
    {
        Debug.Log($"[Interactable] Player entered: {gameObject.name}");
        onPlayerEnteredArea?.Invoke();
    }

    /// <summary>
    /// Called when the Player exits the trigger area.
    /// Override in a derived class to add custom behaviour, or wire up
    /// callbacks through the onPlayerExitedArea UnityEvent in the Inspector.
    /// </summary>
    public virtual void player_exited_area()
    {
        Debug.Log($"[Interactable] Player exited: {gameObject.name}");
        onPlayerExitedArea?.Invoke();
    }
}
