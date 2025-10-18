using UnityEngine;

namespace MadKnight.UI
{
    /// <summary>
    /// Debug helper để kiểm tra Settings Panel hoạt động đúng chưa
    /// Gắn vào bất kỳ GameObject nào trong scene
    /// </summary>
    public class SettingsDebugger : MonoBehaviour
    {
        [Header("Debug Options")]
        [SerializeField] private bool logOnStart = true;
        [SerializeField] private bool logOnUpdate = false;
        
        [Header("Test Actions")]
        [SerializeField] private KeyCode testSaveKey = KeyCode.F1;
        [SerializeField] private KeyCode testLoadKey = KeyCode.F2;
        [SerializeField] private KeyCode testResetKey = KeyCode.F3;
        [SerializeField] private KeyCode testPrintKey = KeyCode.F4;
        
        private void Start()
        {
            if (logOnStart)
            {
                LogCurrentSettings();
            }
            
            Debug.Log("[SettingsDebugger] Press F1=Save, F2=Load, F3=Reset, F4=Print");
        }
        
        private void Update()
        {
            if (logOnUpdate)
            {
                // Hiển thị real-time volume
                if (AudioManager.Instance != null)
                {
                    GameSettings settings = GameSettings.Load();
                    Debug.Log($"[Audio] Master:{settings.masterVolume:F2} | Music:{settings.musicVolume:F2} | SFX:{settings.sfxVolume:F2}");
                }
            }
            
            // Test keys
            if (Input.GetKeyDown(testSaveKey))
            {
                TestSave();
            }
            
            if (Input.GetKeyDown(testLoadKey))
            {
                TestLoad();
            }
            
            if (Input.GetKeyDown(testResetKey))
            {
                TestReset();
            }
            
            if (Input.GetKeyDown(testPrintKey))
            {
                LogCurrentSettings();
            }
        }
        
        private void TestSave()
        {
            GameSettings settings = GameSettings.Load();
            settings.musicVolume = Random.Range(0f, 1f);
            settings.Save();
            
            Debug.Log($"[Test] Saved random music volume: {settings.musicVolume:F2}");
            LogCurrentSettings();
        }
        
        private void TestLoad()
        {
            GameSettings settings = GameSettings.Load();
            settings.ApplyAll();
            
            Debug.Log("[Test] Loaded and applied settings");
            LogCurrentSettings();
        }
        
        private void TestReset()
        {
            GameSettings settings = GameSettings.GetDefault();
            settings.Save();
            settings.ApplyAll();
            
            Debug.Log("[Test] Reset to default settings");
            LogCurrentSettings();
        }
        
        private void LogCurrentSettings()
        {
            GameSettings settings = GameSettings.Load();
            
            Debug.Log("========== CURRENT SETTINGS ==========");
            Debug.Log($"[Graphics] Quality: {settings.qualityLevel} | Resolution: {settings.resolutionWidth}x{settings.resolutionHeight}");
            Debug.Log($"[Graphics] Fullscreen: {settings.fullscreen} | VSync: {settings.vsync} | FPS: {settings.targetFPS}");
            Debug.Log($"[Audio] Master: {settings.masterVolume:F2} | Music: {settings.musicVolume:F2} | SFX: {settings.sfxVolume:F2} | Ambience: {settings.ambienceVolume:F2}");
            Debug.Log($"[Controls] Mouse Sens: {settings.mouseSensitivity:F2} | Invert Y: {settings.invertYAxis}");
            Debug.Log("======================================");
            
            // Check AudioManager
            if (AudioManager.Instance != null)
            {
                Debug.Log("✅ AudioManager Instance exists");
            }
            else
            {
                Debug.LogError("❌ AudioManager Instance is NULL!");
            }
        }
        
        [ContextMenu("Print All PlayerPrefs")]
        private void PrintAllPlayerPrefs()
        {
            Debug.Log("========== PLAYERPREFS ==========");
            
            string[] keys = new string[]
            {
                "Settings_Quality",
                "Settings_ResWidth",
                "Settings_ResHeight",
                "Settings_Fullscreen",
                "Settings_VSync",
                "Settings_TargetFPS",
                "Settings_Brightness",
                "Settings_MasterVolume",
                "Settings_MusicVolume",
                "Settings_SFXVolume",
                "Settings_AmbienceVolume",
                "Settings_MouseSensitivity",
                "Settings_InvertY",
            };
            
            foreach (string key in keys)
            {
                if (PlayerPrefs.HasKey(key))
                {
                    if (key.Contains("Volume") || key.Contains("Brightness") || key.Contains("Sensitivity"))
                    {
                        Debug.Log($"{key} = {PlayerPrefs.GetFloat(key):F2}");
                    }
                    else
                    {
                        Debug.Log($"{key} = {PlayerPrefs.GetInt(key)}");
                    }
                }
                else
                {
                    Debug.Log($"{key} = <not set>");
                }
            }
            
            Debug.Log("==================================");
        }
        
        [ContextMenu("Clear All Settings")]
        private void ClearAllSettings()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("⚠️ Cleared all PlayerPrefs!");
        }
    }
}
