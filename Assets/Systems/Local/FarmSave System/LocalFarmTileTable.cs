using UnityEngine;
using LLMValley.Farm;

public class LocalFarmTileTable : MonoBehaviour
{
    [SerializeField] public GameObject[] plantableAreas;

    private GlobalFarmTileDataTable globalTable;

    private void Awake()
    {
        // Subscribe to events early so this table is ready whenever the bus fires.
        FarmSaveEventBus.SaveFarmField += SaveFarmField;
        FarmSaveEventBus.SaveSpecificFarmTile += SaveSpecificFarmTile;
        FarmSaveEventBus.LoadFarmField += LoadFarmField;
    }

    private void Start()
    {
        globalTable = FindObjectOfType<GlobalFarmTileDataTable>();
        if (globalTable == null)
        {
            Debug.LogError("[LocalFarmTileTable] GlobalFarmTileDataTable not found!");
        }
        else
        {
            // On scene load, immediately sync scene tile objects from the global data table.
            FarmSaveEventBus.PublishLoadFarmField();
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        FarmSaveEventBus.SaveFarmField -= SaveFarmField;
        FarmSaveEventBus.SaveSpecificFarmTile -= SaveSpecificFarmTile;
        FarmSaveEventBus.LoadFarmField -= LoadFarmField;
    }

    private void SaveSpecificFarmTile(int dataId, FarmTileData tileData)
    {
        if (globalTable == null || tileData == null || dataId < 0)
            return;

        if (globalTable.farmTileDataArray == null || globalTable.farmTileDataArray.Length <= dataId)
        {
            int newSize = Mathf.Max(dataId + 1, globalTable.farmTileDataArray != null ? globalTable.farmTileDataArray.Length : 0);
            var newArray = new FarmTileData[newSize];
            if (globalTable.farmTileDataArray != null)
                globalTable.farmTileDataArray.CopyTo(newArray, 0);
            globalTable.farmTileDataArray = newArray;
        }

        globalTable.farmTileDataArray[dataId] = tileData;
        Debug.Log($"[LocalFarmTileTable] Saved tile ID {dataId} to global farm data table.");
    }

    private FarmTileData CreateTileData(FarmableArea farmableArea)
    {
        if (farmableArea == null)
            return null;

        return new FarmTileData
        {
            isTilled = farmableArea.IsTilled,
            isWatered = farmableArea.IsWatered,
            levelData = farmableArea.HasPlant ? farmableArea.CurrentPlant.CurrentLevel : 0,
            plantItemData = farmableArea.HasPlant ? farmableArea.CurrentPlant.PlantData : null
        };
    }

    private void SaveFarmField()
    {
        if (globalTable == null || plantableAreas == null) return;

        globalTable.farmTileDataArray = new FarmTileData[plantableAreas.Length];

        for (int i = 0; i < plantableAreas.Length; i++)
        {
            var farmableArea = plantableAreas[i]?.GetComponent<FarmableArea>();
            if (farmableArea == null) continue;

            globalTable.farmTileDataArray[i] = CreateTileData(farmableArea);
        }

        Debug.Log($"[LocalFarmTileTable] Saved {plantableAreas.Length} farm tiles.");
    }

    private void LoadFarmField()
    {
        if (globalTable == null || plantableAreas == null || globalTable.farmTileDataArray == null) return;

        for (int i = 0; i < plantableAreas.Length; i++)
        {
            var farmableArea = plantableAreas[i]?.GetComponent<FarmableArea>();
            if (farmableArea == null)
                continue;

            if (farmableArea.DataID < 0 || farmableArea.DataID >= globalTable.farmTileDataArray.Length)
                continue;

            var tileData = globalTable.farmTileDataArray[farmableArea.DataID];
            if (tileData == null)
                continue;

            farmableArea.RestoreFromData(tileData);
        }

        Debug.Log($"[LocalFarmTileTable] Loaded {Mathf.Min(plantableAreas.Length, globalTable.farmTileDataArray.Length)} farm tiles.");
    }
}
