using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class SlideInPanel : MonoBehaviour
{
    [Header("Target")]
    public RectTransform panel;                 // Để trống sẽ tự lấy trên chính GameObject

    [Header("Motion")]
    public float offsetY = 800f;                // Panel sẽ bắt đầu thấp hơn vị trí đích offsetY px
    public float duration = 0.6f;               // Thời gian trượt
    public float delay = 0f;                    // Trễ trước khi chạy
    public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1); // Easing

    [Header("Auto")]
    public bool playOnEnable = true;            // Bật là chạy luôn

    Vector2 endPos;
    Vector2 startPos;
    Coroutine playing;

    void Reset()
    {
        panel = GetComponent<RectTransform>();
    }

    void Awake()
    {
        if (!panel) panel = GetComponent<RectTransform>();
        endPos = panel.anchoredPosition;
        startPos = endPos + new Vector2(0, -offsetY);
    }

    void OnEnable()
    {
        if (playOnEnable) Play();
    }

    public void Play()
    {
        if (playing != null) StopCoroutine(playing);
        playing = StartCoroutine(DoSlide());
    }

    public void JumpToStart()
    {
        if (!panel) panel = GetComponent<RectTransform>();
        panel.anchoredPosition = startPos;
    }

    IEnumerator DoSlide()
    {
        // đặt về điểm bắt đầu
        panel.anchoredPosition = startPos;

        if (delay > 0f) yield return new WaitForSeconds(delay);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, duration);
            float k = ease.Evaluate(t);
            panel.anchoredPosition = Vector2.LerpUnclamped(startPos, endPos, k);
            yield return null;
        }
        panel.anchoredPosition = endPos;
        playing = null;
    }
}
