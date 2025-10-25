using UnityEngine;
using UnityEngine.SceneManagement;
using MadKnight.Save;
using System.Collections;

namespace MadKnight
{
    /// <summary>
    /// Ví dụ về cách sử dụng SaveSystem trong game
    /// Gắn script này vào một GameObject trong scene để quản lý việc save/load
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        
        public Player Player; // Reference to the player
        private float playTimeSeconds = 0f;
        private bool shouldApplySaveDataOnStart = false;
        
        private MadKnight.Save.PlayerSaveData currentSaveData;
        
        private void Awake()
        {
            // Singleton pattern - nhưng KHÔNG dùng DontDestroyOnLoad nếu GameManager nằm trong scene
            if (Instance == null)
            {
                Instance = this;
                // Chỉ uncomment dòng dưới nếu bạn muốn GameManager persist qua scenes
                // DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            Debug.Log("[GameManager] Awake() - Instance created");
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                Debug.Log("[GameManager] Instance destroyed");
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"[GameManager] Scene loaded: {scene.name}");
            
            // Tìm player trong scene mới - cải thiện logic tìm kiếm
            StartCoroutine(FindPlayerInScene());

            // Nếu có flag để apply save data, thực hiện ngay
            if (shouldApplySaveDataOnStart && NSaveSystem.HasSaveFile())
            {
                shouldApplySaveDataOnStart = false;
                // Đợi player được tìm thấy và khởi tạo xong
                StartCoroutine(ApplySaveDataWhenReady());
            }
        }

        private System.Collections.IEnumerator FindPlayerInScene()
        {
            // Đợi 1 frame để scene khởi tạo
            yield return null;
            
            if (Player == null)
            {
                var playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    Player = playerObj.GetComponent<Player>();
                    Debug.Log($"[GameManager] Đã tìm thấy Player trong scene tại vị trí: {Player.transform.position}");
                }
                else
                {
                    Debug.LogWarning("[GameManager] Không tìm thấy Player với tag 'Player'!");
                }
            }
        }

        private System.Collections.IEnumerator ApplySaveDataWhenReady()
        {
            // Đợi player được tìm thấy
            int maxAttempts = 10;
            int attempts = 0;
            
            while (Player == null && attempts < maxAttempts)
            {
                Debug.Log($"[GameManager] Đang đợi Player... (attempt {attempts + 1}/{maxAttempts})");
                yield return new WaitForSeconds(0.1f);
                attempts++;
            }
            
            if (Player == null)
            {
                Debug.LogError("[GameManager] Không tìm thấy Player sau khi đợi!");
                yield break;
            }
            
            // Đợi thêm 2 frames để đảm bảo Player đã khởi tạo hoàn toàn
            yield return null;
            yield return null;
            
            Debug.Log($"[GameManager] Player sẵn sàng, đang apply save data...");
            NSaveSystem.ApplySaveData();
        }

        private System.Collections.IEnumerator ApplySaveDataNextFrame()
        {
            yield return null; // Đợi 1 frame
            NSaveSystem.ApplySaveData();
        }
        
        private void Start()
        {
            Debug.Log("[GameManager] Start() called");
            
            // Kiểm tra xem có cần load save data không (từ LoadGamePanel)
            if (NSaveSystem.ShouldLoadOnInit() && NSaveSystem.HasSaveFile())
            {
                Debug.Log("[GameManager] Phát hiện flag load save data, đang apply...");
                StartCoroutine(LoadSaveDataAfterInit());
            }
            else
            {
                // Load auto save khi bắt đầu game (logic cũ)
                LoadGameData();
            }
        }
        
        private IEnumerator LoadSaveDataAfterInit()
        {
            // Đợi player được tìm thấy
            int maxAttempts = 20;
            int attempts = 0;
            
            while (Player == null && attempts < maxAttempts)
            {
                Debug.Log($"[GameManager] Đang tìm Player... (attempt {attempts + 1}/{maxAttempts})");
                var playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    Player = playerObj.GetComponent<Player>();
                    Debug.Log($"[GameManager] Đã tìm thấy Player tại: {Player.transform.position}");
                }
                yield return new WaitForSeconds(0.1f);
                attempts++;
            }
            
            if (Player == null)
            {
                Debug.LogError("[GameManager] Không tìm thấy Player!");
                yield break;
            }
            
            // Đợi thêm vài frames để đảm bảo Player khởi tạo xong
            yield return null;
            yield return null;
            yield return null;
            
            // Apply save data
            Debug.Log("[GameManager] Đang apply save data...");
            NSaveSystem.ApplySaveData();
        }
        
        private void Update()
        {
            // Track play time
            if (Input.GetKeyDown(KeyCode.Keypad0))
            {
                if (Player != null)
                {
                    Debug.Log($"[GameManager] Đang save game - Player position: {Player.transform.position}");
                    NSaveSystem.Save();
                }
                else
                {
                    Debug.LogError("[GameManager] Không thể save - Player null!");
                }
            }
            if(Input.GetKeyDown(KeyCode.Keypad9)) 
            {
                Debug.Log("[GameManager] Đang load game...");
                shouldApplySaveDataOnStart = true;
                NSaveSystem.Load();
            }
            playTimeSeconds += Time.deltaTime;
        }
        
        public float GetPlayTime()
        {
            return playTimeSeconds;
        }
        
        public void SetPlayTime(float time)
        {
            playTimeSeconds = time;
        }
        
        /// <summary>
        /// Load dữ liệu game
        /// </summary>
        private void LoadGameData()
        {
            currentSaveData = SaveSystem.LoadAutoSave();
            
            if (currentSaveData != null)
            {
                Debug.Log($"Đã load game data: Scene: {currentSaveData.currentScene}");
                // Áp dụng dữ liệu vào game
                // Ví dụ: Set vị trí player, health, etc.
            }
            else
            {
                Debug.Log("Không có dữ liệu save, sử dụng giá trị mặc định");
            }
        }
        
        /// <summary>
        /// Lưu dữ liệu game (gọi hàm này khi cần auto save)
        /// </summary>
        public void SaveGameData()
        {
            if (currentSaveData == null)
            {
                currentSaveData = new PlayerSaveData();
            }
            
            // Cập nhật dữ liệu hiện tại
            currentSaveData.saveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            currentSaveData.currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            
            // Lấy thông tin từ player (cần tùy chỉnh theo game của bạn)
            // Ví dụ:
            // GameObject player = GameObject.FindGameObjectWithTag("Player");
            // currentSaveData.playerPosition = player.transform.position;
            // currentSaveData.playerRotation = player.transform.rotation;
            // currentSaveData.playerHealth = playerController.health;
            
            // Lưu auto save
            SaveSystem.SaveAutoSave(currentSaveData);
        }
        
        /// <summary>
        /// Gọi hàm này để auto save theo interval (ví dụ mỗi 30 giây)
        /// </summary>
        private void AutoSave()
        {
            InvokeRepeating(nameof(SaveGameData), 30f, 30f); // Auto save mỗi 30 giây
        }
    }
}
