using UnityEngine;

namespace MiniGames.DotConnect
{
    /// <summary>
    /// Utility để tạo material tối ưu cho LineRenderer trong 2D
    /// </summary>
    public static class LineRendererMaterialHelper
    {
        private static Material cachedMaterial;
        
        /// <summary>
        /// Tạo hoặc lấy material tối ưu cho LineRenderer 2D
        /// </summary>
        public static Material GetOptimizedMaterial(Color color)
        {
            // Thử các shader theo thứ tự ưu tiên
            string[] shaderNames = new string[]
            {
                "Unlit/Color",
                "Sprites/Default",
                "UI/Default",
                "Unlit/Transparent",
                "Legacy Shaders/Particles/Alpha Blended"
            };
            
            Shader shader = null;
            foreach (string shaderName in shaderNames)
            {
                shader = Shader.Find(shaderName);
                if (shader != null)
                {
                    Debug.Log($"[LineRendererMaterial] Sử dụng shader: {shaderName}");
                    break;
                }
            }
            
            if (shader == null)
            {
                Debug.LogError("[LineRendererMaterial] Không tìm thấy shader phù hợp!");
                return new Material(Shader.Find("Standard")); // Fallback
            }
            
            Material material = new Material(shader);
            material.color = color;
            
            // Cấu hình cho 2D rendering
            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }
            
            // Đảm bảo material có thể nhìn thấy
            material.renderQueue = 3000; // Transparent queue
            
            return material;
        }
        
        /// <summary>
        /// Tạo material với texture gradient (đẹp hơn)
        /// </summary>
        public static Material GetGradientMaterial(Color color)
        {
            Material material = GetOptimizedMaterial(color);
            
            // Tạo texture gradient đơn giản
            Texture2D texture = CreateGradientTexture(color);
            material.mainTexture = texture;
            
            return material;
        }
        
        private static Texture2D CreateGradientTexture(Color color)
        {
            int width = 64;
            int height = 1;
            Texture2D texture = new Texture2D(width, height);
            
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < width; i++)
            {
                // Gradient từ trong suốt ở giữa đến đậm ở mép
                float t = Mathf.Abs((i / (float)width) - 0.5f) * 2f;
                Color pixelColor = color;
                pixelColor.a = 1f - (t * 0.3f); // Giảm độ trong suốt ở mép
                pixels[i] = pixelColor;
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            texture.wrapMode = TextureWrapMode.Clamp;
            
            return texture;
        }
        
        /// <summary>
        /// Debug: Liệt kê tất cả shaders có sẵn
        /// </summary>
        public static void ListAvailableShaders()
        {
            string[] commonShaders = new string[]
            {
                "Unlit/Color",
                "Unlit/Transparent",
                "Sprites/Default",
                "UI/Default",
                "Standard",
                "Legacy Shaders/Particles/Alpha Blended",
                "Legacy Shaders/Particles/Additive"
            };
            
            Debug.Log("=== Available Shaders ===");
            foreach (string shaderName in commonShaders)
            {
                Shader shader = Shader.Find(shaderName);
                if (shader != null)
                {
                    Debug.Log($"✓ {shaderName}");
                }
                else
                {
                    Debug.Log($"✗ {shaderName} (not found)");
                }
            }
        }
    }
}
