#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using LLMValley.Interaction;
using LLMValley.UI;
using UnityEngine.UI;

namespace LLMValley.Editor
{
    public class ChestSetupTool
    {
        [MenuItem("LLMValley/Setup Chest Scene")]
        public static void SetupChest()
        {
            // 1. Create ChestUI in the scene
            GameObject chestUIGO = new GameObject("ChestUI");
            Canvas canvas = chestUIGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            chestUIGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            chestUIGO.AddComponent<GraphicRaycaster>();

            GameObject panel = new GameObject("Panel");
            panel.transform.SetParent(chestUIGO.transform, false);
            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(500, 300);
            
            Image bg = panel.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

            GameObject grid = new GameObject("Grid");
            grid.transform.SetParent(panel.transform, false);
            RectTransform gridRect = grid.AddComponent<RectTransform>();
            gridRect.anchorMin = new Vector2(0, 0);
            gridRect.anchorMax = new Vector2(1, 1);
            gridRect.offsetMin = new Vector2(10, 10);
            gridRect.offsetMax = new Vector2(-10, -10);

            GridLayoutGroup glg = grid.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(64, 64);
            glg.spacing = new Vector2(5, 5);

            ChestUI chestUI = chestUIGO.AddComponent<ChestUI>();

            // Load InventorySlotUI prefab
            GameObject slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/UI/Systems/Inventory/InventorySlotUI.prefab");
            
            if (slotPrefab == null)
            {
                Debug.LogError("Could not find InventorySlotUI prefab at expected path.");
                return;
            }

            // Create 30 slots
            InventorySlotUI[] slots = new InventorySlotUI[30];
            for (int i = 0; i < 30; i++)
            {
                GameObject slot = PrefabUtility.InstantiatePrefab(slotPrefab, grid.transform) as GameObject;
                slots[i] = slot.GetComponent<InventorySlotUI>();
            }

            // Reflection to set private 'slots' field
            var field = typeof(ChestUI).GetField("slots", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(chestUI, slots);
            }
            
            // Reflection to set private 'panelRoot' field
            var panelField = typeof(ChestUI).GetField("panelRoot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (panelField != null)
            {
                panelField.SetValue(chestUI, panel);
            }

            panel.SetActive(false); // Default hidden

            // 2. Create Chest GameObject
            GameObject chestGO = new GameObject("Chest");
            SpriteRenderer sr = chestGO.AddComponent<SpriteRenderer>();
            
            // Load sprite
            Sprite chestSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Objects/outsideObjects/chest.png");
            if (chestSprite != null)
            {
                sr.sprite = chestSprite;
            }
            else
            {
                // The texture is chest.png, but it's a multiple sprite sheet, so the type might be Texture2D and we need to load sub-sprites
                Object[] sprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Objects/outsideObjects/chest.png");
                foreach (var obj in sprites)
                {
                    if (obj is Sprite s)
                    {
                        sr.sprite = s;
                        break;
                    }
                }
            }

            BoxCollider2D col = chestGO.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1.5f, 1.5f);

            Chest chest = chestGO.AddComponent<Chest>();
            
            GameObject interactBtnPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/UI/Buttons/BUTTON_Interact.prefab");
            if (interactBtnPrefab != null)
            {
                var btnField = typeof(Interactable).GetField("interactButtonPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (btnField != null)
                {
                    btnField.SetValue(chest, interactBtnPrefab);
                }
            }

            chestGO.transform.position = new Vector3(0, 0, 0); // Put it at origin
            
            Selection.activeGameObject = chestGO;
            EditorGUIUtility.PingObject(chestGO);

            Debug.Log("Chest setup complete! You can move the 'Chest' object next to the farmhouse.");
        }
    }
}
#endif
