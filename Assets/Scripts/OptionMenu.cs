using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace MadKnight
{
    public class OptionMenu : MonoBehaviour
    {
        [Header("Buttons (Main)")]
        [SerializeField] private Button btnAudio;
        [SerializeField] private Button btnRestart;
        [SerializeField] private Button btnControl;
        [SerializeField] private Button btnLanguage;
        [SerializeField] private Button btnkeysMap;
        [SerializeField] private Button btnQuit;

        [Header("Buttons (Confirm)")]
        [SerializeField] private Button btnYes;
        [SerializeField] private Button btnNo;

        [Header("Panels")]
        [SerializeField] private CanvasGroup panelMain;
        [SerializeField] private CanvasGroup panelAudio;
        [SerializeField] private CanvasGroup panelControl;
        [SerializeField] private CanvasGroup panelLanguage;
        [SerializeField] private CanvasGroup panelKeysMap;
        [SerializeField] private CanvasGroup panelQuit;

        private Dictionary<string, CanvasGroup> panels;
        private Selectable lastSelected;

        private void Awake()
        {
            panels = new Dictionary<string, CanvasGroup>
            {
                { "Main",     panelMain     },
                { "Audio",    panelAudio    },
                { "Control",  panelControl  },
                { "Language", panelLanguage },
                { "KeysMap",  panelKeysMap  },
                { "Quit",     panelQuit     },
            };

            foreach (var kv in panels)
                if (kv.Value != null && !kv.Value.gameObject.activeSelf)
                    kv.Value.gameObject.SetActive(true);
        }

        private void Start()
        {
            btnAudio   .onClick.AddListener(() => Show("Audio",    btnAudio));
            btnRestart .onClick.AddListener(RestartScene);
            btnControl .onClick.AddListener(() => Show("Control",  btnControl));
            btnLanguage.onClick.AddListener(() => Show("Language", btnLanguage));
            btnkeysMap.onClick.AddListener(() => Show("KeysMap", btnkeysMap));
            btnQuit    .onClick.AddListener(() => Show("Quit",     btnQuit));

            btnYes     .onClick.AddListener(QuitGame);
            btnNo      .onClick.AddListener(() => Show("Main",btnNo));

            Show("Main", btnAudio);
        }

        private void Show(string key, Selectable focus)
        {
            foreach (var kv in panels) SetVisible(kv.Value, false);

            if (!panels.TryGetValue(key, out var cg) || cg == null)
            {
                Debug.LogWarning($"Panel '{key}' không tồn tại. Trở về Main.");
                SetVisible(panelMain, true);
                TryFocus(lastSelected);
                return;
            }

            SetVisible(cg, true);

            if (focus != null) lastSelected = focus;

            var first = cg.GetComponentInChildren<Selectable>(true);
            var target = first != null ? first : lastSelected;

            TryFocus(target);
        }

        private static void SetVisible(CanvasGroup cg, bool v)
        {
            if (!cg) return;

            cg.gameObject.SetActive(v);      
            cg.alpha = v ? 1f : 0f;             
            cg.interactable = v;
            cg.blocksRaycasts = v;
        }

        private static void TryFocus(Selectable sel)
        {
            if (sel == null) return;
            var es = EventSystem.current;
            if (es == null)
            {
                Debug.LogWarning("Thiếu EventSystem trong scene. Thêm đúng 1 EventSystem.");
                return;
            }
            if (!sel.gameObject.activeInHierarchy) return;
            es.SetSelectedGameObject(sel.gameObject);
        }

        private void RestartScene()
        {
            Time.timeScale = 1f;
            var idx = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(idx);
        }

        private void QuitGame()
        {
    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
    #else
            Application.Quit();
    #endif
        }
    }
}
