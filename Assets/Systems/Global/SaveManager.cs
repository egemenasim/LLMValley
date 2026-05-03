using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using LLMValley.Player;
using Systems.Calendar;

namespace LLMValley.SaveSystem
{
    /// <summary>
    /// Manages saving and loading of player data across scene transitions.
    /// Handles inventory, calendar state, player position, and other persistent data.
    /// </summary>
    public static class SaveManager
    {
        private const string SaveFileName = "player_save.json";
        private const string SaveFolderName = "GameSaves";

        /// <summary>
        /// Data structure for all savable player information
        /// </summary>
        [Serializable]
        public class PlayerSaveData
        {
            public string currentSceneName;
            public float[] playerPosition; // [x, y, z]
            public float[] playerRotation; // [x, y, z, w] quaternion
            public CalendarDate calendarDate;
            public Systems.Calendar.DayOfWeek dayOfWeek;
            public int currentHour;
            public ToolType selectedTool;
            public InventorySaveData inventory;
            public string lastSaveTime;
        }

        /// <summary>
        /// Data structure for inventory persistence
        /// </summary>
        [Serializable]
        public class InventorySaveData
        {
            public InventoryItemData[] items;

            [Serializable]
            public class InventoryItemData
            {
                public int itemId; // ItemData.itemID
                public int quantity;
            }
        }

        /// <summary>
        /// Saves the current player state to disk
        /// </summary>
        public static void SaveGame()
        {
            try
            {
                var saveData = CollectSaveData();
                string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);
                string path = GetSaveFilePath();

                // Ensure directory exists
                string directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(path, json);
                Debug.Log($"[SaveManager] Game saved successfully to: {path}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to save game: {e.Message}");
            }
        }

        /// <summary>
        /// Loads player state from disk and applies it
        /// </summary>
        /// <param name="skipPlayerPositioning">If true, skips setting player position/rotation (useful for scene transitions)</param>
        public static bool LoadGame(bool skipPlayerPositioning = false)
        {
            try
            {
                string path = GetSaveFilePath();
                if (!File.Exists(path))
                {
                    Debug.LogWarning("[SaveManager] No save file found. Starting new game.");
                    return false;
                }

                string json = File.ReadAllText(path);
                var saveData = JsonConvert.DeserializeObject<PlayerSaveData>(json);

                if (saveData == null)
                {
                    Debug.LogError("[SaveManager] Save data is corrupted or empty.");
                    return false;
                }

                ApplySaveData(saveData, skipPlayerPositioning);
                Debug.Log($"[SaveManager] Game loaded successfully from: {path}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to load game: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Checks if a save file exists
        /// </summary>
        public static bool SaveExists()
        {
            return File.Exists(GetSaveFilePath());
        }

        /// <summary>
        /// Collects all current player data for saving
        /// </summary>
        private static PlayerSaveData CollectSaveData()
        {
            var saveData = new PlayerSaveData
            {
                currentSceneName = SceneManager.GetActiveScene().name,
                lastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            // Get player transform
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                saveData.playerPosition = new float[] { player.transform.position.x, player.transform.position.y, player.transform.position.z };
                saveData.playerRotation = new float[] { player.transform.rotation.x, player.transform.rotation.y, player.transform.rotation.z, player.transform.rotation.w };
            }
            else
            {
                saveData.playerPosition = new float[] { 0f, 0f, 0f };
                saveData.playerRotation = new float[] { 0f, 0f, 0f, 1f };
            }

            // Get calendar data
            if (CalendarSystem.Instance != null)
            {
                saveData.calendarDate = CalendarSystem.Instance.GetCurrentDate();
                saveData.dayOfWeek = CalendarSystem.Instance.GetCurrentDayOfWeek();
                saveData.currentHour = CalendarSystem.Instance.GetCurrentHour();
            }

            // Get player tool selection
            var toolController = player?.GetComponent<PlayerToolController>();
            if (toolController != null)
            {
                saveData.selectedTool = toolController.GetSelectedTool();
            }

            // Get inventory data
            var inventory = player?.GetComponent<PlayerInventory>();
            if (inventory != null)
            {
                saveData.inventory = new InventorySaveData
                {
                    items = new InventorySaveData.InventoryItemData[inventory.Items.Count]
                };

                for (int i = 0; i < inventory.Items.Count; i++)
                {
                    var itemStack = inventory.Items[i];
                    saveData.inventory.items[i] = new InventorySaveData.InventoryItemData
                    {
                        itemId = itemStack.item.itemID,
                        quantity = itemStack.quantity
                    };
                }
            }

            return saveData;
        }

        /// <summary>
        /// Applies loaded save data to the current game state
        /// </summary>
        /// <param name="saveData">The save data to apply</param>
        /// <param name="skipPlayerPositioning">If true, skips setting player position/rotation</param>
        private static void ApplySaveData(PlayerSaveData saveData, bool skipPlayerPositioning = false)
        {
            // Find player once for all operations
            var player = GameObject.FindGameObjectWithTag("Player");

            // Apply calendar data
            if (CalendarSystem.Instance != null)
            {
                CalendarSystem.Instance.SetDate(saveData.calendarDate);
                CalendarSystem.Instance.SetDayOfWeek(saveData.dayOfWeek);
                CalendarSystem.Instance.SetHour(saveData.currentHour);
            }

            // Apply player position and rotation (skip if requested)
            if (!skipPlayerPositioning && player != null)
            {
                if (saveData.playerPosition != null && saveData.playerPosition.Length >= 3)
                {
                    player.transform.position = new Vector3(saveData.playerPosition[0], saveData.playerPosition[1], saveData.playerPosition[2]);
                }

                if (saveData.playerRotation != null && saveData.playerRotation.Length >= 4)
                {
                    player.transform.rotation = new Quaternion(saveData.playerRotation[0], saveData.playerRotation[1], saveData.playerRotation[2], saveData.playerRotation[3]);
                }
            }

            // Apply tool selection
            if (player != null)
            {
                var toolController = player.GetComponent<PlayerToolController>();
                if (toolController != null)
                {
                    toolController.SetSelectedTool(saveData.selectedTool);
                }
            }

            // Apply inventory data
            if (player != null)
            {
                var inventory = player.GetComponent<PlayerInventory>();
                if (inventory != null && saveData.inventory != null)
                {
                    // Clear current inventory
                    inventory.ClearInventory();

                    // Load items back
                    foreach (var itemData in saveData.inventory.items)
                    {
                        var item = GetItemById(itemData.itemId);
                        if (item != null)
                        {
                            inventory.AddItem(item, itemData.quantity);
                        }
                        else
                        {
                            Debug.LogWarning($"[SaveManager] Could not find item with ID: {itemData.itemId}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the full path to the save file
        /// </summary>
        private static string GetSaveFilePath()
        {
            return Path.Combine(Application.persistentDataPath, SaveFolderName, SaveFileName);
        }

        /// <summary>
        /// Finds an ItemData by its ID at runtime
        /// </summary>
        private static LLMValley.Items.ItemData GetItemById(int itemId)
        {
            // Find all ItemData assets in the project
            var allItems = Resources.FindObjectsOfTypeAll<LLMValley.Items.ItemData>();
            foreach (var item in allItems)
            {
                if (item.itemID == itemId)
                {
                    return item;
                }
            }
            return null;
        }
    }
}