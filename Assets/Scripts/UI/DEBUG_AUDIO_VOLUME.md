# 🔍 DEBUG CHECKLIST - Audio Volume Không Thay Đổi

## ⚠️ VẤN ĐỀ
Kéo slider volume trong Settings Panel nhưng âm lượng nhạc không thay đổi.

---

## 📋 CHECKLIST DEBUG (Làm theo thứ tự)

### ✅ Bước 1: Kiểm tra Console Logs

Chạy game, mở Settings Panel, kéo Music Volume slider, xem Console có các log này không:

```
[SettingsPanel] OnMusicVolumeChanged: 0.XX
[SettingsPanel] Calling AudioManager.SetMusicVolume(0.XX)
[AudioManager] SetMusicVolume called: 0.XX
[AudioManager] Set musicSource.volume = 0.XX (clip: <tên clip>, playing: true)
```

#### Nếu KHÔNG thấy log:
- ❌ Slider chưa được gán callback
- **FIX:** 
  1. Chọn Music Volume Slider trong Hierarchy
  2. Inspector → Scroll xuống "On Value Changed (Single)"
  3. Click "+" để add listener
  4. Kéo GameObject có SettingsPanel vào ô Object
  5. Chọn function: `SettingsPanel > OnMusicVolumeChanged`

#### Nếu thấy log nhưng hiển thị:
```
[SettingsPanel] AudioManager.Instance is NULL!
```
- ❌ AudioManager chưa được tạo
- **FIX:** Đọc Bước 2

---

### ✅ Bước 2: Kiểm tra AudioManager được tạo

Khi Start game, trong Console phải có:
```
[MainMenuUI] Created AudioManager instance
[AudioManager] Instance created
[AudioManager] Created musicSource component
[AudioManager] Created sfxSource component
[AudioManager] Created ambienceSource component
[AudioManager] AudioMixer is not assigned! Will use direct AudioSource.volume control.
```

#### Nếu KHÔNG thấy log AudioManager:
- ❌ MainMenuUI.EnsureAudioManager() không chạy
- **FIX:** Kiểm tra MainMenuUI có được enable không

#### Kiểm tra trong Hierarchy:
- Tìm GameObject có tên "AudioManager" (DontDestroyOnLoad)
- Nếu không có → AudioManager chưa được tạo

---

### ✅ Bước 3: Kiểm tra nhạc đang phát từ đâu

Trong Console khi game start, tìm:
```
[AudioManager] Playing music: <tên clip>, loop=true, volume=0.XX
```

#### Nếu KHÔNG thấy log này:
- ❌ Nhạc không được phát qua AudioManager
- ❌ Có thể nhạc đang phát từ AudioSource cũ trong scene

**FIX:**
1. Pause game
2. Hierarchy → Tìm tất cả AudioSource đang playing
3. Xem AudioSource nào đang phát nhạc
4. Nếu AudioSource đó KHÔNG nằm trong GameObject "AudioManager" → Đây là vấn đề!

---

### ✅ Bước 4: Kiểm tra trong MainMenuUI Inspector

Chọn GameObject có MainMenuUI component:

#### Phải có:
- ✅ **Intro Music Clip** → Gán AudioClip (không phải AudioSource!)
- ✅ **Background Music Clip** → Gán AudioClip (không phải AudioSource!)

#### KHÔNG được có (đã xóa):
- ❌ ~~Intro Music (AudioSource)~~
- ❌ ~~Background Music (AudioSource)~~

#### Nếu vẫn còn field AudioSource cũ:
**FIX:**
1. Xóa component MainMenuUI
2. Add lại component MainMenuUI (script đã được update)
3. Gán lại các references

---

### ✅ Bước 5: Kiểm tra AudioMixer Setup

Trong Console, nếu thấy:
```
[AudioManager] AudioMixer is not assigned! Will use direct AudioSource.volume control.
```

→ Đây là BÌNH THƯỜNG nếu bạn chưa setup AudioMixer.

AudioManager sẽ dùng `AudioSource.volume` trực tiếp.

#### Nếu muốn dùng AudioMixer:
1. Project → Create → Audio → Audio Mixer
2. Tạo các Exposed Parameters: MasterVolume, MusicVolume, SFXVolume, AmbienceVolume
3. Gán AudioMixer vào AudioManager component

---

### ✅ Bước 6: Test trực tiếp trong Inspector

1. Chạy game
2. Hierarchy → Chọn GameObject "AudioManager"
3. Inspector → Tìm component "Audio Source"
4. Thay đổi "Volume" bằng tay trong Inspector
5. Nhạc có thay đổi không?

