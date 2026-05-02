using UnityEngine;

/// <summary>
/// Example subclass of Interactable. Attach this instead of Interactable on a bed object.
/// Override interact_event() to implement the actual sleep logic.
/// player_entered_area / player_exited_area are inherited automatically.
/// </summary>
[AddComponentMenu("Interaction/Sleepable")]
public class Sleepable : Interactable
{
    // ──────────────────────────────────────────────────────────
    //  Inspector
    // ──────────────────────────────────────────────────────────

    [Header("Sleep Settings")]
    [Tooltip("How many in-game hours sleeping will advance the clock.")]
    [SerializeField] private int hoursToSleep = 8;

    // ──────────────────────────────────────────────────────────
    //  Interaction
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Triggered by pressing [E] or clicking the interact button while in range.
    /// Put all sleep-specific logic here.
    /// </summary>
    public override void interact_event()
    {
        base.interact_event(); // fires the base UnityEvent (onInteract)

        Debug.Log("[Sleepable] Player is sleeping for " + hoursToSleep + " hours.");

        // TODO: hook into your Calendar / GameTime system here
        // e.g. CalendarManager.Instance.AdvanceHours(hoursToSleep);
        // e.g. SceneManager.LoadScene("SleepTransition");
    }
}
