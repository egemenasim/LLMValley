using System;
using UnityEngine;

namespace LLMValley.Items
{
    /// <summary>
    /// A runtime pairing of an <see cref="ItemData"/> asset and a stack quantity.
    /// Used as the common data contract between the inventory system and all UI panels.
    /// </summary>
    [Serializable]
    public class ItemStack
    {
        // ─── Fields ───────────────────────────────────────────────────────────────

        [Tooltip("The item definition (ScriptableObject asset).")]
        public ItemData item;

        [Tooltip("Number of units in this stack. Must be >= 1.")]
        [Min(1)]
        public int quantity = 1;

        [Tooltip("Current durability of the item in this stack. -1 if not initialized or has no durability.")]
        public int currentDurability = -1;

        // ─── Constructors ─────────────────────────────────────────────────────────

        public ItemStack() { }

        public ItemStack(ItemData item, int quantity = 1)
        {
            this.item     = item;
            this.quantity = Mathf.Max(1, quantity);
            
            if (item != null && item.hasDurability)
            {
                this.currentDurability = item.maxDurability;
            }
            else
            {
                this.currentDurability = -1;
            }
        }

        // ─── Helpers ──────────────────────────────────────────────────────────────

        /// <summary>Returns true when this stack holds a valid (non-null) item reference.</summary>
        public bool IsValid => item != null;

        /// <summary>Returns the display name of the item, or an empty string if empty.</summary>
        public string ItemName => item != null ? item.itemName : string.Empty;

        public override string ToString() =>
            item != null ? $"{item.itemName} x{quantity}" : "(empty)";
    }
}
