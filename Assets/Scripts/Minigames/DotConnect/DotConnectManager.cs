using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace MiniGames.DotConnect
{
    /// <summary>
    /// UnityEvent được gọi khi hoàn thành tất cả puzzle
    /// </summary>
    [System.Serializable]
    public class DotConnectCompletedEvent : UnityEvent { }
    
    /// <summary>
    /// UnityEvent được gọi khi một cặp dots được nối thành công
    /// </summary>
    [System.Serializable]
    public class DotPairConnectedEvent : UnityEvent<int, Color> { }
    
    /// <summary>
    /// Manager chính điều khiển game Dot-Connect
    /// </summary>
    public class DotConnectManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private DotConnectConfig config;
        
        [Header("References")]
        [SerializeField] private Transform boardContainer;
        [SerializeField] private GameObject dotPrefab;
        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private LineRenderer lineRendererPrefab;
        
        [Header("Events")]
        public DotConnectCompletedEvent onPuzzleCompleted;
        public DotPairConnectedEvent onDotPairConnected;
        public UnityEvent onPuzzleStarted;
        
        // Game state
        private List<DotPair> dotPairs;
        private int[,] board; // 0 = trống, >0 = có đường nối của pair ID
        private Dictionary<int, GameObject> dotObjects;
        private Dictionary<int, LineRenderer> lineRenderers;
        private DotConnectGenerator generator;
        
        // Input state
        private bool isDragging;
        private DotPair currentPair;
        private List<GridCell> currentPath;
        private LineRenderer currentLineRenderer;
        private GameObject startDotObject;
        
        private Camera mainCamera;
        private float cellSize;
        
<<<<<<< HEAD
        // Fallback mechanism
        private bool isGenerating = false;
        private float generationStartTime;
        private const float GENERATION_TIMEOUT = 5.0f; // Timeout 5 giây
        private int failedAttempts = 0;
        private const int MAX_FAILED_ATTEMPTS = 3;
        
=======
>>>>>>> 609294f (Added DotsConnect Minigame)
        private void Awake()
        {
            mainCamera = Camera.main;
            dotObjects = new Dictionary<int, GameObject>();
            lineRenderers = new Dictionary<int, LineRenderer>();
            
            if (config == null)
            {
                Debug.LogError("DotConnectConfig chưa được gán!");
                return;
            }
            
            generator = new DotConnectGenerator(config);
        }
        
        private void Start()
        {
            InitializePuzzle();
        }
        
        /// <summary>
        /// Khởi tạo puzzle mới
        /// </summary>
        public void InitializePuzzle()
        {
<<<<<<< HEAD
            // Kiểm tra nếu đang trong quá trình tạo
            if (isGenerating)
            {
                Debug.LogWarning("Đang trong quá trình tạo puzzle, vui lòng đợi...");
                return;
            }
            
            isGenerating = true;
            generationStartTime = Time.realtimeSinceStartup;
            
            try
            {
                ClearBoard();
                
                // Tạo puzzle với timeout
                dotPairs = GeneratePuzzleWithFallback();
                
                if (dotPairs == null || dotPairs.Count == 0)
                {
                    Debug.LogError("Không thể tạo puzzle!");
                    isGenerating = false;
                    return;
                }
                
                // Khởi tạo bàn cờ
                board = new int[config.boardWidth, config.boardHeight];
                
                // Đánh dấu vị trí các dots
                foreach (var pair in dotPairs)
                {
                    board[pair.startDot.x, pair.startDot.y] = -(pair.pairId + 1); // Dấu âm để phân biệt dot
                    board[pair.endDot.x, pair.endDot.y] = -(pair.pairId + 1);
                }
                
                // Tạo visual
                CreateBoardVisual();
                CreateDotsVisual();
                
                onPuzzleStarted?.Invoke();
                
                Debug.Log($"Puzzle đã được khởi tạo với {dotPairs.Count} cặp dots");
                failedAttempts = 0; // Reset failed attempts khi thành công
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Lỗi khi khởi tạo puzzle: {ex.Message}");
                failedAttempts++;
                
                // Nếu thất bại quá nhiều, tạo puzzle đơn giản nhất
                if (failedAttempts >= MAX_FAILED_ATTEMPTS)
                {
                    Debug.LogWarning("Tạo puzzle đơn giản do thất bại nhiều lần...");
                    CreateEmergencyPuzzle();
                }
            }
            finally
            {
                isGenerating = false;
            }
        }
        
        /// <summary>
        /// Tạo puzzle với cơ chế fallback
        /// </summary>
        private List<DotPair> GeneratePuzzleWithFallback()
        {
            List<DotPair> result = null;
            float startTime = Time.realtimeSinceStartup;
            
            try
            {
                // Thử tạo với generator chính
                result = generator.GeneratePuzzle();
                
                // Kiểm tra timeout
                if (Time.realtimeSinceStartup - startTime > GENERATION_TIMEOUT)
                {
                    Debug.LogWarning("Timeout khi tạo puzzle! Sử dụng fallback...");
                    result = generator.GenerateSimplePuzzle();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Lỗi khi tạo puzzle: {ex.Message}. Sử dụng fallback...");
                result = generator.GenerateSimplePuzzle();
            }
            
            return result;
        }
        
        /// <summary>
        /// Tạo puzzle cực kỳ đơn giản trong trường hợp khẩn cấp
        /// </summary>
        private void CreateEmergencyPuzzle()
        {
            ClearBoard();
            
            dotPairs = new List<DotPair>();
            
            // Tạo chỉ 2 cặp dots đơn giản
            int pairCount = Mathf.Min(2, config.numberOfDotPairs);
            
            for (int i = 0; i < pairCount; i++)
            {
                GridCell start = new GridCell(0, i);
                GridCell end = new GridCell(config.boardWidth - 1, i);
                Color color = config.dotColors[i % config.dotColors.Length];
                
                dotPairs.Add(new DotPair(i, start, end, color));
            }
=======
            ClearBoard();
            
            // Tạo puzzle
            dotPairs = generator.GeneratePuzzle();
>>>>>>> 609294f (Added DotsConnect Minigame)
            
            // Khởi tạo bàn cờ
            board = new int[config.boardWidth, config.boardHeight];
            
<<<<<<< HEAD
            foreach (var pair in dotPairs)
            {
                board[pair.startDot.x, pair.startDot.y] = -(pair.pairId + 1);
                board[pair.endDot.x, pair.endDot.y] = -(pair.pairId + 1);
            }
            
=======
            // Đánh dấu vị trí các dots
            foreach (var pair in dotPairs)
            {
                board[pair.startDot.x, pair.startDot.y] = -(pair.pairId + 1); // Dấu âm để phân biệt dot
                board[pair.endDot.x, pair.endDot.y] = -(pair.pairId + 1);
            }
            
            // Tạo visual
>>>>>>> 609294f (Added DotsConnect Minigame)
            CreateBoardVisual();
            CreateDotsVisual();
            
            onPuzzleStarted?.Invoke();
            
<<<<<<< HEAD
            Debug.Log("Đã tạo emergency puzzle với 2 cặp đơn giản");
=======
            Debug.Log($"Puzzle đã được khởi tạo với {dotPairs.Count} cặp dots");
>>>>>>> 609294f (Added DotsConnect Minigame)
        }
        
        /// <summary>
        /// Xóa bàn cờ hiện tại
        /// </summary>
        private void ClearBoard()
        {
            if (boardContainer != null)
            {
                foreach (Transform child in boardContainer)
                {
                    Destroy(child.gameObject);
                }
            }
            
            dotObjects.Clear();
            lineRenderers.Clear();
            dotPairs = null;
            board = null;
        }
        
        /// <summary>
        /// Tạo visual cho bàn cờ
        /// </summary>
        private void CreateBoardVisual()
        {
            if (cellPrefab == null)
            {
                Debug.LogWarning("Cell Prefab chưa được gán. Tạo cells đơn giản...");
                CreateSimpleCells();
                return;
            }
            
            cellSize = 1.0f; // Kích thước mỗi ô
            
            // Căn giữa bàn cờ
            float offsetX = -(config.boardWidth - 1) * cellSize / 2f;
            float offsetY = -(config.boardHeight - 1) * cellSize / 2f;
            
            for (int x = 0; x < config.boardWidth; x++)
            {
                for (int y = 0; y < config.boardHeight; y++)
                {
                    Vector3 position = new Vector3(
                        offsetX + x * cellSize,
                        offsetY + y * cellSize,
                        0
                    );
                    
                    GameObject cell = Instantiate(cellPrefab, position, Quaternion.identity, boardContainer);
                    cell.name = $"Cell_{x}_{y}";
                    
                    // Thêm collider nếu chưa có
                    if (cell.GetComponent<Collider2D>() == null)
                    {
                        BoxCollider2D collider = cell.AddComponent<BoxCollider2D>();
                        collider.size = Vector2.one * cellSize;
                    }
                }
            }
        }
        
        /// <summary>
        /// Tạo cells đơn giản nếu không có prefab
        /// </summary>
        private void CreateSimpleCells()
        {
            cellSize = 1.0f;
            float offsetX = -(config.boardWidth - 1) * cellSize / 2f;
            float offsetY = -(config.boardHeight - 1) * cellSize / 2f;
            
            for (int x = 0; x < config.boardWidth; x++)
            {
                for (int y = 0; y < config.boardHeight; y++)
                {
                    GameObject cell = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    cell.transform.SetParent(boardContainer);
                    cell.transform.position = new Vector3(
                        offsetX + x * cellSize,
                        offsetY + y * cellSize,
                        0
                    );
                    cell.transform.localScale = Vector3.one * cellSize * 0.95f;
                    cell.name = $"Cell_{x}_{y}";
                    
                    // Màu nền
                    var renderer = cell.GetComponent<Renderer>();
                    renderer.material = new Material(Shader.Find("Sprites/Default"));
                    renderer.material.color = new Color(0.9f, 0.9f, 0.9f, 1f);
                }
            }
        }
        
        /// <summary>
        /// Tạo visual cho các dots
        /// </summary>
        private void CreateDotsVisual()
        {
            float offsetX = -(config.boardWidth - 1) * cellSize / 2f;
            float offsetY = -(config.boardHeight - 1) * cellSize / 2f;
            
            foreach (var pair in dotPairs)
            {
                // Tạo dot bắt đầu
                CreateDot(pair.startDot, pair.pairId, pair.color, offsetX, offsetY, true);
                
                // Tạo dot kết thúc
                CreateDot(pair.endDot, pair.pairId, pair.color, offsetX, offsetY, false);
                
                // Tạo LineRenderer cho cặp này
                CreateLineRenderer(pair.pairId, pair.color);
            }
        }
        
        private void CreateDot(GridCell cell, int pairId, Color color, float offsetX, float offsetY, bool isStart)
        {
            Vector3 position = new Vector3(
                offsetX + cell.x * cellSize,
                offsetY + cell.y * cellSize,
                -0.1f
            );
            
            GameObject dot;
            
            if (dotPrefab != null)
            {
                dot = Instantiate(dotPrefab, position, Quaternion.identity, boardContainer);
            }
            else
            {
                // Tạo dot đơn giản
                dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                dot.transform.SetParent(boardContainer);
                dot.transform.position = position;
                Destroy(dot.GetComponent<Collider>()); // Xóa collider 3D
            }
            
            dot.transform.localScale = Vector3.one * config.dotSize;
            dot.name = $"Dot_{pairId}_{(isStart ? "Start" : "End")}";
            
            // Set màu
            var renderer = dot.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = new Material(Shader.Find("Sprites/Default"));
                renderer.material.color = color;
            }
            
            // Thêm collider 2D
            if (dot.GetComponent<Collider2D>() == null)
            {
                CircleCollider2D collider = dot.AddComponent<CircleCollider2D>();
                collider.radius = config.dotSize / 2f;
            }
            
            // Thêm component để nhận diện dot
            var dotComponent = dot.AddComponent<DotComponent>();
            dotComponent.pairId = pairId;
            dotComponent.gridCell = cell;
            dotComponent.isStart = isStart;
            
            // Lưu vào dictionary
            string key = isStart ? $"{pairId}_start" : $"{pairId}_end";
            if (!dotObjects.ContainsKey(GetDotKey(pairId, isStart)))
            {
                dotObjects.Add(GetDotKey(pairId, isStart), dot);
            }
        }
        
        private void CreateLineRenderer(int pairId, Color color)
        {
            GameObject lineObj = new GameObject($"Line_{pairId}");
            lineObj.transform.SetParent(boardContainer);
            lineObj.transform.localPosition = Vector3.zero;
            
            LineRenderer lineRenderer;
            
            if (lineRendererPrefab != null)
            {
                lineRenderer = Instantiate(lineRendererPrefab, lineObj.transform);
            }
            else
            {
                lineRenderer = lineObj.AddComponent<LineRenderer>();
            }
            
            // Cấu hình LineRenderer
            lineRenderer.startWidth = config.lineWidth;
            lineRenderer.endWidth = config.lineWidth;
            
            // Sử dụng helper để tạo material tối ưu
            lineRenderer.material = LineRendererMaterialHelper.GetOptimizedMaterial(color);
            
            // Set màu sắc
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            
            // Cấu hình position
            lineRenderer.positionCount = 0;
            lineRenderer.useWorldSpace = true;
            
            // Cấu hình rendering cho 2D
            lineRenderer.sortingLayerName = "Default";
            lineRenderer.sortingOrder = 1; // Hiển thị trên cells nhưng dưới dots
            
            // Cấu hình alignment và texture mode
            lineRenderer.alignment = LineAlignment.TransformZ;
            lineRenderer.textureMode = LineTextureMode.Tile;
            
            // Tắt shadows
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            
            lineRenderers.Add(pairId, lineRenderer);
        }
        
        private int GetDotKey(int pairId, bool isStart)
        {
            return pairId * 2 + (isStart ? 0 : 1);
        }
        
        private void Update()
        {
<<<<<<< HEAD
            // Kiểm tra deadlock trong quá trình tạo puzzle
            if (isGenerating && Time.realtimeSinceStartup - generationStartTime > GENERATION_TIMEOUT)
            {
                Debug.LogError("DEADLOCK DETECTED! Force stopping generation...");
                isGenerating = false;
                failedAttempts++;
                
                // Tạo emergency puzzle
                if (dotPairs == null || dotPairs.Count == 0)
                {
                    CreateEmergencyPuzzle();
                }
            }
            
=======
>>>>>>> 609294f (Added DotsConnect Minigame)
            HandleInput();
            
            // Debug: Nhấn D để debug LineRenderer
            if (Input.GetKeyDown(KeyCode.D))
            {
                DebugLineRenderers();
            }
            
            // Debug: Nhấn L để list available shaders
            if (Input.GetKeyDown(KeyCode.L))
            {
                LineRendererMaterialHelper.ListAvailableShaders();
            }
<<<<<<< HEAD
            
            // Debug: Nhấn R để force reset
            if (Input.GetKeyDown(KeyCode.R))
            {
                Debug.Log("Force reset puzzle...");
                isGenerating = false;
                InitializePuzzle();
            }
=======
>>>>>>> 609294f (Added DotsConnect Minigame)
        }
        
        /// <summary>
        /// Debug thông tin LineRenderer
        /// </summary>
        private void DebugLineRenderers()
        {
            Debug.Log("=== DEBUG LINE RENDERERS ===");
            foreach (var kvp in lineRenderers)
            {
                LineRenderer lr = kvp.Value;
                Debug.Log($"Line {kvp.Key}:");
                Debug.Log($"  - Active: {lr.gameObject.activeSelf}");
                Debug.Log($"  - Enabled: {lr.enabled}");
                Debug.Log($"  - Position Count: {lr.positionCount}");
                Debug.Log($"  - Width: {lr.startWidth} -> {lr.endWidth}");
                Debug.Log($"  - Color: {lr.startColor} -> {lr.endColor}");
                Debug.Log($"  - Material: {lr.material?.name}");
                Debug.Log($"  - Shader: {lr.material?.shader?.name}");
                Debug.Log($"  - Sorting Order: {lr.sortingOrder}");
                Debug.Log($"  - Use World Space: {lr.useWorldSpace}");
                
                if (lr.positionCount > 0)
                {
                    Debug.Log($"  - First Position: {lr.GetPosition(0)}");
                    Debug.Log($"  - Last Position: {lr.GetPosition(lr.positionCount - 1)}");
                }
            }
        }
        
        /// <summary>
        /// Xử lý input từ người chơi
        /// </summary>
        private void HandleInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                OnTouchStart();
            }
            else if (Input.GetMouseButton(0) && isDragging)
            {
                OnTouchDrag();
            }
            else if (Input.GetMouseButtonUp(0) && isDragging)
            {
                OnTouchEnd();
            }
        }
        
        private void OnTouchStart()
        {
            Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            
            if (hit.collider != null)
            {
                DotComponent dotComponent = hit.collider.GetComponent<DotComponent>();
                
                if (dotComponent != null)
                {
                    // Bắt đầu kéo từ một dot
                    StartDragging(dotComponent);
                }
            }
        }
        
        private void StartDragging(DotComponent dotComponent)
        {
            isDragging = true;
            currentPair = dotPairs[dotComponent.pairId];
            currentPath = new List<GridCell>();
            startDotObject = dotComponent.gameObject;
            
            // Nếu đã có đường nối, xóa đi
            if (currentPair.isCompleted)
            {
                ClearPath(currentPair.pairId);
                currentPair.isCompleted = false;
            }
            
            // Thêm điểm bắt đầu vào path
            currentPath.Add(dotComponent.gridCell);
            
            // Lấy LineRenderer
            currentLineRenderer = lineRenderers[currentPair.pairId];
            UpdateLineRenderer();
        }
        
        private void OnTouchDrag()
        {
            Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            GridCell cell = WorldToGrid(mousePos);
            
            // Kiểm tra cell hợp lệ
            if (!IsValidCell(cell))
            {
                return;
            }
            
            // Kiểm tra cell đã có trong path chưa
            if (currentPath.Contains(cell))
            {
                // Nếu quay lại cell trước đó, xóa các cell sau nó
                int index = currentPath.IndexOf(cell);
                if (index < currentPath.Count - 1)
                {
                    currentPath.RemoveRange(index + 1, currentPath.Count - index - 1);
                    UpdateLineRenderer();
                }
                return;
            }
            
            // Kiểm tra cell có thể đi qua không
            if (!CanMoveTo(cell))
            {
                return;
            }
            
            // Kiểm tra cell có kề với cell cuối cùng không
            if (!IsAdjacentTo(cell, currentPath[currentPath.Count - 1]))
            {
                return;
            }
            
            // Thêm cell vào path
            currentPath.Add(cell);
            UpdateLineRenderer();
        }
        
        private void OnTouchEnd()
        {
            if (currentPath.Count < 2)
            {
                CancelDragging();
                return;
            }
            
            // Kiểm tra có kết thúc ở dot đúng không
            GridCell lastCell = currentPath[currentPath.Count - 1];
            GridCell targetDot = currentPath[0] == currentPair.startDot ? currentPair.endDot : currentPair.startDot;
            
            if (lastCell == targetDot)
            {
                // Hoàn thành nối cặp dots
                CompletePair();
            }
            else
            {
                // Không kết thúc đúng, xóa đường
                CancelDragging();
            }
        }
        
        private void CompletePair()
        {
            // Đánh dấu đường đi trên board
            foreach (var cell in currentPath)
            {
<<<<<<< HEAD
                // Chỉ ghi đường lên ô trống (không ghi đè lên các dot âm)
                if (board[cell.x, cell.y] == 0)
=======
                if (board[cell.x, cell.y] == 0 || board[cell.x, cell.y] < 0)
>>>>>>> 609294f (Added DotsConnect Minigame)
                {
                    board[cell.x, cell.y] = currentPair.pairId + 1;
                }
            }
            
            currentPair.isCompleted = true;
            
            // Gọi event
            onDotPairConnected?.Invoke(currentPair.pairId, currentPair.color);
            
            Debug.Log($"Đã hoàn thành cặp {currentPair.pairId}");
            
            // Reset state
            isDragging = false;
            currentPair = null;
            currentPath = null;
            currentLineRenderer = null;
            
            // Kiểm tra hoàn thành tất cả
            CheckPuzzleCompletion();
        }
        
        private void CancelDragging()
        {
            if (currentLineRenderer != null)
            {
                currentLineRenderer.positionCount = 0;
            }
            
            isDragging = false;
            currentPair = null;
            currentPath = null;
            currentLineRenderer = null;
        }
        
        private void ClearPath(int pairId)
        {
            // Xóa đường đi trên board
            for (int x = 0; x < config.boardWidth; x++)
            {
                for (int y = 0; y < config.boardHeight; y++)
                {
                    if (board[x, y] == pairId + 1)
                    {
                        board[x, y] = 0;
                    }
                }
            }
            
            // Xóa LineRenderer
            if (lineRenderers.ContainsKey(pairId))
            {
                lineRenderers[pairId].positionCount = 0;
            }
        }
        
        private void UpdateLineRenderer()
        {
            if (currentLineRenderer == null || currentPath == null)
            {
                return;
            }
            
            currentLineRenderer.positionCount = currentPath.Count;
            
            float offsetX = -(config.boardWidth - 1) * cellSize / 2f;
            float offsetY = -(config.boardHeight - 1) * cellSize / 2f;
            
            for (int i = 0; i < currentPath.Count; i++)
            {
                Vector3 position = new Vector3(
                    offsetX + currentPath[i].x * cellSize,
                    offsetY + currentPath[i].y * cellSize,
                    0f // Đặt z = 0 để cùng layer với dots và cells
                );
                currentLineRenderer.SetPosition(i, position);
            }
        }
        
        private GridCell WorldToGrid(Vector2 worldPos)
        {
            float offsetX = -(config.boardWidth - 1) * cellSize / 2f;
            float offsetY = -(config.boardHeight - 1) * cellSize / 2f;
            
            int x = Mathf.RoundToInt((worldPos.x - offsetX) / cellSize);
            int y = Mathf.RoundToInt((worldPos.y - offsetY) / cellSize);
            
            return new GridCell(x, y);
        }
        
        private bool IsValidCell(GridCell cell)
        {
            return cell.x >= 0 && cell.x < config.boardWidth &&
                   cell.y >= 0 && cell.y < config.boardHeight;
        }
        
        private bool CanMoveTo(GridCell cell)
        {
            if (!IsValidCell(cell))
            {
                return false;
            }
            
            int cellValue = board[cell.x, cell.y];
            
            // Có thể đi qua ô trống hoặc ô là dot đích
            GridCell targetDot = currentPath[0] == currentPair.startDot ? currentPair.endDot : currentPair.startDot;
<<<<<<< HEAD

            // Nếu ô là một dot (âm), chỉ cho phép đi nếu đó là dot đích thuộc cặp hiện tại
            if (cellValue < 0)
            {
                return (cell == targetDot && cellValue == -(currentPair.pairId + 1));
            }

            // Nếu ô đã có đường của cặp khác (dương), không cho đi
            if (cellValue > 0)
            {
                return false;
            }

            // Cuối cùng, cho đi nếu ô trống
            return cellValue == 0;
        }

        /// <summary>
        /// Kiểm tra ô có chứa dot (start hoặc end của bất kỳ cặp nào)
        /// </summary>
        private bool IsDotCell(GridCell cell)
        {
            if (dotPairs == null) return false;
            foreach (var p in dotPairs)
            {
                if (p.startDot == cell || p.endDot == cell) return true;
            }
            return false;
=======
            
            return cellValue == 0 || 
                   (cell == targetDot && cellValue == -(currentPair.pairId + 1));
>>>>>>> 609294f (Added DotsConnect Minigame)
        }
        
        private bool IsAdjacentTo(GridCell cell, GridCell other)
        {
            int dx = Mathf.Abs(cell.x - other.x);
            int dy = Mathf.Abs(cell.y - other.y);
            
            return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
        }
        
        private void CheckPuzzleCompletion()
        {
            foreach (var pair in dotPairs)
            {
                if (!pair.isCompleted)
                {
                    return; // Vẫn còn cặp chưa hoàn thành
                }
            }
            
            // Tất cả đã hoàn thành
            Debug.Log("🎉 Hoàn thành puzzle!");
            onPuzzleCompleted?.Invoke();
        }
        
        /// <summary>
        /// Reset puzzle hiện tại
        /// </summary>
        public void ResetPuzzle()
        {
            // Xóa tất cả đường nối
            foreach (var pair in dotPairs)
            {
                if (pair.isCompleted)
                {
                    ClearPath(pair.pairId);
                    pair.isCompleted = false;
                }
            }
            
            // Reset board (chỉ giữ lại dots)
            for (int x = 0; x < config.boardWidth; x++)
            {
                for (int y = 0; y < config.boardHeight; y++)
                {
                    if (board[x, y] > 0)
                    {
                        board[x, y] = 0;
                    }
                }
            }
            
            CancelDragging();
            
            Debug.Log("Đã reset puzzle");
        }
        
        /// <summary>
        /// Tạo puzzle mới
        /// </summary>
        public void NewPuzzle()
        {
            InitializePuzzle();
        }
    }
<<<<<<< HEAD

=======
    
>>>>>>> 609294f (Added DotsConnect Minigame)
    /// <summary>
    /// Component gắn vào mỗi dot để nhận diện
    /// </summary>
    public class DotComponent : MonoBehaviour
    {
        public int pairId;
        public GridCell gridCell;
        public bool isStart;
    }
<<<<<<< HEAD
}
=======
}
>>>>>>> 609294f (Added DotsConnect Minigame)
