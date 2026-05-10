using UnityEngine;
using UnityEngine.Playables;

public class FarmableArea : MonoBehaviour
{
    [SerializeField] private Plantable currentPlant;
    [SerializeField] private Plantable basePlantPrefab;

    [Header("Soil")]
    [SerializeField] private bool isTilled;

    [Header("Interaction")]
    [SerializeField] private float interactRadius = 2.0f;
    private Transform _player;

    public bool HasPlant => currentPlant != null;
    public Plantable CurrentPlant => currentPlant;

    public bool IsTilled => isTilled;

    public bool IsPlayerInRange()
    {
        if (_player == null)
        {
            var playerGo = GameObject.FindGameObjectWithTag("Player");
            if (playerGo != null)
                _player = playerGo.transform;
        }

        if (_player == null)
            return false; // Player not found, fail safe

        return Vector2.Distance(transform.position, _player.position) <= interactRadius;
    }

    public bool Till()
    {
        if (!IsPlayerInRange())
        {
            Debug.Log($"[FarmableArea] Player is too far to till '{name}'.");
            return false;
        }

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
        if (!IsPlayerInRange())
        {
            Debug.Log($"[FarmableArea] Player is too far to plant on '{name}'.");
            return false;
        }

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
        if (!IsPlayerInRange())
        {
            Debug.Log($"[FarmableArea] Player is too far to plant on '{name}'.");
            return false;
        }

        if (!CanPlant(plantData))
        {
            Debug.LogWarning($"[FarmableArea] Plant(ItemData) blocked on area '{name}'. HasPlant: {HasPlant}, IsTilled: {isTilled}, PlantData null: {plantData == null}");
            return false;
        }

        if (basePlantPrefab == null)
        {
            Debug.LogError($"[FarmableArea] No basePlantPrefab assigned on area '{name}'. Cannot plant.");
            return false;
        }

        currentPlant = Instantiate(basePlantPrefab, transform.position, Quaternion.identity, transform);
        currentPlant.transform.localScale = basePlantPrefab.transform.localScale;

        var sr = currentPlant.GetComponent<SpriteRenderer>();

        // Try to render above the tile/ground sprite.
        var tileRenderer = GetComponent<SpriteRenderer>();
        if (tileRenderer != null && sr != null)
        {
            sr.sortingLayerID = tileRenderer.sortingLayerID;
            sr.sortingOrder = tileRenderer.sortingOrder + 1;
        }

        currentPlant.Initialize(plantData, sr);

        Debug.Log($"[FarmableArea] Planted ItemData '{plantData.itemName}' on area '{name}'.");
        return true;
    }

    public bool Water()
    {
        if (!IsPlayerInRange())
        {
            Debug.Log($"[FarmableArea] Player is too far to water '{name}'.");
            return false;
        }

        if (currentPlant == null)
        {
            Debug.LogWarning($"[FarmableArea] Water() failed: no plant on area '{name}'.");
            return false;
        }

        currentPlant.Water();
        Debug.Log($"[FarmableArea] Watered plant '{currentPlant.name}' on area '{name}'.");
        return true;
    }

    public bool RemovePlant(Plantable plant)
    {
        if (plant == null)
        {
            return false;
        }

        if (currentPlant != plant)
        {
            return false;
        }

        currentPlant = null;
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
