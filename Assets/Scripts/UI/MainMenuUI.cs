using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using MadKnight.Save;

namespace MadKnight.UI
{
    /// <summary>
    /// Quản lý UI của Main Menu với hiệu ứng fade in
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Menu Components")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI subtitleText;
        [SerializeField] private CanvasGroup menuButtonsGroup;
        [SerializeField] private SettingsPanel settingsPanel;
        [SerializeField] private LoadGamePanel loadGamePanel;
        [SerializeField] private CanvasGroup creditsPanel;
        
        [Header("Menu Buttons")]
        [SerializeField] private MenuButton newGameButton;
        [SerializeField] private MenuButton continueButton;
        [SerializeField] private MenuButton settingsButton;
        [SerializeField] private MenuButton creditsButton;
        [SerializeField] private MenuButton exitButton;
        
        [Header("Animation Settings")]
        [SerializeField] private float titleFadeInDuration = 2f;
        [SerializeField] private float subtitleFadeInDuration = 1.5f;
        [SerializeField] private float buttonsFadeInDuration = 1f;
        [SerializeField] private float delayBetweenElements = 0.5f;
        
        [Header("Background Music")]
        [SerializeField] private AudioClip introMusicClip;  // Clip phát 1 lần đầu tiên
        [SerializeField] private AudioClip backgroundMusicClip;  // Clip loop sau khi intro hết
        [SerializeField] private float musicFadeInDuration = 3f;
        [SerializeField] private float crossfadeDuration = 2f;  // Thời gian chuyển từ intro sang loop
        
        private AudioSource introMusicSource;
        private AudioSource backgroundMusicSource;
        
        private void Start()
        {
            // Khởi tạo AudioManager nếu chưa có
            EnsureAudioManager();
            
            // Khởi tạo alpha = 0 cho tất cả
            SetAlpha(titleText, 0);
            SetAlpha(subtitleText, 0);
            if (menuButtonsGroup != null) menuButtonsGroup.alpha = 0;
            if (creditsPanel != null) creditsPanel.alpha = 0;
            
            // Kiểm tra save file và disable Continue button nếu cần
            CheckSaveFileStatus();
            
            // Bắt đầu animation fade in
            StartCoroutine(FadeInSequence());
            
            // Fade in music với intro -> background loop
            StartCoroutine(PlayMusicSequence());
        }
        
        /// <summary>
        /// Đảm bảo có AudioManager trong scene
        /// </summary>
        private void EnsureAudioManager()
        {
            if (AudioManager.Instance == null)
            {
                Debug.Log("[MainMenuUI] AudioManager not found, creating new instance...");
                GameObject audioManagerGO = new GameObject("AudioManager");
                audioManagerGO.AddComponent<AudioManager>();
                Debug.Log("[MainMenuUI] Created AudioManager instance");
            }
            else
            {
                Debug.Log("[MainMenuUI] AudioManager instance already exists");
            }
            
            // Load và apply settings
            GameSettings settings = GameSettings.Load();
            Debug.Log($"[MainMenuUI] Loaded settings - Music Volume: {settings.musicVolume:F2}");
            settings.ApplyAll();
            Debug.Log("[MainMenuUI] Applied all settings");
        }
        
        private void CheckSaveFileStatus()
        {
            bool hasSaveFile = SaveSystem.HasAnySaveFile();
            
            Debug.Log($"[MainMenuUI] Checking save file status: {hasSaveFile}");
            
            if (continueButton != null)
            {
                continueButton.SetInteractable(hasSaveFile);
            }
        }
        
        private IEnumerator FadeInSequence()
        {
            // Fade in Title
            yield return StartCoroutine(FadeInText(titleText, titleFadeInDuration));
            
            yield return new WaitForSeconds(delayBetweenElements);
            
            // Fade in Subtitle
            yield return StartCoroutine(FadeInText(subtitleText, subtitleFadeInDuration));
            
            yield return new WaitForSeconds(delayBetweenElements);
            
            // Fade in Buttons
            if (menuButtonsGroup != null)
            {
                yield return StartCoroutine(FadeInCanvasGroup(menuButtonsGroup, buttonsFadeInDuration));
            }
        }
        
