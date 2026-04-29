using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using LLMValley.UI;

namespace LLMValley.Editor.UI
{
    /// <summary>
    /// Editor utility that builds the InventoryUI hotbar prefab from scratch.
    ///
    /// Run via:  Tools → LLMValley → Create InventoryUI Prefab
    ///
    /// Prerequisites:
    ///   - InventorySlotUI.prefab must exist at Assets/Prefabs/UI/InventorySlotUI.prefab
    ///     (create it first via Tools → LLMValley → Create InventorySlotUI Prefab)
    /// </summary>
    public static class InventoryUIBuilder
    {
        private const string SlotPrefabPath    = "Assets/Prefab/UI/Systems/Inventory/InventorySlotUI.prefab";
        private const string PrefabOutputPath  = "Assets/Prefab/UI/Systems/Inventory/InventoryUI.prefab";
        private const int    SlotCount         = InventoryUI.SlotCount;   // 10

        // ─── Colours ──────────────────────────────────────────────────────────────
        private static readonly Color PanelBg      = HexColor("#1A1A1AE6");   // near-black, 90 % opaque
        private static readonly Color TooltipBg    = HexColor("#111111CC");   // darker, 80 % opaque
        private static readonly Color TooltipBorder= HexColor("#FFD700");     // gold accent
        private static readonly Color NameColor     = Color.white;
        private static readonly Color DescColor     = HexColor("#BBBBBB");

        // ─── Menu entry ───────────────────────────────────────────────────────────

        [MenuItem("Tools/LLMValley/Create InventoryUI Prefab")]
        public static void CreatePrefab()
        {
            // ── Validate slot prefab ──────────────────────────────────────────────
            GameObject slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SlotPrefabPath);
            if (slotPrefab == null)
            {
                Debug.LogError(
                    $"[InventoryUIBuilder] InventorySlotUI prefab not found at '{SlotPrefabPath}'.\n" +
                    "Run Tools → LLMValley → Create InventorySlotUI Prefab first.");
                return;
            }

            // ── Ensure output directory ───────────────────────────────────────────
            EnsureFolder("Assets", "Prefabs");
            EnsureFolder("Assets/Prefabs", "UI");

            // ── Build hierarchy ───────────────────────────────────────────────────
            GameObject root = BuildHierarchy(slotPrefab);

            // ── Save ──────────────────────────────────────────────────────────────
            GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(root, PrefabOutputPath, out bool saved);
            Object.DestroyImmediate(root);

            if (saved)
            {
                AssetDatabase.Refresh();
                EditorGUIUtility.PingObject(prefabAsset);
                Debug.Log($"[InventoryUIBuilder] Prefab saved to {PrefabOutputPath}");
            }
            else
            {
                Debug.LogError("[InventoryUIBuilder] Failed to save prefab.");
            }
        }

        // ─── Hierarchy ────────────────────────────────────────────────────────────

        private static GameObject BuildHierarchy(GameObject slotPrefab)
        {
            // ── Root canvas-space panel ───────────────────────────────────────────
            // Anchored to bottom-centre; sits 8 px above the screen bottom edge.
            GameObject root = new GameObject("InventoryUI");
            RectTransform rootRect = root.AddComponent<RectTransform>();
            SetAnchors(rootRect, 0.5f, 0f, 0.5f, 0f);
            rootRect.pivot            = new Vector2(0.5f, 0f);
            rootRect.anchoredPosition = new Vector2(0f, 8f);
            // Width = 10 slots × 64 px + 9 gaps × 4 px + 2 × 8 px padding = 712 px
            rootRect.sizeDelta        = new Vector2(712f, 80f);

            // Background image
            Image rootBg = root.AddComponent<Image>();
            rootBg.color = PanelBg;

            // CanvasGroup — required by InventoryUI.Show()/Hide() so Update() is never killed.
            root.AddComponent<CanvasGroup>();

            // InventoryUI script
            InventoryUI inventoryUI = root.AddComponent<InventoryUI>();

            // ── Slots container (HorizontalLayoutGroup) ───────────────────────────
            GameObject slotsGO = new GameObject("Slots");
            slotsGO.transform.SetParent(root.transform, false);
            RectTransform slotsRect = slotsGO.AddComponent<RectTransform>();
            // Stretch fill, with 8 px padding on all sides
            SetAnchors(slotsRect, 0f, 0f, 1f, 1f);
            slotsRect.offsetMin = new Vector2(8f,  8f);
            slotsRect.offsetMax = new Vector2(-8f, -8f);

            HorizontalLayoutGroup hlg = slotsGO.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing                  = 4f;
            hlg.childAlignment           = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth    = false;
            hlg.childForceExpandHeight   = false;
            hlg.childControlWidth        = false;
            hlg.childControlHeight       = false;

            // ── Instantiate 10 slot children ──────────────────────────────────────
            InventorySlotUI[] slotComponents = new InventorySlotUI[SlotCount];
            for (int i = 0; i < SlotCount; i++)
            {
                GameObject slotGO = (GameObject)PrefabUtility.InstantiatePrefab(slotPrefab, slotsGO.transform);
                slotGO.name = $"Slot_{i:D2}";
                InventorySlotUI slotUI = slotGO.GetComponent<InventorySlotUI>();
                slotUI.SetSlotIndex(i);
                slotComponents[i] = slotUI;
            }

            // ── Tooltip panel (above the hotbar) ──────────────────────────────────
            GameObject tooltip = BuildTooltipPanel(root.transform);

            // ── Wire InventoryUI serialized references ────────────────────────────
            SerializedObject so = new SerializedObject(inventoryUI);

            SerializedProperty slotsProp = so.FindProperty("slots");
            slotsProp.arraySize = SlotCount;
            for (int i = 0; i < SlotCount; i++)
                slotsProp.GetArrayElementAtIndex(i).objectReferenceValue = slotComponents[i];

            so.FindProperty("tooltipPanel").objectReferenceValue =
                tooltip;

            so.FindProperty("tooltipName").objectReferenceValue =
                tooltip.transform.Find("TooltipName")?.GetComponent<TextMeshProUGUI>();

            so.FindProperty("tooltipDesc").objectReferenceValue =
                tooltip.transform.Find("TooltipDesc")?.GetComponent<TextMeshProUGUI>();

            so.ApplyModifiedPropertiesWithoutUndo();

            return root;
        }

