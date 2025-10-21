using UnityEngine;
using UnityEngine.UI;

namespace MadKnight.UI
{
    /// <summary>
    /// Helper script to auto-setup Grid Layout for inventory slots
    /// Attach this to the SlotContainer GameObject
    /// </summary>
    [RequireComponent(typeof(GridLayoutGroup))]
    public class InventoryGridSetup : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private int _columnsPerRow = 5;
        [SerializeField] private Vector2 _cellSize = new Vector2(80, 80);
        [SerializeField] private Vector2 _spacing = new Vector2(10, 10);
        [SerializeField] private Vector2 _padding = new Vector2(10, 10);

        private GridLayoutGroup _gridLayout;

        private void Awake()
        {
            SetupGridLayout();
        }

        private void OnValidate()
        {
            SetupGridLayout();
        }

        private void SetupGridLayout()
        {
            _gridLayout = GetComponent<GridLayoutGroup>();
            
            if (_gridLayout == null) return;

            // Remove Horizontal/Vertical Layout Group if exists
            var horizontalLayout = GetComponent<HorizontalLayoutGroup>();
            if (horizontalLayout != null)
            {
                DestroyImmediate(horizontalLayout);
            }

            var verticalLayout = GetComponent<VerticalLayoutGroup>();
            if (verticalLayout != null)
            {
                DestroyImmediate(verticalLayout);
            }

            // Setup Grid Layout
            _gridLayout.cellSize = _cellSize;
            _gridLayout.spacing = _spacing;
            _gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            _gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            _gridLayout.childAlignment = TextAnchor.UpperLeft;
            _gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            _gridLayout.constraintCount = _columnsPerRow;

            // Setup padding
            _gridLayout.padding = new RectOffset(
                (int)_padding.x, 
                (int)_padding.x, 
                (int)_padding.y, 
                (int)_padding.y
            );
        }

        /// <summary>
        /// Call this to update grid settings at runtime
        /// </summary>
        public void UpdateGridLayout(int columns, Vector2 cellSize)
        {
            _columnsPerRow = columns;
            _cellSize = cellSize;
            SetupGridLayout();
        }
    }
}
