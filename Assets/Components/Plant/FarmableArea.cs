using UnityEngine;
using UnityEngine.Playables;

public class FarmableArea : MonoBehaviour
{
    [SerializeField] private Plantable currentPlant;

    [Header("Soil")]
    [SerializeField] private bool isTilled;

    public bool HasPlant => currentPlant != null;
    public Plantable CurrentPlant => currentPlant;

    public bool IsTilled => isTilled;

    public bool Till()
    {
        if (isTilled)
            return false;

        isTilled = true;
        Debug.Log($"[FarmableArea] Tilled area '{name}'.");
        return true;
    }

    public bool CanPlant(Plantable Plant)
    {
        return Plant != null && currentPlant == null && isTilled;
    }

    public bool CanPlant(LLMValley.Items.ItemData plantData)
    {
        return plantData != null && currentPlant == null && isTilled;
    }

    public bool Plant(Plantable Plant)
    {
        if (!CanPlant(Plant))
        {
            return false;
        }

        currentPlant = Instantiate(Plant, transform.position, Quaternion.identity, transform);
        // Prefab'ın scale'ını koru
        if (Plant != null)
        {
            currentPlant.transform.localScale = Plant.transform.localScale;
        }
        return true;
    }

    public bool Plant(LLMValley.Items.ItemData plantData)
    {
        if (!CanPlant(plantData))
        {
            Debug.LogWarning($"[FarmableArea] Plant(ItemData) blocked on area '{name}'. HasPlant: {HasPlant}, IsTilled: {isTilled}, PlantData null: {plantData == null}");
            return false;
        }

        var go = new GameObject("Plant");
        go.transform.SetParent(transform, worldPositionStays: false);
        go.transform.position = transform.position;

        var sr = go.AddComponent<SpriteRenderer>();

        // Try to render above the tile/ground sprite.
        var tileRenderer = GetComponent<SpriteRenderer>();
        if (tileRenderer != null)
        {
            sr.sortingLayerID = tileRenderer.sortingLayerID;
            sr.sortingOrder = tileRenderer.sortingOrder + 1;
        }

        currentPlant = go.AddComponent<BasicPlant>();
        currentPlant.Initialize(plantData, sr);

        Debug.Log($"[FarmableArea] Planted ItemData '{plantData.itemName}' on area '{name}'.");
        return true;
    }

    public bool Water()
    {
        if (currentPlant == null)
        {
            Debug.LogWarning($"[FarmableArea] Water() failed: no plant on area '{name}'.");
            return false;
        }

        currentPlant.Water();
        Debug.Log($"[FarmableArea] Watered plant '{currentPlant.name}' on area '{name}'.");
        return true;
    }

    public void Clear()
    {
        if (currentPlant != null)
        {
            Destroy(currentPlant.gameObject);
            currentPlant = null;
        }

        isTilled = false;
    }
}
