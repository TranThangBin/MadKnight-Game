using UnityEngine;

namespace MadKnight.UI
{
    /// <summary>
    /// Simple test để kiểm tra volume control có hoạt động không
    /// Gắn vào GameObject bất kỳ, chạy test bằng phím tắt
    /// </summary>
    public class AudioVolumeTest : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private AudioClip testClip;
        [SerializeField] private KeyCode playKey = KeyCode.P;
        [SerializeField] private KeyCode increaseVolumeKey = KeyCode.UpArrow;
        [SerializeField] private KeyCode decreaseVolumeKey = KeyCode.DownArrow;
        [SerializeField] private KeyCode stopKey = KeyCode.S;
        
        private float currentVolume = 0.7f;
        
        private void Start()
        {
            Debug.Log("=== AUDIO VOLUME TEST ===");
            Debug.Log("P = Play test music");
            Debug.Log("↑ = Increase volume (+0.1)");
            Debug.Log("↓ = Decrease volume (-0.1)");
            Debug.Log("S = Stop music");
            Debug.Log("========================");
            
            // Check AudioManager
            if (AudioManager.Instance == null)
            {
                Debug.LogError("[AudioVolumeTest] ❌ AudioManager.Instance is NULL!");
                Debug.LogError("[AudioVolumeTest] Tạo AudioManager trước khi test!");
            }
            else
            {
                Debug.Log("[AudioVolumeTest] ✅ AudioManager.Instance found");
            }
        }
        
        private void Update()
        {
            if (AudioManager.Instance == null) return;
            
            // Play test music
            if (Input.GetKeyDown(playKey))
            {
                if (testClip != null)
                {
                    Debug.Log($"[AudioVolumeTest] Playing test clip: {testClip.name}");
                    AudioManager.Instance.PlayMusic(testClip, true);
                    AudioManager.Instance.SetMusicVolume(currentVolume);
                }
                else
                {
                    Debug.LogWarning("[AudioVolumeTest] Test clip is not assigned!");
                }
            }
            
            // Increase volume
            if (Input.GetKeyDown(increaseVolumeKey))
            {
                currentVolume = Mathf.Clamp01(currentVolume + 0.1f);
                Debug.Log($"[AudioVolumeTest] 🔊 Increasing volume to {currentVolume:F2}");
                AudioManager.Instance.SetMusicVolume(currentVolume);
            }
            
            // Decrease volume
            if (Input.GetKeyDown(decreaseVolumeKey))
            {
                currentVolume = Mathf.Clamp01(currentVolume - 0.1f);
                Debug.Log($"[AudioVolumeTest] 🔉 Decreasing volume to {currentVolume:F2}");
                AudioManager.Instance.SetMusicVolume(currentVolume);
            }
            
            // Stop music
            if (Input.GetKeyDown(stopKey))
            {
                Debug.Log("[AudioVolumeTest] ⏹ Stopping music");
                AudioManager.Instance.StopMusic();
            }
        }
        
        [ContextMenu("Test Volume Control")]
        private void TestVolumeControl()
        {
            if (AudioManager.Instance == null)
            {
                Debug.LogError("[AudioVolumeTest] AudioManager.Instance is NULL!");
                return;
            }
            
            Debug.Log("=== Testing Volume Control ===");
            
            // Test volume từ 0 -> 1
            for (float v = 0f; v <= 1f; v += 0.2f)
            {
                AudioManager.Instance.SetMusicVolume(v);
                Debug.Log($"Set volume to {v:F1}");
            }
            
            Debug.Log("=== Test Complete ===");
        }
        
        [ContextMenu("Check AudioManager State")]
        private void CheckAudioManagerState()
        {
            if (AudioManager.Instance == null)
            {
                Debug.LogError("❌ AudioManager.Instance is NULL!");
                return;
            }
            
            Debug.Log("=== AudioManager State ===");
            Debug.Log($"Instance exists: ✅");
            
            // Reflection để check private fields
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            var type = AudioManager.Instance.GetType();
            
            var musicSourceField = type.GetField("musicSource", flags);
            if (musicSourceField != null)
            {
                var musicSource = musicSourceField.GetValue(AudioManager.Instance) as AudioSource;
                if (musicSource != null)
                {
                    Debug.Log($"musicSource: ✅");
                    Debug.Log($"  - clip: {musicSource.clip?.name ?? "none"}");
                    Debug.Log($"  - volume: {musicSource.volume:F2}");
                    Debug.Log($"  - isPlaying: {musicSource.isPlaying}");
                    Debug.Log($"  - loop: {musicSource.loop}");
                }
                else
                {
                    Debug.LogError("musicSource is NULL!");
                }
            }
            
            var mixerField = type.GetField("audioMixer", flags);
            if (mixerField != null)
            {
                var mixer = mixerField.GetValue(AudioManager.Instance);
                if (mixer != null)
                {
                    Debug.Log($"audioMixer: ✅ {mixer}");
                }
                else
                {
                    Debug.LogWarning("audioMixer: Not assigned (will use direct volume control)");
                }
            }
            
            Debug.Log("==========================");
        }
    }
}
