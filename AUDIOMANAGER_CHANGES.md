# AudioManager & MenuButton - Cập nhật

## Tổng quan thay đổi

### AudioManager.cs
AudioManager đã được refactor để quản lý âm thanh theo cách có tổ chức hơn:

#### 1. **GameObject References**
Thay vì tạo AudioSource trực tiếp, AudioManager giờ nhận 3 GameObject:
- `musicGameObject` - GameObject chứa AudioSource cho nhạc nền
- `sfxGameObject` - GameObject chứa AudioSource cho sound effects
- `ambienceGameObject` - GameObject chứa AudioSource cho âm thanh môi trường

#### 2. **Audio Clip Lists**
Quản lý các list âm thanh riêng biệt:
- `musicClips` - List các AudioClip cho nhạc nền
- `sfxClips` - List các AudioClip cho sound effects
- `ambienceClips` - List các AudioClip cho âm thanh môi trường

#### 3. **Tính năng mới**

##### Phát âm thanh theo index:
```csharp
// Phát music clip thứ 0 trong list
AudioManager.Instance.PlayMusic(0);

// Phát SFX clip thứ 2 trong list
AudioManager.Instance.PlaySFX(2);

// Phát ambience clip thứ 1 trong list
AudioManager.Instance.PlayAmbience(1);
```

##### Phát âm thanh từ AudioClip trực tiếp (vẫn giữ):
```csharp
AudioManager.Instance.PlayMusic(myAudioClip);
AudioManager.Instance.PlaySFX(myAudioClip);
AudioManager.Instance.PlayAmbience(myAudioClip);
```

##### Các phương thức điều khiển mới:
```csharp
// Music controls
AudioManager.Instance.PauseMusic();
AudioManager.Instance.ResumeMusic();
AudioManager.Instance.StopMusic();

// Ambience controls
AudioManager.Instance.PauseAmbience();
AudioManager.Instance.ResumeAmbience();
AudioManager.Instance.StopAmbience();
```

##### Getters:
```csharp
// Lấy clip từ list
AudioClip music = AudioManager.Instance.GetMusicClip(0);
AudioClip sfx = AudioManager.Instance.GetSFXClip(1);
AudioClip ambience = AudioManager.Instance.GetAmbienceClip(2);

// Lấy số lượng clips
int musicCount = AudioManager.Instance.GetMusicClipCount();
int sfxCount = AudioManager.Instance.GetSFXClipCount();
int ambienceCount = AudioManager.Instance.GetAmbienceClipCount();

// Check trạng thái
bool isPlaying = AudioManager.Instance.IsMusicPlaying();
bool isAmbiencePlaying = AudioManager.Instance.IsAmbiencePlaying();
```

#### 4. **Volume Control được cải thiện**
- Thêm volume control cho Ambience
- Tất cả volume đều được clamp từ 0-1
- Hỗ trợ cả AudioMixer và direct volume control
- Phương thức `ApplyVolumes()` để áp dụng tất cả volume settings khi khởi tạo

### MenuButton.cs

#### 1. **Tích hợp AudioManager**
- Thêm option `useAudioManager` (default = true)
- Khi `useAudioManager = true`: sử dụng AudioManager.Instance để phát sound
- Khi `useAudioManager = false`: sử dụng local AudioSource như trước

#### 2. **Cách hoạt động**
```csharp
// Trong OnPointerEnter và OnPointerClick:
if (useAudioManager && AudioManager.Instance != null)
{
    AudioManager.Instance.PlaySFX(hoverSound);
}
else if (audioSource != null)
{
    audioSource.PlayOneShot(hoverSound);
}
```

## Hướng dẫn Setup trong Unity

### 1. Setup AudioManager
1. Tạo 3 Empty GameObject trong scene:
   - `MusicSource`
   - `SFXSource`
   - `AmbienceSource`

2. Gán vào AudioManager Inspector:
   - Kéo `MusicSource` vào field `Music Game Object`
   - Kéo `SFXSource` vào field `SFX Game Object`
   - Kéo `AmbienceSource` vào field `Ambience Game Object`

3. Thêm AudioClips vào lists:
   - Kéo các music clips vào `Music Clips` list
   - Kéo các SFX clips vào `SFX Clips` list
   - Kéo các ambience clips vào `Ambience Clips` list

4. (Optional) Gán AudioMixer nếu có

### 2. Setup MenuButton
1. Đảm bảo `Use Audio Manager` = true (mặc định)
2. Gán hover sound và click sound như bình thường
3. Button sẽ tự động sử dụng AudioManager để phát sound

### 3. Sử dụng trong Code

```csharp
using MadKnight.UI;

// Phát nhạc nền
AudioManager.Instance.PlayMusic(0, loop: true);

// Phát sound effect
AudioManager.Instance.PlaySFX(2, volumeScale: 0.5f);

// Phát âm thanh môi trường
AudioManager.Instance.PlayAmbience(1, loop: true);

// Điều chỉnh volume
AudioManager.Instance.SetMasterVolume(0.8f);
AudioManager.Instance.SetMusicVolume(0.7f);
AudioManager.Instance.SetSFXVolume(0.9f);
AudioManager.Instance.SetAmbienceVolume(0.6f);

// Tạm dừng và tiếp tục
AudioManager.Instance.PauseMusic();
AudioManager.Instance.ResumeMusic();
```

## Lợi ích

1. **Tổ chức tốt hơn**: Tất cả audio clips được quản lý tập trung
2. **Dễ bảo trì**: Thay đổi clips trong Inspector thay vì code
3. **Linh hoạt**: Có thể phát bằng index hoặc AudioClip trực tiếp
4. **Tích hợp tốt**: MenuButton tự động sử dụng AudioManager
5. **Tương thích ngược**: Vẫn support phát AudioClip trực tiếp
6. **Volume control đầy đủ**: Hỗ trợ cả 4 loại volume (Master, Music, SFX, Ambience)

## Breaking Changes

Không có breaking changes. Tất cả code cũ vẫn hoạt động bình thường vì các phương thức cũ vẫn được giữ lại.
