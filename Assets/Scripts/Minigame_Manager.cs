using UnityEngine;

namespace MadKnight
{
    [DefaultExecutionOrder(-1000)]
    public class Minigame_Manager : MonoBehaviour
    {
        public static Minigame_Manager Instance { get; private set; }

        [Header("Refs")]
        public GameObject WinText;

        [SerializeField] int totalPipe = 0;
        [SerializeField] int correctPipes = 0;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            totalPipe = 12;
            if (WinText) WinText.SetActive(false);
        }

        public void CorrectMove()
        {
            correctPipes = Mathf.Clamp(correctPipes + 1, 0, totalPipe);
            if(correctPipes == totalPipe)
            {
                WinText.SetActive(true);
            }
        }

        public void WrongMove()
        {
            correctPipes = Mathf.Clamp(correctPipes - 1, 0, totalPipe);
        }
    }
}
