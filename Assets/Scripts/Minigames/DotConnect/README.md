# Mini-Game: Dot-Connect

## Hướng Dẫn Setup

### 1. Tạo ScriptableObject Config

1. Trong Unity, chuột phải trong Project window
2. Chọn `Create > MiniGames > DotConnect > Config`
3. Đặt tên: `DotConnectConfig`
4. Cấu hình các thông số:
   - **Board Width/Height**: Kích thước bàn cờ (3-10)
   - **Number Of Dot Pairs**: Số cặp dots (2-8)
   - **Minimum Difficulty**: Độ khó (1-10)
   - **Dot Colors**: Mảng màu sắc cho dots

### 2. Tạo Scene

1. Tạo Empty GameObject, đặt tên `DotConnectManager`
2. Add component `DotConnectManager`
3. Gán `DotConnectConfig` vào field Config

### 3. Tạo Board Container

1. Tạo Empty GameObject con của `DotConnectManager`, đặt tên `Board`
2. Gán vào field `Board Container` trong DotConnectManager

### 4. Tạo Prefabs (Optional)

#### Cell Prefab (Optional - nếu không có sẽ tự tạo):
- Tạo Quad hoặc Sprite
- Thêm BoxCollider2D
- Kích thước: 1x1 unit

#### Dot Prefab (Optional - nếu không có sẽ tự tạo):
- Tạo Sphere hoặc Sprite hình tròn
- Thêm CircleCollider2D
- Material để set màu động

#### LineRenderer Prefab (Optional - nếu không có sẽ tự tạo):
- GameObject với LineRenderer component
- Set width và material

### 5. Setup Camera

- Đảm bảo Main Camera có tag "MainCamera"
- Đặt Projection: Orthographic
- Size phù hợp với kích thước board

### 6. Subscribe Events

```csharp
using UnityEngine;
using MiniGames.DotConnect;

public class GameController : MonoBehaviour
{
    [SerializeField] private DotConnectManager dotConnectManager;
    
    private void Start()
    {
        // Đăng ký events
        dotConnectManager.onPuzzleCompleted.AddListener(OnPuzzleCompleted);
        dotConnectManager.onDotPairConnected.AddListener(OnDotPairConnected);
        dotConnectManager.onPuzzleStarted.AddListener(OnPuzzleStarted);
    }
    
    private void OnPuzzleCompleted()
    {
        Debug.Log("🎉 Hoàn thành tất cả puzzle!");
        // Xử lý logic của bạn ở đây (mở khóa, reward, etc.)
    }
    
    private void OnDotPairConnected(int pairId, Color color)
    {
        Debug.Log($"Đã nối cặp {pairId} màu {color}");
        // Có thể phát sound effect, animation, etc.
    }
    
    private void OnPuzzleStarted()
    {
        Debug.Log("Puzzle mới đã bắt đầu!");
    }
}
```

## Cách Chơi

1. **Bắt đầu**: Click/touch vào một dot
2. **Kéo**: Giữ và kéo qua các ô để vẽ đường
3. **Kết thúc**: Kéo đến dot cùng màu để hoàn thành
4. **Xóa**: Click vào dot đã nối để xóa đường cũ
5. **Hoàn thành**: Nối tất cả các cặp dots

## Quy Tắc

- Đường nối chỉ đi ngang/dọc (không đi chéo)
- Đường nối không được chồng lên nhau
- Mỗi ô chỉ có thể có một đường đi qua
- Phải nối đúng 2 dots cùng màu

## API Reference

### DotConnectManager

#### Public Methods:
- `InitializePuzzle()`: Tạo puzzle mới
- `ResetPuzzle()`: Xóa tất cả đường nối, giữ nguyên puzzle
- `NewPuzzle()`: Tạo puzzle hoàn toàn mới

#### Public Events:
- `onPuzzleCompleted`: Gọi khi hoàn thành tất cả
- `onDotPairConnected(int pairId, Color color)`: Gọi khi nối một cặp
- `onPuzzleStarted`: Gọi khi bắt đầu puzzle mới

### DotConnectConfig

