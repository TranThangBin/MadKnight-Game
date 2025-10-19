using System.Collections.Generic;
using UnityEngine;

namespace MiniGames.DotConnect
{
    /// <summary>
    /// Thuật toán tạo puzzle Dot-Connect có thể giải được với độ khó nhất định
    /// Cải tiến: Tạo đường phức tạp hơn, nhiều rẽ, tránh đường thẳng
    /// </summary>
    public class DotConnectGenerator
    {
        private DotConnectConfig config;
        private System.Random random;
        
        // Các hướng di chuyển: lên, xuống, trái, phải
        private static readonly Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),   // Lên
            new Vector2Int(0, -1),  // Xuống
            new Vector2Int(-1, 0),  // Trái
            new Vector2Int(1, 0)    // Phải
        };
        
        // Hệ số khó
        private const int MIN_TURNS_PER_PATH = 3; // Tối thiểu 3 lượt rẽ
        private const float PREFER_TURN_PROBABILITY = 0.7f; // 70% khả năng rẽ thay vì đi thẳng
        private const int MIN_PATH_LENGTH = 5; // Độ dài tối thiểu của đường
        
        public DotConnectGenerator(DotConnectConfig config)
        {
            this.config = config;
            this.random = new System.Random();
        }
        
        /// <summary>
        /// Tạo puzzle mới với các cặp dots có thể giải được
        /// Với timeout protection
        /// </summary>
        public List<DotPair> GeneratePuzzle()
        {
            List<DotPair> dotPairs = null;
            int attempts = 0;
            float startTime = Time.realtimeSinceStartup;
            const float TIMEOUT = 4.0f; // Timeout 4 giây
            
            while (attempts < config.maxGenerationAttempts)
            {
                // Kiểm tra timeout
                if (Time.realtimeSinceStartup - startTime > TIMEOUT)
                {
                    Debug.LogWarning($"Timeout sau {attempts} lần thử. Tạo puzzle đơn giản hơn...");
                    return GenerateSimplePuzzle();
                }
                
                attempts++;
                dotPairs = TryGeneratePuzzle();
                
                if (dotPairs != null)
                {
                    Debug.Log($"Đã tạo puzzle khó sau {attempts} lần thử");
                    return dotPairs;
                }
            }
            
            Debug.LogWarning("Không thể tạo puzzle với độ khó yêu cầu. Tạo puzzle đơn giản hơn...");
            return GenerateSimplePuzzle();
        }
        
        private List<DotPair> TryGeneratePuzzle()
        {
            // Tạo bàn cờ trống
            int[,] board = new int[config.boardWidth, config.boardHeight];
            List<DotPair> dotPairs = new List<DotPair>();
            
            // Thử tạo từng cặp dots với yêu cầu khó hơn
            int consecutiveFailures = 0;
            
            for (int i = 0; i < config.numberOfDotPairs; i++)
            {
                // Tìm đường đi và đặt dots
                List<GridCell> path = FindValidPath(board, i + 1);
                
                if (path == null || path.Count < MIN_PATH_LENGTH)
                {
                    consecutiveFailures++;
                    
                    // Nếu thất bại quá 3 lần liên tiếp, từ bỏ
                    if (consecutiveFailures > 3)
                    {
                        return null;
                    }
                    
                    i--; // Thử lại
                    continue;
                }
                
                // Kiểm tra độ khó của đường đi
                if (!IsPathDifficultEnough(path))
                {
                    consecutiveFailures++;
                    
                    if (consecutiveFailures > 3)
                    {
                        return null;
                    }
                    
                    i--; // Nếu đường quá đơn giản, thử lại
                    continue;
                }
                
                // Reset counter khi thành công
                consecutiveFailures = 0;
                
                // Đánh dấu đường đi trên bàn cờ
                foreach (var cell in path)
                {
                    board[cell.x, cell.y] = i + 1;
                }
                
                // Tạo màu ngẫu nhiên
                Color color = config.dotColors[i % config.dotColors.Length];
                
                // Tạo cặp dots
                DotPair pair = new DotPair(
                    i,
                    path[0],
                    path[path.Count - 1],
                    color
                );
                
                dotPairs.Add(pair);
            }
            
            // Kiểm tra puzzle có thể giải được không với timeout
            float solveStartTime = Time.realtimeSinceStartup;
            bool isSolvable = false;
            
            try
            {
                isSolvable = IsPuzzleSolvable(dotPairs, board);
                
                // Kiểm tra timeout trong quá trình solve
                if (Time.realtimeSinceStartup - solveStartTime > 1.0f)
                {
                    Debug.LogWarning("Timeout khi kiểm tra puzzle solvable");
                    return null;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Lỗi khi kiểm tra solvable: {ex.Message}");
                return null;
            }
            
            if (isSolvable)
            {
                return dotPairs;
            }
            
            return null;
        }
        
        /// <summary>
        /// Tìm đường đi hợp lệ cho một cặp dots với yêu cầu khó hơn
        /// </summary>
        private List<GridCell> FindValidPath(int[,] board, int pairId)
        {
            // Lấy danh sách các ô trống
            List<GridCell> emptyCells = GetEmptyCells(board);
            
            if (emptyCells.Count < MIN_PATH_LENGTH)
            {
                return null;
            }
            
            // Thử nhiều lần để tìm đường đi tốt
            int maxAttempts = Mathf.Min(100, emptyCells.Count * 2);
            
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                // Chọn ngẫu nhiên điểm bắt đầu
                GridCell start = emptyCells[random.Next(emptyCells.Count)];
                
                // Tìm đường đi từ điểm bắt đầu
                List<GridCell> path = GenerateRandomPath(board, start, pairId);
                
                if (path != null && path.Count >= MIN_PATH_LENGTH && IsPathDifficultEnough(path))
                {
                    return path;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Tạo đường đi ngẫu nhiên từ điểm bắt đầu với nhiều rẽ và phức tạp hơn
        /// </summary>
        private List<GridCell> GenerateRandomPath(int[,] board, GridCell start, int pairId)
        {
            List<GridCell> path = new List<GridCell>();
            path.Add(start);
            
            GridCell current = start;
            int minPathLength = Mathf.Max(MIN_PATH_LENGTH, config.minimumDifficulty);
            int maxPathLength = (config.boardWidth * config.boardHeight) / config.numberOfDotPairs;
            int targetLength = random.Next(minPathLength, maxPathLength + 1);
            
            HashSet<GridCell> visited = new HashSet<GridCell>();
            visited.Add(start);
            
            Vector2Int? lastDirection = null;
            int straightCount = 0;
            int turnCount = 0;
            
            for (int i = 0; i < targetLength; i++)
            {
                // Lấy danh sách hướng có thể đi
                List<Vector2Int> availableDirections = GetAvailableDirections(board, current, visited, pairId);
                
                if (availableDirections.Count == 0)
                {
                    break; // Không còn hướng nào để đi
                }
                
                Vector2Int chosenDirection;
                
                // Ưu tiên rẽ để tăng độ khó
                if (lastDirection.HasValue && straightCount < 2)
                {
                    // Nếu đi thẳng chưa đủ 2 bước, cho phép đi thẳng tiếp
                    List<Vector2Int> turnDirections = availableDirections.FindAll(d => d != lastDirection.Value);
                    
                    if (turnDirections.Count > 0 && (float)random.NextDouble() < PREFER_TURN_PROBABILITY)
                    {
                        // Ưu tiên rẽ (70% khả năng)
                        chosenDirection = turnDirections[random.Next(turnDirections.Count)];
                        turnCount++;
                        straightCount = 0;
                    }
                    else if (availableDirections.Contains(lastDirection.Value))
                    {
                        // Đi thẳng tiếp
                        chosenDirection = lastDirection.Value;
                        straightCount++;
                    }
                    else
                    {
                        // Buộc phải rẽ
                        chosenDirection = availableDirections[random.Next(availableDirections.Count)];
                        turnCount++;
                        straightCount = 0;
                    }
                }
                else
                {
                    // Chọn ngẫu nhiên khi bắt đầu hoặc đi thẳng quá nhiều
                    chosenDirection = availableDirections[random.Next(availableDirections.Count)];
                    
                    if (lastDirection.HasValue && chosenDirection != lastDirection.Value)
                    {
                        turnCount++;
                    }
                    
                    straightCount = 1;
                }
                
                GridCell next = new GridCell(current.x + chosenDirection.x, current.y + chosenDirection.y);
                
                path.Add(next);
                visited.Add(next);
                current = next;
                lastDirection = chosenDirection;
            }
            
            // Kiểm tra đủ số lượt rẽ
            if (path.Count >= MIN_PATH_LENGTH && turnCount >= MIN_TURNS_PER_PATH)
            {
                return path;
            }
            
            return path.Count >= 3 ? path : null;
        }
        
        /// <summary>
        /// Lấy các hướng có thể di chuyển từ ô hiện tại
        /// </summary>
        private List<Vector2Int> GetAvailableDirections(int[,] board, GridCell current, HashSet<GridCell> visited, int pairId)
        {
            List<Vector2Int> available = new List<Vector2Int>();
            
            foreach (var dir in directions)
            {
                int newX = current.x + dir.x;
                int newY = current.y + dir.y;
                
                // Kiểm tra trong giới hạn
                if (newX < 0 || newX >= config.boardWidth || newY < 0 || newY >= config.boardHeight)
                {
                    continue;
                }
                
                GridCell next = new GridCell(newX, newY);
                
                // Kiểm tra ô trống và chưa được thăm
                if (board[newX, newY] == 0 && !visited.Contains(next))
                {
                    available.Add(dir);
                }
            }
            
            return available;
        }
        
        /// <summary>
        /// Lấy danh sách các ô trống
        /// </summary>
        private List<GridCell> GetEmptyCells(int[,] board)
        {
            List<GridCell> emptyCells = new List<GridCell>();
            
            for (int x = 0; x < config.boardWidth; x++)
            {
                for (int y = 0; y < config.boardHeight; y++)
                {
                    if (board[x, y] == 0)
                    {
                        emptyCells.Add(new GridCell(x, y));
                    }
                }
            }
            
            return emptyCells;
        }
        
        /// <summary>
        /// Kiểm tra đường đi có đủ khó không - Tiêu chí nghiêm ngặt hơn
        /// </summary>
        private bool IsPathDifficultEnough(List<GridCell> path)
        {
            if (path.Count < MIN_PATH_LENGTH)
            {
                return false;
            }
            
            // Đếm số lượt rẽ (càng nhiều rẽ, càng khó)
            int turns = 0;
            int maxStraightSegment = 0;
            int currentStraight = 0;
            
            for (int i = 1; i < path.Count - 1; i++)
            {
                Vector2Int dir1 = new Vector2Int(path[i].x - path[i - 1].x, path[i].y - path[i - 1].y);
                Vector2Int dir2 = new Vector2Int(path[i + 1].x - path[i].x, path[i + 1].y - path[i].y);
                
                if (dir1 != dir2)
                {
                    turns++;
                    maxStraightSegment = Mathf.Max(maxStraightSegment, currentStraight);
                    currentStraight = 0;
                }
                else
                {
                    currentStraight++;
                }
            }
            
            // Kiểm tra:
            // 1. Đủ số lượt rẽ tối thiểu
            // 2. Không có đoạn thẳng quá dài (tối đa 3 ô)
            // 3. Tỷ lệ rẽ/tổng độ dài > 0.3 (ít nhất 30% là rẽ)
            bool hasEnoughTurns = turns >= MIN_TURNS_PER_PATH;
            bool noLongStraight = maxStraightSegment <= 3;
            bool goodTurnRatio = (float)turns / path.Count >= 0.3f;
            
            return hasEnoughTurns && noLongStraight && goodTurnRatio;
        }
        
        /// <summary>
        /// Kiểm tra puzzle có thể giải được không bằng backtracking với timeout
        /// Thử nhiều thứ tự cặp pairs để tăng khả năng phát hiện solvable
        /// </summary>
        private bool IsPuzzleSolvable(List<DotPair> dotPairs, int[,] solutionBoard)
        {
            // Tạo bàn cờ mới để thử giải
            int[,] testBoard = new int[config.boardWidth, config.boardHeight];
            
            // Đặt các dots lên bàn cờ
            foreach (var pair in dotPairs)
            {
                testBoard[pair.startDot.x, pair.startDot.y] = -(pair.pairId + 1); // Dấu âm để phân biệt dot
                testBoard[pair.endDot.x, pair.endDot.y] = -(pair.pairId + 1);
            }
            
            // Thử giải bằng backtracking với giới hạn depth
            int maxDepth = dotPairs.Count * 10; // Giới hạn số bước để tránh infinite loop
            
            // Thử với thứ tự ban đầu
            if (SolveRecursive(CloneBoard(testBoard), dotPairs, 0, 0, maxDepth))
            {
                return true;
            }
            
            // Nếu thất bại, thử với các thứ tự khác (shuffle nhẹ)
            // Tạo danh sách chỉ số để shuffle
            List<int> indices = new List<int>();
            for (int i = 0; i < dotPairs.Count; i++)
            {
                indices.Add(i);
            }
            
            // Thử tối đa 5 lần với thứ tự khác nhau
            for (int attempt = 0; attempt < 5; attempt++)
            {
                // Shuffle indices
                for (int i = indices.Count - 1; i > 0; i--)
                {
                    int j = random.Next(i + 1);
                    int temp = indices[i];
                    indices[i] = indices[j];
                    indices[j] = temp;
                }
                
                // Tạo danh sách pairs theo thứ tự mới
                List<DotPair> shuffledPairs = new List<DotPair>();
                foreach (int idx in indices)
                {
                    shuffledPairs.Add(dotPairs[idx]);
                }
                
                if (SolveRecursive(CloneBoard(testBoard), shuffledPairs, 0, 0, maxDepth))
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Clone bàn cờ để thử nhiều lần
        /// </summary>
        private int[,] CloneBoard(int[,] original)
        {
            int[,] clone = new int[config.boardWidth, config.boardHeight];
            for (int x = 0; x < config.boardWidth; x++)
            {
                for (int y = 0; y < config.boardHeight; y++)
                {
                    clone[x, y] = original[x, y];
                }
            }
            return clone;
        }
        
        private bool SolveRecursive(int[,] board, List<DotPair> dotPairs, int pairIndex, int depth, int maxDepth)
        {
            // Kiểm tra depth limit
            if (depth > maxDepth)
            {
                return false; // Timeout
            }
            
            if (pairIndex >= dotPairs.Count)
            {
                return true; // Đã nối hết tất cả các cặp
            }
            
            DotPair pair = dotPairs[pairIndex];
            
            // Thử tìm đường từ startDot đến endDot
            List<GridCell> path = FindPathBFS(board, pair.startDot, pair.endDot, pair.pairId + 1);
            
            if (path == null)
            {
                return false; // Không tìm được đường
            }
            
            // Đánh dấu đường đi
            foreach (var cell in path)
            {
                if (board[cell.x, cell.y] == 0)
                {
                    board[cell.x, cell.y] = pair.pairId + 1;
                }
            }
            
            // Thử giải cặp tiếp theo
            if (SolveRecursive(board, dotPairs, pairIndex + 1, depth + 1, maxDepth))
            {
                return true;
            }
            
            // Backtrack: xóa đường đi
            foreach (var cell in path)
            {
                if (board[cell.x, cell.y] == pair.pairId + 1)
                {
                    board[cell.x, cell.y] = 0;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Tìm đường đi ngắn nhất bằng BFS
        /// </summary>
        private List<GridCell> FindPathBFS(int[,] board, GridCell start, GridCell end, int pairId)
        {
            Queue<GridCell> queue = new Queue<GridCell>();
            Dictionary<GridCell, GridCell> parent = new Dictionary<GridCell, GridCell>();
            HashSet<GridCell> visited = new HashSet<GridCell>();
            
            queue.Enqueue(start);
            visited.Add(start);
            
            while (queue.Count > 0)
            {
                GridCell current = queue.Dequeue();
                
                if (current == end)
                {
                    // Tìm thấy đường đi, reconstruct path
                    return ReconstructPath(parent, start, end);
                }
                
                foreach (var dir in directions)
                {
                    int newX = current.x + dir.x;
                    int newY = current.y + dir.y;
                    
                    if (newX < 0 || newX >= config.boardWidth || newY < 0 || newY >= config.boardHeight)
                    {
                        continue;
                    }
                    
                    GridCell next = new GridCell(newX, newY);
                    
                    if (visited.Contains(next))
                    {
                        continue;
                    }
                    
                    int cellValue = board[newX, newY];
                    
                    // Có thể đi qua ô trống hoặc ô đích (dot cuối)
                    if (cellValue == 0 || (next == end && cellValue == -pairId))
                    {
                        queue.Enqueue(next);
                        visited.Add(next);
                        parent[next] = current;
                    }
                }
            }
            
            return null; // Không tìm thấy đường đi
        }
        
        private List<GridCell> ReconstructPath(Dictionary<GridCell, GridCell> parent, GridCell start, GridCell end)
        {
            List<GridCell> path = new List<GridCell>();
            GridCell current = end;
            
            while (current != start)
            {
                path.Add(current);
                current = parent[current];
            }
            
            path.Add(start);
            path.Reverse();
            
            return path;
        }
        
        /// <summary>
        /// Tạo puzzle đơn giản khi không thể tạo puzzle phức tạp
        /// Method công khai để có thể gọi từ bên ngoài
        /// </summary>
        public List<DotPair> GenerateSimplePuzzle()
        {
            List<DotPair> dotPairs = new List<DotPair>();
            List<GridCell> usedCells = new List<GridCell>();
            
            // Giảm số cặp nếu board quá nhỏ
            int actualPairCount = Mathf.Min(config.numberOfDotPairs, (config.boardWidth * config.boardHeight) / 4);
            
            for (int i = 0; i < actualPairCount; i++)
            {
                GridCell start, end;
                int attempts = 0;
                
                // Tìm 2 ô trống ngẫu nhiên với timeout
                do
                {
                    start = new GridCell(random.Next(config.boardWidth), random.Next(config.boardHeight));
                    attempts++;
                    
                    if (attempts > 100)
                    {
                        Debug.LogWarning("Không thể tìm đủ ô trống cho simple puzzle");
                        return dotPairs.Count > 0 ? dotPairs : CreateMinimalPuzzle();
                    }
                } while (usedCells.Contains(start));
                
                attempts = 0;
                do
                {
                    end = new GridCell(random.Next(config.boardWidth), random.Next(config.boardHeight));
                    attempts++;
                    
                    if (attempts > 100)
                    {
                        Debug.LogWarning("Không thể tìm đủ ô trống cho simple puzzle");
                        return dotPairs.Count > 0 ? dotPairs : CreateMinimalPuzzle();
                    }
                } while (usedCells.Contains(end) || end == start);
                
                usedCells.Add(start);
                usedCells.Add(end);
                
                Color color = config.dotColors[i % config.dotColors.Length];
                DotPair pair = new DotPair(i, start, end, color);
                dotPairs.Add(pair);
            }
            
            return dotPairs;
        }
        
        /// <summary>
        /// Tạo puzzle tối giản nhất (chỉ 1-2 cặp)
        /// </summary>
        private List<DotPair> CreateMinimalPuzzle()
        {
            List<DotPair> dotPairs = new List<DotPair>();
            
            // Tạo 1 cặp ở 2 góc
            GridCell start = new GridCell(0, 0);
            GridCell end = new GridCell(config.boardWidth - 1, config.boardHeight - 1);
            Color color = config.dotColors[0];
            
            dotPairs.Add(new DotPair(0, start, end, color));
            
            Debug.Log("Đã tạo minimal puzzle với 1 cặp duy nhất");
            return dotPairs;
        }
    }
}
