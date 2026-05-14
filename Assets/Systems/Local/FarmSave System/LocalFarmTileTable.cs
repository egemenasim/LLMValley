using UnityEngine;
using LLMValley.Farm;

public class LocalFarmTileTable : MonoBehaviour
{
    [SerializeField] public GameObject[] plantableAreas;

    private GlobalFarmTileDataTable globalTable;

    private void Awake()
    {
        globalTable = FindObjectOfType<GlobalFarmTileDataTable>();
        if (globalTable == null)
        {
            Debug.LogError("[LocalFarmTileTable] GlobalFarmTileDataTable not found!");
            return;
        }

        // Subscribe to events
        FarmSaveEventBus.SaveFarmField += SaveFarmField;
        FarmSaveEventBus.LoadFarmField += LoadFarmField;
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        FarmSaveEventBus.SaveFarmField -= SaveFarmField;
        FarmSaveEventBus.LoadFarmField -= LoadFarmField;
    }

    private void SaveFarmField()
    {
        if (globalTable == null || plantableAreas == null) return;

        globalTable.farmTileDataArray = new FarmTileData[plantableAreas.Length];

        for (int i = 0; i < plantableAreas.Length; i++)
        {
            var farmableArea = plantableAreas[i]?.GetComponent<FarmableArea>();
            if (farmableArea == null) continue;

            var tileData = new FarmTileData
            {
                isTilled = farmableArea.IsTilled,
                isWatered = farmableArea.IsWatered,
                levelData = farmableArea.HasPlant ? farmableArea.CurrentPlant.CurrentLevel : 0,
                plantItemData = farmableArea.HasPlant ? farmableArea.CurrentPlant.PlantData : null
            };

            globalTable.farmTileDataArray[i] = tileData;
        }

        Debug.Log($"[LocalFarmTileTable] Saved {plantableAreas.Length} farm tiles.");
    }

    private void LoadFarmField()
    {
        if (globalTable == null || plantableAreas == null || globalTable.farmTileDataArray == null) return;

        for (int i = 0; i < Mathf.Min(plantableAreas.Length, globalTable.farmTileDataArray.Length); i++)
        {
            var farmableArea = plantableAreas[i]?.GetComponent<FarmableArea>();
            var tileData = globalTable.farmTileDataArray[i];

            if (farmableArea == null || tileData == null) continue;

            // Clear existing state
            farmableArea.Clear();

            // Apply saved state
            if (tileData.isTilled)
            {
                farmableArea.Till();
            }

            if (tileData.isWatered)
            {
                farmableArea.Water();
            }

            if (tileData.plantItemData != null)
            {
                farmableArea.Plant(tileData.plantItemData);
                if (farmableArea.CurrentPlant != null && tileData.levelData > 0)
                {
                    // Assuming Plantable has a way to set level, but for now, just plant
                    // You may need to add level setting logic here
                }
            }
        }

        Debug.Log($"[LocalFarmTileTable] Loaded {Mathf.Min(plantableAreas.Length, globalTable.farmTileDataArray.Length)} farm tiles.");
    }
}