#### Nếu thay đổi Volume trong Inspector → Nhạc vẫn không thay đổi:
- ❌ AudioSource này không phải đang phát nhạc
- ❌ Có AudioSource khác đang phát nhạc

**FIX:**
1. Pause game
2. Window → Audio → Audio Mixer (nếu có)
3. Hoặc tìm tất cả AudioSource trong scene
4. Xác định AudioSource nào đang phát nhạc thật sự

---

### ✅ Bước 7: Kiểm tra Scene Setup

Trong MainMenu scene:

1. **Có GameObject với MainMenuUI?** ✅
2. **MainMenuUI.settingsPanel được gán?** ✅
3. **SettingsPanel.musicVolumeSlider được gán?** ✅
4. **Slider có Event "On Value Changed"?** ✅

---

### ✅ Bước 8: Test với SettingsDebugger

1. Tạo Empty GameObject trong scene
2. Add Component → Settings Debugger
3. Chạy game
4. Bấm **F4** → Xem settings trong Console
5. Bấm **F1** → Save random volume
6. Nhạc có thay đổi không?

---

## 🎯 CÁC TRƯỜNG HỢP PHỔ BIẾN

### Case 1: Callback không được gọi
**Triệu chứng:** Không thấy log `[SettingsPanel] OnMusicVolumeChanged` khi kéo slider

**Nguyên nhân:** Slider chưa gán callback

**Fix:** Gán callback trong Inspector (xem Bước 1)

---

### Case 2: AudioManager.Instance is NULL
**Triệu chứng:** Log hiển thị `AudioManager.Instance is NULL!`

**Nguyên nhân:** AudioManager chưa được tạo hoặc bị destroy

**Fix:** 
- Kiểm tra MainMenuUI.Start() có chạy `EnsureAudioManager()`
- Kiểm tra không có script nào destroy AudioManager

---

### Case 3: Nhạc phát từ AudioSource cũ
**Triệu chứng:** 
- SetMusicVolume() được gọi
- musicSource.volume thay đổi
- Nhưng nhạc vẫn không thay đổi

**Nguyên nhân:** Nhạc đang phát từ AudioSource khác trong scene (AudioSource cũ chưa xóa)

**Fix:**
1. Tìm tất cả AudioSource trong MainMenu scene
2. Xóa AudioSource cũ (không thuộc AudioManager)
3. Đảm bảo chỉ có AudioManager phát nhạc

---

### Case 4: AudioMixer đè lên AudioSource.volume
**Triệu chứng:** 
- SetMusicVolume() set audioMixer.SetFloat()
- Nhưng volume không thay đổi

**Nguyên nhân:** AudioMixer parameter không đúng tên hoặc chưa expose

**Fix:**
1. Mở AudioMixer
2. Kiểm tra parameter "MusicVolume" đã expose chưa
3. Kiểm tra AudioSource có routing qua Mixer Group không

---

## 📊 EXPECTED OUTPUT

Khi mọi thứ hoạt động đúng, Console sẽ hiện:

```
[MainMenuUI] Created AudioManager instance
[AudioManager] Instance created
[AudioManager] Created musicSource component
[AudioManager] AudioMixer is not assigned! Will use direct AudioSource.volume control.
[AudioManager] Playing music: BackgroundMusic, loop=true, volume=0.70

// Khi kéo slider:
[SettingsPanel] OnMusicVolumeChanged: 0.50
[SettingsPanel] Calling AudioManager.SetMusicVolume(0.50)
[AudioManager] SetMusicVolume called: 0.50
[AudioManager] Set musicSource.volume = 0.50 (clip: BackgroundMusic, playing: true)
```

---

## 🛠️ QUICK FIX

Nếu tất cả đều fail, thử cách này:

1. **Xóa toàn bộ MainMenuUI và SettingsPanel khỏi scene**
2. **Tạo lại từ đầu:**
   - Create Empty GameObject → Add MainMenuUI
   - Create UI Panel → Add SettingsPanel
   - Gán lại tất cả references
3. **Chạy test**

---

## 📞 LIÊN HỆ DEBUG

Nếu vẫn không hoạt động, cung cấp:
1. Screenshot Hierarchy (scene đang chạy)
2. Screenshot AudioManager Inspector (khi game chạy)
3. Screenshot MainMenuUI Inspector
4. Copy/paste toàn bộ Console log

**Tôi sẽ giúp debug cụ thể hơn!**
