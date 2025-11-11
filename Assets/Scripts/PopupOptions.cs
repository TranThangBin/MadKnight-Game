using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class OptionPopup : MonoBehaviour
{
    [SerializeField] CanvasGroup cg;
    [SerializeField] Transform window;
    [SerializeField] float fadeTime = 0.15f;
    Vector3 showScale = Vector3.one;
    Vector3 hideScale = new Vector3(0.9f, 0.9f, 1);

    void Awake()
    {
        if (!cg) cg = GetComponent<CanvasGroup>();
        gameObject.SetActive(false);
    }

    public void Open()
    {
        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(Fade(true));
    }

    // CÁI BẠN CẦN
    public void Close()
    {
        StopAllCoroutines();
        StartCoroutine(Fade(false));
    }

    IEnumerator Fade(bool show)
    {
        if (!cg) yield break;
        float t = 0;
        float start = cg.alpha;
        float end = show ? 1f : 0f;
        cg.interactable = false; cg.blocksRaycasts = false;
        if (window) window.localScale = show ? hideScale : showScale;

        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / fadeTime);
            cg.alpha = Mathf.Lerp(start, end, k);
            if (window) window.localScale = Vector3.Lerp(
                show ? hideScale : showScale,
                show ? showScale : hideScale, k);
            yield return null;
        }

        cg.alpha = end;
        if (show) { cg.interactable = true; cg.blocksRaycasts = true; }
        else { gameObject.SetActive(false); }
    }
}
