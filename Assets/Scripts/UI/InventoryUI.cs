using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MadKnight.InventorySystem;

namespace MadKnight.UI
{
    /// <summary>
    /// Manages the UI display for the inventory system
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Inventory _inventory;
        [SerializeField] private Player _player;
        [SerializeField] private GameObject _inventoryPanel;
        [SerializeField] private Transform _slotsContainer;
        [SerializeField] private GameObject _slotPrefab;

        [Header("Settings")]
        [SerializeField] private KeyCode _toggleKey = KeyCode.I;

        private InventorySlot[] _slots;
        private bool _isOpen;

        private void Start()
        {
            // Validate references
            if (_player == null)
            {
                _player = FindFirstObjectByType<Player>();
                if (_player == null)
                {
                    Debug.LogError("InventoryUI: Player reference is missing!");
                    return;
                }
            }

            if (_inventory == null)
            {
                _inventory = _player.GetComponent<Inventory>();
                if (_inventory == null)
                {
                    Debug.LogError("InventoryUI: Player doesn't have Inventory component!");
                    return;
                }
            }

            if (_inventoryPanel == null)
            {
                Debug.LogError("InventoryUI: Inventory Panel reference is missing!");
                return;
            }

            if (_slotsContainer == null)
            {
                Debug.LogError("InventoryUI: Slots Container reference is missing!");
                return;
            }

            if (_slotPrefab == null)
            {
                Debug.LogError("InventoryUI: Slot Prefab reference is missing!");
                return;
            }

            // Subscribe to inventory events
            _inventory.OnInventoryChanged += UpdateUI;

            InitializeSlots();
            _inventoryPanel.SetActive(false);
            _isOpen = false;
        }

        private void OnDestroy()
        {
            if (_inventory != null)
            {
                _inventory.OnInventoryChanged -= UpdateUI;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(_toggleKey))
            {
                Debug.Log($"Toggle key pressed! Current state: {_isOpen}");
                ToggleInventory();
            }
        }

        private void InitializeSlots()
        {
            if (_inventory == null || _slotsContainer == null || _slotPrefab == null) 
            {
                Debug.LogError("InitializeSlots: Missing references!");
                return;
            }

            // Clear existing slots
            foreach (Transform child in _slotsContainer)
            {
                Destroy(child.gameObject);
            }

            // Create new slots
            _slots = new InventorySlot[_inventory.MaxSlots];
            int successCount = 0;
            
            for (int i = 0; i < _inventory.MaxSlots; i++)
            {
                GameObject slotObj = Instantiate(_slotPrefab, _slotsContainer);
                slotObj.name = $"Slot_{i}";
                
                InventorySlot slot = slotObj.GetComponent<InventorySlot>();
                
                if (slot == null)
                {
                    // If prefab doesn't have InventorySlot, add it dynamically
                    Debug.LogWarning($"Slot prefab missing InventorySlot component! Adding it to {slotObj.name}");
                    slot = slotObj.AddComponent<InventorySlot>();
                }
                
                if (slot != null)
                {
                    int index = i; // Capture for closure
                    slot.Initialize(index, OnSlotClicked);
                    _slots[i] = slot;
                    successCount++;
                }
                else
                {
                    Debug.LogError($"Failed to create slot {i}!");
                }
            }

            Debug.Log($"InitializeSlots: Created {successCount}/{_inventory.MaxSlots} slots successfully");
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (_inventory == null || _slots == null) 
            {
                Debug.LogWarning("UpdateUI: Inventory or slots is null!");
                return;
            }

            Debug.Log($"UpdateUI: Inventory has {_inventory.Items.Count} items, {_slots.Length} slots");

            // Update each slot
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i] == null) 
                {
                    Debug.LogWarning($"Slot {i} is null!");
                    continue; // Skip if slot is null
                }

                if (i < _inventory.Items.Count)
                {
                    Item item = _inventory.GetItem(i);
                    Debug.Log($"Setting slot {i} with item: {item?.Name ?? "null"}");
                    _slots[i].SetItem(item);
                }
                else
                {
                    _slots[i].Clear();
                }
            }
        }

        private void OnSlotClicked(int index)
        {
            if (_inventory == null || _player == null) return;

            Item item = _inventory.GetItem(index);
            if (item != null)
            {
                Debug.Log($"Clicked on {item.Name}");
                _inventory.UseItem(index, _player);
            }
        }

        public void ToggleInventory()
        {
            if (_inventoryPanel == null)
            {
                Debug.LogError("InventoryPanel is null! Cannot toggle.");
                return;
            }

            _isOpen = !_isOpen;
            _inventoryPanel.SetActive(_isOpen);
            Debug.Log($"Inventory toggled! IsOpen: {_isOpen}, Panel Active: {_inventoryPanel.activeSelf}");

            if (_isOpen)
            {
                UpdateUI();
            }
        }

        public void OpenInventory()
        {
            _isOpen = true;
            _inventoryPanel.SetActive(true);
            UpdateUI();
        }

        public void CloseInventory()
        {
            _isOpen = false;
            _inventoryPanel.SetActive(false);
        }
    }
}
