using UnityEngine;
using UnityEngine.Playables;
using Systems.Calendar;

public class FarmableArea : MonoBehaviour
{
    [SerializeField] private Plantable currentPlant;
    [SerializeField] private Plantable basePlantPrefab;

    [Header("Visuals")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite tilledSprite;
    [SerializeField] private Sprite wateredSprite;

    private SpriteRenderer _spriteRenderer;

    [Header("Soil")]
    [SerializeField] private bool isTilled;
    [SerializeField] private bool isWatered;

    [Header("Interaction")]
    [SerializeField] private float interactRadius = 2.0f;
    private Transform _player;

    public int DataID; // Unique ID for saving/loading

    public bool HasPlant => currentPlant != null;
    public Plantable CurrentPlant => currentPlant;

    public bool IsTilled => isTilled;
    public bool IsWatered => isWatered;

    private void Awake()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (_spriteRenderer == null) return;

        if (isWatered && isTilled && wateredSprite != null)
        {
            _spriteRenderer.sprite = wateredSprite;
        }
        else if (isTilled && tilledSprite != null)
        {
            _spriteRenderer.sprite = tilledSprite;
        }
        else if (!isTilled && normalSprite != null)
        {
            _spriteRenderer.sprite = normalSprite;
        }
    }

    private void Start()
    {
        if (CalendarSystem.Instance != null)
        {
            CalendarSystem.Instance.OnDayChanged.AddListener(HandleDayChanged);
        }
    }

    private void OnDestroy()
    {
        if (CalendarSystem.Instance != null)
        {
            CalendarSystem.Instance.OnDayChanged.RemoveListener(HandleDayChanged);
        }
    }

    private void HandleDayChanged(CalendarDate _)
    {
        if (isWatered)
        {
            isWatered = false;
            UpdateVisuals();
        }
    }

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

            return false;
        }

        if (isTilled)
            return false;

        isTilled = true;
        UpdateVisuals();

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

            return false;
        }

        if (!CanPlant(Plant))
        {
            return false;
        }

        Vector3 spawnPosition = transform.position + new Vector3(0f, 0.8f, 0f);
        currentPlant = Instantiate(Plant, spawnPosition, Quaternion.identity, transform);
        // Prefab'ın scale'ını koru
        if (Plant != null)
        {
            currentPlant.transform.localScale = Plant.transform.localScale;
        }

        if (isWatered)
        {
            currentPlant.Water();
        }

        return true;
    }

    public bool Plant(LLMValley.Items.ItemData plantData)
    {
        if (!IsPlayerInRange())
        {

            return false;
        }

        if (!CanPlant(plantData))
        {

            return false;
        }

        if (basePlantPrefab == null)
        {
            Debug.LogError($"[FarmableArea] No basePlantPrefab assigned on area '{name}'. Cannot plant.");
            return false;
        }

        Vector3 spawnPosition = transform.position + new Vector3(0f, 0.8f, 0f);
        currentPlant = Instantiate(basePlantPrefab, spawnPosition, Quaternion.identity, transform);
        currentPlant.transform.localScale = basePlantPrefab.transform.localScale;

        var sr = currentPlant.GetComponentInChildren<SpriteRenderer>();

        // Try to render above the tile/ground sprite.
        if (_spriteRenderer != null && sr != null)
        {
            sr.sortingLayerID = _spriteRenderer.sortingLayerID;
            sr.sortingOrder = _spriteRenderer.sortingOrder + 1;
        }

        currentPlant.Initialize(plantData, sr);

        if (isWatered)
        {
            currentPlant.Water();
        }


        return true;
    }

    public bool Water()
    {
        if (!IsPlayerInRange())
        {

            return false;
        }

        if (!isTilled)
        {

            return false;
        }

        if (isWatered) return false;

        isWatered = true;
        UpdateVisuals();

        if (currentPlant != null)
        {
            currentPlant.Water();
        }
        

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
        isWatered = false;
        UpdateVisuals();
    }
}
