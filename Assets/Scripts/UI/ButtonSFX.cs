using UnityEngine;
using UnityEngine.UI;

namespace MadKnight.UI
{
    /// <summary>
    /// Gắn vào Button để play SFX qua AudioManager
    /// Tự động phát SFX khi button được click
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ButtonSFX : MonoBehaviour
    {
        [Header("SFX Settings")]
        [SerializeField] private AudioClip clickSound;
        [SerializeField] private AudioClip hoverSound;
        [SerializeField] [Range(0f, 1f)] private float volumeScale = 1f;
        
        [Header("Fallback (nếu không có AudioManager)")]
        [SerializeField] private AudioSource fallbackAudioSource;
        
        private Button button;
        
        private void Awake()
        {
            button = GetComponent<Button>();
            
            // Add listener
            if (button != null)
            {
                button.onClick.AddListener(PlayClickSound);
            }
        }
        
        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(PlayClickSound);
            }
        }
        
        public void PlayClickSound()
        {
            if (clickSound == null) return;
            
            // Thử dùng AudioManager trước
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(clickSound, volumeScale);
            }
            // Fallback: dùng AudioSource riêng
            else if (fallbackAudioSource != null)
            {
                fallbackAudioSource.PlayOneShot(clickSound, volumeScale);
            }
            else
            {
                Debug.LogWarning($"[ButtonSFX] {gameObject.name}: Cannot play SFX - No AudioManager and no fallback AudioSource!");
            }
        }
        
        public void PlayHoverSound()
        {
            if (hoverSound == null) return;
            
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(hoverSound, volumeScale * 0.5f); // Hover nhỏ hơn click
            }
            else if (fallbackAudioSource != null)
            {
                fallbackAudioSource.PlayOneShot(hoverSound, volumeScale * 0.5f);
            }
        }
        
        /// <summary>
        /// Gọi từ EventTrigger cho Pointer Enter
        /// </summary>
        public void OnPointerEnter()
        {
            PlayHoverSound();
        }
    }
}
