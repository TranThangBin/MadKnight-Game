using UnityEngine;

namespace MadKnight
{
    public class Blackfade : MonoBehaviour
    {
        public enum EndAction { DisableObject, DestroyObject }

        [Header("Timing")]
        [Tooltip("Chờ trước khi bắt đầu fade (giây)")]
        public float delay = 0f;

        [Tooltip("Thời gian fade (giây)")]
        public float duration = 1f;

        [Tooltip("Dùng unscaled time (bỏ qua Time.timeScale)")]
        public bool useUnscaledTime = false;

        [Header("After Fade")]
        public EndAction endAction = EndAction.DisableObject;

        SpriteRenderer sr;
        CanvasGroup cg;

        void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            cg = GetComponent<CanvasGroup>();
            EnsureAlpha1();
        }

        void OnEnable()
        {
            EnsureAlpha1();
            StopAllCoroutines();
            StartCoroutine(FadeRoutine());
        }

        void EnsureAlpha1()
        {
            if (cg != null) cg.alpha = 1f;
            else if (sr != null) { var c = sr.color; c.a = 1f; sr.color = c; }
        }

        System.Collections.IEnumerator FadeRoutine()
        {
            // delay
            if (delay > 0f)
            {
                float t = 0f;
                while (t < delay)
                {
                    t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                    yield return null;
                }
            }

            // fade
            float d = Mathf.Max(0.0001f, duration);
            float e = 0f;
            while (e < d)
            {
                e += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                float k = 1f - Mathf.Clamp01(e / d); // 1 -> 0
                SetAlpha(k);
                yield return null;
            }

            // đảm bảo = 0
            SetAlpha(0f);

            // kết thúc
            if (endAction == EndAction.DisableObject) gameObject.SetActive(false);
            else Destroy(gameObject);
        }

        void SetAlpha(float a)
        {
            if (cg != null) cg.alpha = a;
            else if (sr != null)
            {
                var c = sr.color;
                c.a = a;
                sr.color = c;
            }
        }
    }
}
