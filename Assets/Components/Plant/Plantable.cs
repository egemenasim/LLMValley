using LLMValley.Items;
using Systems.Calendar;
using UnityEngine;

public abstract class Plantable : MonoBehaviour
{
    private bool _isHarvested = false;
    [Header("Plant Data")]
    [SerializeField] private ItemData plantData;
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    [Header("Harvesting")]
    [SerializeField] private CollectibleItem collectiblePrefab;

    [Header("State")]
    [SerializeField] private int _currentLevel;
    [SerializeField] private int _minLevel;
    [SerializeField] private int _maxLevel;
    [SerializeField] private bool _isWateredToday;
    [SerializeField] [UnityEngine.Serialization.FormerlySerializedAs("_daysSinceLevel")] private int _daysGrown;

    public int CurrentLevel => _currentLevel;
    public int DaysGrown => _daysGrown;
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

    protected virtual void Start()
    {
    }

    protected virtual void OnDestroy()
    {
    }

    public void Water()
    {
        _isWateredToday = true;
    }

    public void RestoreGrowthState(int daysGrown)
    {
        _daysGrown = Mathf.Max(0, daysGrown);
        UpdateLevelBasedOnDaysGrown();
    }

    public void ResetPlant()
    {
        _currentLevel = _minLevel;
        _daysGrown = 0;
        _isWateredToday = false;
        UpdateVisuals();
    }

    public void IncreaseLevel(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        if (plantData != null && _maxLevel > _minLevel)
        {
            int daysForIntermediate = plantData.totalGrowthDays;
            int intermediateLevels = _maxLevel - _minLevel;
            
            if (daysForIntermediate > 0 && intermediateLevels > 0)
            {
                float daysPerLevel = (float)daysForIntermediate / intermediateLevels;
                _daysGrown += Mathf.CeilToInt(daysPerLevel * amount);
            }
            else
            {
                _daysGrown += amount;
            }
            UpdateLevelBasedOnDaysGrown();
        }
        else
        {
            _currentLevel = Mathf.Clamp(_currentLevel + amount, _minLevel, _maxLevel);
            UpdateVisuals();
        }
    }

    private void UpdateLevelBasedOnDaysGrown()
    {
        if (plantData == null) return;

        int intermediateLevels = _maxLevel - _minLevel;
        if (intermediateLevels <= 0)
        {
            _currentLevel = _maxLevel;
            UpdateVisuals();
            return;
        }

        int targetGrowthDays = plantData.totalGrowthDays;
        int daysForIntermediate = targetGrowthDays;

        if (_daysGrown >= daysForIntermediate)
        {
            _currentLevel = _maxLevel;
        }
        else if (daysForIntermediate > 0)
        {
            float progress = (float)_daysGrown / daysForIntermediate;
            _currentLevel = _minLevel + Mathf.FloorToInt(progress * intermediateLevels);
        }
        else
        {
            _currentLevel = _maxLevel;
        }

        UpdateVisuals();
    }

    private void ApplyPlantData()
    {
        if (plantData == null)
        {
            return;
        }

        _minLevel = Mathf.Max(0, plantData.minGrowthLevel);
        _maxLevel = Mathf.Max(_minLevel, plantData.maxGrowthLevel);
    }

    protected virtual void UpdateVisuals()
    {
        if (spriteRenderer == null || plantData == null)
        {
            return;
        }

        if (plantData.growthSprites == null || plantData.growthSprites.Length == 0)
        {
            return;
        }

        // Final toplanabilir seviyeye ulaştıysa kesinlikle en son sprite'ı kullan.
        if (IsFullyGrown)
        {
            spriteRenderer.sprite = plantData.growthSprites[plantData.growthSprites.Length - 1];
            return;
        }

        // Ara seviyeler için son sprite hariç diğerlerini orantısal olarak paylaştır.
        int availableIntermediateSprites = plantData.growthSprites.Length - 1;
        if (availableIntermediateSprites <= 0)
        {
            spriteRenderer.sprite = plantData.growthSprites[0];
            return;
        }

        int intermediateLevels = _maxLevel - _minLevel;
        if (intermediateLevels <= 0)
        {
            spriteRenderer.sprite = plantData.growthSprites[0];
            return;
        }

        float levelProgress = (float)(_currentLevel - _minLevel) / intermediateLevels;
        int spriteIndex = Mathf.FloorToInt(levelProgress * availableIntermediateSprites);
        
        // Güvenlik amaçlı sınırlandırma (asla son sprite'a ulaşmasın)
        spriteIndex = Mathf.Clamp(spriteIndex, 0, availableIntermediateSprites - 1);
        
        spriteRenderer.sprite = plantData.growthSprites[spriteIndex];
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
            return false;
        }

        _isHarvested = true;

        if (collectiblePrefab != null)
        {

            var pickup = Instantiate(collectiblePrefab, transform.position, Quaternion.identity);
            pickup.name = $"Collectible_{plantData.outputCrop.itemName}";
            pickup.itemData = plantData.outputCrop;
            pickup.quantity = 1;
            
            var sr = pickup.GetComponent<SpriteRenderer>();
            if (sr != null && plantData.outputCrop.icon != null)
            {
                sr.sprite = plantData.outputCrop.icon;
            }
            if (sr != null && spriteRenderer != null)
            {
                sr.sortingLayerID = spriteRenderer.sortingLayerID;
                sr.sortingOrder = spriteRenderer.sortingOrder;
            }
        }
        else
        {

            GameObject pickupGo = new GameObject($"Collectible_{plantData.outputCrop.itemName}");
            pickupGo.transform.position = transform.position;

            var sr = pickupGo.AddComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                sr.sortingLayerID = spriteRenderer.sortingLayerID;
                sr.sortingOrder = spriteRenderer.sortingOrder;
            }
            
            if (plantData.outputCrop.icon != null)
            {
                sr.sprite = plantData.outputCrop.icon;
            }

            var pickup = pickupGo.AddComponent<CollectibleItem>();
            pickup.itemData = plantData.outputCrop;
            pickup.quantity = 1;

            var collider = pickupGo.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
        }

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
