using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MadKnight.UI
{
    /// <summary>
    /// Hiển thị một panel khi click vào object này
    /// và tự ẩn panel khi click ra ngoài.
    /// </summary>
    public class PanelToggleOnClick : MonoBehaviour, IPointerClickHandler
    {
        [Header("References")]
        [SerializeField] private GameObject targetPanel;

        [Header("Behaviour")]
        [SerializeField] private bool hideOnStart = true;
        [SerializeField] private bool hideWhenClickOutside = true;

        private bool panelVisible;

        private void Awake()
        {
            if (targetPanel == null)
            {
                Debug.LogWarning($"{nameof(PanelToggleOnClick)} on {name} missing Target Panel reference.");
                return;
            }

            if (hideOnStart)
            {
                HidePanel();
            }
            else
            {
                panelVisible = targetPanel.activeSelf;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            ShowPanel();
        }

        private void Update()
        {
            if (!panelVisible || !hideWhenClickOutside || targetPanel == null) return;

            if (Input.GetMouseButtonDown(0) && !IsPointerOverTarget(targetPanel) &&
                !IsPointerOverTarget(gameObject))
            {
                HidePanel();
            }
        }

        private bool IsPointerOverTarget(GameObject target)
        {
            if (target == null || EventSystem.current == null) return false;

            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, hits);

            foreach (RaycastResult hit in hits)
            {
                if (hit.gameObject == null) continue;

                if (hit.gameObject == target || hit.gameObject.transform.IsChildOf(target.transform))
                {
                    return true;
                }
            }

            return false;
        }

        private void ShowPanel()
        {
            if (targetPanel == null) return;
            targetPanel.SetActive(true);
            panelVisible = true;
        }

        public void HidePanel()
        {
            if (targetPanel == null) return;
            targetPanel.SetActive(false);
            panelVisible = false;
        }
    }
}
