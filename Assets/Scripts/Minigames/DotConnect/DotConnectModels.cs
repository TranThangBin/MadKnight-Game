using UnityEngine;

namespace MiniGames.DotConnect
{
    /// <summary>
    /// Đại diện cho một ô trên bàn cờ
    /// </summary>
    public struct GridCell
    {
        public int x;
        public int y;
        
        public GridCell(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        
        public override bool Equals(object obj)
        {
            if (obj is GridCell other)
            {
                return x == other.x && y == other.y;
            }
            return false;
        }
        
        public override int GetHashCode()
        {
            return x * 1000 + y;
        }
        
        public static bool operator ==(GridCell a, GridCell b)
        {
            return a.x == b.x && a.y == b.y;
        }
        
        public static bool operator !=(GridCell a, GridCell b)
        {
            return !(a == b);
        }
    }
    
    /// <summary>
    /// Đại diện cho một cặp dots cần nối
    /// </summary>
    public class DotPair
    {
        public int pairId;
        public GridCell startDot;
        public GridCell endDot;
        public Color color;
        public bool isCompleted;
        
        public DotPair(int id, GridCell start, GridCell end, Color color)
        {
            this.pairId = id;
            this.startDot = start;
            this.endDot = end;
            this.color = color;
            this.isCompleted = false;
        }
    }
}
