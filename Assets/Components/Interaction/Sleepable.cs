using UnityEngine;
using Systems.Calendar;
using System.Collections;
using LLMValley.SaveSystem;

/// <summary>
/// Subclass of Interactable for bed objects. Handles sleep logic and prevents
/// player movement/interaction during sleep.
/// </summary>
[AddComponentMenu("Interaction/Sleepable")]
public class Sleepable : Interactable
{
    private static bool _isPlayerSleeping = false;

    /// <summary>True when the player is currently sleeping.</summary>
    public static bool IsPlayerSleeping => _isPlayerSleeping;

    // ──────────────────────────────────────────────────────────
    //  Interaction
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Triggered by pressing [E] or clicking the interact button while in range.
    /// Prevents interaction if player is already sleeping.
    /// </summary>
    public override void interact_event()
    {
        // Don't allow sleep interaction if already sleeping
        if (_isPlayerSleeping)
        {
            Debug.Log("[Sleepable] Player is already sleeping. Ignoring interaction.");
            return;
        }

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
        // Lock player input while sleeping
        _isPlayerSleeping = true;
        PlayerInputLock.Lock();

        try
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
                SaveManager.SaveGame();
            }

            // Wait a tiny bit more while black
            yield return new WaitForSeconds(0.5f);

            // 3. Fade back out (Wake up)
            TransitionUI.Instance.fade_out(fadeTime);
        }
        finally
        {
            // Always unlock input when sleep is done, even if something fails
            _isPlayerSleeping = false;
            PlayerInputLock.Unlock();
        }
    }
}
