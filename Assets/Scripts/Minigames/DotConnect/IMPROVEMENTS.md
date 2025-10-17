# DotConnect Minigame - Cải Tiến

## 📋 Tổng Quan

File này mô tả các cải tiến đã được thực hiện cho DotConnect minigame để khắc phục lỗi đứng máy và tăng độ khó.

---

## 🛡️ Fallback Mechanism (Cơ chế dự phòng)

### Vấn đề trước đây:
- Game đôi khi bị đứng (freeze) khi tạo puzzle
- Unity không load được gì khi bị deadlock
- Không có cơ chế recovery

### Giải pháp đã thực hiện:

#### 1. **Timeout Protection**
```csharp
private const float GENERATION_TIMEOUT = 5.0f;
```
- Giới hạn thời gian tạo puzzle tối đa 5 giây
- Tự động chuyển sang fallback nếu quá thời gian

#### 2. **Deadlock Detection**
- Kiểm tra trong `Update()` nếu quá trình tạo puzzle bị kẹt
- Tự động force stop và tạo emergency puzzle

#### 3. **Multiple Fallback Levels**
1. **Level 1**: Thử tạo puzzle khó (complex algorithm)
2. **Level 2**: Tạo puzzle đơn giản (simple algorithm) 
3. **Level 3**: Tạo puzzle cực kỳ đơn giản (emergency puzzle - 2 cặp)
4. **Level 4**: Tạo puzzle tối thiểu (minimal puzzle - 1 cặp ở 2 góc)

#### 4. **Failed Attempts Counter**
```csharp
private int failedAttempts = 0;
private const int MAX_FAILED_ATTEMPTS = 3;
```
- Đếm số lần tạo puzzle thất bại
- Sau 3 lần, tự động chuyển sang emergency mode

#### 5. **Try-Catch Protection**
- Bọc tất cả logic tạo puzzle trong try-catch
- Đảm bảo game không bao giờ crash hoàn toàn

#### 6. **Debug Commands**
- Nhấn `R`: Force reset puzzle
- Nhấn `D`: Debug LineRenderer info
- Nhấn `L`: List available shaders

---

## 🎯 Thuật Toán Khó Hơn

### Vấn đề trước đây:
- Đường đi quá đơn giản, thường là đường thẳng
- Ít rẽ, dễ dàng giải
- Không đủ thách thức cho người chơi

### Cải tiến đã thực hiện:

#### 1. **Yêu Cầu Độ Khó Mới**
```csharp
private const int MIN_TURNS_PER_PATH = 3;      // Tối thiểu 3 lượt rẽ
private const float PREFER_TURN_PROBABILITY = 0.7f;  // 70% ưu tiên rẽ
private const int MIN_PATH_LENGTH = 5;         // Độ dài tối thiểu 5 ô
```

#### 2. **Smart Path Generation**
- **Ưu tiên rẽ**: 70% khả năng rẽ thay vì đi thẳng
- **Giới hạn đường thẳng**: Tối đa 3 ô liên tiếp cùng hướng
- **Đếm số lượt rẽ**: Theo dõi và đảm bảo đủ số lượt rẽ

#### 3. **Tiêu Chí Kiểm Tra Độ Khó Nghiêm Ngặt**
Một đường đi được coi là "đủ khó" khi thỏa mãn TẤT CẢ:
- ✅ Ít nhất 3 lượt rẽ
- ✅ Không có đoạn thẳng dài hơn 3 ô
- ✅ Tỷ lệ rẽ/tổng độ dài ≥ 30%

```csharp
bool hasEnoughTurns = turns >= MIN_TURNS_PER_PATH;
bool noLongStraight = maxStraightSegment <= 3;
bool goodTurnRatio = (float)turns / path.Count >= 0.3f;
```

#### 4. **Consecutive Failures Handling**
- Nếu tạo đường khó thất bại 3 lần liên tiếp → bỏ qua puzzle này
- Tránh infinite loop khi tìm đường đi

#### 5. **Timeout trong Validation**
```csharp
// Timeout 1 giây cho việc kiểm tra solvable
if (Time.realtimeSinceStartup - solveStartTime > 1.0f)
{
    return null;
}
```

#### 6. **Depth Limit cho Backtracking**
```csharp
int maxDepth = dotPairs.Count * 10;
```
- Giới hạn độ sâu recursion để tránh stack overflow

---

## 📊 So Sánh Trước & Sau

### Trước:
- ❌ Đường đi thẳng: phổ biến
- ❌ Số lượt rẽ: 0-2
- ❌ Độ dài: 2-4 ô
- ❌ Khả năng đứng máy: cao
- ❌ Recovery: không có

### Sau:
- ✅ Đường đi thẳng: hạn chế (max 3 ô)
- ✅ Số lượt rẽ: tối thiểu 3
- ✅ Độ dài: tối thiểu 5 ô
- ✅ Khả năng đứng máy: gần như không
- ✅ Recovery: 4 cấp độ fallback

---

## 🎮 Hướng Dẫn Sử Dụng

### Khi Puzzle Bị Đứng:
1. Chờ 5 giây - hệ thống sẽ tự động fallback
2. Hoặc nhấn `R` để force reset
3. Game sẽ tự động tạo puzzle đơn giản hơn

### Điều Chỉnh Độ Khó:
Trong `DotConnectGenerator.cs`, thay đổi các constant:
```csharp
private const int MIN_TURNS_PER_PATH = 3;      // Tăng để khó hơn
private const float PREFER_TURN_PROBABILITY = 0.7f;  // Tăng để rẽ nhiều hơn
private const int MIN_PATH_LENGTH = 5;         // Tăng để đường dài hơn
```

---

## 🔧 Technical Details

### Architecture:
```
DotConnectManager (Main Controller)
    ├── Fallback Mechanism
    │   ├── Timeout Detection
    │   ├── Deadlock Detection
    │   └── Multi-level Recovery
    │
    └── Generator (DotConnectGenerator)
        ├── Complex Algorithm (Level 1)
        ├── Simple Algorithm (Level 2)
        ├── Emergency Algorithm (Level 3)
        └── Minimal Algorithm (Level 4)
```

### Performance:
- Timeout tạo puzzle: 5 giây
- Timeout kiểm tra solvable: 1 giây
- Max attempts: 100 (configurable)
- Memory: Tối ưu, không leak

---

## 📝 Notes

1. **Không nên tắt timeout** - đây là cơ chế bảo vệ quan trọng
2. **Nếu muốn dễ hơn**: Giảm `MIN_TURNS_PER_PATH` và `MIN_PATH_LENGTH`
3. **Nếu muốn khó hơn**: Tăng các constant và `PREFER_TURN_PROBABILITY`
4. **Debug mode**: Luôn bật để theo dõi quá trình tạo puzzle

---

## 🐛 Troubleshooting

### Vẫn bị đứng?
- Kiểm tra console có lỗi không
- Thử giảm `numberOfDotPairs` trong config
- Tăng kích thước board

### Quá dễ?
- Tăng `MIN_TURNS_PER_PATH` lên 4-5
- Tăng `PREFER_TURN_PROBABILITY` lên 0.8-0.9
- Tăng `MIN_PATH_LENGTH` lên 7-10

### Quá khó?
- Giảm các constant xuống
- Tăng `maxGenerationAttempts` trong config

---

**Phiên bản**: 2.0  
**Ngày cập nhật**: 17/10/2025  
**Tác giả**: GitHub Copilot
