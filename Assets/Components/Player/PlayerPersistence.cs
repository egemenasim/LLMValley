using UnityEngine;
using UnityEngine.SceneManagement;

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
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        /// <summary>
        /// Called when a new scene is loaded to ensure player is properly positioned
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"[PlayerPersistence] Scene loaded: {scene.name}, Player position: {transform.position}");
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