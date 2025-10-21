using UnityEngine;
using MadKnight.ScriptableObjects;

namespace MadKnight.InventorySystem
{
    /// <summary>
    /// Represents a single item instance in the inventory
    /// </summary>
    [System.Serializable]
    public class Item
    {
        [SerializeField] private ItemData _itemData;
        [SerializeField] private int _quantity;

        public ItemData ItemData => _itemData;
        public int Quantity => _quantity;
        public string Name => _itemData != null ? _itemData.ItemName : "Unknown";
        public Sprite Icon => _itemData != null ? _itemData.Icon : null;
        public ItemType Type => _itemData != null ? _itemData.Type : ItemType.Miscellaneous;
        public int MaxStackSize => _itemData != null ? _itemData.MaxStackSize : 1;

        public Item(ItemData itemData, int quantity = 1)
        {
            _itemData = itemData;
            _quantity = Mathf.Clamp(quantity, 0, itemData.MaxStackSize);
        }

        /// <summary>
        /// Add quantity to this item
        /// </summary>
        /// <returns>Remaining quantity that couldn't be added (if stack is full)</returns>
        public int AddQuantity(int amount)
        {
            int newQuantity = _quantity + amount;
            if (newQuantity > MaxStackSize)
            {
                _quantity = MaxStackSize;
                return newQuantity - MaxStackSize;
            }

            _quantity = newQuantity;
            return 0;
        }

        /// <summary>
        /// Remove quantity from this item
        /// </summary>
        /// <returns>True if the item quantity is now 0 or less</returns>
        public bool RemoveQuantity(int amount)
        {
            _quantity -= amount;
            return _quantity <= 0;
        }

        /// <summary>
        /// Check if this item can stack with another item
        /// </summary>
        public bool CanStackWith(Item other)
        {
            return other != null && 
                   _itemData == other._itemData && 
                   _quantity < MaxStackSize;
        }

        /// <summary>
        /// Use the item
        /// </summary>
        public void Use(Player player)
        {
            if (_itemData != null)
            {
                _itemData.Use(player);
            }
        }
    }
}
