using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MadKnight.ScriptableObjects;

namespace MadKnight.InventorySystem
{
    /// <summary>
    /// Manages the player's inventory system
    /// </summary>
    public class Inventory : MonoBehaviour
    {
        [Header("Inventory Settings")]
        [SerializeField] private int _maxSlots = 20;
        
        // Don't serialize items - they should be added at runtime only
        private List<Item> _items = new List<Item>();

        public int MaxSlots => _maxSlots;
        public int UsedSlots => _items.Count;
        public int FreeSlots => _maxSlots - _items.Count;
        public IReadOnlyList<Item> Items => _items.AsReadOnly();

        // Events
        public event Action<Item> OnItemAdded;
        public event Action<Item> OnItemRemoved;
        public event Action<Item, int> OnItemUsed;
        public event Action OnInventoryChanged;

        private void Awake()
        {
            // Initialize inventory - always start with empty inventory
            _items = new List<Item>();
        }

        /// <summary>
        /// Add an item to the inventory
        /// </summary>
        /// <returns>True if the item was added successfully</returns>
        public bool AddItem(ItemData itemData, int quantity = 1)
        {
            if (itemData == null) return false;

            int remainingQuantity = quantity;

            // Try to stack with existing items first
            if (itemData.IsStackable)
            {
                foreach (var item in _items)
                {
                    if (item.ItemData == itemData && item.Quantity < item.MaxStackSize)
                    {
                        remainingQuantity = item.AddQuantity(remainingQuantity);
                        OnInventoryChanged?.Invoke();
                        
                        if (remainingQuantity == 0)
                        {
                            OnItemAdded?.Invoke(item);
                            return true;
                        }
                    }
                }
            }

            // Add new item stacks
            while (remainingQuantity > 0)
            {
                if (FreeSlots <= 0)
                {
                    Debug.LogWarning("Inventory is full!");
                    return false;
                }

                int stackSize = Mathf.Min(remainingQuantity, itemData.MaxStackSize);
                Item newItem = new Item(itemData, stackSize);
                _items.Add(newItem);
                remainingQuantity -= stackSize;

                OnItemAdded?.Invoke(newItem);
            }

            OnInventoryChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Remove an item from the inventory
        /// </summary>
        /// <returns>True if the item was removed successfully</returns>
        public bool RemoveItem(ItemData itemData, int quantity = 1)
        {
            if (itemData == null) return false;

            int remainingToRemove = quantity;

            // Remove from existing stacks
            for (int i = _items.Count - 1; i >= 0; i--)
            {
                if (_items[i].ItemData == itemData)
                {
                    int removeFromStack = Mathf.Min(remainingToRemove, _items[i].Quantity);
                    bool isEmpty = _items[i].RemoveQuantity(removeFromStack);
                    remainingToRemove -= removeFromStack;

                    if (isEmpty)
                    {
                        Item removedItem = _items[i];
                        _items.RemoveAt(i);
                        OnItemRemoved?.Invoke(removedItem);
                    }

                    if (remainingToRemove <= 0)
                    {
                        OnInventoryChanged?.Invoke();
                        return true;
                    }
                }
            }

            OnInventoryChanged?.Invoke();
            return remainingToRemove <= 0;
        }

        /// <summary>
        /// Remove an item at a specific index
        /// </summary>
        public bool RemoveItemAt(int index, int quantity = 1)
        {
            if (index < 0 || index >= _items.Count) return false;

            Item item = _items[index];
            bool isEmpty = item.RemoveQuantity(quantity);

            if (isEmpty)
            {
                _items.RemoveAt(index);
                OnItemRemoved?.Invoke(item);
            }

            OnInventoryChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Use an item from the inventory
        /// </summary>
        public void UseItem(int index, Player player)
        {
            if (index < 0 || index >= _items.Count) return;

            Item item = _items[index];
            item.Use(player);
            OnItemUsed?.Invoke(item, index);

            // Remove consumable items after use
            if (item.ItemData.IsConsumable)
            {
                RemoveItemAt(index, 1);
            }
        }

        /// <summary>
        /// Get an item at a specific index
        /// </summary>
        public Item GetItem(int index)
        {
            if (index < 0 || index >= _items.Count) return null;
            return _items[index];
        }

        /// <summary>
        /// Check if the inventory contains a specific item
        /// </summary>
        public bool HasItem(ItemData itemData, int quantity = 1)
        {
            if (itemData == null) return false;

            int totalQuantity = _items
                .Where(item => item.ItemData == itemData)
                .Sum(item => item.Quantity);

            return totalQuantity >= quantity;
        }

        /// <summary>
        /// Get the total quantity of a specific item
        /// </summary>
        public int GetItemQuantity(ItemData itemData)
        {
            if (itemData == null) return 0;

            return _items
                .Where(item => item.ItemData == itemData)
                .Sum(item => item.Quantity);
        }

        /// <summary>
        /// Clear all items from the inventory
        /// </summary>
        public void Clear()
        {
            _items.Clear();
            OnInventoryChanged?.Invoke();
        }

        /// <summary>
        /// Sort inventory by item type or name
        /// </summary>
        public void Sort(bool byType = true)
        {
            if (byType)
            {
                _items = _items.OrderBy(item => item.Type).ThenBy(item => item.Name).ToList();
            }
            else
            {
                _items = _items.OrderBy(item => item.Name).ToList();
            }

            OnInventoryChanged?.Invoke();
        }
    }
}
