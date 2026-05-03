using UnityEngine;
using Systems.Calendar;
using System.Collections;
using UnityEngine.SceneManagement;
using LLMValley.SaveSystem;
using System.Linq;
using LLMValley.SceneManagement;

/// <summary>
/// Subclass of Interactable for Door objects. Handles Scene transition logic and prevents
/// player movement/interaction during scene transitions.
/// </summary>
[AddComponentMenu("Interaction/Door")]
public class Door : Interactable
{
    // ──────────────────────────────────────────────────────────
    //  Inspector
    // ──────────────────────────────────────────────────────────

    [Header("Scene Transition")]
    [Tooltip("The name of the scene to load when this door is used.")]
    [SerializeField] private string targetSceneName;

    [Tooltip("Name of the spawn point GameObject in the target scene. If empty, player keeps current position.")]
    [SerializeField] private string spawnPointName;

    // ──────────────────────────────────────────────────────────
    //  Interaction
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Triggered by pressing [E] or clicking the interact button while in range.
    /// Prevents interaction if player is already transitioning scenes.
    /// </summary>
    public override void interact_event()
    {
        // Don't allow door interaction if player is already transitioning scenes
        if (SceneTransitionManager.IsTransitioning)
        {
            Debug.Log("[Door] Already transitioning scenes. Ignoring interaction.");
            return;
        }

        // Check if target scene is specified
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogWarning("[Door] No target scene specified for door interaction.");
            return;
        }

        base.interact_event(); // fires the base UnityEvent (onInteract)

        // Use the SceneTransitionManager for proper scene transitions
        Debug.Log($"[Door] Starting transition to scene: '{targetSceneName}' with spawn point: '{spawnPointName}'");
        SceneTransitionManager.TransitionToScene(targetSceneName, spawnPointName);
    }

}
