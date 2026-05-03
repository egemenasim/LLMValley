using UnityEngine;
using Systems.Calendar;
using System.Collections;
using UnityEngine.SceneManagement;
using LLMValley.SaveSystem;
using System.Linq;

/// <summary>
/// Subclass of Interactable for Door objects. Handles Scene transition logic and prevents
/// player movement/interaction during scene transitions.
/// </summary>
[AddComponentMenu("Interaction/Door")]
public class Door : Interactable
{
    private static bool _isPlayerTransiting = false;

    /// <summary>True when the player is currently transitioning scenes.</summary>
    public static bool IsPlayerTransiting => _isPlayerTransiting;

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
        if (_isPlayerTransiting)
        {
            Debug.Log("[Door] Player is already transitioning scenes. Ignoring interaction.");
            return;
        }

        // Check if target scene is specified
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogWarning("[Door] No target scene specified for door interaction.");
            return;
        }

        base.interact_event(); // fires the base UnityEvent (onInteract)

        if (TransitionUI.Instance != null)
        {
            StartCoroutine(DoorSequenceRoutine());
        }
        else
        {
            Debug.LogWarning("[Door] TransitionUI.Instance not found!.");
        }
    }

    private IEnumerator DoorSequenceRoutine()
    {
        // Lock player input while transitioning
        _isPlayerTransiting = true;
        PlayerInputLock.Lock();

        try
        {
            // 1. Save current game state
            Debug.Log("[Door] Saving current game state...");
            SaveManager.SaveGame();

            // 2. Fade to black (Transition)
            float fadeTime = 1.0f;
            TransitionUI.Instance.fade_in(fadeTime);
            
            // Wait for fade to complete + a small pause for the "night"
            yield return new WaitForSeconds(fadeTime + 0.5f);

            Debug.Log($"[Door] Loading scene: {targetSceneName}");

            // Check if player exists before scene load
            var playerBeforeLoad = GameObject.FindGameObjectWithTag("Player");
            Debug.Log($"[Door] Player exists before scene load: {playerBeforeLoad != null}");
            if (playerBeforeLoad != null)
            {
                Debug.Log($"[Door] Player position before load: {playerBeforeLoad.transform.position}");
            }

            // 3. Load the target scene
            var asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);
            if (asyncLoad == null)
            {
                Debug.LogError($"[Door] Failed to load scene '{targetSceneName}'. Scene does not exist.");
                yield break;
            }
            
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            // Check if player exists after scene load
            var playerAfterLoad = GameObject.FindGameObjectWithTag("Player");
            Debug.Log($"[Door] Player exists after scene load: {playerAfterLoad != null}");
            if (playerAfterLoad != null)
            {
                Debug.Log($"[Door] Player position after load: {playerAfterLoad.transform.position}");
            }

            // Wait a frame for scene objects to initialize
            yield return null;

            // 4. Load saved game data into the new scene
            Debug.Log("[Door] Loading saved game data...");
            SaveManager.LoadGame(skipPlayerPositioning: true);

            // Wait another frame for all components to initialize
            yield return null;

            // 5. Position player at spawn point if specified
            if (!string.IsNullOrEmpty(spawnPointName))
            {
                var spawnPoint = GameObject.Find(spawnPointName);
                if (spawnPoint != null)
                {
                    var player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null)
                    {
                        Debug.Log($"[Door] Moving player to spawn point '{spawnPointName}' at position: {spawnPoint.transform.position}");
                        player.transform.position = spawnPoint.transform.position;
                        player.transform.rotation = spawnPoint.transform.rotation;
                        
                        // Wait a frame and verify position
                        StartCoroutine(VerifyPlayerPosition(spawnPoint.transform.position));
                    }
                    else
                    {
                        Debug.LogError($"[Door] Player not found in scene '{targetSceneName}' after loading!");
                    }
                }
                else
                {
                    Debug.LogWarning($"[Door] Spawn point '{spawnPointName}' not found in scene '{targetSceneName}'");
                    Debug.Log($"[Door] Available GameObjects in scene: {string.Join(", ", FindObjectsOfType<GameObject>().Select(go => go.name).ToArray())}");
                }
            }

            // Wait a tiny bit more while black
            yield return new WaitForSeconds(0.5f);

            // 6. Fade back out (Wake up)
            TransitionUI.Instance.fade_out(fadeTime);
        }
        finally
        {
            // Always unlock input when transition is done, even if something fails
            _isPlayerTransiting = false;
            PlayerInputLock.Unlock();
        }
    }

    private IEnumerator VerifyPlayerPosition(Vector3 expectedPosition)
    {
        yield return null; // Wait one more frame
        
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float distance = Vector3.Distance(player.transform.position, expectedPosition);
            if (distance > 0.1f)
            {
                Debug.LogWarning($"[Door] Player position was reset! Expected: {expectedPosition}, Actual: {player.transform.position}, Distance: {distance}");
                // Force position again
                player.transform.position = expectedPosition;
            }
            else
            {
                Debug.Log($"[Door] Player successfully positioned at spawn point: {expectedPosition}");
            }
        }
    }
}
