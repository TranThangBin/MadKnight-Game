using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace MadKnight.UI
{
    /// <summary>
    /// Save Game Panel - Cho phép lưu game sử dụng NSaveSystem
    /// </summary>
    public class SaveGamePanel : MonoBehaviour
    {
        [Header("Panel References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private GameObject panelRoot;
        
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI saveInfoText;
        
        [Header("Buttons")]
        [SerializeField] private Button saveButton;
        [SerializeField] private Button backButton;
        
        [Header("Animation")]
        [SerializeField] private float fadeSpeed = 5f;
        
        private bool isVisible = false;
        
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
            
            // Setup event listeners
            SetupEventListeners();
        }
        
        private void Start()
        {
            // Force ẩn panel khi start
            Hide();
        }
        
        private void SetupEventListeners()
        {
            // Save button
            if (saveButton != null)
            {
                saveButton.onClick.AddListener(OnSaveClick);
            }
            
            // Back button
            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBackClick);
            }
        }
        
        #region Show/Hide Panel
        
        public void Show()
        {
            if (isVisible)
            {
                Debug.Log("[SaveGamePanel] Already visible");
                return;
            }
            
            Debug.Log("[SaveGamePanel] Show() called");
            
            isVisible = true;
            
            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }
            
            // Refresh save info
            RefreshSaveInfo();
            
            StartCoroutine(FadeIn());
        }
        
        public void Hide()
        {
            if (!isVisible) return;
            
            Debug.Log("[SaveGamePanel] Hiding panel");
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
        
        #region Save Info
        
        private void RefreshSaveInfo()
        {
            if (saveInfoText == null) return;
            
            try
            {
                bool hasSave = NSaveSystem.HasSaveFile();
                
                if (hasSave)
                {
                    NSaveSystem.SaveData data = NSaveSystem.ReadSaveDataFromFile();
                    
                    string info = $"Existing Save:\n";
                    info += $"Scene: {data.CurrentScene}\n";
                    info += $"Saved: {data.SaveTime}\n";
                    info += $"Play Time: {FormatPlayTime(data.PlayTimeSeconds)}";
                    
                    saveInfoText.text = info;
                }
                else
                {
                    saveInfoText.text = "No existing save file";
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveGamePanel] Error refreshing save info: {e.Message}");
                saveInfoText.text = "Error loading save info";
            }
        }
        
        private string FormatPlayTime(float seconds)
        {
            int hours = Mathf.FloorToInt(seconds / 3600f);
            int minutes = Mathf.FloorToInt((seconds % 3600f) / 60f);
            int secs = Mathf.FloorToInt(seconds % 60f);
            
            if (hours > 0)
            {
                return $"{hours}h {minutes}m {secs}s";
            }
            else if (minutes > 0)
            {
                return $"{minutes}m {secs}s";
            }
            else
            {
                return $"{secs}s";
            }
        }
        
        #endregion
        
        #region Button Handlers
        
        private void OnSaveClick()
        {
            Debug.Log("[SaveGamePanel] Saving game...");
            
            try
            {
                NSaveSystem.Save();
                
                if (statusText != null)
                {
                    statusText.text = "Game saved successfully!";
                    statusText.color = Color.green;
                }
                
                // Refresh info to show new save data
                RefreshSaveInfo();
                
                // Auto hide after 1.5 seconds
                StartCoroutine(AutoHideAfterDelay(1.5f));
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveGamePanel] Error saving game: {e.Message}");
                
                if (statusText != null)
                {
                    statusText.text = "Failed to save game!";
                    statusText.color = Color.red;
                }
            }
        }
        
        private void OnBackClick()
        {
            Hide();
        }
        
        private IEnumerator AutoHideAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            Hide();
        }
        
        #endregion
    }
}
