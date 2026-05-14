using System;
using UnityEngine;

namespace LLMValley.Farm
{
    /// <summary>
    /// A global event bus for farm save flow.
    /// Lives on a persistent GameObject and survives scene loads.
    /// </summary>
    public class FarmSaveEventBus : MonoBehaviour
    {
        public static FarmSaveEventBus Instance { get; private set; }

        public static event Action SaveFarmField;
        public static event Action<int, FarmTileData> SaveSpecificFarmTile;
        public static event Action FarmDataSaveRequested;
        public static event Action LoadFarmField;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[FarmSaveEventBus] Duplicate detected. Destroying extra component.");
                Destroy(this);
                return;
            }

            Instance = this;

            if (transform.parent == null)
                DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public static void EnsureInstanceExists()
        {
            if (Instance != null)
                return;

            var existing = FindObjectOfType<FarmSaveEventBus>();
            if (existing != null)
                return;

            var go = new GameObject(nameof(FarmSaveEventBus));
            go.AddComponent<FarmSaveEventBus>();
        }

        public static void PublishSaveFarmField()
        {
            SaveFarmField?.Invoke();
        }

        public static void PublishSaveSpecificFarmTile(int dataId, FarmTileData tileData)
        {
            SaveSpecificFarmTile?.Invoke(dataId, tileData);
        }

        public static void PublishFarmDataSaveRequested()
        {
            FarmDataSaveRequested?.Invoke();
        }

        public static void PublishLoadFarmField()
        {
            LoadFarmField?.Invoke();
        }
    }
}
