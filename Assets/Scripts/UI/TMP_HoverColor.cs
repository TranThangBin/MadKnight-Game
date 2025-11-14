using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
namespace MadKnight
{
    public class TMP_HoverColor : MonoBehaviour
    {
        public TextMeshProUGUI tmp;
        public Color normalColor = Color.white;
        public Color hoverColor = Color.yellow;
        public float fadeSpeed = 8f;

        private Color targetColor;

        void Start()
        {
            if (tmp == null)
                tmp = GetComponent<TextMeshProUGUI>();

            tmp.color = normalColor;
            targetColor = normalColor;
        }

        void Update()
        {
            tmp.color = Color.Lerp(tmp.color, targetColor, Time.deltaTime * fadeSpeed);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            targetColor = hoverColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            targetColor = normalColor;
        }
    }
}
