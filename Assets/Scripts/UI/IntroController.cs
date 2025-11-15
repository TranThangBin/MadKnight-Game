using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class IntroController : MonoBehaviour
{
    public VideoPlayer videoPlayer;   // drag VideoPlayer vào
    public GameObject stageRoot;      // drag object StageRoot vào
    public CanvasGroup blackFade;     // drag CanvasGroup của Image đen vào
    public float fadeTime = 1f;       // thời gian fade (giây)

    void Start()
    {
        // Stage thật ẩn lúc đầu
        if (stageRoot != null)
            stageRoot.SetActive(false);

        // Màn hình đen trong suốt lúc đầu
        if (blackFade != null)
            blackFade.alpha = 0f;

        // Event khi video chạy xong
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        // Khi video hết, bắt đầu coroutine fade
        StartCoroutine(FadeAndShowStage());
    }

    IEnumerator FadeAndShowStage()
    {
        float t = 0f;

        // 1. Fade từ 0 → 1
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Clamp01(t / fadeTime);

            if (blackFade != null)
                blackFade.alpha = alpha;

            // Khi alpha đạt 0.9 → xóa Image đen
            if (alpha >= 0.9f)
            {
                Destroy(blackFade.gameObject);
                break; // Thoát loop
            }

            yield return null;
        }

        // 2. Tắt Intro video
        gameObject.SetActive(false);

        // 3. Hiển thị Stage
        if (stageRoot != null)
            stageRoot.SetActive(true);
    }

}
