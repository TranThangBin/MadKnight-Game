using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace MadKnight
{
    public class PipesScript : MonoBehaviour
    {
        private readonly int[] rotations = { 0, 90, 180, 270 };

        [Tooltip("Các góc đúng, ví dụ [0,180] hoặc [90,270]")]
        public int[] correctRotation = { 0 };

        [FormerlySerializedAs("mnGame_Manager")]
        [SerializeField] private Minigame_Manager manager;

        [SerializeField]
        private bool isPlaced;

        void Awake()
        {
            if (Minigame_Manager.Instance)
            {
                manager = Minigame_Manager.Instance;
            }
            else
            {
                manager = FindFirstObjectByType<Minigame_Manager>(FindObjectsInactive.Include);
            }

            if (!manager)
                Debug.LogError("[PipesScript] manager NULL. Cần 1 Minigame_Manager active trong scene.");
            int randomIndex = Random.Range(0, rotations.Length);
            transform.eulerAngles = new Vector3(0f, 0f, rotations[randomIndex]);
        }

        void Start()
        {
            if (correctRotation == null || correctRotation.Length == 0)
            {
                correctRotation = new[] { 0 };
            }

            isPlaced = IsCorrectAngle(GetSnapAngle());

            if (isPlaced && manager) manager.CorrectMove();
        }

        void OnMouseDown()
        {
            transform.Rotate(0f, 0f, 90f);

            bool prevPlaced = isPlaced;
            isPlaced = IsCorrectAngle(GetSnapAngle());

            if (!manager) return;

            if (!prevPlaced && isPlaced) manager.CorrectMove();
            else if (prevPlaced && !isPlaced) manager.WrongMove();
        }

        private int GetSnapAngle()
        {
            float raw = transform.eulerAngles.z;
            int snapped = Mathf.RoundToInt(((raw % 360f) + 360f) % 360f / 90f) * 90;
            if (snapped == 360) snapped = 0;
            return snapped;
        }

        private bool IsCorrectAngle(int angle)
        {
            return correctRotation.Any(r => Mathf.RoundToInt(((r % 360) + 360) % 360) == angle);
        }

        void OnValidate()
        {
            if (correctRotation == null || correctRotation.Length == 0)
                correctRotation = new[] { 0 };
        }
    }
}
