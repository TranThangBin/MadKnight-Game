using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MadKnight.InventorySystem;

namespace MadKnight.UI
{
    /// <summary>
    /// Represents a single slot in the inventory UI
    /// This component should be attached to each inventory slot prefab
    /// </summary>
    public class InventorySlot : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _quantityText;
        [SerializeField] private TextMeshProUGUI _itemNameText;
        [SerializeField] private Button _button;
        [SerializeField] private GameObject _emptyIndicator;

        [Header("Optional")]
        [SerializeField] private GameObject _tooltipPanel;

        private int _slotIndex;
        private Item _currentItem;
        private System.Action<int> _onClickCallback;

        /// <summary>
        /// Initialize the slot with index and click callback
        /// </summary>
        public void Initialize(int index, System.Action<int> onClick)
        {
            _slotIndex = index;
            _onClickCallback = onClick;

            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
                _button.onClick.AddListener(() => _onClickCallback?.Invoke(_slotIndex));
            }

            // Hide tooltip initially
            if (_tooltipPanel != null)
            {
                _tooltipPanel.SetActive(false);
            }

            Clear();
        }

        /// <summary>
        /// Set the item to display in this slot
        /// </summary>
        public void SetItem(Item item)
        {
            if (item == null)
            {
                Clear();
                return;
            }

            _currentItem = item;
            Debug.Log($"Slot {_slotIndex}: Setting item {item.Name} x{item.Quantity}, Icon: {(item.Icon != null ? "OK" : "NULL")}");

            // Set icon
            if (_iconImage != null)
            {
                _iconImage.sprite = item.Icon;
                _iconImage.enabled = true;
                _iconImage.color = Color.white;
            }
            else
            {
                Debug.LogWarning($"Slot {_slotIndex}: _iconImage is null!");
            }

            // Set quantity
            if (_quantityText != null)
            {
                _quantityText.text = item.Quantity > 1 ? item.Quantity.ToString() : "";
                _quantityText.enabled = true;
            }
            else
            {
                Debug.LogWarning($"Slot {_slotIndex}: _quantityText is null!");
            }

            // Set item name
            if (_itemNameText != null)
            {
                _itemNameText.text = item.Name;
                _itemNameText.enabled = true;
            }

            // Hide empty indicator
            if (_emptyIndicator != null)
            {
                _emptyIndicator.SetActive(false);
            }
        }

        /// <summary>
        /// Clear the slot (remove item display)
        /// </summary>
        public void Clear()
        {
            _currentItem = null;

            if (_iconImage != null)
            {
                _iconImage.sprite = null;
                _iconImage.enabled = false;
            }

            if (_quantityText != null)
            {
                _quantityText.text = "";
                _quantityText.enabled = false;
            }

            if (_itemNameText != null)
            {
                _itemNameText.text = "";
                _itemNameText.enabled = false;
            }

            if (_emptyIndicator != null)
            {
                _emptyIndicator.SetActive(true);
            }

            if (_tooltipPanel != null)
            {
                _tooltipPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Get the current item in this slot
        /// </summary>
        public Item GetItem()
        {
            return _currentItem;
        }

        // Optional: Show tooltip on pointer enter
        public void OnPointerEnter()
        {
            if (_currentItem != null && _tooltipPanel != null)
            {
                _tooltipPanel.SetActive(true);
            }
        }

        // Optional: Hide tooltip on pointer exit
        public void OnPointerExit()
        {
            if (_tooltipPanel != null)
            {
                _tooltipPanel.SetActive(false);
            }
        }
    }
}
