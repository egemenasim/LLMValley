using UnityEngine;

public class LocalFarmTileTable : MonoBehaviour
{
    [SerializeField] private GameObject[] plantableAreas;

    void Start()
    {
        // This component is just a marker for the FarmManager to find all tiles in the scene.
        // It doesn't need to do anything on its own.
    }
}
