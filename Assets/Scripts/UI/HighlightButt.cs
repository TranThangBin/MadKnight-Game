using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; 
using System.Collections.Generic; 
namespace MadKnight
{
    public class HighlightButt : MonoBehaviour 
    {
        public GraphicRaycaster raycaster;
        public EventSystem eventSystem;
        private ParticleSystem ps;
        void Awake()
        {
            ps = GetComponent<ParticleSystem>();

        }
        void Update()
        {
            PointerEventData pointerData = new PointerEventData(eventSystem)
            {
                position = Input.mousePosition
            };
            bool hover = false;
            
            List<RaycastResult> results = new List<RaycastResult>();
            raycaster.Raycast(pointerData, results);
            foreach (var result in results)
            {
                if (result.gameObject == gameObject)
                {
                    hover = true;
                    break;
                }

            }
            if (hover)
            {
                ps.Play();
            }
            else
            {
                ps.Stop();
            }
        }
         
    }
}
