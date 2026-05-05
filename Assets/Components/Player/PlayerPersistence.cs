using UnityEngine;
using UnityEngine.SceneManagement;
using LLMValley.SaveSystem;

namespace LLMValley.Player
{
    /// <summary>
    /// Ensures the player GameObject persists across scene transitions.
    /// Should be attached to the Player GameObject in the initial scene.
    /// </summary>
    public class PlayerPersistence : MonoBehaviour
    {
        private static PlayerPersistence _instance;

        private void Awake()
        {
            // Singleton pattern to ensure only one player exists
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning("[PlayerPersistence] Multiple players detected, destroying duplicate");
                Destroy(gameObject);
                return;
            }

            _instance = this;

            // Make player persistent across scenes
            DontDestroyOnLoad(gameObject);

            Debug.Log("[PlayerPersistence] Player will persist across scene transitions");
            Debug.Log($"[PlayerPersistence] Player GameObject: {gameObject.name}, Tag: {gameObject.tag}");
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
                Debug.Log("[PlayerPersistence] PlayerPersistence instance destroyed");
            }
        }

        /// <summary>
        /// Called when a new scene is loaded to ensure player is properly positioned
        /// and save data is loaded
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"[PlayerPersistence] Scene loaded: {scene.name}, Player position: {transform.position}, Player active: {gameObject.activeInHierarchy}");
            
            // Load save data on every scene load
            if (SaveManager.SaveExists())
            {
                Debug.Log("[PlayerPersistence] Loading save data...");
                SaveManager.LoadGame(skipPlayerPositioning: true); // Skip positioning since scene transition manager handles it
                Debug.Log("[PlayerPersistence] Save data loaded successfully");
            }
            else
            {
                Debug.Log("[PlayerPersistence] No save data found, starting fresh");
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}