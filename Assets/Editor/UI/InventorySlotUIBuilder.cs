using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using LLMValley.UI;

namespace LLMValley.Editor.UI
{
    /// <summary>
    /// Editor utility that builds the InventorySlotUI prefab from scratch.
    /// Run via:  Tools → LLMValley → Create InventorySlotUI Prefab
    /// </summary>
    public static class InventorySlotUIBuilder
    {
        private const string PrefabOutputPath = "Assets/Prefabs/UI/InventorySlotUI.prefab";

        [MenuItem("Tools/LLMValley/Create InventorySlotUI Prefab")]
        public static void CreatePrefab()
        {
            // ── Ensure output directory exists ────────────────────────────────────
            const string dir = "Assets/Prefabs/UI";
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            if (!AssetDatabase.IsValidFolder(dir))
                AssetDatabase.CreateFolder("Assets/Prefabs", "UI");

            // ── Build hierarchy in a temporary scene object ───────────────────────
            GameObject root = BuildHierarchy();

            // ── Save as prefab ────────────────────────────────────────────────────
            bool saved;
            GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(root, PrefabOutputPath, out saved);
            Object.DestroyImmediate(root);

            if (saved)
            {
                AssetDatabase.Refresh();
                EditorGUIUtility.PingObject(prefabAsset);
                Debug.Log($"[InventorySlotUIBuilder] Prefab saved to {PrefabOutputPath}");
            }
            else
            {
                Debug.LogError("[InventorySlotUIBuilder] Failed to save prefab.");
            }
        }

        // ─── Hierarchy construction ───────────────────────────────────────────────

        private static GameObject BuildHierarchy()
        {
            // ── Root (64×64) ──────────────────────────────────────────────────────
            GameObject root = new GameObject("InventorySlotUI");
            RectTransform rootRect = root.AddComponent<RectTransform>();
            SetSize(rootRect, 64f, 64f);

            // Background image – dark grey #2A2A2A
            Image bgImage = root.AddComponent<Image>();
            bgImage.color = HexColor("#2A2A2A");
            bgImage.raycastTarget = true;

            // Button component (click handling)
            Button btn = root.AddComponent<Button>();
            btn.targetGraphic = bgImage;

            // Darken tint on highlight/press
            ColorBlock cols = btn.colors;
            cols.highlightedColor = HexColor("#3E3E3E");
            cols.pressedColor     = HexColor("#1E1E1E");
            cols.selectedColor    = HexColor("#2A2A2A");
            btn.colors = cols;

            // InventorySlotUI script
            InventorySlotUI slotScript = root.AddComponent<InventorySlotUI>();

            // ── itemIconImage (56×56, centered) ───────────────────────────────────
            GameObject iconGO = new GameObject("ItemIcon");
            iconGO.transform.SetParent(root.transform, false);
            RectTransform iconRect = iconGO.AddComponent<RectTransform>();
            SetAnchors(iconRect, 0.5f, 0.5f, 0.5f, 0.5f);   // anchored to centre
            iconRect.anchoredPosition = Vector2.zero;
            SetSize(iconRect, 56f, 56f);
            Image iconImage = iconGO.AddComponent<Image>();
            iconImage.color         = Color.white;
            iconImage.preserveAspect = true;
            iconImage.raycastTarget  = false;
            iconImage.enabled        = false;   // hidden until SetItem is called

            // ── quantityText (bottom-right, size 12, white) ───────────────────────
            GameObject qtyGO = new GameObject("QuantityText");
            qtyGO.transform.SetParent(root.transform, false);
            RectTransform qtyRect = qtyGO.AddComponent<RectTransform>();
            // Anchor to bottom-right corner
            SetAnchors(qtyRect, 1f, 0f, 1f, 0f);
            qtyRect.anchoredPosition = new Vector2(-2f, 2f);   // 2 px inset
            SetSize(qtyRect, 28f, 16f);
            TextMeshProUGUI qtyTMP = qtyGO.AddComponent<TextMeshProUGUI>();
            qtyTMP.text               = string.Empty;
            qtyTMP.fontSize           = 12f;
            qtyTMP.color              = Color.white;
            qtyTMP.alignment          = TextAlignmentOptions.BottomRight;
            qtyTMP.raycastTarget      = false;
            qtyTMP.enabled            = false;   // hidden until a stack > 1 is set

            // ── selectionHighlight (full-size, yellow border, hidden) ─────────────
            GameObject hlGO = new GameObject("SelectionHighlight");
            hlGO.transform.SetParent(root.transform, false);
            RectTransform hlRect = hlGO.AddComponent<RectTransform>();
            // Stretch to fill the root
            SetAnchors(hlRect, 0f, 0f, 1f, 1f);
            hlRect.offsetMin = Vector2.zero;
            hlRect.offsetMax = Vector2.zero;
            Image hlImage = hlGO.AddComponent<Image>();

            // Use a Sliced image type for a clean border effect
            hlImage.color        = HexColor("#FFD700");   // gold / yellow
            hlImage.type         = Image.Type.Sliced;
            hlImage.fillCenter   = false;                  // hollow – border only
            hlImage.raycastTarget = false;
            hlImage.enabled      = false;   // initially hidden

            // ── Wire script references via SerializedObject ───────────────────────
            SerializedObject so = new SerializedObject(slotScript);
            so.FindProperty("itemIconImage").objectReferenceValue      = iconImage;
            so.FindProperty("quantityText").objectReferenceValue       = qtyTMP;
            so.FindProperty("selectionHighlight").objectReferenceValue = hlImage;
            so.ApplyModifiedPropertiesWithoutUndo();

            return root;
        }

        // ─── Helpers ──────────────────────────────────────────────────────────────

        private static void SetSize(RectTransform rt, float width, float height)
        {
            rt.sizeDelta = new Vector2(width, height);
        }

        private static void SetAnchors(RectTransform rt,
            float minX, float minY, float maxX, float maxY)
        {
            rt.anchorMin = new Vector2(minX, minY);
            rt.anchorMax = new Vector2(maxX, maxY);
            rt.pivot     = new Vector2(maxX, minY);   // pivot follows anchor corner
        }

        private static Color HexColor(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color c);
            return c;
        }
    }
}
