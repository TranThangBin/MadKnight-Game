using UnityEngine;
using System;

namespace MiniGames.DotConnect
{
    [CreateAssetMenu(fileName = "DotConnectConfig", menuName = "MiniGames/DotConnect/Config")]
    public class DotConnectConfig : ScriptableObject
    {
        [Header("Board Settings")]
        [Tooltip("Kích thước bàn cờ (số ô ngang)")]
        [Range(3, 10)]
        public int boardWidth = 5;
        
        [Tooltip("Kích thước bàn cờ (số ô dọc)")]
        [Range(3, 10)]
        public int boardHeight = 5;
        
        [Header("Dot Pair Settings")]
        [Tooltip("Số lượng cặp dots cần nối")]
        [Range(2, 8)]
        public int numberOfDotPairs = 3;
        
        [Header("Difficulty Settings")]
        [Tooltip("Độ khó tối thiểu (1-10). Số càng cao, puzzle càng khó")]
        [Range(1, 10)]
        public int minimumDifficulty = 3;
        
        [Tooltip("Số lần thử tạo puzzle tối đa")]
        public int maxGenerationAttempts = 100;
        
        [Header("Visual Settings")]
        [Tooltip("Danh sách màu sắc cho các cặp dots")]
        public Color[] dotColors = new Color[]
        {
            Color.red,
            Color.blue,
            Color.green,
            Color.yellow,
            Color.magenta,
            Color.cyan,
            new Color(1f, 0.5f, 0f), // Orange
            new Color(0.5f, 0f, 1f)  // Purple
        };
        
        [Tooltip("Kích thước của dot")]
        public float dotSize = 0.8f;
        
        [Tooltip("Độ dày của đường nối")]
        public float lineWidth = 0.15f;
        
        [Header("Animation Settings")]
        [Tooltip("Thời gian animation khi hoàn thành")]
        public float completionAnimationDuration = 0.5f;
    }
}
