using UnityEngine;
using MiniGames.DotConnect;

namespace MiniGames.DotConnect.Examples
{
    /// <summary>
    /// Ví dụ về cách sử dụng DotConnectManager trong game
    /// </summary>
    public class DotConnectExample : MonoBehaviour
    {
        [SerializeField] private DotConnectManager dotConnectManager;
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private AudioClip pairConnectedSound;
        [SerializeField] private AudioClip puzzleCompleteSound;
        
        private AudioSource audioSource;
        private int completedPairs = 0;
        
        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        private void Start()
        {
            if (dotConnectManager == null)
            {
                Debug.LogError("DotConnectManager chưa được gán!");
                return;
            }
            
            // Đăng ký events
            dotConnectManager.onPuzzleCompleted.AddListener(OnPuzzleCompleted);
            dotConnectManager.onDotPairConnected.AddListener(OnDotPairConnected);
            dotConnectManager.onPuzzleStarted.AddListener(OnPuzzleStarted);
            
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(false);
            }
        }
        
        private void OnDestroy()
        {
            if (dotConnectManager != null)
            {
                dotConnectManager.onPuzzleCompleted.RemoveListener(OnPuzzleCompleted);
                dotConnectManager.onDotPairConnected.RemoveListener(OnDotPairConnected);
                dotConnectManager.onPuzzleStarted.RemoveListener(OnPuzzleStarted);
            }
        }
        
        private void OnPuzzleStarted()
        {
            Debug.Log("🎮 Puzzle mới đã bắt đầu!");
            completedPairs = 0;
            
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(false);
            }
        }
        
        private void OnDotPairConnected(int pairId, Color color)
        {
            completedPairs++;
            Debug.Log($"✅ Đã nối cặp {pairId} - Màu: {ColorToHex(color)} ({completedPairs} cặp)");
            
            // Phát sound effect
            PlaySound(pairConnectedSound);
            
            // Có thể thêm particle effect, animation, etc.
        }
        
        private void OnPuzzleCompleted()
        {
            Debug.Log("🎉 HOÀN THÀNH TẤT CẢ PUZZLE!");
            
            // Phát sound effect
            PlaySound(puzzleCompleteSound);
            
            // Hiển thị victory panel
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(true);
            }
            
            // Có thể thêm:
            // - Mở khóa level tiếp theo
            // - Lưu progress
            // - Tặng reward
            // - Hiển thị score/time
            
            // Auto tạo puzzle mới sau 3 giây
            Invoke(nameof(LoadNextPuzzle), 3f);
        }
        
        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
        
        private void LoadNextPuzzle()
        {
            dotConnectManager.NewPuzzle();
        }
        
        // UI Button callbacks
        public void OnResetButtonClicked()
        {
            dotConnectManager.ResetPuzzle();
            completedPairs = 0;
            Debug.Log("🔄 Đã reset puzzle");
        }
        
        public void OnNewPuzzleButtonClicked()
        {
            dotConnectManager.NewPuzzle();
            completedPairs = 0;
            Debug.Log("🆕 Tạo puzzle mới");
        }
        
        private string ColorToHex(Color color)
        {
            return $"#{ColorUtility.ToHtmlStringRGB(color)}";
        }
    }
}