        private IEnumerator FadeInText(TextMeshProUGUI text, float duration)
        {
            if (text == null) yield break;
            
            float elapsed = 0f;
            Color color = text.color;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
                color.a = alpha;
                text.color = color;
                yield return null;
            }
            
            color.a = 1f;
            text.color = color;
        }
        
        private IEnumerator FadeInCanvasGroup(CanvasGroup group, float duration)
        {
            if (group == null) yield break;
            
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                group.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
                yield return null;
            }
            
            group.alpha = 1f;
        }
        
        private IEnumerator FadeInMusic()
        {
            if (AudioManager.Instance == null || backgroundMusicClip == null) yield break;
            
            float elapsed = 0f;
            float targetVolume = GameSettings.Load().musicVolume;
            
            // Phát music qua AudioManager
            AudioManager.Instance.PlayMusic(backgroundMusicClip, true, 0f);
            
            while (elapsed < musicFadeInDuration)
            {
                elapsed += Time.deltaTime;
                float volume = Mathf.Lerp(0f, targetVolume, elapsed / musicFadeInDuration);
                AudioManager.Instance.SetMusicVolume(volume);
                yield return null;
            }
            
            AudioManager.Instance.SetMusicVolume(targetVolume);
        }
        
        /// <summary>
        /// Phát music theo trình tự: Intro (1 lần) -> Background (loop)
        /// </summary>
        private IEnumerator PlayMusicSequence()
        {
            if (AudioManager.Instance == null)
            {
                Debug.LogWarning("[MainMenuUI] AudioManager not found!");
                yield break;
            }
            
            GameSettings settings = GameSettings.Load();
            float targetVolume = settings.musicVolume;
            
            // Nếu không có intro, chỉ phát background loop
            if (introMusicClip == null)
            {
                if (backgroundMusicClip != null)
                {
                    yield return StartCoroutine(FadeInMusic());
                }
                yield break;
            }
            
            // Tạo AudioSource tạm cho intro
            GameObject tempGO = new GameObject("IntroMusic");
            introMusicSource = tempGO.AddComponent<AudioSource>();
            introMusicSource.clip = introMusicClip;
            introMusicSource.loop = false;
            introMusicSource.volume = 0f;
            introMusicSource.Play();
            
            // Fade in intro
            float elapsed = 0f;
            while (elapsed < musicFadeInDuration)
            {
                elapsed += Time.deltaTime;
                introMusicSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / musicFadeInDuration);
                yield return null;
            }
            introMusicSource.volume = targetVolume;
            
            // Chờ intro gần hết (trừ đi thời gian crossfade)
            float introDuration = introMusicClip.length;
            float waitTime = introDuration - crossfadeDuration;
            
            if (waitTime > 0)
            {
                yield return new WaitForSeconds(waitTime);
            }
            
            // Crossfade: Intro fade out, Background fade in
            if (backgroundMusicClip != null)
            {
                // Bắt đầu phát background
                AudioManager.Instance.PlayMusic(backgroundMusicClip, true, 0f);
                
                elapsed = 0f;
                while (elapsed < crossfadeDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / crossfadeDuration;
                    
                    // Fade out intro
                    if (introMusicSource != null)
                    {
                        introMusicSource.volume = Mathf.Lerp(targetVolume, 0f, t);
                    }
                    
                    // Fade in background
                    float bgVolume = Mathf.Lerp(0f, targetVolume, t);
                    AudioManager.Instance.SetMusicVolume(bgVolume);
                    
                    yield return null;
                }
                
                // Hoàn tất
                if (introMusicSource != null)
                {
                    introMusicSource.Stop();
                    Destroy(introMusicSource.gameObject);
                }
                AudioManager.Instance.SetMusicVolume(targetVolume);
            }
            else
            {
                // Không có background music, chỉ fade out intro
                elapsed = 0f;
                while (elapsed < crossfadeDuration)
                {
                    elapsed += Time.deltaTime;
                    if (introMusicSource != null)
                    {
                        introMusicSource.volume = Mathf.Lerp(targetVolume, 0f, elapsed / crossfadeDuration);
                    }
                    yield return null;
                }
                
                if (introMusicSource != null)
                {
                    introMusicSource.Stop();
                    Destroy(introMusicSource.gameObject);
                }
            }
        }
        
        private void SetAlpha(TextMeshProUGUI text, float alpha)
        {
            if (text == null) return;
            Color color = text.color;
            color.a = alpha;
            text.color = color;
        }
        
        // Menu Button Actions
        public void OnNewGameClick()
        {
            Debug.Log("[MainMenuUI] New Game clicked!");
            
            // Tạo auto save mới (luôn tạo mới khi New Game)
            SaveSystem.CreateAutoSave();
            Debug.Log("[MainMenuUI] Auto save created, loading Level01...");
            
            // Fade out và load scene
            StartCoroutine(LoadSceneWithFade("Level01"));
        }
        
        public void OnContinueClick()
        {
            Debug.Log("[MainMenuUI] Continue clicked!");
            
            try
            {
                if (!SaveSystem.HasAnySaveFile())
                {
                    Debug.LogWarning("[MainMenuUI] Không có file save!");
                    return;
                }
                
                Debug.Log("[MainMenuUI] Has save file, showing Load Game Panel...");
                
                // Hiển thị Load Game Panel với 11 slots
                if (loadGamePanel != null)
                {
                    Debug.Log("[MainMenuUI] LoadGamePanel reference found, calling Show()...");
                    loadGamePanel.Show();
                    Debug.Log("[MainMenuUI] Show() called successfully!");
                }
                else
                {
                    Debug.LogError("[MainMenuUI] Load Game Panel is NULL!");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[MainMenuUI] ERROR in OnContinueClick: {e.Message}\n{e.StackTrace}");
            }
        }
        
        public void OnSettingsClick()
        {
            Debug.Log("Settings button clicked!");
            
            if (settingsPanel == null)
            {
                Debug.LogError("Settings Panel is not assigned in MainMenuUI!");
                return;
            }
            
            Debug.Log("Showing Settings Panel...");
            settingsPanel.Show();
        }
        
        public void OnCreditsClick()
        {
            if (creditsPanel != null)
            {
                StartCoroutine(TogglePanel(creditsPanel));
            }
        }
        
        public void OnExitClick()
        {
            StartCoroutine(ExitGame());
        }
        
        private IEnumerator TogglePanel(CanvasGroup panel)
        {
            bool isActive = panel.alpha > 0.5f;
            float targetAlpha = isActive ? 0f : 1f;
            float startAlpha = panel.alpha;
            float duration = 0.3f;
            float elapsed = 0f;
            
            panel.interactable = !isActive;
            panel.blocksRaycasts = !isActive;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                panel.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
                yield return null;
            }
            
            panel.alpha = targetAlpha;
        }
        
        private IEnumerator LoadSceneWithFade(string sceneName)
        {
            // Fade out tất cả
            float duration = 1f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                
                SetAlpha(titleText, alpha);
                SetAlpha(subtitleText, alpha);
                if (menuButtonsGroup != null) menuButtonsGroup.alpha = alpha;
                
                yield return null;
            }
            
            // Load scene
            SceneManager.LoadScene(sceneName);
        }
        
        private IEnumerator ExitGame()
        {
            // Fade out
            yield return StartCoroutine(LoadSceneWithFade(""));
            
            // Quit
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
