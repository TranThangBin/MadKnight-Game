using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

// Thêm CanvasGroup vào object để fade cả nút + text
[RequireComponent(typeof(CanvasGroup))]
public class SkipBtnController : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public string sceneToLoad;       // Scene muốn chuyển tới
    public float fadeSpeed = 2f;     // Tốc độ tăng/giảm alpha

    private CanvasGroup cg;          // Thay Image bằng CanvasGroup
    private bool isHovering = false;

    void Start()
    {
        cg = GetComponent<CanvasGroup>();

        // Khởi đầu ẩn hoàn toàn
        cg.alpha = 0f;
        // Có thể để click khi ẩn, nhưng ta sẽ chặn bằng alpha check
        // Nếu muốn chặn luôn, mở 2 dòng dưới:
        // cg.interactable = false;
        // cg.blocksRaycasts = true; // vẫn nhận raycast để hover/hiện dần
    }

    void Update()
    {
        float target = isHovering ? 1f : 0f;
        cg.alpha = Mathf.MoveTowards(cg.alpha, target, Time.deltaTime * fadeSpeed);

        // (tuỳ chọn) chỉ cho phép click khi đã hiện rõ
        // cg.interactable = cg.alpha > 0.95f;
    }

    public void OnPointerEnter(PointerEventData _) => isHovering = true;
    public void OnPointerExit (PointerEventData _) => isHovering = false;

    public void OnPointerClick(PointerEventData _)
    {
        if (cg.alpha > 0.9f)
            SceneManager.LoadScene(sceneToLoad);
    }
}
