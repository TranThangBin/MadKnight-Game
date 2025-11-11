using UnityEngine;

namespace MadKnight
{
    public class ReduceAlpha : MonoBehaviour
    {
     [Range(0f, 1f)]
    public float fadedAlpha = 0.3f;   // Alpha khi chạm
    public float fadeSpeed = 3f;      // Tốc độ mờ dần

    private SpriteRenderer sprite;
    private Color originalColor;
    private Color targetColor;
    private bool isColliding = false;

    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        if (sprite == null)
        {
            Debug.LogError("Không tìm thấy SpriteRenderer trên object này!");
            return;
        }

        originalColor = sprite.color;
        targetColor = new Color(originalColor.r, originalColor.g, originalColor.b, fadedAlpha);
    }

    void Update()
    {
        if (sprite == null) return;

        if (isColliding)
        {
            sprite.color = Color.Lerp(sprite.color, targetColor, Time.deltaTime * fadeSpeed);
        }
        else
        {
            sprite.color = Color.Lerp(sprite.color, originalColor, Time.deltaTime * fadeSpeed);
        }
    }

     

    // Nếu dùng Trigger thay cho Collision (ví dụ hitbox mềm)
    void OnTriggerEnter2D(Collider2D other)
    {
        isColliding = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        isColliding = false;
    }    
    }
}
