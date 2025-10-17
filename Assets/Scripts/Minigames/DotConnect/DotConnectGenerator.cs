using System.Collections.Generic;
using UnityEngine;

namespace MiniGames.DotConnect
{
    /// <summary>
    /// Thuật toán tạo puzzle Dot-Connect có thể giải được với độ khó nhất định
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
        
        public DotConnectGenerator(DotConnectConfig config)
        {
            this.config = config;
            this.random = new System.Random();
        }
        
        /// <summary>
        /// Tạo puzzle mới với các cặp dots có thể giải được
        /// </summary>
        public List<DotPair> GeneratePuzzle()
        {
            List<DotPair> dotPairs = null;
            int attempts = 0;
            
            while (attempts < config.maxGenerationAttempts)
            {
                attempts++;
                dotPairs = TryGeneratePuzzle();
                
                if (dotPairs != null)
                {
                    Debug.Log($"Đã tạo puzzle sau {attempts} lần thử");
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
            
            // Thử tạo từng cặp dots
            for (int i = 0; i < config.numberOfDotPairs; i++)
            {
                // Tìm đường đi và đặt dots
                List<GridCell> path = FindValidPath(board, i + 1);
                
                if (path == null || path.Count < 2)
                {
                    return null; // Không tìm được đường đi hợp lệ
                }
                
                // Kiểm tra độ khó của đường đi
                if (!IsPathDifficultEnough(path))
                {
                    // Nếu đường quá đơn giản, thử lại
                    i--;
                    continue;
                }
                
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
            
            // Kiểm tra puzzle có thể giải được không
            if (IsPuzzleSolvable(dotPairs, board))
            {
                return dotPairs;
            }
            
            return null;
        }
        
        /// <summary>
        /// Tìm đường đi hợp lệ cho một cặp dots
        /// </summary>
        private List<GridCell> FindValidPath(int[,] board, int pairId)
        {
            // Lấy danh sách các ô trống
            List<GridCell> emptyCells = GetEmptyCells(board);
            
            if (emptyCells.Count < 2)
            {
                return null;
            }
            
            // Thử nhiều lần để tìm đường đi tốt
            for (int attempt = 0; attempt < 50; attempt++)
            {
                // Chọn ngẫu nhiên điểm bắt đầu
                GridCell start = emptyCells[random.Next(emptyCells.Count)];
                
                // Tìm đường đi từ điểm bắt đầu
                List<GridCell> path = GenerateRandomPath(board, start, pairId);
                
                if (path != null && path.Count >= 3)
                {
                    return path;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Tạo đường đi ngẫu nhiên từ điểm bắt đầu
        /// </summary>
        private List<GridCell> GenerateRandomPath(int[,] board, GridCell start, int pairId)
        {
            List<GridCell> path = new List<GridCell>();
            path.Add(start);
            
            GridCell current = start;
            int minPathLength = Mathf.Max(3, config.minimumDifficulty);
            int maxPathLength = (config.boardWidth * config.boardHeight) / config.numberOfDotPairs;
            int targetLength = random.Next(minPathLength, maxPathLength + 1);
            
            HashSet<GridCell> visited = new HashSet<GridCell>();
            visited.Add(start);
            
            for (int i = 0; i < targetLength; i++)
            {
                // Lấy danh sách hướng có thể đi
                List<Vector2Int> availableDirections = GetAvailableDirections(board, current, visited, pairId);
                
                if (availableDirections.Count == 0)
                {
                    break; // Không còn hướng nào để đi
                }
                
                // Chọn ngẫu nhiên một hướng
                Vector2Int direction = availableDirections[random.Next(availableDirections.Count)];
                GridCell next = new GridCell(current.x + direction.x, current.y + direction.y);
                
                path.Add(next);
                visited.Add(next);
                current = next;
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
        /// Kiểm tra đường đi có đủ khó không
        /// </summary>
        private bool IsPathDifficultEnough(List<GridCell> path)
        {
            if (path.Count < 3)
            {
                return false;
            }
            
            // Đếm số lượt rẽ (càng nhiều rẽ, càng khó)
            int turns = 0;
            for (int i = 1; i < path.Count - 1; i++)
            {
                Vector2Int dir1 = new Vector2Int(path[i].x - path[i - 1].x, path[i].y - path[i - 1].y);
                Vector2Int dir2 = new Vector2Int(path[i + 1].x - path[i].x, path[i + 1].y - path[i].y);
                
                if (dir1 != dir2)
                {
                    turns++;
                }
            }
            
            // Độ khó tối thiểu: số rẽ >= minimumDifficulty / 2
            return turns >= config.minimumDifficulty / 2;
        }
        
        /// <summary>
        /// Kiểm tra puzzle có thể giải được không bằng backtracking
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
            
            // Thử giải bằng backtracking
            return SolveRecursive(testBoard, dotPairs, 0);
        }
        
        private bool SolveRecursive(int[,] board, List<DotPair> dotPairs, int pairIndex)
        {
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
            if (SolveRecursive(board, dotPairs, pairIndex + 1))
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
        /// </summary>
        private List<DotPair> GenerateSimplePuzzle()
        {
            List<DotPair> dotPairs = new List<DotPair>();
            List<GridCell> usedCells = new List<GridCell>();
            
            for (int i = 0; i < config.numberOfDotPairs; i++)
            {
                GridCell start, end;
                
                // Tìm 2 ô trống ngẫu nhiên
                do
                {
                    start = new GridCell(random.Next(config.boardWidth), random.Next(config.boardHeight));
                } while (usedCells.Contains(start));
                
                do
                {
                    end = new GridCell(random.Next(config.boardWidth), random.Next(config.boardHeight));
                } while (usedCells.Contains(end) || end == start);
                
                usedCells.Add(start);
                usedCells.Add(end);
                
                Color color = config.dotColors[i % config.dotColors.Length];
                DotPair pair = new DotPair(i, start, end, color);
                dotPairs.Add(pair);
            }
            
            return dotPairs;
        }
    }
}
