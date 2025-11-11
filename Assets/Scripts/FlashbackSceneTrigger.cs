using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class FlashbackSceneTrigger : MonoBehaviour
{
    [Header("Target Scene")]
    public string nextSceneName;
    public float loadDelayAfterFlash = 0.15f;

    [Header("Overlay")]
    public CanvasGroup overlayGroup;     // CanvasGroup trên FlashbackOverlay
    public Image overlayImage;           // Image con full-screen (để thay sprite)

    [Header("Flash Frames")]
    public List<Sprite> frames = new List<Sprite>(); // Kéo nhiều sprite vào
    public float flashIn = 0.06f;        // thời gian fade in mỗi frame
    public float hold = 0.04f;           // giữ frame sáng
    public float flashOut = 0.06f;       // thời gian fade out
    public int loops = 1;                // số vòng lặp toàn bộ frames
    public bool randomOrder = false;     // phát ngẫu nhiên
    public Color tint = Color.white;     // màu/tint cho ảnh flash

    [Header("Extra FX (optional)")]
    public AudioSource sfx;              // gán AudioSource nếu muốn
    public AudioClip stinger;            // âm thanh lúc bắt đầu flash
    public AudioClip whooshEach;         // âm thanh mỗi lần đổi frame

    bool triggered;

    void Reset()
    {
        // Thử tự tìm overlay trong cùng object
        overlayGroup = FindObjectOfType<CanvasGroup>();
        overlayImage = overlayGroup ? overlayGroup.GetComponentInChildren<Image>(true) : null;
    }

    void Awake()
    {
        if (overlayGroup) overlayGroup.alpha = 0f; // ẩn lúc đầu
        if (overlayImage) overlayImage.enabled = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (other.CompareTag("Player"))
        {
            triggered = true;
            StartCoroutine(DoFlashAndLoad());
        }
    }

    IEnumerator DoFlashAndLoad()
    {
        if (overlayGroup == null || overlayImage == null || frames.Count == 0)
        {
            Debug.LogWarning("[Flashback] Thiếu overlay hoặc frames → Load thẳng.");
            yield return new WaitForSecondsRealtime(loadDelayAfterFlash);
            SceneManager.LoadScene(nextSceneName);
            yield break;
        }

        overlayImage.enabled = true;
        overlayImage.color = tint;

        if (sfx && stinger) sfx.PlayOneShot(stinger);

        // Dùng thời gian thực để không bị ảnh hưởng Time.timeScale
        for (int l = 0; l < Mathf.Max(1, loops); l++)
        {
            int count = frames.Count;

            for (int i = 0; i < count; i++)
            {
                var sprite = frames[randomOrder ? Random.Range(0, count) : i];
                overlayImage.sprite = sprite;

                // Fade in
                yield return FadeCanvasGroup(overlayGroup, 0f, 1f, flashIn);

                if (sfx && whooshEach) sfx.PlayOneShot(whooshEach);

                // Hold
                yield return new WaitForSecondsRealtime(hold);

                // Fade out
                yield return FadeCanvasGroup(overlayGroup, 1f, 0f, flashOut);
            }
        }

        // (Tuỳ chọn) chốt bằng fade đen rồi load
        yield return FadeCanvasGroup(overlayGroup, 0f, 1f, 0.12f);
        yield return new WaitForSecondsRealtime(loadDelayAfterFlash);

        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
        else
            Debug.LogWarning("[Flashback] nextSceneName trống!");
    }

    static IEnumerator FadeCanvasGroup(CanvasGroup g, float a, float b, float d)
    {
        if (g == null) yield break;
        d = Mathf.Max(0.0001f, d);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / d;
            g.alpha = Mathf.Lerp(a, b, t);
            yield return null;
        }
        g.alpha = b;
    }
}
