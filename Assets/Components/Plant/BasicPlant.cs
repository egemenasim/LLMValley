using UnityEngine;

/// <summary>
/// Concrete Plantable implementation so plant prefabs can be created in the editor.
/// Plantable is abstract, so it cannot be added directly to a GameObject.
/// </summary>
[DisallowMultipleComponent]
public sealed class BasicPlant : Plantable
{
    [SerializeField] private float harvestRadius = 1.25f;
    [SerializeField] private Vector3 harvestPromptOffset = new(0.1f, 0.15f, 0f);
    [Header("UI Prefab")]
    [SerializeField] private GameObject harvestPromptPrefab;

    private Transform _player;
    private GameObject _harvestPromptInstance;

    private void Awake()
    {
        CreateHarvestPromptIfNeeded();
        SetHarvestPromptVisible(false);
    }

    private void Update()
    {
        bool inRange = false;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (_player == null)
            {
                var playerGo = GameObject.FindGameObjectWithTag("Player");
                if (playerGo != null)
                    _player = playerGo.transform;
            }

            if (_player == null)
                return;

            inRange = Vector2.Distance(transform.position, _player.position) <= harvestRadius;
            if (inRange)
                TryHarvest();
        }

        if (_player == null)
        {
            var playerGo = GameObject.FindGameObjectWithTag("Player");
            if (playerGo != null)
                _player = playerGo.transform;
        }

        if (_player != null)
            inRange = Vector2.Distance(transform.position, _player.position) <= harvestRadius;

        SetHarvestPromptVisible(IsFullyGrown && inRange);
    }

    private void CreateHarvestPromptIfNeeded()
    {
        if (_harvestPromptInstance != null)
            return;

        if (harvestPromptPrefab != null)
        {
            // Prefab'ı direkt bitkinin içine (child olarak) oluşturuyoruz
            _harvestPromptInstance = Instantiate(harvestPromptPrefab, transform);
            _harvestPromptInstance.transform.localPosition = harvestPromptOffset;
        }
    }

    private void SetHarvestPromptVisible(bool visible)
    {
        if (_harvestPromptInstance != null)
        {
            _harvestPromptInstance.SetActive(visible);
        }
    }

    private void OnDestroy()
    {
        if (_harvestPromptInstance != null)
            Destroy(_harvestPromptInstance);
    }
}
