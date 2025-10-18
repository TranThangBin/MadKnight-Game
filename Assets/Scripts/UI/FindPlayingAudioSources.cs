using UnityEngine;

namespace MadKnight.UI
{
    /// <summary>
    /// Tool để tìm TẤT CẢ AudioSource đang phát trong scene
    /// Gắn vào GameObject bất kỳ và chạy game
    /// </summary>
    public class FindPlayingAudioSources : MonoBehaviour
    {
        [Header("Auto Search")]
        [SerializeField] private bool searchOnStart = true;
        [SerializeField] private bool searchEveryFrame = false;
        [SerializeField] private KeyCode searchKey = KeyCode.F5;
        
        private void Start()
        {
            if (searchOnStart)
            {
                SearchAllAudioSources();
            }
        }
        
        private void Update()
        {
            if (searchEveryFrame)
            {
                SearchAllAudioSources();
            }
            
            if (Input.GetKeyDown(searchKey))
            {
                SearchAllAudioSources();
            }
        }
        
        [ContextMenu("Search All Audio Sources")]
        public void SearchAllAudioSources()
        {
            Debug.Log("========================================");
            Debug.Log("🔍 SEARCHING ALL AUDIO SOURCES...");
            Debug.Log("========================================");
            
            // Tìm TẤT CẢ AudioSource trong scene (bao gồm cả inactive và DontDestroyOnLoad)
            AudioSource[] allSources = Resources.FindObjectsOfTypeAll<AudioSource>();
            
            int playingCount = 0;
            int totalCount = 0;
            
            foreach (AudioSource source in allSources)
            {
                // Bỏ qua những object đã bị destroy hoặc trong prefab
                if (source.gameObject.scene.name == null) continue;
                
                totalCount++;
                
                string status = source.isPlaying ? "▶️ PLAYING" : "⏸️ Stopped";
                string clipName = source.clip != null ? source.clip.name : "none";
                string path = GetGameObjectPath(source.gameObject);
                
                if (source.isPlaying)
                {
                    playingCount++;
                    Debug.Log($"🔊 {status} | Volume: {source.volume:F2} | Clip: {clipName}");
                    Debug.Log($"   └─ Path: {path}");
                    Debug.Log($"   └─ Loop: {source.loop} | Time: {source.time:F2}/{source.clip.length:F2}");
                }
                else
                {
                    Debug.Log($"   {status} | Volume: {source.volume:F2} | Clip: {clipName} | Path: {path}");
                }
            }
            
            Debug.Log("========================================");
            Debug.Log($"📊 SUMMARY: {playingCount} playing / {totalCount} total AudioSources");
            Debug.Log("========================================");
            
            if (playingCount == 0)
            {
                Debug.LogWarning("⚠️ NO AUDIO SOURCE IS PLAYING!");
            }
            else if (playingCount > 1)
            {
                Debug.LogWarning($"⚠️ {playingCount} AUDIO SOURCES ARE PLAYING AT THE SAME TIME!");
            }
        }
        
        private string GetGameObjectPath(GameObject obj)
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
        
        [ContextMenu("Stop All Audio Sources")]
        public void StopAllAudioSources()
        {
            AudioSource[] allSources = Resources.FindObjectsOfTypeAll<AudioSource>();
            int stoppedCount = 0;
            
            foreach (AudioSource source in allSources)
            {
                if (source.gameObject.scene.name == null) continue;
                
                if (source.isPlaying)
                {
                    source.Stop();
                    stoppedCount++;
                    Debug.Log($"⏹️ Stopped: {GetGameObjectPath(source.gameObject)}");
                }
            }
            
            Debug.Log($"Stopped {stoppedCount} audio sources");
        }
    }
}
