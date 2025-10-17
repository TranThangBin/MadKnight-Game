# LineRenderer Không Hiển Thị - Hướng Dẫn Fix

## 🔧 Quick Fixes (Thử theo thứ tự)

### Fix 1: Kiểm tra Material và Shader
```csharp
// Trong Unity Editor:
1. Chọn GameObject có LineRenderer
2. Trong Inspector, check Material
3. Nếu Material = None hoặc màu hồng -> Có vấn đề!

// Fix:
- Thay shader thành "Unlit/Color"
- Hoặc "Sprites/Default"  
- Hoặc "UI/Default"
```

### Fix 2: Kiểm tra Width
```csharp
// LineRenderer phải có độ dày > 0
startWidth = 0.15f  (ít nhất 0.1)
endWidth = 0.15f
```

### Fix 3: Kiểm tra Sorting Order
```csharp
// Trong Inspector của LineRenderer:
Sorting Layer = "Default"
Order in Layer = 1 hoặc cao hơn

// Dots nên có Order in Layer = 2 để hiển thị trên line
```

### Fix 4: Kiểm tra Position Count
```csharp
// LineRenderer cần ít nhất 2 points
Position Count >= 2

// Kiểm tra các positions có hợp lệ không:
- Z position = 0 (cho 2D)
- X, Y trong phạm vi camera
```

### Fix 5: Kiểm tra Camera
```csharp
// Camera phải là Orthographic (không phải Perspective)
Projection = Orthographic
Size = 5-10 (phù hợp với board size)

// Camera phải có tag "MainCamera"
```

## 🚀 Auto Fix - Cách Nhanh Nhất

### Sử dụng LineRendererFixer Script:
1. Select GameObject có LineRenderer trong Hierarchy
2. Add Component -> Search "LineRendererFixer"
3. Script sẽ tự động fix khi Play

### Sử dụng Material Helper:
```csharp
// Trong code, thay đổi cách tạo material:
// BEFORE:
lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

// AFTER:
lineRenderer.material = LineRendererMaterialHelper.GetOptimizedMaterial(color);
```

## 🔍 Debug Tools

### 1. Debug Key Commands (trong Play Mode)
- Nhấn **D**: In ra thông tin tất cả LineRenderers
- Nhấn **L**: Liệt kê các shaders có sẵn

### 2. Kiểm tra trong Console
```csharp
// Khi play, check console logs:
[LineRendererMaterial] Sử dụng shader: Unlit/Color  ✓ GOOD
[LineRendererFixer] Đã fix LineRenderer: Line_0     ✓ GOOD

// Nếu thấy errors:
Shader 'X' not found  ✗ BAD - Cần thay shader khác
```

### 3. Scene View Gizmos
- Nếu dùng LineRendererFixer, sẽ thấy wireframe vàng trong Scene view
- Nếu không thấy gì -> LineRenderer không có positions

## 📋 Checklist Đầy Đủ

```
☐ Camera là Orthographic
☐ Camera có tag "MainCamera"  
☐ LineRenderer.enabled = true
☐ GameObject.activeSelf = true
☐ Material không null
☐ Shader không null (không màu hồng)
☐ startWidth > 0 (thử 0.15)
☐ endWidth > 0 (thử 0.15)
☐ positionCount >= 2
☐ Các positions có z = 0
☐ Sorting Order >= 1
☐ startColor.alpha > 0
☐ endColor.alpha > 0
```

## 🎨 Các Shader Khả Dụng (Theo Thứ Tự Ưu Tiên)

1. **Unlit/Color** ⭐ (Tốt nhất cho 2D)
2. **Sprites/Default** ⭐ (Unity 2D standard)
3. **UI/Default** (Backup tốt)
4. **Unlit/Transparent**
5. **Legacy Shaders/Particles/Alpha Blended**

## 💡 Tips

### Nếu vẫn không thấy line:
1. Tạo một LineRenderer test đơn giản:
```csharp
void TestLineRenderer()
{
    GameObject go = new GameObject("TestLine");
    LineRenderer lr = go.AddComponent<LineRenderer>();
    
    lr.material = new Material(Shader.Find("Unlit/Color"));
    lr.material.color = Color.red;
    lr.startWidth = 0.5f;
    lr.endWidth = 0.5f;
    lr.positionCount = 2;
    lr.SetPosition(0, new Vector3(-2, 0, 0));
    lr.SetPosition(1, new Vector3(2, 0, 0));
    lr.sortingOrder = 10;
    
    Debug.Log("Test line created!");
}
```

2. Gọi trong Start() và chạy
3. Nếu test line hiện -> vấn đề ở logic game
4. Nếu test line không hiện -> vấn đề ở Unity/project settings

### Kiểm tra Layer và Culling:
- LineRenderer không bị ẩn bởi Culling Mask của Camera
- Check Camera.cullingMask có include layer của LineRenderer

## 🆘 Last Resort

Nếu tất cả đều thử rồi mà vẫn không hiển thị:

1. **Restart Unity Editor** (đôi khi cache bị lỗi)
2. **Reimport Scripts**: Right-click Scripts folder -> Reimport
3. **Clear Library**: Xóa folder Library và để Unity regenerate
4. **Check Unity Version**: Update Unity nếu quá cũ
5. **Check Build Settings**: Đảm bảo Graphics API tương thích

## 📞 Support

Nếu vẫn gặp vấn đề, thu thập thông tin sau:
- Unity version
- Output của Debug key (D)
- Screenshot Inspector của LineRenderer
- Console logs/errors
