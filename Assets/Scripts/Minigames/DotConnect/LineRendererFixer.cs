using UnityEngine;

namespace MiniGames.DotConnect
{
    /// <summary>
    /// Helper script để fix LineRenderer không hiển thị
    /// Attach vào GameObject có LineRenderer component
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class LineRendererFixer : MonoBehaviour
    {
        private LineRenderer lineRenderer;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;
        
        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            FixLineRenderer();
        }
        
        private void FixLineRenderer()
        {
            if (lineRenderer == null) return;
            
            // Đảm bảo có material
            if (lineRenderer.material == null)
            {
                Material mat = new Material(Shader.Find("Unlit/Color"));
                if (mat.shader == null)
                {
                    mat = new Material(Shader.Find("Sprites/Default"));
                }
                mat.color = lineRenderer.startColor;
                lineRenderer.material = mat;
                
                if (showDebugInfo)
                    Debug.Log($"[LineRendererFixer] Đã tạo material mới cho {gameObject.name}");
            }
            
            // Đảm bảo shader không null
            if (lineRenderer.material.shader == null)
            {
                lineRenderer.material.shader = Shader.Find("Unlit/Color");
                if (showDebugInfo)
                    Debug.Log($"[LineRendererFixer] Đã set shader mới cho {gameObject.name}");
            }
            
            // Cấu hình cho 2D
            lineRenderer.useWorldSpace = true;
            lineRenderer.alignment = LineAlignment.TransformZ;
            
            // Sorting layer
            lineRenderer.sortingLayerName = "Default";
            lineRenderer.sortingOrder = 1;
            
            // Tắt shadows
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            
            if (showDebugInfo)
            {
                Debug.Log($"[LineRendererFixer] Đã fix LineRenderer: {gameObject.name}");
                Debug.Log($"  - Material: {lineRenderer.material.name}");
                Debug.Log($"  - Shader: {lineRenderer.material.shader?.name}");
                Debug.Log($"  - Width: {lineRenderer.startWidth}");
                Debug.Log($"  - Color: {lineRenderer.startColor}");
                Debug.Log($"  - Sorting Order: {lineRenderer.sortingOrder}");
                Debug.Log($"  - Position Count: {lineRenderer.positionCount}");
            }
        }
        
        // Editor only - để kiểm tra trong Scene view
        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (lineRenderer == null) return;
            
            // Vẽ debug gizmos cho các điểm của line
            Gizmos.color = Color.yellow;
            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                Vector3 pos = lineRenderer.GetPosition(i);
                Gizmos.DrawWireSphere(pos, 0.1f);
                
                if (i > 0)
                {
                    Vector3 prevPos = lineRenderer.GetPosition(i - 1);
                    Gizmos.DrawLine(prevPos, pos);
                }
            }
        }
        #endif
    }
}
