using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using MadKnight.Save;
using UnityEngine.SceneManagement;

namespace MadKnight.UI
{
    /// <summary>
    /// Load Game Panel - Hiển thị save game sử dụng NSaveSystem
    /// </summary>
    public class LoadGamePanel : MonoBehaviour
    {
        [Header("Panel References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private GameObject panelRoot;
        
        [Header("Save Slot")]
        [SerializeField] private SaveSlotUI saveSlot;  // Chỉ có 1 save slot duy nhất
        
        [Header("Buttons")]
        [SerializeField] private Button backButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button deleteButton;
        
        [Header("Confirmation Dialog")]
        [SerializeField] private GameObject confirmDialog;
        [SerializeField] private TextMeshProUGUI confirmText;
        [SerializeField] private Button confirmYesButton;
        [SerializeField] private Button confirmNoButton;
        
        [Header("Animation")]
        [SerializeField] private float fadeSpeed = 5f;
        
        private bool isVisible = false;
        private bool hasSaveFile = false;
        
        private void Awake()
        {
            // Get or create CanvasGroup
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }
            
            // Initial state - hidden
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
            
            if (confirmDialog != null)
            {
                confirmDialog.SetActive(false);
            }
            
            // Setup event listeners
            SetupEventListeners();
        }
        
        private void Start()
        {
            // Force ẩn panel khi start (đảm bảo không đè lên main menu)
            Hide();
        }
        
        private void SetupEventListeners()
        {
            // Back button
            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBackClick);
            }
            
            // Load button
            if (loadButton != null)
            {
                loadButton.onClick.AddListener(OnLoadClick);
            }
            
            // Delete button
            if (deleteButton != null)
            {
                deleteButton.onClick.AddListener(OnDeleteClick);
            }
            
            // Confirmation buttons
            if (confirmYesButton != null)
            {
                confirmYesButton.onClick.AddListener(OnConfirmYes);
            }
            
            if (confirmNoButton != null)
            {
                confirmNoButton.onClick.AddListener(OnConfirmNo);
            }
            
            // Save slot click handlers
            if (saveSlot != null)
            {
                saveSlot.onLoadClicked += OnLoadClick;
                saveSlot.onDeleteClicked += OnDeleteClick;
            }
        }
        
        #region Show/Hide Panel
        
        public void Show()
        {
            if (isVisible)
            {
                Debug.Log("[LoadGamePanel] Already visible, skipping...");
                return;
            }
            
            Debug.Log("[LoadGamePanel] Show() called - Starting...");
            
            try
            {
                isVisible = true;
                
                if (panelRoot != null)
                {
                    Debug.Log("[LoadGamePanel] Activating panel root...");
                    panelRoot.SetActive(true);
                }
                else
                {
                    Debug.LogError("[LoadGamePanel] Panel Root is NULL!");
                }
                
                // Refresh save slot
                Debug.Log("[LoadGamePanel] Starting RefreshSaveSlot...");
                RefreshSaveSlot();
                Debug.Log("[LoadGamePanel] RefreshSaveSlot completed!");
                
                Debug.Log("[LoadGamePanel] Starting FadeIn coroutine...");
                StartCoroutine(FadeIn());
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[LoadGamePanel] CRITICAL ERROR in Show(): {e.Message}\n{e.StackTrace}");
            }
        }
        
        public void Hide()
        {
            if (!isVisible) return;
            
            Debug.Log("[LoadGamePanel] Hiding panel...");
            isVisible = false;
            StartCoroutine(FadeOut());
        }
        
        private IEnumerator FadeIn()
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = true;
            
