using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CanvasGroup))]
public class SkipBtnController : MonoBehaviour, IPointerClickHandler
{
    public string sceneToLoad;
    public float fadeSpeed = 2f;      // tốc độ fade
    public float holdTime = 0.2f;     // thời gian sau khi di chuyển chuột thì nút còn sáng

    private CanvasGroup cg;
    private Vector3 lastMousePos;
    private float lastMoveTime;

    void Start()
    {
        cg = GetComponent<CanvasGroup>();
        cg.alpha = 0f;

        lastMousePos = Input.mousePosition;
        lastMoveTime = -999f;  // để lúc start nó không hiện
    }

    void Update()
    {
        // Kiểm tra xem chuột có di chuyển không
        if (Input.mousePosition != lastMousePos)
        {
            lastMousePos = Input.mousePosition;
            lastMoveTime = Time.time;   // vừa di chuyển → đánh dấu
        }

        // Nếu vừa di chuyển trong "holdTime" → hiện nút
        bool shouldShow = (Time.time - lastMoveTime) < holdTime;

        float targetAlpha = shouldShow ? 1f : 0f;
        cg.alpha = Mathf.MoveTowards(cg.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (cg.alpha > 0.9f)
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
