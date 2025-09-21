using UnityEngine;
using DG.Tweening;
using System.Collections;

namespace MadKnight
{
    public class Test : MonoBehaviour
    {
        private bool canSkip = false;
        private bool hasStartedFadeOut = false;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            GetComponent<CanvasGroup>().alpha = 0;
            DOTween.Sequence()
                .AppendInterval(1)
                .Append(GetComponent<CanvasGroup>().DOFade(1, 1f))
                .OnComplete(() => {
                    canSkip = true;
                    StartCoroutine(WaitAndFadeOut());
                });
        }
        
        void Update()
        {
            if (canSkip && !hasStartedFadeOut && Input.anyKeyDown)
            {
                hasStartedFadeOut = true;
                StopAllCoroutines();
                FadeOut();
            }
        }
        
        private IEnumerator WaitAndFadeOut()
        {
            yield return new WaitForSeconds(3f);
            if (!hasStartedFadeOut)
            {
                hasStartedFadeOut = true;
                FadeOut();
            }
        }
        
        private void FadeOut()
        {
            GetComponent<CanvasGroup>().DOFade(0, 1f);
        }
    }
}