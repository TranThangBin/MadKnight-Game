using UnityEngine;
using MadKnight.InventorySystem;
using MadKnight.ScriptableObjects;

namespace MadKnight.Test
{
    /// <summary>
    /// Example script showing how to use the Inventory system
    /// Attach this to any GameObject to test the inventory
    /// </summary>
    public class InventoryTestScript : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Player _player;
        [SerializeField] private Inventory _inventory;

        [Header("Test Items")]
        [SerializeField] private ItemData _testItem1;
        [SerializeField] private ItemData _testItem2;
        [SerializeField] private ItemData _testItem3;

        private void Start()
        {
            // Get inventory from player if not assigned
            if (_inventory == null && _player != null)
            {
                _inventory = _player.GetComponent<Inventory>();
            }

            // Subscribe to events for testing
            if (_inventory != null)
            {
                _inventory.OnItemAdded += OnItemAdded;
                _inventory.OnItemRemoved += OnItemRemoved;
                _inventory.OnItemUsed += OnItemUsed;
                _inventory.OnInventoryChanged += OnInventoryChanged;
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (_inventory != null)
            {
                _inventory.OnItemAdded -= OnItemAdded;
                _inventory.OnItemRemoved -= OnItemRemoved;
                _inventory.OnItemUsed -= OnItemUsed;
                _inventory.OnInventoryChanged -= OnInventoryChanged;
            }
        }

        private void Update()
        {
            // Test controls
            if (_inventory == null) return;

            // Press 1 to add test item 1
            if (Input.GetKeyDown(KeyCode.Alpha1) && _testItem1 != null)
            {
                _inventory.AddItem(_testItem1, 1);
                Debug.Log("Added item 1");
            }

            // Press 2 to add test item 2
            if (Input.GetKeyDown(KeyCode.Alpha2) && _testItem2 != null)
            {
                _inventory.AddItem(_testItem2, 5);
                Debug.Log("Added 5x item 2");
            }

            // Press 3 to add test item 3
            if (Input.GetKeyDown(KeyCode.Alpha3) && _testItem3 != null)
            {
                _inventory.AddItem(_testItem3, 1);
                Debug.Log("Added item 3");
            }

            // Press R to remove first item
            if (Input.GetKeyDown(KeyCode.R))
            {
                if (_inventory.UsedSlots > 0)
                {
                    Item firstItem = _inventory.GetItem(0);
                    if (firstItem != null)
                    {
                        _inventory.RemoveItem(firstItem.ItemData, 1);
                        Debug.Log($"Removed 1x {firstItem.Name}");
                    }
                }
            }

            // Press U to use first item
            if (Input.GetKeyDown(KeyCode.U))
            {
                if (_inventory.UsedSlots > 0 && _player != null)
                {
                    _inventory.UseItem(0, _player);
                }
            }

            // Press C to clear inventory
            if (Input.GetKeyDown(KeyCode.C))
            {
                _inventory.Clear();
                Debug.Log("Cleared inventory");
            }

            // Press S to sort inventory
            if (Input.GetKeyDown(KeyCode.S))
            {
                _inventory.Sort(true);
                Debug.Log("Sorted inventory");
            }

            // Press L to log inventory contents
            if (Input.GetKeyDown(KeyCode.L))
            {
                LogInventoryContents();
            }
        }

        private void LogInventoryContents()
        {
            Debug.Log("=== Inventory Contents ===");
            Debug.Log($"Slots: {_inventory.UsedSlots}/{_inventory.MaxSlots}");
            
            for (int i = 0; i < _inventory.Items.Count; i++)
            {
                Item item = _inventory.GetItem(i);
                if (item != null)
                {
                    Debug.Log($"[{i}] {item.Name} x{item.Quantity} ({item.Type})");
                }
            }
            Debug.Log("========================");
        }

        // Event handlers
        private void OnItemAdded(Item item)
        {
            Debug.Log($"<color=green>[Event] Item Added: {item.Name} x{item.Quantity}</color>");
        }

        private void OnItemRemoved(Item item)
        {
            Debug.Log($"<color=red>[Event] Item Removed: {item.Name}</color>");
        }

        private void OnItemUsed(Item item, int index)
        {
            Debug.Log($"<color=yellow>[Event] Item Used: {item.Name} at slot {index}</color>");
        }

        private void OnInventoryChanged()
        {
            Debug.Log("<color=cyan>[Event] Inventory Changed</color>");
        }
    }
}
