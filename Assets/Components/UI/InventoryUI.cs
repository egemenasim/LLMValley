using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LLMValley.Items;

namespace LLMValley.UI
{
    /// <summary>
    /// Manages the player's hotbar / inventory panel UI.
    ///
    /// Layout (assign in Inspector):
    ///   - <see cref="slots"/>        : 10 pre-instantiated InventorySlotUI children
    ///   - <see cref="tooltipPanel"/> : GameObject that holds the tooltip
    ///   - <see cref="tooltipName"/>  : TMP label for the selected item name
    ///   - <see cref="tooltipDesc"/>  : TMP label for the selected item description
    ///
    /// Usage:
    ///   Call <see cref="Refresh"/> every time the underlying inventory changes.
    ///   The panel is toggled with the "I" key or via <see cref="Show"/>/<see cref="Hide"/>.
    ///
    /// IMPORTANT — visibility:
    ///   Show/Hide use a <see cref="CanvasGroup"/> instead of SetActive so that
    ///   Update() keeps running and the "I" key can always re-open the panel.
    ///   Do NOT disable the root GameObject externally; use Hide() instead.
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        // ─── Constants ────────────────────────────────────────────────────────────

        public const int SlotCount = 10;

        // ─── Inspector References ─────────────────────────────────────────────────

        [Header("Slot References")]
        [Tooltip("All 10 InventorySlotUI components, in order.")]
        [SerializeField] private InventorySlotUI[] slots = new InventorySlotUI[SlotCount];

        [Header("Tooltip")]
        [Tooltip("Root GameObject for the tooltip panel. Shown when a valid item is selected.")]
        [SerializeField] private GameObject tooltipPanel;

        [Tooltip("TMP label for the selected item's display name.")]
        [SerializeField] private TextMeshProUGUI tooltipName;

        [Tooltip("TMP label for the selected item's description.")]
        [SerializeField] private TextMeshProUGUI tooltipDesc;

        // ─── State ────────────────────────────────────────────────────────────────

        private CanvasGroup _canvasGroup;
        private int _selectedIndex = -1;
        private List<ItemStack> _currentItems = new List<ItemStack>();
        private bool _isVisible = true;

        // ─── Unity Lifecycle ──────────────────────────────────────────────────────

        private void Awake()
        {
            // CanvasGroup — get or add so Show/Hide never kill Update().
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            // Wire each slot's click event and stamp its index.
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null) continue;
                int captured = i;   // closure capture
                slots[i].SetSlotIndex(captured);
                slots[i].OnSlotClicked.AddListener(OnSlotClicked);
            }

            HideTooltip();
        }

        private void Update()
        {
            // Toggle panel with "I" key.
            if (Input.GetKeyDown(KeyCode.I))
                ToggleVisibility();

            // Number keys 1–0 select hotbar slots (matches Minecraft / Stardew convention).
            for (int i = 0; i < SlotCount; i++)
            {
                KeyCode key = (KeyCode)((int)KeyCode.Alpha1 + i);   // Alpha1…Alpha9, then Alpha0
                if (i == 9) key = KeyCode.Alpha0;

                if (Input.GetKeyDown(key))
                {
                    SelectSlot(i);
                    break;
                }
            }
        }

        // ─── Public API ───────────────────────────────────────────────────────────

        public event System.Action<int> OnSlotClickedEvent;

        public int SelectedIndex => _selectedIndex;

        /// <summary>
        /// Refreshes all slot visuals from the provided item list.
        /// Slots beyond the list length are cleared.
        /// </summary>
        /// <param name="items">The player's current inventory contents.</param>
        public void Refresh(List<ItemStack> items)
        {
            _currentItems = items ?? new List<ItemStack>();

            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null) continue;

                if (i < _currentItems.Count && _currentItems[i] != null && _currentItems[i].IsValid)
                    slots[i].SetItem(_currentItems[i].item, _currentItems[i].quantity);
                else
                    slots[i].SetEmpty();
            }

            // Refresh tooltip in case the selected slot's item changed.
            RefreshTooltip();
        }

        public void SelectSavedSlot(int index)
        {
            if (index < 0 || index >= SlotCount)
                return;

            SelectSlot(index);
        }

        /// <summary>Selects the slot at <paramref name="index"/> and highlights it.</summary>
        public void SelectSlot(int index)
        {
            // Deselect old
            if (_selectedIndex >= 0 && _selectedIndex < slots.Length)
                slots[_selectedIndex]?.SetSelected(false);

            _selectedIndex = index;

            Debug.Log($"[InventoryUI] Selected slot: {_selectedIndex}");

            // Select new
            if (_selectedIndex >= 0 && _selectedIndex < slots.Length)
                slots[_selectedIndex]?.SetSelected(true);

            RefreshTooltip();
        }

        /// <summary>Whether the panel is currently visible.</summary>
        public bool IsVisible => _isVisible;

        /// <summary>
        /// Shows the inventory panel.
        /// Uses CanvasGroup so Update() keeps running.
        /// </summary>
        public void Show()
        {
            _isVisible = true;
            _canvasGroup.alpha          = 1f;
            _canvasGroup.interactable   = true;
            _canvasGroup.blocksRaycasts = true;
        }

        /// <summary>
        /// Hides the inventory panel.
        /// Uses CanvasGroup so Update() keeps running and "I" can reopen it.
        /// </summary>
        public void Hide()
        {
            _isVisible = false;
            _canvasGroup.alpha          = 0f;
            _canvasGroup.interactable   = false;
            _canvasGroup.blocksRaycasts = false;
            HideTooltip();
        }

        /// <summary>Toggles the inventory panel's visibility.</summary>
        public void ToggleVisibility()
        {
            if (_isVisible) Hide();
            else Show();
        }

        /// <summary>
        /// Returns the <see cref="ItemStack"/> currently selected, or null if none / empty.
        /// </summary>
        public ItemStack GetSelectedStack()
        {
            if (_selectedIndex < 0 || _selectedIndex >= _currentItems.Count)
                return null;
            return _currentItems[_selectedIndex];
        }

        // ─── Private Helpers ──────────────────────────────────────────────────────

        private void OnSlotClicked(int index)
        {
            SelectSlot(index);
            OnSlotClickedEvent?.Invoke(index);
        }

        private void RefreshTooltip()
        {
            ItemStack stack = GetSelectedStack();

            if (stack == null || !stack.IsValid)
            {
                HideTooltip();
                return;
            }

            if (tooltipName != null)
                tooltipName.text = stack.item.itemName;

            if (tooltipDesc != null)
                tooltipDesc.text = stack.item.description;

            if (tooltipPanel != null)
                tooltipPanel.SetActive(true);
        }

        private void HideTooltip()
        {
            if (tooltipPanel != null)
                tooltipPanel.SetActive(false);
        }
    }
}
