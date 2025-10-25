using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MadKnight;
using UnityEngine.SceneManagement;

public class NSaveSystem
{
    private static SaveData _saveData = new SaveData();
    private static bool _shouldLoadOnNextGameManagerInit = false; // Flag để load khi GameManager khởi tạo
    
    [System.Serializable]
    public struct SaveData
    {
        public NPlayerSaveData PlayerData;
        public string SaveTime;
        public string CurrentScene;
        public float PlayTimeSeconds;
    }
    public static string SaveFileName()
    {
        string saveFile = Application.persistentDataPath + "/save" + ".save";
        return saveFile;
    }

    public static void Save()
    {
        HandleSaveData();

        File.WriteAllText(SaveFileName(), JsonUtility.ToJson(_saveData, true));
        Debug.Log($"[NSaveSystem] Đã save game tại vị trí: {_saveData.PlayerData.Position} - File: {SaveFileName()}");
    }
    private static void HandleSaveData()
    {
        GameManager.Instance.Player.Save(ref _saveData.PlayerData);
        _saveData.SaveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        _saveData.CurrentScene = SceneManager.GetActiveScene().name;
        _saveData.PlayTimeSeconds = GameManager.Instance.GetPlayTime();
    }

    public static void Load()
    {
        if (!File.Exists(SaveFileName()))
        {
            Debug.LogWarning($"[NSaveSystem] Không tìm thấy file save: {SaveFileName()}");
            return;
        }

        string saveContent = File.ReadAllText(SaveFileName());
        _saveData = JsonUtility.FromJson<SaveData>(saveContent);
        Debug.Log($"[NSaveSystem] Đã load save data - Position: {_saveData.PlayerData.Position}");
        HandleLoadData();
    }
    private static void HandleLoadData()
    {
        // Load scene nếu khác scene hiện tại
        if (!string.IsNullOrEmpty(_saveData.CurrentScene) && 
            SceneManager.GetActiveScene().name != _saveData.CurrentScene)
        {
            Debug.Log($"[NSaveSystem] Đang load scene: {_saveData.CurrentScene}");
            SceneManager.LoadScene(_saveData.CurrentScene);
            // Sau khi load scene, cần đợi scene load xong rồi mới set player data
            // Sẽ được xử lý trong GameManager
        }
        else
        {
            // Nếu cùng scene, load ngay
            if (GameManager.Instance != null && GameManager.Instance.Player != null)
            {
                GameManager.Instance.Player.Load(_saveData.PlayerData);
                GameManager.Instance.SetPlayTime(_saveData.PlayTimeSeconds);
                Debug.Log($"[NSaveSystem] Đã load player vào vị trí: {_saveData.PlayerData.Position}");
            }
            else
            {
                Debug.LogError("[NSaveSystem] GameManager.Instance hoặc Player bị null!");
            }
        }
    }
    
    /// <summary>
    /// Lấy SaveData hiện tại (cho adapter)
    /// </summary>
    public static SaveData GetCurrentSaveData()
    {
        return _saveData;
    }

    /// <summary>
    /// Đọc SaveData từ file mà không load vào game (dùng để hiển thị UI)
    /// </summary>
    public static SaveData ReadSaveDataFromFile()
    {
        if (!File.Exists(SaveFileName()))
        {
            Debug.LogWarning($"[NSaveSystem] Không tìm thấy file save khi đọc: {SaveFileName()}");
            return new SaveData();
        }

        try
        {
            string saveContent = File.ReadAllText(SaveFileName());
            SaveData data = JsonUtility.FromJson<SaveData>(saveContent);
            Debug.Log($"[NSaveSystem] Đã đọc save data từ file - Scene: {data.CurrentScene}, Time: {data.SaveTime}");
            return data;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[NSaveSystem] Lỗi khi đọc file save: {e.Message}");
            return new SaveData();
        }
    }

    /// <summary>
    /// Kiểm tra xem có file save không
    /// </summary>
    public static bool HasSaveFile()
    {
        return File.Exists(SaveFileName());
    }

    /// <summary>
    /// Set flag để load save data khi GameManager khởi tạo (dùng khi load từ Main Menu)
    /// </summary>
    public static void SetLoadOnNextInit()
    {
        _shouldLoadOnNextGameManagerInit = true;
        Debug.Log("[NSaveSystem] Set flag to load save data on next GameManager init");
    }

    /// <summary>
    /// Kiểm tra và clear flag load
    /// </summary>
    public static bool ShouldLoadOnInit()
    {
        bool result = _shouldLoadOnNextGameManagerInit;
        _shouldLoadOnNextGameManagerInit = false;
        return result;
    }

    /// <summary>
    /// Apply save data vào game (gọi sau khi scene load xong)
    /// </summary>
    public static void ApplySaveData()
    {
        // Đọc save data từ file
        if (!File.Exists(SaveFileName()))
        {
            Debug.LogWarning("[NSaveSystem] Không có file save để apply!");
            return;
        }

        try
        {
            string saveContent = File.ReadAllText(SaveFileName());
            _saveData = JsonUtility.FromJson<SaveData>(saveContent);
            
            if (GameManager.Instance != null && GameManager.Instance.Player != null)
            {
                GameManager.Instance.Player.Load(_saveData.PlayerData);
                GameManager.Instance.SetPlayTime(_saveData.PlayTimeSeconds);
                Debug.Log($"[NSaveSystem] Đã apply save data - Position: {_saveData.PlayerData.Position}");
            }
            else
            {
                Debug.LogError("[NSaveSystem] Không thể apply save data - GameManager hoặc Player null!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[NSaveSystem] Lỗi khi apply save data: {e.Message}");
        }
    }
}