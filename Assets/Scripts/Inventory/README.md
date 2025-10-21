# Hệ Thống Inventory cho MadKnight

## 📦 Tổng Quan

Hệ thống inventory đã được thiết kế hoàn chỉnh với các thành phần:

1. **Item.cs** - Class đại diện cho một item instance
2. **ItemType.cs** - Enum định nghĩa các loại item
3. **ItemData.cs** - ScriptableObject lưu trữ dữ liệu item
4. **Inventory.cs** - Quản lý inventory của player
5. **InventoryUI.cs** - Hiển thị UI inventory

## 🎮 Cách Thiết Lập

### Bước 1: Thêm Inventory vào Player GameObject

1. Mở Scene của bạn
2. Chọn Player GameObject
3. Add Component → Inventory

### Bước 2: Tạo ItemData (ScriptableObject)

1. Right-click trong Project window
2. Create → MadKnight → Inventory → ItemData
3. Đặt tên (ví dụ: "HealthPotion")
4. Cấu hình các thuộc tính:
   - Item Name: "Health Potion"
   - Description: "Restores 50 HP"
   - Icon: Drag sprite vào đây
   - Type: Consumable
   - Max Stack Size: 10
   - Is Consumable: ✓
   - Health Restore: 50

### Bước 3: Thiết Lập UI

1. Tạo Canvas trong Scene (nếu chưa có)
2. Tạo structure sau:

```
Canvas
└── InventoryPanel (Panel)
    ├── Header (Text: "Inventory")
    ├── SlotsContainer (GridLayoutGroup)
    └── CloseButton
```

3. Tạo Slot Prefab:
   - Tạo GameObject mới tên "InventorySlot"
   - Add components:
     - Image (background)
     - Button
     - Image (icon) - child object
     - TextMeshPro (quantity) - child object
     - GameObject (empty indicator) - child object
   - Add component: InventorySlot script
   - Kéo references vào inspector
   - Lưu thành Prefab

4. Tạo InventoryUI GameObject:
   - Add component: InventoryUI
   - Gán references:
     - Inventory: Player's Inventory component
     - Player: Player GameObject
     - Inventory Panel: InventoryPanel
     - Slots Container: SlotsContainer
     - Slot Prefab: Your slot prefab

## 🎯 Cách Sử Dụng

### Trong Code

```csharp
// Lấy reference đến inventory
Inventory inventory = player.GetComponent<Inventory>();

// Thêm item
[SerializeField] private ItemData healthPotion;
inventory.AddItem(healthPotion, 3); // Thêm 3 health potions

// Xóa item
inventory.RemoveItem(healthPotion, 1); // Xóa 1 health potion

// Kiểm tra có item không
bool hasPotion = inventory.HasItem(healthPotion, 2); // Có ít nhất 2?

// Lấy số lượng item
int count = inventory.GetItemQuantity(healthPotion);

// Sử dụng item (tại slot index)
inventory.UseItem(0, player);

// Sắp xếp inventory
inventory.Sort(byType: true); // Sắp xếp theo loại

// Xóa toàn bộ inventory
inventory.Clear();
```

### Events

```csharp
// Subscribe to inventory events
inventory.OnItemAdded += (item) => {
    Debug.Log($"Added: {item.Name}");
};

inventory.OnItemRemoved += (item) => {
    Debug.Log($"Removed: {item.Name}");
};

inventory.OnItemUsed += (item, index) => {
    Debug.Log($"Used: {item.Name} at slot {index}");
};

inventory.OnInventoryChanged += () => {
    Debug.Log("Inventory changed!");
    // Update UI, save game, etc.
};
```

## 🎨 Tùy Chỉnh

### Tạo Custom ItemData

Tạo class kế thừa từ ItemData:

```csharp
using MadKnight.Inventory;
using MadKnight.ScriptableObjects;

[CreateAssetMenu(menuName = "MadKnight/Inventory/WeaponData")]
public class WeaponData : ItemData
{
    [SerializeField] private int _damage;
    [SerializeField] private float _attackSpeed;

    public int Damage => _damage;
    public float AttackSpeed => _attackSpeed;

    public override void Use(Player player)
    {
        // Equip weapon logic
        Debug.Log($"Equipped {ItemName} with {_damage} damage");
    }
}
```

### Mở rộng Item Types

Thêm vào `ItemType.cs`:

```csharp
public enum ItemType
{
    Weapon,
    Armor,
    Consumable,
    QuestItem,
    Material,
    Miscellaneous,
    // Thêm loại mới:
    Key,
    Tool,
    Accessory
}
```

## ⌨️ Controls

- **I** (hoặc key được set): Toggle inventory
- **Click** vào slot: Sử dụng item

## 📋 Features

✅ Stack items (configurable max stack size)  
✅ Multiple item types  
✅ Consumable items  
✅ Drag and drop ready (có thể extend)  
✅ Events system  
✅ Sort functionality  
✅ UI system hoàn chỉnh  
✅ ScriptableObject-based items  

## 🔧 Tips

1. **Tối ưu hóa**: Nếu có nhiều item, consider object pooling cho UI slots
2. **Save System**: Integrate với SaveSystem hiện có để lưu inventory
3. **Tooltips**: Thêm tooltip hiển thị description khi hover
4. **Drag & Drop**: Extend InventorySlot với IBeginDragHandler, IDragHandler, IEndDragHandler

## 📝 Next Steps

1. Integrate với Save System
2. Thêm item drop từ enemies
3. Tạo merchant/shop system
4. Add item crafting
5. Equipment system (weapon/armor slots)
6. Quick slots/hotbar

## ⚠️ Lưu Ý

- ItemData phải được tạo dưới dạng ScriptableObject
- Inventory component phải được attach vào Player GameObject
- UI references phải được gán đúng trong Inspector
- Icon sprites cần được import với Texture Type: Sprite (2D and UI)
