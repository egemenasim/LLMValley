using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using LLMValley.Player;
using LLMValley.Items;
using Systems.Calendar;

namespace LLMValley.SaveSystem
{
    /// <summary>
    /// Manages saving and loading of player data across scene transitions.
    /// Handles inventory, calendar state, player position, and other persistent data.
    /// Uses PlayerPrefs for storage.
    /// </summary>
    public static class SaveManager
    {
        private const string CalendarDateKey = "CalendarDate";
        private const string DayOfWeekKey = "DayOfWeek";
        private const string CurrentHourKey = "CurrentHour";
        private const string PlayerCoinsKey = "PlayerCoins";
        private const string InventoryKey = "Inventory";
        private const string SelectedHotbarIndexKey = "SelectedHotbarIndex";
        private const string PlayerPositionKey = "PlayerPosition";
        private const string PlayerRotationKey = "PlayerRotation";
        private const string CurrentSceneKey = "CurrentScene";
        private const string LastSaveTimeKey = "LastSaveTime";

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
            public int playerCoins;
            public int selectedHotbarIndex;
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
        /// Saves the current player state to PlayerPrefs
        /// </summary>
        public static void SaveGame()
        {
            try
            {
                var saveData = CollectSaveData();
                
                // Save calendar data
                PlayerPrefs.SetString(CalendarDateKey, JsonConvert.SerializeObject(saveData.calendarDate));
                PlayerPrefs.SetInt(DayOfWeekKey, (int)saveData.dayOfWeek);
                PlayerPrefs.SetInt(CurrentHourKey, saveData.currentHour);
                
                // Save player coins
                PlayerPrefs.SetInt(PlayerCoinsKey, saveData.playerCoins);
                
                // Save inventory
                PlayerPrefs.SetString(InventoryKey, JsonConvert.SerializeObject(saveData.inventory));
                
                // Save selected hotbar slot index
                PlayerPrefs.SetInt(SelectedHotbarIndexKey, saveData.selectedHotbarIndex);
                
                // Save player position
                if (saveData.playerPosition != null && saveData.playerPosition.Length >= 3)
                {
                    PlayerPrefs.SetFloat(PlayerPositionKey + "X", saveData.playerPosition[0]);
                    PlayerPrefs.SetFloat(PlayerPositionKey + "Y", saveData.playerPosition[1]);
                    PlayerPrefs.SetFloat(PlayerPositionKey + "Z", saveData.playerPosition[2]);
                }
                
                // Save player rotation
                if (saveData.playerRotation != null && saveData.playerRotation.Length >= 4)
                {
                    PlayerPrefs.SetFloat(PlayerRotationKey + "X", saveData.playerRotation[0]);
                    PlayerPrefs.SetFloat(PlayerRotationKey + "Y", saveData.playerRotation[1]);
                    PlayerPrefs.SetFloat(PlayerRotationKey + "Z", saveData.playerRotation[2]);
                    PlayerPrefs.SetFloat(PlayerRotationKey + "W", saveData.playerRotation[3]);
                }
                
                // Save scene and time
                PlayerPrefs.SetString(CurrentSceneKey, saveData.currentSceneName);
                PlayerPrefs.SetString(LastSaveTimeKey, saveData.lastSaveTime);
                
                PlayerPrefs.Save();
                Debug.Log("[SaveManager] Game saved successfully to PlayerPrefs");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to save game: {e.Message}");
            }
        }

