using UnityEngine;
using Systems.Calendar;
using System.Collections;

/// <summary>
/// Example subclass of Interactable. Attach this instead of Interactable on a bed object.
/// Override interact_event() to implement the actual sleep logic.
/// player_entered_area / player_exited_area are inherited automatically.
/// </summary>
[AddComponentMenu("Interaction/Sleepable")]
public class Sleepable : Interactable
{
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

        if (TransitionUI.Instance != null)
        {
            StartCoroutine(SleepSequenceRoutine());
        }
        else
        {
            Debug.LogWarning("[Sleepable] TransitionUI.Instance not found! Advancing day immediately.");
            if (CalendarSystem.Instance != null)
            {
                CalendarSystem.Instance.AdvanceDay();
            }
        }
    }

    private IEnumerator SleepSequenceRoutine()
    {
        // 1. Fade to black (Sleep)
        float fadeTime = 1.0f;
        TransitionUI.Instance.fade_in(fadeTime);
        
        // Wait for fade to complete + a small pause for the "night"
        yield return new WaitForSeconds(fadeTime + 0.5f);

        Debug.Log("[Sleepable] It's a new day! Advancing calendar.");

        // 2. Advance the day
        if (CalendarSystem.Instance != null)
        {
            CalendarSystem.Instance.AdvanceDay();
        }

        // Wait a tiny bit more while black
        yield return new WaitForSeconds(0.5f);

        // 3. Fade back out (Wake up)
        TransitionUI.Instance.fade_out(fadeTime);
    }
}
