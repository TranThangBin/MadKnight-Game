using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace MadKnight.UI
{
    /// <summary>
    /// Handles tooltip display for inventory slots on hover
    /// Attach this to each slot GameObject
    /// </summary>
    public class InventorySlotTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Tooltip Settings")]
        [SerializeField] private GameObject _tooltipPanel;
        [SerializeField] private TextMeshProUGUI _tooltipTitle;
        [SerializeField] private TextMeshProUGUI _tooltipDescription;
        [SerializeField] private TextMeshProUGUI _tooltipStats;
        [SerializeField] private float _tooltipDelay = 0.5f;

        private InventorySlot _inventorySlot;
        private float _hoverTimer;
        private bool _isHovering;

        private void Awake()
        {
            _inventorySlot = GetComponent<InventorySlot>();
            
            if (_tooltipPanel != null)
            {
                _tooltipPanel.SetActive(false);
            }
        }

        private void Update()
        {
            if (_isHovering)
            {
                _hoverTimer += Time.deltaTime;
                
                if (_hoverTimer >= _tooltipDelay && _tooltipPanel != null && !_tooltipPanel.activeSelf)
                {
                    ShowTooltip();
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovering = true;
            _hoverTimer = 0f;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovering = false;
            _hoverTimer = 0f;
            HideTooltip();
        }

        private void ShowTooltip()
        {
            // Get item from inventory slot (you'll need to expose this in InventorySlot)
            // For now, just show the panel
            if (_tooltipPanel != null)
            {
                _tooltipPanel.SetActive(true);
            }
        }

        private void HideTooltip()
        {
            if (_tooltipPanel != null)
            {
                _tooltipPanel.SetActive(false);
            }
        }

        public void SetTooltipContent(string title, string description, string stats = "")
        {
            if (_tooltipTitle != null)
            {
                _tooltipTitle.text = title;
            }

            if (_tooltipDescription != null)
            {
                _tooltipDescription.text = description;
            }

            if (_tooltipStats != null)
            {
                _tooltipStats.text = stats;
            }
        }
    }
}