            while (canvasGroup.alpha < 1f)
            {
                canvasGroup.alpha += Time.deltaTime * fadeSpeed;
                yield return null;
            }
            
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
        }
        
        private IEnumerator FadeOut()
        {
            canvasGroup.interactable = false;
            
            while (canvasGroup.alpha > 0f)
            {
                canvasGroup.alpha -= Time.deltaTime * fadeSpeed;
                yield return null;
            }
            
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }
        
        #endregion
        
        #region Refresh Slot
        
        private void RefreshSaveSlot()
        {
            try
            {
                Debug.Log("[LoadGamePanel] RefreshSaveSlot - Start");
                
                if (saveSlot == null)
                {
                    Debug.LogWarning("[LoadGamePanel] Save slot UI is null!");
                    return;
                }
                
                // Check if save file exists
                hasSaveFile = NSaveSystem.HasSaveFile();
                Debug.Log($"[LoadGamePanel] Has save file: {hasSaveFile}");
                
                if (hasSaveFile)
                {
                    // Đọc save data từ file (không load vào game)
                    NSaveSystem.SaveData data = NSaveSystem.ReadSaveDataFromFile();
                    
                    Debug.Log($"[LoadGamePanel] Save data loaded: {data.CurrentScene}, Time: {data.SaveTime}");
                    
                    // Update UI
                    saveSlot.SetSlotData(
                        slotNumber: 1,
                        isEmpty: false,
                        saveTime: data.SaveTime,
                        sceneName: data.CurrentScene,
                        playTime: data.PlayTimeSeconds,
                        level: 0,
                        difficulty: 0
                    );
                    
                    // Enable load/delete buttons
                    if (loadButton != null) loadButton.interactable = true;
                    if (deleteButton != null) deleteButton.interactable = true;
                }
                else
                {
                    Debug.Log("[LoadGamePanel] No save file found");
                    saveSlot.SetEmpty(1);
                    
                    // Disable load/delete buttons
                    if (loadButton != null) loadButton.interactable = false;
                    if (deleteButton != null) deleteButton.interactable = false;
                }
                
                Debug.Log("[LoadGamePanel] RefreshSaveSlot - Complete");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[LoadGamePanel] Error in RefreshSaveSlot: {e.Message}\n{e.StackTrace}");
            }
        }
        
        #endregion
        
        #region Load/Delete Actions
        
        private void OnLoadClick()
        {
            if (!hasSaveFile)
            {
                Debug.LogWarning("[LoadGamePanel] No save file to load!");
                return;
            }
            
            Debug.Log("[LoadGamePanel] Loading game...");
            
            try
            {
                // Đọc save data để lấy scene cần load
                NSaveSystem.SaveData data = NSaveSystem.ReadSaveDataFromFile();
                
                if (string.IsNullOrEmpty(data.CurrentScene))
                {
                    Debug.LogError("[LoadGamePanel] Save data không có thông tin scene!");
                    return;
                }
                
                // Set flag để GameManager tự động load save data khi khởi tạo
                NSaveSystem.SetLoadOnNextInit();
                
                Debug.Log($"[LoadGamePanel] Loading scene: {data.CurrentScene}");
                Hide();
                
                // Load scene - GameManager sẽ tự động apply save data trong Start()
                SceneManager.LoadScene(data.CurrentScene);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[LoadGamePanel] Lỗi khi load game: {e.Message}");
            }
        }
        
        private void OnDeleteClick()
        {
            if (!hasSaveFile)
            {
                Debug.LogWarning("[LoadGamePanel] No save file to delete!");
                return;
            }
            
            // Show confirmation dialog
            if (confirmDialog != null)
            {
                confirmDialog.SetActive(true);
                
                if (confirmText != null)
                {
                    confirmText.text = "Delete save file?\nThis action cannot be undone.";
                }
            }
        }
        
        private void OnConfirmYes()
        {
            Debug.Log("[LoadGamePanel] Deleting save file...");
            
            try
            {
                string saveFilePath = NSaveSystem.SaveFileName();
                if (System.IO.File.Exists(saveFilePath))
                {
                    System.IO.File.Delete(saveFilePath);
                    Debug.Log("[LoadGamePanel] Save file deleted successfully!");
                }
                
                // Refresh UI
                RefreshSaveSlot();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[LoadGamePanel] Error deleting save file: {e.Message}");
            }
            
            if (confirmDialog != null)
            {
                confirmDialog.SetActive(false);
            }
        }
        
        private void OnConfirmNo()
        {
            if (confirmDialog != null)
            {
                confirmDialog.SetActive(false);
            }
        }
        
        #endregion
        
        #region Button Handlers
        
        private void OnBackClick()
        {
            Hide();
        }
        
        #endregion
    }
}
