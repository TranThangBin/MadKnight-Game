using UnityEngine;

namespace MadKnight.UI
{
    /// <summary>
    /// Tool tự động fix vấn đề AudioSource trùng lặp
    /// Sẽ disable tất cả AudioSource KHÔNG thuộc AudioManager
    /// </summary>
    public class AutoFixAudioSources : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool autoFixOnStart = false;
        [SerializeField] private bool showConfirmation = true;
        
        private void Start()
        {
            if (autoFixOnStart)
            {
                AutoFix();
            }
        }
        
        [ContextMenu("Auto Fix - Disable Non-AudioManager Sources")]
        public void AutoFix()
        {
            Debug.Log("========================================");
            Debug.Log("🔧 AUTO FIX: Disabling non-AudioManager AudioSources...");
            Debug.Log("========================================");
            
            AudioSource[] allSources = Resources.FindObjectsOfTypeAll<AudioSource>();
            int disabledCount = 0;
            int audioManagerCount = 0;
            
            foreach (AudioSource source in allSources)
            {
                // Bỏ qua destroyed objects và prefabs
                if (source.gameObject.scene.name == null) continue;
                
                // Kiểm tra xem AudioSource có thuộc AudioManager không
                bool isAudioManager = source.GetComponent<AudioManager>() != null ||
                                     source.transform.parent?.GetComponent<AudioManager>() != null ||
                                     source.gameObject.name == "AudioManager";
                
                if (isAudioManager)
                {
                    audioManagerCount++;
                    Debug.Log($"✅ Keeping: {GetPath(source.gameObject)} (AudioManager)");
                }
                else
                {
                    // Disable AudioSource không thuộc AudioManager
                    if (source.enabled)
                    {
                        source.enabled = false;
                        disabledCount++;
                        
                        if (source.isPlaying)
                        {
                            source.Stop();
                        }
                        
                        Debug.LogWarning($"❌ Disabled: {GetPath(source.gameObject)} (Clip: {source.clip?.name ?? "none"})");
                    }
                }
            }
            
            Debug.Log("========================================");
            Debug.Log($"🎯 RESULT:");
            Debug.Log($"   - AudioManager sources: {audioManagerCount}");
            Debug.Log($"   - Disabled sources: {disabledCount}");
            
            if (disabledCount > 0)
            {
                Debug.Log("========================================");
                Debug.LogWarning($"⚠️ {disabledCount} AudioSource(s) have been DISABLED!");
                Debug.LogWarning("These AudioSources were NOT part of AudioManager.");
                Debug.LogWarning("Check Hierarchy and remove them if not needed.");
                Debug.Log("========================================");
            }
            else
            {
                Debug.Log("✅ No non-AudioManager AudioSources found!");
                Debug.Log("========================================");
            }
            
            if (showConfirmation && disabledCount > 0)
            {
                Debug.Log("💡 TIP: You can now delete those disabled AudioSources from Hierarchy.");
            }
        }
        
        [ContextMenu("Find All AudioSources (Detailed)")]
        public void FindAllDetailed()
        {
            Debug.Log("========================================");
            Debug.Log("🔍 DETAILED AUDIO SOURCE SCAN");
            Debug.Log("========================================");
            
            AudioSource[] allSources = Resources.FindObjectsOfTypeAll<AudioSource>();
            
            foreach (AudioSource source in allSources)
            {
                if (source.gameObject.scene.name == null) continue;
                
                string path = GetPath(source.gameObject);
                bool isAudioManager = source.GetComponent<AudioManager>() != null ||
                                     source.transform.parent?.GetComponent<AudioManager>() != null ||
                                     source.gameObject.name == "AudioManager";
                
                string tag = isAudioManager ? "[AudioManager]" : "[Other]";
                string status = source.enabled ? (source.isPlaying ? "▶️ Playing" : "⏸️ Stopped") : "🚫 Disabled";
                string clip = source.clip != null ? source.clip.name : "none";
                
                Debug.Log($"{tag} {status} | Enabled: {source.enabled} | Volume: {source.volume:F2} | Clip: {clip}");
                Debug.Log($"         └─ Path: {path}");
            }
            
            Debug.Log("========================================");
        }
        
        [ContextMenu("Re-enable All AudioSources")]
        public void ReEnableAll()
        {
            AudioSource[] allSources = Resources.FindObjectsOfTypeAll<AudioSource>();
            int enabledCount = 0;
            
            foreach (AudioSource source in allSources)
            {
                if (source.gameObject.scene.name == null) continue;
                
                if (!source.enabled)
                {
                    source.enabled = true;
                    enabledCount++;
                    Debug.Log($"✅ Re-enabled: {GetPath(source.gameObject)}");
                }
            }
            
            Debug.Log($"Re-enabled {enabledCount} AudioSource(s)");
        }
        
        private string GetPath(GameObject obj)
        {
            string path = obj.name;
            Transform current = obj.transform;
            
            while (current.parent != null)
            {
                current = current.parent;
                path = current.name + "/" + path;
            }
            
            return path;
        }
    }
}
