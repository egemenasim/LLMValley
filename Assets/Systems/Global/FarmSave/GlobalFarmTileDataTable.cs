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
}
