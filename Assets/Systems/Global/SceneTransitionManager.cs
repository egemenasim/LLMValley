using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using LLMValley.SaveSystem;
using System.Linq;
using LLMValley.NPCChat;

namespace LLMValley.SceneManagement
{
    /// <summary>
    /// Handles scene transitions with proper save/load functionality.
    /// Persists across scenes to ensure transitions complete properly.
    /// </summary>
    public class SceneTransitionManager : MonoBehaviour
    {
        private static SceneTransitionManager _instance;

        private string _targetSceneName;
        private string _spawnPointName;
        private bool _isTransitioning;

        private void Awake()
        {
            // Singleton pattern
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Initiates a scene transition with save/load
        /// </summary>
        public static void TransitionToScene(string sceneName, string spawnPointName = null)
        {
            Debug.Log($"[SceneTransitionManager] TransitionToScene called with scene: '{sceneName}', spawnPoint: '{spawnPointName}'");

            if (_instance == null)
            {
                // Create instance if it doesn't exist
                GameObject go = new GameObject("SceneTransitionManager");
                _instance = go.AddComponent<SceneTransitionManager>();
                Debug.Log("[SceneTransitionManager] Created new SceneTransitionManager instance");
            }

            _instance.StartTransition(sceneName, spawnPointName);
        }

        private void StartTransition(string sceneName, string spawnPointName)
        {
            if (_isTransitioning)
            {
                Debug.LogWarning("[SceneTransitionManager] Already transitioning, ignoring request");
                return;
            }

            _targetSceneName = sceneName;
            _spawnPointName = spawnPointName;
            _isTransitioning = true;

            StartCoroutine(TransitionCoroutine());
        }

        private IEnumerator TransitionCoroutine()
        {
            Debug.Log($"[SceneTransitionManager] Starting transition to: {_targetSceneName}");
            Debug.Log($"[SceneTransitionManager] Spawn point: {_spawnPointName}");

            // Check if player exists before transition
            var playerBefore = GameObject.FindGameObjectWithTag("Player");
            Debug.Log($"[SceneTransitionManager] Player exists before transition: {playerBefore != null}");
            if (playerBefore != null)
            {
                Debug.Log($"[SceneTransitionManager] Player position before: {playerBefore.transform.position}");
            }

            var npcChatUI = NPCChatUIManager.FindExisting();
            if (npcChatUI != null && npcChatUI.IsOpen)
            {
                npcChatUI.CloseConversation();
            }

            PlayerInputLock.ForceReset();
            DialogInputManager.ClearAll();

            // Lock input
            PlayerInputLock.Lock();

            try
            {
                // 1. Save current game state
                Debug.Log("[SceneTransitionManager] Saving current game state...");
                SaveManager.SaveGame();

                // 2. Fade to black (if TransitionUI exists)
                if (TransitionUI.Instance != null)
                {
                    float fadeTime = 1.0f;
                    TransitionUI.Instance.fade_in(fadeTime);
                    yield return new WaitForSeconds(fadeTime + 0.5f);
                }

                // 3. Load the target scene
                Debug.Log($"[SceneTransitionManager] Loading scene: {_targetSceneName}");
                var asyncLoad = SceneManager.LoadSceneAsync(_targetSceneName);

                if (asyncLoad == null)
                {
                    Debug.LogError($"[SceneTransitionManager] Failed to load scene '{_targetSceneName}'. Scene does not exist.");
                    // List all available scenes
                    var sceneCount = SceneManager.sceneCountInBuildSettings;
                    Debug.Log($"[SceneTransitionManager] Available scenes in build settings: {sceneCount}");
                    for (int i = 0; i < sceneCount; i++)
                    {
                        var scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                        var sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                        Debug.Log($"[SceneTransitionManager] Scene {i}: {sceneName}");
                    }
                    yield break;
                }

                while (!asyncLoad.isDone)
                {
                    yield return null;
                }

                Debug.Log($"[SceneTransitionManager] Scene loaded: {_targetSceneName}");

                // Check if player exists after scene load
                var playerAfter = GameObject.FindGameObjectWithTag("Player");
                Debug.Log($"[SceneTransitionManager] Player exists after scene load: {playerAfter != null}");
                if (playerAfter != null)
                {
                    Debug.Log($"[SceneTransitionManager] Player position after load: {playerAfter.transform.position}");
                }

                // Debug: List all GameObjects in the scene
                var allObjects = FindObjectsOfType<GameObject>();
                Debug.Log($"[SceneTransitionManager] Total GameObjects in scene: {allObjects.Length}");
                var playerObjects = allObjects.Where(go => go.CompareTag("Player") || go.name.Contains("Player")).ToArray();
                Debug.Log($"[SceneTransitionManager] Player-tagged objects: {playerObjects.Length}");
                foreach (var obj in playerObjects)
                {
                    Debug.Log($"[SceneTransitionManager] Player object: {obj.name}, Position: {obj.transform.position}, Active: {obj.activeInHierarchy}");
                }

                // 4. Wait for scene initialization
                yield return null;

                // 5. Load saved game data
                Debug.Log("[SceneTransitionManager] Loading saved game data...");
                try
                {
                    SaveManager.LoadGame(skipPlayerPositioning: true);
                    Debug.Log("[SceneTransitionManager] Save data loaded successfully");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[SceneTransitionManager] Failed to load save data: {e.Message}");
                }

                // 6. Wait another frame
                yield return null;

                // 7. Position player at spawn point
                if (!string.IsNullOrEmpty(_spawnPointName))
                {
                    Debug.Log($"[SceneTransitionManager] Positioning player at spawn point: {_spawnPointName}");
                    var spawnPoint = GameObject.Find(_spawnPointName);
                    
                    // Try multiple ways to find the player
                    GameObject player = GameObject.FindGameObjectWithTag("Player");
                    if (player == null)
                    {
                        // Try finding by name
                        player = GameObject.Find("Player");
                    }
                    if (player == null)
                    {
                        // Try finding any object with PlayerPersistence component
                        var persistenceComponents = FindObjectsOfType<LLMValley.Player.PlayerPersistence>();
                        if (persistenceComponents.Length > 0)
                        {
                            player = persistenceComponents[0].gameObject;
                            Debug.Log($"[SceneTransitionManager] Found player via PlayerPersistence component: {player.name}");
                        }
                    }

                    if (spawnPoint != null && player != null)
                    {
                        Debug.Log($"[SceneTransitionManager] Moving player to {spawnPoint.transform.position} (forcing Z=0)");
                        player.transform.position = new Vector3(spawnPoint.transform.position.x, spawnPoint.transform.position.y, 0f);
                        player.transform.rotation = spawnPoint.transform.rotation;
                        Debug.Log($"[SceneTransitionManager] Player successfully positioned at spawn point");
                    }
                    else
                    {
                        Debug.LogWarning($"[SceneTransitionManager] Spawn point '{_spawnPointName}' or player not found");
                        if (spawnPoint == null) Debug.LogWarning("[SceneTransitionManager] Spawn point is null");
                        if (player == null) 
                        {
                            Debug.LogWarning("[SceneTransitionManager] Player is null");
                            // Player objects are already listed above, no need to duplicate
                        }
                        else
                        {
                            // If spawn point not found but player exists, position at origin as fallback
                            Debug.LogWarning($"[SceneTransitionManager] Positioning player at origin as fallback");
                            player.transform.position = Vector3.zero;
                            player.transform.rotation = Quaternion.identity;
                        }
                    }
                }
                else
                {
                    Debug.Log("[SceneTransitionManager] No spawn point specified");
                }

                // 8. Fade back in
                if (TransitionUI.Instance != null)
                {
                    yield return new WaitForSeconds(0.5f);
                    TransitionUI.Instance.fade_out(1.0f);
                }

                Debug.Log("[SceneTransitionManager] Transition complete");

            }
            finally
            {
                // Always unlock input
                PlayerInputLock.Unlock();
                PlayerInputLock.ForceReset();
                _isTransitioning = false;
            }
        }

        public static bool IsTransitioning => _instance != null && _instance._isTransitioning;
    }
}
