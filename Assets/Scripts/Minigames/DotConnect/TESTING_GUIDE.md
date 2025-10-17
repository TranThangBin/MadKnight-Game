# DotConnect - Quick Testing Guide

## 🧪 Cách Test Các Lỗi Đã Sửa

### Test 1: Không thể kéo qua dots khác ✅

**Mục đích**: Xác nhận người chơi KHÔNG thể kéo đường đi qua dots của cặp khác

**Bước test**:
1. Chạy game trong Unity (Play mode)
2. Chọn một dot bất kỳ để bắt đầu kéo
3. Cố tình kéo đường đi qua một dot khác (không phải dot đích của cặp đang kéo)

**Kết quả mong đợi**:
- ❌ Đường KHÔNG kéo qua dot khác được
- ❌ LineRenderer KHÔNG hiện ở vị trí dot khác
- ✅ Chỉ kéo qua các ô trống

**Visual check**:
```
Trước sửa:
🟡──🔴──🟡  ← Có thể kéo vàng qua đỏ (SAI!)

Sau sửa:
🟡  🔴  🟡  ← KHÔNG kéo được qua đỏ (ĐÚNG!)
```

---

### Test 2: Puzzle luôn có lời giải ✅

**Mục đích**: Xác nhận tất cả puzzle tạo ra đều giải được

**Bước test**:
1. Nhấn `R` nhiều lần để tạo puzzle mới (10-20 lần)
2. Với mỗi puzzle, thử giải hoàn toàn
3. Ghi lại số puzzle có thể giải được

**Kết quả mong đợi**:
- ✅ ≥ 95% puzzle có thể giải được
- ✅ Không có puzzle "deadlock" (không thể nối hết)

**Console check**:
```
Đã tạo puzzle khó sau 3 lần thử   ← Tốt
Timeout sau 100 lần thử...        ← Hiếm thấy, OK
Đã tạo puzzle đơn giản            ← Fallback, OK
```

---

### Test 3: Fallback hoạt động ✅

**Mục đích**: Xác nhận hệ thống fallback kích hoạt khi cần

**Bước test**:
1. Trong `DotConnectConfig`, set:
   - `boardWidth` = 3
   - `boardHeight` = 3
   - `numberOfDotPairs` = 5 (quá nhiều cho board nhỏ)
2. Chạy game

**Kết quả mong đợi**:
```
Console:
[Warning] Timeout sau 50 lần thử. Tạo puzzle đơn giản hơn...
[Log] Đã tạo puzzle đơn giản
hoặc
[Log] Đã tạo emergency puzzle với 2 cặp đơn giản
```

- ✅ Game KHÔNG bị đứng/crash
- ✅ Vẫn tạo được puzzle (dù đơn giản)

---

### Test 4: Board state không bị ghi đè ✅

**Mục đích**: Xác nhận dots không bị ghi đè khi hoàn thành path

**Bước test**:
1. Tạo puzzle mới
2. Nối một cặp dots thành công
3. Nhấn `D` (debug key) để xem board state

**Kết quả mong đợi**:
```
Console Debug Output:
Cell (0,0): -1   ← Dot vẫn giữ nguyên giá trị âm (ĐÚNG!)
Cell (1,0):  1   ← Path được ghi giá trị dương
Cell (2,0):  1
Cell (3,0): -1   ← Dot đích vẫn giữ nguyên (ĐÚNG!)
```

- ✅ Dots (negative values) KHÔNG bị ghi đè
- ✅ Chỉ path cells (0 → positive) được cập nhật

---

## 🎮 Debug Keys

Khi đang chơi trong Unity Editor:

- **`R`**: Reset puzzle (tạo puzzle mới)
- **`D`**: Debug LineRenderer info (xem console)
- **`L`**: List available shaders (xem console)

---

## 📊 Performance Check

### Thời gian tạo puzzle:

**Chạy test**:
```csharp
// Thêm vào Start() để test
float startTime = Time.realtimeSinceStartup;
InitializePuzzle();
float elapsedTime = Time.realtimeSinceStartup - startTime;
Debug.Log($"Tạo puzzle trong {elapsedTime:F3}s");
```

**Kết quả mong đợi**:
- ✅ Board 5x5, 5 cặp: < 1 giây
- ✅ Board 6x6, 6 cặp: < 2 giây
- ✅ Board 8x8, 8 cặp: < 4 giây
- ⚠️ Nếu > 5 giây → Timeout kick in → Fallback

---

## ✅ Checklist Tổng Hợp

Sau khi test, xác nhận:

- [ ] Không thể kéo qua dots khác
- [ ] Không thể kéo qua đường của cặp khác
- [ ] ≥ 95% puzzle có lời giải
- [ ] Fallback hoạt động khi board quá nhỏ
- [ ] Game không bị đứng/crash
- [ ] Dots không bị ghi đè
- [ ] Phím `R` reset được
- [ ] Tạo puzzle < 5 giây
- [ ] Console không có error màu đỏ

---

## 🐛 Nếu Phát Hiện Lỗi

**Bug: Vẫn kéo qua dots được**
→ Kiểm tra `CanMoveTo()` có đúng logic không
→ Log `cellValue` để debug

**Bug: Puzzle không giải được**
→ Tăng số lần shuffle trong `IsPuzzleSolvable()` từ 5 → 10
→ Giảm độ khó (MIN_TURNS_PER_PATH, MIN_PATH_LENGTH)

**Bug: Bị đứng máy**
→ Kiểm tra timeout có đang hoạt động không
→ Giảm `maxGenerationAttempts` trong config

---

**Last Updated**: 17/10/2025  
**Version**: 2.1 (with bugfixes)