#### Settings:
- `boardWidth`: Chiều rộng (3-10)
- `boardHeight`: Chiều cao (3-10)
- `numberOfDotPairs`: Số cặp dots (2-8)
- `minimumDifficulty`: Độ khó (1-10)
- `maxGenerationAttempts`: Số lần thử tạo puzzle
- `dotColors`: Mảng màu sắc
- `dotSize`: Kích thước dot
- `lineWidth`: Độ dày đường

## Thuật Toán

### Generator:
1. **Tạo đường đi ngẫu nhiên**: Mỗi cặp dots được tạo bằng cách random walk
2. **Kiểm tra độ khó**: Đếm số lần rẽ, đường dài = khó hơn
3. **Backtracking verification**: Đảm bảo puzzle có thể giải được
4. **Retry mechanism**: Thử lại nếu không thỏa độ khó

### Validation:
- BFS để tìm đường đi ngắn nhất
- Backtracking để verify puzzle có solution
- Collision detection khi người chơi vẽ đường

## Tính Năng

✅ Tự động tạo puzzle có thể giải
✅ Cấu hình linh hoạt kích thước và độ khó
✅ Random màu sắc cho mỗi cặp dots
✅ Event system để xử lý logic game
✅ Hỗ trợ touch và mouse input
✅ Visual feedback với LineRenderer
✅ Tự động kiểm tra hoàn thành

## Mở Rộng

### Thêm Sound Effects:
```csharp
private void OnDotPairConnected(int pairId, Color color)
{
    AudioManager.Instance.PlaySound("DotConnected");
}

private void OnPuzzleCompleted()
{
    AudioManager.Instance.PlaySound("PuzzleComplete");
}
```

### Thêm Animation:
```csharp
private void OnPuzzleCompleted()
{
    // Animate tất cả dots
    foreach (var dotObj in dotObjects.Values)
    {
        dotObj.GetComponent<Animator>()?.SetTrigger("Complete");
    }
}
```

### Thêm Timer:
```csharp
private float elapsedTime;

private void Update()
{
    if (!isPuzzleCompleted)
    {
        elapsedTime += Time.deltaTime;
    }
}
```

### Thêm Hint System:
```csharp
public void ShowHint()
{
    // Tìm một cặp chưa hoàn thành
    var uncompletedPair = dotPairs.Find(p => !p.isCompleted);
    if (uncompletedPair != null)
    {
        // Highlight dots hoặc show một phần đường đi
        HighlightDots(uncompletedPair);
    }
}
```

## Troubleshooting

**Q: Không tạo được puzzle?**
- Giảm `minimumDifficulty`
- Tăng kích thước board
- Giảm số lượng dot pairs

**Q: Puzzle quá dễ?**
- Tăng `minimumDifficulty`
- Tăng số lượng dot pairs
- Giảm kích thước board

**Q: Line không hiển thị?**
- **Check Shader**: LineRenderer cần shader phù hợp
  - Thử shader: `Unlit/Color`, `Sprites/Default`, hoặc `UI/Default`
  - Material phải có màu (alpha > 0)
- **Check Sorting Order**: Set sorting order > 0 để hiển thị trên background
- **Check Z-Position**: Đặt z = 0 hoặc gần camera
- **Check Width**: startWidth và endWidth phải > 0 (thử 0.15)
- **Check Position Count**: Phải >= 2 points
- **Quick Fix**: Attach script `LineRendererFixer` vào GameObject có LineRenderer
- Check Camera có tag "MainCamera"
- Check Camera projection mode (Orthographic cho 2D)

**Cách Debug LineRenderer:**
```csharp
// Thêm vào Update() để debug
if (Input.GetKeyDown(KeyCode.D))
{
    foreach (var lr in lineRenderers.Values)
    {
        Debug.Log($"Line - Positions: {lr.positionCount}, " +
                  $"Width: {lr.startWidth}, " +
                  $"Material: {lr.material?.name}, " +
                  $"Shader: {lr.material?.shader?.name}");
    }
}
```

**Q: Touch không hoạt động?**
- Đảm bảo dots có Collider2D
- Check Camera projection mode
- Check Input System settings
