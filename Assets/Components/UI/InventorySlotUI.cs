using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;
using LLMValley.Items;

namespace LLMValley.UI
{
    /// <summary>
    /// Controls a single inventory slot in the UI.
    /// Attach to the InventorySlotUI prefab root.
    ///
    /// Assign references via the Inspector:
    ///   - itemIconImage      : child Image used to display the item icon
    ///   - quantityText       : child TextMeshProUGUI showing the stack count
    ///   - selectionHighlight : child Image used as a selection border
    ///
    /// The slot index must be set externally (e.g. by the inventory panel)
    /// before any click events fire.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class InventorySlotUI : MonoBehaviour, IPointerClickHandler
    {
        // ─── Inspector References ─────────────────────────────────────────────────

        [Header("Slot References")]

        [Tooltip("Image component that renders the item icon.")]
        [SerializeField] private Image itemIconImage;

        [Tooltip("TextMeshPro label that displays the stack quantity.")]
        [SerializeField] private TextMeshProUGUI quantityText;

        [Tooltip("Image used as a selection highlight / border.")]
        [SerializeField] public Image selectionHighlight;

        // ─── Events ───────────────────────────────────────────────────────────────

        [Header("Events")]

        /// <summary>
        /// Fired when this slot is clicked. Passes the slot index.
        /// </summary>
        public UnityEvent<int> OnSlotClicked = new UnityEvent<int>();

        // ─── State ────────────────────────────────────────────────────────────────

        /// <summary>Zero-based index of this slot within the owning inventory panel.</summary>
        public int SlotIndex { get; private set; }

        // ─── Public API ───────────────────────────────────────────────────────────

        /// <summary>
        /// Assigns a slot index. Must be called by the inventory panel after
        /// instantiation so that click events carry the correct index.
        /// </summary>
        public void SetSlotIndex(int index)
        {
            SlotIndex = index;
        }

        /// <summary>
        /// Populates the slot with the given item and quantity.
        /// Hides the quantity label when the stack size is 1 or the item is not stackable.
        /// </summary>
        /// <param name="item">The ItemData asset to display.</param>
        /// <param name="quantity">Number of items in this stack.</param>
        public void SetItem(ItemData item, int quantity)
        {
            if (item == null)
            {
                SetEmpty();
                return;
            }

            // Icon
            if (itemIconImage != null)
            {
                itemIconImage.sprite  = item.icon;
                itemIconImage.enabled = item.icon != null;
                itemIconImage.color   = Color.white;
            }

            // Quantity — only show when stackable and more than 1 unit
            if (quantityText != null)
            {
                bool showQuantity = item.isStackable && quantity > 1;
                quantityText.text    = quantity.ToString();
                quantityText.enabled = showQuantity;
            }
        }

        /// <summary>
        /// Clears the slot, removing any item icon and hiding the quantity label.
        /// </summary>
        public void SetEmpty()
        {
            if (itemIconImage != null)
            {
                itemIconImage.sprite  = null;
                itemIconImage.enabled = false;
            }

            if (quantityText != null)
            {
                quantityText.text    = string.Empty;
                quantityText.enabled = false;
            }
        }

        /// <summary>
        /// Toggles the selection highlight border on or off.
        /// </summary>
        /// <param name="selected">True to show the highlight; false to hide it.</param>
        public void SetSelected(bool selected)
        {
            if (selectionHighlight != null)
                selectionHighlight.enabled = selected;
        }

        // ─── IPointerClickHandler ─────────────────────────────────────────────────

        /// <inheritdoc/>
        public void OnPointerClick(PointerEventData eventData)
        {
            OnSlotClicked?.Invoke(SlotIndex);
        }

        // ─── Unity Lifecycle ──────────────────────────────────────────────────────

        private void Awake()
        {
            // Ensure the highlight starts hidden at runtime.
            SetSelected(false);
        }
    }
}
