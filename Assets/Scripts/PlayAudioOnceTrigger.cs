using UnityEngine;

[DisallowMultipleComponent]
public class PlayAudioOnceTrigger : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource audioSource;   // Kéo AudioSource có clip vào đây
    public bool destroyAfterPlay = false; // Nếu muốn xóa trigger sau khi phát xong

    private bool hasPlayed = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasPlayed) return;
        if (other.CompareTag("Player"))
        {
            hasPlayed = true;

            if (audioSource != null)
            {
                audioSource.Play();
                if (destroyAfterPlay)
                    Destroy(gameObject, audioSource.clip.length + 0.1f);
            }
            else
            {
                Debug.LogWarning("[PlayAudioOnceTrigger] Thiếu AudioSource!");
            }
        }
    }
}
