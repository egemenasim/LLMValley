using LLMValley.Items;
using Systems.Calendar;
using UnityEngine;

public abstract class Plantable : MonoBehaviour
{
    private bool _isHarvested = false;
    [Header("Plant Data")]
    [SerializeField] private ItemData plantData;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private bool _warnedMissingSprites;

    [Header("State")]
    [SerializeField] private int _currentLevel;
    [SerializeField] private int _minLevel;
    [SerializeField] private int _maxLevel;
    [SerializeField] private bool _isWateredToday;
    [SerializeField] private int _daysSinceLevel;
    [SerializeField] private int _baseDaysPerLevel;
    [SerializeField] private int _extraDaysForLastLevel;

    public int CurrentLevel => _currentLevel;
    public bool IsWateredToday => _isWateredToday;
    public ItemData PlantData => plantData;

    public bool IsFullyGrown => plantData != null && _currentLevel >= _maxLevel;

    public void Initialize(ItemData data, SpriteRenderer renderer = null)
    {
        plantData = data;

        if (spriteRenderer == null)
        {
            spriteRenderer = renderer != null ? renderer : GetComponent<SpriteRenderer>();
        }

        ApplyPlantData();
        ResetPlant();
    }

    protected virtual void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        ApplyPlantData();
        ResetPlant();
    }

    protected virtual void OnEnable()
    {
        if (CalendarSystem.Instance != null)
        {
            CalendarSystem.Instance.OnDayChanged.AddListener(HandleDayChanged);
        }
    }

    protected virtual void OnDisable()
    {
        if (CalendarSystem.Instance != null)
        {
            CalendarSystem.Instance.OnDayChanged.RemoveListener(HandleDayChanged);
        }
    }

    public void Water()
    {
        _isWateredToday = true;
    }

    public void ResetPlant()
    {
        _currentLevel = _minLevel;
        _daysSinceLevel = 0;
        CalculateGrowthDays();
        _isWateredToday = false;
        UpdateVisuals();
    }

    public void IncreaseLevel(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        _currentLevel = Mathf.Clamp(_currentLevel + amount, _minLevel, _maxLevel);
        UpdateVisuals();
    }

    private void HandleDayChanged(CalendarDate _)
    {
        if (_isWateredToday)
        {
            TryGrow();
        }

        _isWateredToday = false;
    }

    private void TryGrow()
    {
        if (_currentLevel >= _maxLevel)
        {
            return;
        }

        int requiredDays = _baseDaysPerLevel;
        if (IsLastGrowthLevel())
        {
            requiredDays += _extraDaysForLastLevel;
        }

        _daysSinceLevel++;
        if (requiredDays > 0 && _daysSinceLevel < requiredDays)
        {
            return;
        }

        _daysSinceLevel = 0;

        IncreaseLevel(1);
    }

    private void ApplyPlantData()
    {
        if (plantData == null)
        {
            return;
        }

        _minLevel = Mathf.Max(0, plantData.minGrowthLevel);
        _maxLevel = Mathf.Max(_minLevel, plantData.maxGrowthLevel);
        CalculateGrowthDays();
    }

    private void CalculateGrowthDays()
    {
        _baseDaysPerLevel = 0;
        _extraDaysForLastLevel = 0;

        if (plantData == null)
        {
            return;
        }

        int levelCount = Mathf.Max(1, _maxLevel - _minLevel);
        if (plantData.totalGrowthDays <= 0)
        {
            return;
        }

        _baseDaysPerLevel = plantData.totalGrowthDays / levelCount;
        _extraDaysForLastLevel = plantData.totalGrowthDays % levelCount;
    }

    private bool IsLastGrowthLevel()
    {
        return _currentLevel >= _maxLevel - 1;
    }

    protected virtual void UpdateVisuals()
    {
        if (spriteRenderer == null || plantData == null)
        {
            return;
        }

        if (plantData.growthSprites == null || plantData.growthSprites.Length == 0)
        {
            if (!_warnedMissingSprites)
            {
                _warnedMissingSprites = true;
                Debug.LogWarning($"[Plantable] No growthSprites configured on ItemData '{plantData.itemName}' (id: {plantData.itemID}). Plant will be invisible.");
            }
            return;
        }

        int index = Mathf.Clamp(_currentLevel, 0, plantData.growthSprites.Length - 1);
        spriteRenderer.sprite = plantData.growthSprites[index];
    }

    protected bool TryHarvest()
    {
        if (_isHarvested)
        {
            return false;
        }

        if (!IsFullyGrown)
        {
            return false;
        }

        if (plantData == null || plantData.outputCrop == null)
        {
            Debug.LogWarning($"[Plantable] Cannot harvest '{name}': outputCrop not set on ItemData '{plantData?.itemName}'.");
            return false;
        }

        _isHarvested = true;

        GameObject pickupGo = new GameObject($"Collectible_{plantData.outputCrop.itemName}");
        pickupGo.transform.position = transform.position;

        var sr = pickupGo.AddComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            sr.sortingLayerID = spriteRenderer.sortingLayerID;
            sr.sortingOrder = spriteRenderer.sortingOrder;
        }

        var pickup = pickupGo.AddComponent<CollectibleItem>();
        pickup.itemData = plantData.outputCrop;
        pickup.quantity = 1;

        var collider = pickupGo.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;

        // Prevent FarmableArea from holding a destroyed reference.
        var area = GetComponentInParent<FarmableArea>();
        if (area != null)
        {
            area.RemovePlant(this);
        }

        Destroy(gameObject);
        return true;
    }
}