        // ─── Tooltip panel ────────────────────────────────────────────────────────

        private static GameObject BuildTooltipPanel(Transform parent)
        {
            // Container — anchored just above the hotbar root
            GameObject panel = new GameObject("TooltipPanel");
            panel.transform.SetParent(parent, false);
            RectTransform panelRect = panel.AddComponent<RectTransform>();
            SetAnchors(panelRect, 0.5f, 1f, 0.5f, 1f);
            panelRect.pivot            = new Vector2(0.5f, 0f);
            panelRect.anchoredPosition = new Vector2(0f, 6f);
            panelRect.sizeDelta        = new Vector2(320f, 60f);

            // Background
            Image panelBg = panel.AddComponent<Image>();
            panelBg.color = TooltipBg;

            // Gold outline Image (full-size child, border-only)
            GameObject border = new GameObject("Border");
            border.transform.SetParent(panel.transform, false);
            RectTransform borderRect = border.AddComponent<RectTransform>();
            SetAnchors(borderRect, 0f, 0f, 1f, 1f);
            borderRect.offsetMin = Vector2.zero;
            borderRect.offsetMax = Vector2.zero;
            Image borderImg = border.AddComponent<Image>();
            borderImg.color      = TooltipBorder;
            borderImg.type       = Image.Type.Sliced;
            borderImg.fillCenter = false;
            borderImg.raycastTarget = false;

            // Vertical layout for name + desc
            VerticalLayoutGroup vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.padding               = new RectOffset(8, 8, 6, 6);
            vlg.spacing               = 2f;
            vlg.childAlignment        = TextAnchor.UpperLeft;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth     = true;
            vlg.childControlHeight    = true;

            // Name label
            GameObject nameGO  = new GameObject("TooltipName");
            nameGO.transform.SetParent(panel.transform, false);
            TextMeshProUGUI nameTMP = nameGO.AddComponent<TextMeshProUGUI>();
            nameTMP.text         = string.Empty;
            nameTMP.fontSize     = 14f;
            nameTMP.fontStyle    = FontStyles.Bold;
            nameTMP.color        = NameColor;
            nameTMP.raycastTarget = false;
            LayoutElement nameLe = nameGO.AddComponent<LayoutElement>();
            nameLe.minHeight     = 18f;

            // Description label
            GameObject descGO  = new GameObject("TooltipDesc");
            descGO.transform.SetParent(panel.transform, false);
            TextMeshProUGUI descTMP = descGO.AddComponent<TextMeshProUGUI>();
            descTMP.text         = string.Empty;
            descTMP.fontSize     = 11f;
            descTMP.color        = DescColor;
            descTMP.raycastTarget = false;
            LayoutElement descLe = descGO.AddComponent<LayoutElement>();
            descLe.minHeight     = 14f;

            // Starts hidden
            panel.SetActive(false);

            return panel;
        }

        // ─── Helpers ──────────────────────────────────────────────────────────────

        private static void SetAnchors(RectTransform rt,
            float minX, float minY, float maxX, float maxY)
        {
            rt.anchorMin = new Vector2(minX, minY);
            rt.anchorMax = new Vector2(maxX, maxY);
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, child);
        }

        private static Color HexColor(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color c);
            return c;
        }
    }
}
