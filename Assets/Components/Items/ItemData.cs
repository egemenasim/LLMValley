using UnityEngine;

namespace LLMValley.Items
{
    /// <summary>
    /// Base ScriptableObject that describes any item in LLM Valley.
    /// Create new instances via:
    ///   Assets → Create → LLMValley → Items → ItemData
    /// </summary>
    [CreateAssetMenu(fileName = "New ItemData", menuName = "LLMValley/Items/ItemData", order = 0)]
    public class ItemData : ScriptableObject
    {
        // ─── Identity ────────────────────────────────────────────────────────────
        [Header("Identity")]

        [Tooltip("Display name shown to the player.")]
        public string itemName;

        [Tooltip("Flavour text / tooltip shown in the inventory UI.")]
        [TextArea(2, 4)]
        public string description;

        [Tooltip("Inventory / shop icon.")]
        public Sprite icon;

        [Tooltip("Unique numeric identifier. Must not clash with any other ItemData asset.")]
        public int itemID;

        // ─── Classification ──────────────────────────────────────────────────────
        [Header("Classification")]

        [Tooltip("Broad category used for behaviour, shop filters, and UI grouping.")]
        public ItemType itemType;

        // ─── Economy ─────────────────────────────────────────────────────────────
        [Header("Economy")]

        [Tooltip("Base sell price in gold. Shops may apply their own multipliers.")]
        public float baseValue;

        // ─── Stacking ────────────────────────────────────────────────────────────
        [Header("Stacking")]

        [Tooltip("Whether multiple units of this item can occupy one inventory slot.")]
        public bool isStackable;

        [Tooltip("Maximum units per inventory slot. Only relevant when isStackable is true.")]
        [Min(1)]
        public int maxStackSize = 99;

        // ─── Plant (Optional) ────────────────────────────────────────────────────
        [Header("Plant (Optional)")]

        [Tooltip("Maximum growth level for this plant. Leave 0 for non-plant items.")]
        [Min(0)]
        public int maxGrowthLevel;

        [Tooltip("Minimum growth level for this plant.")]
        [Min(0)]
        public int minGrowthLevel;

        [Tooltip("Sprite to use for each growth level (index = level).")]
        public Sprite[] growthSprites;

        [Tooltip("Total days required to reach max growth level.")]
        [Min(0)]
        public int totalGrowthDays;
    }
}
