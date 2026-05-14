using System.Collections.Generic;
using Systems.Calendar;
using LLMValley.Player;
using UnityEngine;

public sealed class PlantDateTest : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private List<FarmableArea> areas = new List<FarmableArea>();

    [Header("Options")]
    [SerializeField] private bool autoFindAreasInScene = true;
    [SerializeField] private bool autoWaterBeforeAdvancing = true;
    [SerializeField] private int advanceDaysPerClick = 1;

    private void Awake()
    {
        if (autoFindAreasInScene)
            RefreshAreasFromScene();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            AdvanceConfiguredDays();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            if (PlayerInventory.Instance != null)
            {
                PlayerInventory.Instance.ClearInventory();
                Debug.Log("[PlantDateTest] Inventory cleared.");
            }
            else
            {
                Debug.LogWarning("[PlantDateTest] PlayerInventory.Instance is null. Cannot clear inventory.");
            }

            // Clear all PlayerPrefs
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("[PlantDateTest] All PlayerPrefs cleared.");
        }
    }
    [ContextMenu("Refresh Areas From Scene")]
    public void RefreshAreasFromScene()
    {
        areas.Clear();
        areas.AddRange(FindObjectsByType<FarmableArea>(FindObjectsSortMode.None));
        Debug.Log($"[PlantDateTest] Found FarmableAreas: {areas.Count}");
    }

    public void AdvanceConfiguredDays()
    {
        AdvanceDays(advanceDaysPerClick);
    }

    public void AdvanceDays(int dayCount)
    {
        if (dayCount <= 0)
            return;

        if (CalendarSystem.Instance == null)
        {
            Debug.LogWarning("[PlantDateTest] CalendarSystem.Instance is null. Ensure CalendarSystem exists in the scene.");
            return;
        }

        for (int i = 0; i < dayCount; i++)
        {
            if (autoWaterBeforeAdvancing)
                WaterAllPlants();

            CalendarSystem.Instance.AdvanceDay();
            LogPlantStates();
        }
    }

    public void WaterAllPlants()
    {
        for (int i = 0; i < areas.Count; i++)
        {
            var area = areas[i];
            if (area == null)
                continue;

            area.Water();
        }
    }

    private void LogPlantStates()
    {
        for (int i = 0; i < areas.Count; i++)
        {
            var area = areas[i];
            if (area == null)
                continue;

            var plant = area.CurrentPlant;
            if (plant == null)
            {
                Debug.Log($"[PlantDateTest] Area '{area.name}': no plant.");
                continue;
            }

            string itemName = plant.PlantData != null ? plant.PlantData.itemName : "(no ItemData)";
            Debug.Log($"[PlantDateTest] Area '{area.name}': Plant '{itemName}', Level {plant.CurrentLevel}, WateredToday {plant.IsWateredToday}");
        }
    }
}
