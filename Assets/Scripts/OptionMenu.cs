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
        [SerializeField] private Button btnKeysMap;
        [SerializeField] private Button btnQuit;

        [Header("Buttons (Confirm)")]
        [SerializeField] private Button btnYes;
        [SerializeField] private Button btnNo;

        [Header("Back Button (global)")]
        [SerializeField] private Button btnBack;

        [Header("Popup Root")]
        [SerializeField] private CanvasGroup popupRoot;      // gốc popup (CanvasGroup)
        [SerializeField] private bool startOpened = true;    // false nếu muốn ẩn lúc bắt đầu

        [Header("Panels")]
        [SerializeField] private CanvasGroup panelMain;
        [SerializeField] private CanvasGroup panelAudio;
        [SerializeField] private CanvasGroup panelControl;
        [SerializeField] private CanvasGroup panelLanguage;
        [SerializeField] private CanvasGroup panelKeysMap;
        [SerializeField] private CanvasGroup panelQuit;

        private Dictionary<string, CanvasGroup> panels;
        private Selectable lastSelected;

        private readonly Stack<string> history = new Stack<string>();
        private string currentKey = null;
        private bool isOpen = false;
        private bool started = false;

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

            // đảm bảo panel có mặt trong hierarchy để lấy Selectable
            foreach (var kv in panels)
                if (kv.Value != null && !kv.Value.gameObject.activeSelf)
                    kv.Value.gameObject.SetActive(true);

            if (popupRoot && !popupRoot.gameObject.activeSelf)
                popupRoot.gameObject.SetActive(true);
        }

        // Khi popup được bật lại bằng SetActive(true) sau lần đầu
        private void OnEnable()
        {
            if (!started) return;   // tránh chạy trước Start lần đầu
            OpenPopup();            // tự về Main khi mở lại
        }

        private void Start()
        {
            if (btnAudio)    btnAudio.onClick.AddListener(() => Show("Audio",    btnAudio));
            if (btnRestart)  btnRestart.onClick.AddListener(RestartScene);
            if (btnControl)  btnControl.onClick.AddListener(() => Show("Control",  btnControl));
            if (btnLanguage) btnLanguage.onClick.AddListener(() => Show("Language", btnLanguage));
            if (btnKeysMap)  btnKeysMap.onClick.AddListener(() => Show("KeysMap",  btnKeysMap));
            if (btnQuit)     btnQuit.onClick.AddListener(() => Show("Quit",     btnQuit));

            if (btnYes) btnYes.onClick.AddListener(QuitGame);
            if (btnNo)  btnNo .onClick.AddListener(ClosePopup);
            if (btnBack) btnBack.onClick.AddListener(Back);

            started = true;

            if (startOpened) OpenPopup();
            else             ClosePopup();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                Back();
        }

        // mở 1 panel, mặc định đẩy panel hiện tại vào history
        private void Show(string key, Selectable focus, bool pushHistory = true)
        {
            if (!isOpen) OpenPopup(); // nếu đang đóng mà gọi Show, tự mở

            foreach (var kv in panels) SetVisible(kv.Value, false);

            if (!panels.TryGetValue(key, out var cg) || cg == null)
            {
                Debug.LogWarning($"Panel '{key}' không tồn tại. Trở về Main.");
                GoMain();
                return;
            }

            if (pushHistory && !string.IsNullOrEmpty(currentKey))
                history.Push(currentKey);

            SetVisible(cg, true);
            currentKey = key;

            if (focus != null) lastSelected = focus;
            var first = cg.GetComponentInChildren<Selectable>(true);
            TryFocus(first != null ? first : lastSelected);
        }

        // Back: ở Main thì đóng popup, còn lại lùi 1 panel
        public void Back()
        {
            if (!isOpen)
            {
                OpenPopup();
                return;
            }

            if (currentKey == "Main" || history.Count == 0)
            {
                ClosePopup();
                return;
            }

            var prev = history.Pop();
            Show(prev, null, pushHistory:false);
        }

        // mở popup và auto vào Main
        public void OpenPopup()
        {
            if (popupRoot) SetVisible(popupRoot, true);
            else gameObject.SetActive(true);

            GoMain();
            isOpen = true;
        }

        // gom logic về Main
        private void GoMain()
        {
            foreach (var kv in panels) SetVisible(kv.Value, false);
            SetVisible(panelMain, true);

            history.Clear();
            currentKey = "Main";

            var first = panelMain ? panelMain.GetComponentInChildren<Selectable>(true) : null;
            lastSelected = first ? first : btnAudio;
            TryFocus(lastSelected);
        }

        // đóng popup hoàn toàn
        public void ClosePopup()
        {
            foreach (var kv in panels) SetVisible(kv.Value, false);

            if (popupRoot) SetVisible(popupRoot, false);
            else gameObject.SetActive(false);

            currentKey = null;
            history.Clear();
            isOpen = false;
        }

        // tiện cho input ngoài
        public void TogglePopup()
        {
            if (isOpen) ClosePopup();
            else OpenPopup();
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
