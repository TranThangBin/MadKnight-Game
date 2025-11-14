using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

namespace MadKnight.UI
{
    /// <summary>
    /// Plays the loading video and loads a target scene after the main menu fades out.
    /// Attach this to the Play button or a central controller and assign references.
    /// </summary>
    public class SceneLoadingController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MainMenuUI mainMenuUI;
        [SerializeField] private VideoPlayer loadingPlayer;

        [Header("Timing")]
        [SerializeField] private float minLoadingTime = 2f;

        [Header("Scene")]
        [SerializeField] private string defaultSceneName = "Level01";

        /// <summary>
        /// Load the configured default scene (Level01 by default).
        /// Use this from a Button OnClick event.
        /// </summary>
        public void LoadDefaultScene()
        {
            LoadScene(defaultSceneName);
        }

        /// <summary>
        /// Starts the loading sequence for a custom scene.
        /// </summary>
        public void LoadScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("[SceneLoadingController] Scene name is empty.");
                return;
            }

            StartCoroutine(LoadSceneCoroutine(sceneName));
        }

        private IEnumerator LoadSceneCoroutine(string sceneName)
        {
            if (mainMenuUI != null)
            {
                yield return mainMenuUI.FadeOutUI();
            }

            var loadingTime = minLoadingTime;
            var op = SceneManager.LoadSceneAsync(sceneName);
            if (op == null)
            {
                Debug.LogError($"[SceneLoadingController] Failed to load scene '{sceneName}'.");
                yield break;
            }

            op.allowSceneActivation = false;

            if (loadingPlayer != null)
            {
                loadingPlayer.gameObject.SetActive(true);
                loadingPlayer.Play();
            }

            while (op.progress < 0.9f)
            {
                loadingTime -= Time.deltaTime;
                yield return null;
            }

            while (loadingTime > 0f)
            {
                loadingTime -= Time.deltaTime;
                yield return null;
            }

            op.allowSceneActivation = true;
        }
    }
}
