using UnityEngine;
using LLMValley.Farm;

public class GlobalFarmTileDataTable : MonoBehaviour
{
    public static GlobalFarmTileDataTable Instance { get; private set; }

    [SerializeField] public FarmTileData[] farmTileDataArray;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[GlobalFarmTileDataTable] Duplicate detected. Destroying extra instance.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        FarmSaveEventBus.playerSlept += HandlePlayerSlept;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            FarmSaveEventBus.playerSlept -= HandlePlayerSlept;
            Instance = null;
        }
    }

    private void HandlePlayerSlept()
    {
        globalGrowWateredPlants();
        globalWaterReset();
        FarmSaveEventBus.PublishLoadFarmField();
    }

    public void globalWaterReset()
    {
        if (farmTileDataArray == null)
            return;

        for (int i = 0; i < farmTileDataArray.Length; i++)
        {
            if (farmTileDataArray[i] != null)
                farmTileDataArray[i].isWatered = false;
        }
    }

    public void globalGrowWateredPlants()
    {
        if (farmTileDataArray == null)
            return;

        for (int i = 0; i < farmTileDataArray.Length; i++)
        {
            var tile = farmTileDataArray[i];
            if (tile == null)
            {
                Debug.Log($"[Grow] tile[{i}] is null, skipping");
                continue;
            }

            string plantName = tile.plantItemData != null ? tile.plantItemData.itemName : "none";
            Debug.Log($"[Grow] BEFORE tile[{i}]: plant={plantName}, isWatered={tile.isWatered}, daysGrown={tile.daysGrown}, levelData={tile.levelData}");

            if (!tile.isWatered || tile.plantItemData == null)
                continue;

            int minLevel = Mathf.Max(0, tile.plantItemData.minGrowthLevel);
            int maxLevel = Mathf.Max(minLevel, tile.plantItemData.maxGrowthLevel);

            if (tile.levelData >= maxLevel)
                continue;

            tile.daysGrown++;

            int intermediateLevels = maxLevel - minLevel;
            int totalGrowthDays = tile.plantItemData.totalGrowthDays;

            if (intermediateLevels <= 0 || totalGrowthDays <= 0)
            {
                tile.levelData = maxLevel;
            }
            else if (tile.daysGrown >= totalGrowthDays)
            {
                tile.levelData = maxLevel;
            }
            else
            {
                float progress = (float)tile.daysGrown / totalGrowthDays;
                tile.levelData = minLevel + Mathf.FloorToInt(progress * intermediateLevels);
            }

            Debug.Log($"[Grow] AFTER  tile[{i}]: daysGrown={tile.daysGrown}, levelData={tile.levelData} (max={maxLevel}, totalGrowthDays={totalGrowthDays})");
        }
    }
}
