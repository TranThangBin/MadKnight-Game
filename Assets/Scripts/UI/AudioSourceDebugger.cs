using UnityEngine;

namespace MadKnight.UI
{
    /// <summary>
    /// DEBUG TOOL - Gắn vào bất kỳ GameObject nào để xem TẤT CẢ AudioSource trong scene
    /// Bấm phím để debug
    /// </summary>
    public class AudioSourceDebugger : MonoBehaviour
    {
        [Header("Hotkeys")]
        [SerializeField] private KeyCode listAllKey = KeyCode.F5;
        [SerializeField] private KeyCode testVolumeKey = KeyCode.F6;
        
        private void Update()
        {
            if (Input.GetKeyDown(listAllKey))
            {
                ListAllAudioSources();
            }
            
            if (Input.GetKeyDown(testVolumeKey))
            {
                TestVolumeChange();
            }
        }
        
        [ContextMenu("List All AudioSources")]
        private void ListAllAudioSources()
        {
            AudioSource[] all = FindObjectsOfType<AudioSource>();
            
            Debug.Log("========== ALL AUDIOSOURCES IN SCENE ==========");
            Debug.Log($"Found {all.Length} AudioSource(s)");
            Debug.Log("");
            
            for (int i = 0; i < all.Length; i++)
            {
                AudioSource source = all[i];
                string status = source.isPlaying ? "🔊 PLAYING" : "⏸️ STOPPED";
                string loopStatus = source.loop ? "🔁 LOOP" : "▶️ ONE-SHOT";
                string clipName = source.clip != null ? source.clip.name : "NO CLIP";
                
                Debug.Log($"[{i + 1}] {source.gameObject.name} → {source.name}");
                Debug.Log($"    Status: {status} | {loopStatus}");
                Debug.Log($"    Clip: {clipName}");
                Debug.Log($"    Volume: {source.volume:F2} | Mute: {source.mute}");
                Debug.Log($"    Time: {source.time:F1}s / {(source.clip ? source.clip.length : 0):F1}s");
                Debug.Log("");
            }
            
            Debug.Log("===============================================");
        }
        
        [ContextMenu("Test Volume Change")]
        private void TestVolumeChange()
        {
            if (AudioManager.Instance == null)
            {
                Debug.LogError("AudioManager Instance is NULL!");
                return;
            }
            
            float randomVolume = Random.Range(0.2f, 1f);
            Debug.Log($"========== TEST VOLUME CHANGE ==========");
            Debug.Log($"Setting Music Volume to: {randomVolume:F2}");
            Debug.Log("");
            
            AudioManager.Instance.SetMusicVolume(randomVolume);
            
            Debug.Log("");
            Debug.Log("Check logs above to see which AudioSources were updated");
            Debug.Log("========================================");
        }
        
        [ContextMenu("Force Set All Playing AudioSources to 0.5")]
        private void ForceSetAllVolumes()
        {
            AudioSource[] all = FindObjectsOfType<AudioSource>();
            int count = 0;
            
            Debug.Log("========== FORCE SET ALL VOLUMES ==========");
            
            foreach (AudioSource source in all)
            {
                if (source.isPlaying)
                {
                    source.volume = 0.5f;
                    count++;
                    Debug.Log($"Set {source.gameObject.name} volume to 0.5");
                }
            }
            
            Debug.Log($"Set {count} AudioSource(s) to volume 0.5");
            Debug.Log("===========================================");
        }
    }
}
