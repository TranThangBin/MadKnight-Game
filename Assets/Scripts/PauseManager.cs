using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;    // New Input System
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject menuRoot;                // Kéo MenuRoot vào
    [SerializeField] private Selectable firstSelected;           // Kéo Btn_Resume (hoặc button đầu tiên) vào

    [Header("Input System")]
    [SerializeField] private InputActionReference pauseAction;   // Kéo Action "Pause" vào
    [SerializeField] private PlayerInput playerInput;            // (Tuỳ chọn) Kéo PlayerInput nếu dùng action map
    [SerializeField] private string gameplayActionMap = "Gameplay";
    [SerializeField] private string uiActionMap = "UI";

    private bool isOpen;

    private void Awake()
    {
        if (menuRoot != null) menuRoot.SetActive(false);
        Time.timeScale = 1f;
    }

    private void OnEnable()
    {
        if (pauseAction != null)
        {
            pauseAction.action.performed += OnPausePerformed;
            pauseAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (pauseAction != null)
        {
            pauseAction.action.performed -= OnPausePerformed;
            pauseAction.action.Disable();
        }
    }

    private void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        Toggle();
    }

    public void Toggle()
    {
        if (isOpen) Close();
        else Open();
    }

    public void Open()
    {
        isOpen = true;
        if (menuRoot != null) menuRoot.SetActive(true);

        // Dừng gameplay
        Time.timeScale = 0f;

        // Chuột
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Chuyển sang UI action map (nếu có)
        if (playerInput != null && !string.IsNullOrEmpty(uiActionMap))
            playerInput.SwitchCurrentActionMap(uiActionMap);

        // Đảm bảo Pause Action vẫn hoạt động
        if (pauseAction != null && !pauseAction.action.enabled)
            pauseAction.action.Enable();

        // Focus button đầu tiên
        if (firstSelected != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(firstSelected.gameObject);
    }

    public void Close()
    {
        isOpen = false;
        if (menuRoot != null) menuRoot.SetActive(false);

        // Resume gameplay
        Time.timeScale = 1f;

        // Chuột
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked; // Nếu game bạn dùng lock chuột; nếu không thì bỏ

        // Trả về gameplay action map
        if (playerInput != null && !string.IsNullOrEmpty(gameplayActionMap))
            playerInput.SwitchCurrentActionMap(gameplayActionMap);

        // Đảm bảo Pause Action vẫn hoạt động
        if (pauseAction != null && !pauseAction.action.enabled)
            pauseAction.action.Enable();
    }

    // ====== Button hooks ======
    public void OnBtnResume() => Close();

    public void OnBtnRestart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnBtnQuit()
    {
        Time.timeScale = 1f;
        Application.Quit();
        // Hoặc: SceneManager.LoadScene("MainMenu");
    }
}
