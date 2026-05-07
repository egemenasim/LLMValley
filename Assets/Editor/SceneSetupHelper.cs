using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using LLMValley.Player;
using LLMValley.Items;
using LLMValley.Components.Animation;

namespace LLMValley.Editor
{
    public static class SceneSetupHelper
    {
        [MenuItem("Tools/LLMValley/Setup Fishing Minigame Scene")]
        public static void SetupFishingScene()
        {
            // Ensure we are in SadoranTest or prompt user to save and switch
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.name != "SadoranTest")
            {
                if (EditorUtility.DisplayDialog("Change Scene", "This script is designed for the SadoranTest scene. Switch to it now?", "Yes", "No"))
                {
                    EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                    EditorSceneManager.OpenScene("Assets/Scenes/Test/Developer/Yusuf Sadi Pesen/SadoranTest.unity");
                }
                else
                {
                    return;
                }
            }

            bool dirty = false;

            // 1. Add Player if missing
            PlayerAnimationManager player = Object.FindFirstObjectByType<PlayerAnimationManager>();
            if (player == null)
            {
                GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Objects/Player/Player.prefab");
                if (playerPrefab != null)
                {
                    GameObject playerInstance = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
                    playerInstance.transform.position = Vector3.zero; // Place at origin
                    player = playerInstance.GetComponentInChildren<PlayerAnimationManager>();
                    Debug.Log("[SetupFishingScene] Instantiated Player.prefab into the scene.");
                    dirty = true;
                }
                else
                {
                    Debug.LogError("[SetupFishingScene] Could not find Player.prefab at Assets/Prefab/Objects/Player/Player.prefab");
                }
            }

            // 2. Assign Fish Item to RodToolAction
            if (player != null)
            {
                RodToolAction rodAction = player.GetComponentInChildren<RodToolAction>();
                if (rodAction != null)
                {
                    SerializedObject so = new SerializedObject(rodAction);
                    SerializedProperty fishProp = so.FindProperty("fishReward");
                    if (fishProp != null && fishProp.objectReferenceValue == null)
                    {
                        ItemData fishData = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Scriptables/Items/Fish/goldfish.asset");
                        if (fishData != null)
                        {
                            fishProp.objectReferenceValue = fishData;
                            so.ApplyModifiedProperties();
                            Debug.Log("[SetupFishingScene] Assigned goldfish to RodToolAction.");
                            dirty = true;
                        }
                        else
                        {
                            Debug.LogWarning("[SetupFishingScene] Could not find goldfish item. Make sure you generated items!");
                        }
                    }
                }
            }

            // 3. Add Water Grid if missing
            GameObject waterGridObj = GameObject.Find("WaterGrid");
            if (waterGridObj == null)
            {
                waterGridObj = new GameObject("WaterGrid");
                Grid grid = waterGridObj.AddComponent<Grid>();
                
                GameObject tilemapObj = new GameObject("WaterTilemap");
                tilemapObj.transform.SetParent(waterGridObj.transform, false);
                tilemapObj.layer = LayerMask.NameToLayer("Water");
                
                Tilemap tilemap = tilemapObj.AddComponent<Tilemap>();
                TilemapRenderer renderer = tilemapObj.AddComponent<TilemapRenderer>();
                
                // Add TilemapCollider2D so Physics2D.OverlapPoint works
                tilemapObj.AddComponent<TilemapCollider2D>();
                
                // Also need CompositeCollider2D and Rigidbody2D to merge the colliders
                Rigidbody2D rb = tilemapObj.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Static;
                CompositeCollider2D compCol = tilemapObj.AddComponent<CompositeCollider2D>();
                compCol.isTrigger = true;
                
                TilemapCollider2D tmCol = tilemapObj.GetComponent<TilemapCollider2D>();
                tmCol.usedByComposite = true;

                Debug.Log("[SetupFishingScene] Created WaterGrid with WaterTilemap (Layer 'Water').");
                dirty = true;
            }

            // 4. Add InventoryUI if missing
            if (Object.FindFirstObjectByType<LLMValley.UI.InventoryUI>() == null)
            {
                Canvas mainCanvas = Object.FindFirstObjectByType<Canvas>();
                if (mainCanvas == null)
                {
                    GameObject canvasObj = new GameObject("MainCanvas");
                    mainCanvas = canvasObj.AddComponent<Canvas>();
                    mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                    canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                }
                
                if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
                {
                    GameObject eventSystemObj = new GameObject("EventSystem");
                    eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                    eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                }

                GameObject inventoryPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/UI/Systems/Inventory/InventoryUI.prefab");
                if (inventoryPrefab != null)
                {
                    GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(inventoryPrefab);
                    inst.transform.SetParent(mainCanvas.transform, false);
                    Debug.Log("[SetupFishingScene] Instantiated InventoryUI.prefab into the scene (Canvas).");
                    dirty = true;
                }
                else
                {
                    Debug.LogWarning("[SetupFishingScene] Could not find InventoryUI.prefab.");
                }
            }

            // 5. Spawn a Collectible Rod near the player
            GameObject collectiblePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Objects/Collectible/ItemCollectible.prefab");
            ItemData rodData = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Scriptables/Items/Tools/rod.asset");
            
            if (collectiblePrefab != null && rodData != null && player != null)
            {
                // Check if one already exists to avoid spamming
                bool hasRodSpawned = false;
                foreach (CollectibleItem ci in Object.FindObjectsByType<CollectibleItem>(FindObjectsSortMode.None))
                {
                    if (ci.itemData == rodData) hasRodSpawned = true;
                }
                
                if (!hasRodSpawned)
                {
                    GameObject collectibleInstance = (GameObject)PrefabUtility.InstantiatePrefab(collectiblePrefab);
                    collectibleInstance.transform.position = player.transform.position + new Vector3(2f, 0f, 0f); // Spawn to the right
                    
                    CollectibleItem comp = collectibleInstance.GetComponent<CollectibleItem>();
                    if (comp != null)
                    {
                        comp.itemData = rodData;
                        // Apply the sprite for editor preview
                        SpriteRenderer sr = collectibleInstance.GetComponent<SpriteRenderer>();
                        if (sr != null && rodData.icon != null) sr.sprite = rodData.icon;
                        
                        Debug.Log("[SetupFishingScene] Spawned a Collectible Rod near the player.");
                        dirty = true;
                    }
                }
            }
            else
            {
                 Debug.LogWarning("[SetupFishingScene] Could not find CollectibleItem prefab or rod asset to spawn.");
            }

            if (dirty)
            {
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                Debug.Log("[SetupFishingScene] Scene modified successfully! Don't forget to save.");
            }
            else
            {
                Debug.Log("[SetupFishingScene] Scene is already set up.");
            }
            
            EditorUtility.DisplayDialog("Fishing Setup", "Setup complete!\n\n1. You can now use the Tile Palette to paint water tiles onto the 'WaterTilemap'.\n2. Give the player a Fishing Rod in the inventory.\n3. Press Play and test fishing!", "OK");
        }
    }
}
