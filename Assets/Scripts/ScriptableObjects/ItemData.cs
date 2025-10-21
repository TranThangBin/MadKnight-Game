using UnityEngine;
using MadKnight.InventorySystem;

namespace MadKnight.ScriptableObjects
{
    /// <summary>
    /// ScriptableObject that defines the properties of an item
    /// </summary>
    [CreateAssetMenu(fileName = "NewItem", menuName = "MadKnight/Inventory/ItemData")]
    public class ItemData : ScriptableObject
    {
        [Header("Basic Info")]
        [SerializeField] private string _itemName;
        [SerializeField] private string _itemId;
        [SerializeField][TextArea(3, 5)] private string _description;
        [SerializeField] private Sprite _icon;
        [SerializeField] private ItemType _type;

        [Header("Stack Settings")]
        [SerializeField] private int _maxStackSize = 1;
        [SerializeField] private bool _isStackable = true;

        [Header("Value")]
        [SerializeField] private int _buyPrice;
        [SerializeField] private int _sellPrice;

        [Header("Consumable Settings (if applicable)")]
        [SerializeField] private bool _isConsumable;
        [SerializeField] private int _healthRestore;
        [SerializeField] private int _manaRestore;

        public string ItemName => _itemName;
        public string ItemId => _itemId;
        public string Description => _description;
        public Sprite Icon => _icon;
        public ItemType Type => _type;
        public int MaxStackSize => _isStackable ? _maxStackSize : 1;
        public bool IsStackable => _isStackable;
        public int BuyPrice => _buyPrice;
        public int SellPrice => _sellPrice;
        public bool IsConsumable => _isConsumable;

        /// <summary>
        /// Use the item on the player
        /// </summary>
        public virtual void Use(Player player)
        {
            if (_isConsumable)
            {
                Debug.Log($"Used {_itemName}. Restored {_healthRestore} health and {_manaRestore} mana.");
                
                // TODO: Apply effects to player
                // Example: player.RestoreHealth(_healthRestore);
                // Example: player.RestoreMana(_manaRestore);
            }
            else
            {
                Debug.Log($"{_itemName} cannot be used.");
            }
        }

        private void OnValidate()
        {
            // Generate ID from name if empty
            if (string.IsNullOrEmpty(_itemId))
            {
                _itemId = _itemName.Replace(" ", "_").ToLower();
            }

            // Ensure max stack size is at least 1
            if (_maxStackSize < 1)
            {
                _maxStackSize = 1;
            }
        }
    }
}