        /// <summary>
        /// Loads player state from PlayerPrefs and applies it
        /// </summary>
        /// <param name="skipPlayerPositioning">If true, skips setting player position/rotation (useful for scene transitions)</param>
        public static bool LoadGame(bool skipPlayerPositioning = false)
        {
            try
            {
                Debug.Log("[SaveManager] Loading game from PlayerPrefs");
                
                if (!SaveExists())
                {
                    Debug.LogWarning("[SaveManager] No save data found in PlayerPrefs. Starting new game.");
                    return false;
                }

                var saveData = new PlayerSaveData();
                
                // Load calendar data
                if (PlayerPrefs.HasKey(CalendarDateKey))
                {
                    saveData.calendarDate = JsonConvert.DeserializeObject<CalendarDate>(PlayerPrefs.GetString(CalendarDateKey));
                }
                if (PlayerPrefs.HasKey(DayOfWeekKey))
                {
                    saveData.dayOfWeek = (Systems.Calendar.DayOfWeek)PlayerPrefs.GetInt(DayOfWeekKey);
                }
                if (PlayerPrefs.HasKey(CurrentHourKey))
                {
                    saveData.currentHour = PlayerPrefs.GetInt(CurrentHourKey);
                }
                
                // Load player coins
                if (PlayerPrefs.HasKey(PlayerCoinsKey))
                {
                    saveData.playerCoins = PlayerPrefs.GetInt(PlayerCoinsKey);
                }
                
                // Load inventory
                if (PlayerPrefs.HasKey(InventoryKey))
                {
                    saveData.inventory = JsonConvert.DeserializeObject<InventorySaveData>(PlayerPrefs.GetString(InventoryKey));
                }
                
                // Load selected hotbar slot index
                if (PlayerPrefs.HasKey(SelectedHotbarIndexKey))
                {
                    saveData.selectedHotbarIndex = PlayerPrefs.GetInt(SelectedHotbarIndexKey);
                }
                
                // Load player position
                if (PlayerPrefs.HasKey(PlayerPositionKey + "X"))
                {
                    saveData.playerPosition = new float[]
                    {
                        PlayerPrefs.GetFloat(PlayerPositionKey + "X"),
                        PlayerPrefs.GetFloat(PlayerPositionKey + "Y"),
                        PlayerPrefs.GetFloat(PlayerPositionKey + "Z")
                    };
                }
                
                // Load player rotation
                if (PlayerPrefs.HasKey(PlayerRotationKey + "X"))
                {
                    saveData.playerRotation = new float[]
                    {
                        PlayerPrefs.GetFloat(PlayerRotationKey + "X"),
                        PlayerPrefs.GetFloat(PlayerRotationKey + "Y"),
                        PlayerPrefs.GetFloat(PlayerRotationKey + "Z"),
                        PlayerPrefs.GetFloat(PlayerRotationKey + "W")
                    };
                }
                
                // Load scene and time
                if (PlayerPrefs.HasKey(CurrentSceneKey))
                {
                    saveData.currentSceneName = PlayerPrefs.GetString(CurrentSceneKey);
                }
                if (PlayerPrefs.HasKey(LastSaveTimeKey))
                {
                    saveData.lastSaveTime = PlayerPrefs.GetString(LastSaveTimeKey);
                }

                ApplySaveData(saveData, skipPlayerPositioning);
                Debug.Log("[SaveManager] Game loaded successfully from PlayerPrefs");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to load game: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Checks if save data exists in PlayerPrefs
        /// </summary>
        public static bool SaveExists()
        {
            return PlayerPrefs.HasKey(CalendarDateKey) || PlayerPrefs.HasKey(PlayerCoinsKey) || PlayerPrefs.HasKey(InventoryKey);
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

            // Get player coin amount
            var wallet = player?.GetComponent<PlayerWallet>();
            if (wallet != null)
            {
                saveData.playerCoins = wallet.CurrentGold;
            }

            // Get inventory data
            var inventory = player?.GetComponent<PlayerInventory>();
            if (inventory != null)
            {
                saveData.selectedHotbarIndex = inventory.InventoryUI != null ? inventory.InventoryUI.SelectedIndex : -1;

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
            Debug.Log("[SaveManager] Applying save data...");
            
            // Find player once for all operations
            var player = GameObject.FindGameObjectWithTag("Player");
            Debug.Log($"[SaveManager] Player found: {player != null}");

            // Apply calendar data
            if (CalendarSystem.Instance != null)
            {
                Debug.Log("[SaveManager] Applying calendar data...");
                CalendarSystem.Instance.SetDate(saveData.calendarDate);
                CalendarSystem.Instance.SetDayOfWeek(saveData.dayOfWeek);
                CalendarSystem.Instance.SetHour(saveData.currentHour);
                Debug.Log("[SaveManager] Calendar data applied");
            }
            else
            {
                Debug.LogWarning("[SaveManager] CalendarSystem.Instance is null");
            }

            // Apply player coin amount
            if (player != null)
            {
                var wallet = player.GetComponent<PlayerWallet>();
                if (wallet != null)
                {
                    wallet.SetGold(saveData.playerCoins);
                    Debug.Log($"[SaveManager] Player coins set to: {saveData.playerCoins}");
                }
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

            // Apply inventory data
            if (player != null)
            {
                var inventory = player.GetComponent<PlayerInventory>();
                if (inventory != null && saveData.inventory != null)
                {
                    Debug.Log($"[SaveManager] Loading inventory with {saveData.inventory.items.Length} items...");
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

                    if (inventory.InventoryUI != null)
                    {
                        inventory.InventoryUI.SelectSavedSlot(saveData.selectedHotbarIndex);
                    }
                    Debug.Log("[SaveManager] Inventory loaded");
                }
                else
                {
                    Debug.LogWarning("[SaveManager] Player inventory not found or no inventory data");
                }
            }
            else
            {
                Debug.LogWarning("[SaveManager] Player not found for inventory loading");
            }
            
            Debug.Log("[SaveManager] Save data application complete");
        }

        /// <summary>
        /// Finds an ItemData by its ID at runtime
        /// </summary>
        private static LLMValley.Items.ItemData GetItemById(int itemId)
        {
            try
            {
                // Find all ItemData assets in the project
                var allItems = Resources.FindObjectsOfTypeAll<LLMValley.Items.ItemData>();
                
                // Fallback to Resources.LoadAll if no items are currently in memory
                if (allItems == null || allItems.Length == 0)
                {
                    allItems = Resources.LoadAll<LLMValley.Items.ItemData>("");
                    Debug.Log($"[SaveManager] Found {allItems.Length} ItemData assets via Resources.LoadAll");
                }
                else
                {
                    Debug.Log($"[SaveManager] Found {allItems.Length} ItemData assets in memory");
                }
                
                foreach (var item in allItems)
                {
                    if (item.itemID == itemId)
                    {
                        return item;
                    }
                }
                
                Debug.LogWarning($"[SaveManager] Item with ID {itemId} not found among {allItems.Length} items");
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Error finding item {itemId}: {e.Message}");
                return null;
            }
        }
    }
}