using UnityEngine;

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
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
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
}
