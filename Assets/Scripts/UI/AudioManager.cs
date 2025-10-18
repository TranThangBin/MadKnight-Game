using UnityEngine;
using UnityEngine.Audio;

namespace MadKnight.UI
{
    /// <summary>
    /// Audio Manager - Quản lý âm thanh toàn game
    /// Singleton pattern
    /// QUAN TRỌNG: Điều khiển TẤT CẢ AudioSource trong scene, không chỉ của riêng nó
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }
        
        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer audioMixer;
        
        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource ambienceSource;
        
        [Header("Volume Control (when no AudioMixer)")]
        [Tooltip("Current music volume (0-1)")]
        [SerializeField] private float currentMusicVolume = 0.7f;
        [SerializeField] private float currentSFXVolume = 0.8f;
        [SerializeField] private float currentMasterVolume = 0.8f;
        
        // Mixer parameter names
        private const string MASTER_VOLUME = "MasterVolume";
        private const string MUSIC_VOLUME = "MusicVolume";
        private const string SFX_VOLUME = "SFXVolume";
        private const string AMBIENCE_VOLUME = "AmbienceVolume";
        
        private void Awake()
        {
            // Singleton
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            // Cảnh báo nếu không có AudioMixer
            if (audioMixer == null)
            {
                Debug.LogWarning("[AudioManager] AudioMixer is not assigned! Will use direct AudioSource.volume control.");
            }
            
            // Create audio sources if not assigned
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }
            
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }
            
            if (ambienceSource == null)
            {
                ambienceSource = gameObject.AddComponent<AudioSource>();
                ambienceSource.loop = true;
                ambienceSource.playOnAwake = false;
            }
            
            // Load saved volumes
            GameSettings settings = GameSettings.Load();
            currentMusicVolume = settings.musicVolume;
            currentSFXVolume = settings.sfxVolume;
            currentMasterVolume = settings.masterVolume;
            
            Debug.Log($"[AudioManager] Initialized with volumes - Music: {currentMusicVolume:F2}, SFX: {currentSFXVolume:F2}, Master: {currentMasterVolume:F2}");
        }
        
        #region Volume Control
        
        public void SetMasterVolume(float volume)
        {
            currentMasterVolume = volume;
            
            if (audioMixer != null)
            {
                // Convert 0-1 to decibels (-80 to 0)
                float db = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
                audioMixer.SetFloat(MASTER_VOLUME, db);
                Debug.Log($"[AudioManager] Set master volume via AudioMixer: {volume:F2}");
            }
            else
            {
                // Re-apply all volumes with new master volume
                SetMusicVolume(currentMusicVolume);
                SetSFXVolume(currentSFXVolume);
                Debug.Log($"[AudioManager] Set master volume: {volume:F2} (re-applied music & SFX)");
            }
        }
        
        public void SetMusicVolume(float volume)
        {
            currentMusicVolume = volume;
            
            if (audioMixer != null)
            {
                float db = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
                audioMixer.SetFloat(MUSIC_VOLUME, db);
                Debug.Log($"[AudioManager] Set music volume via AudioMixer: {volume:F2} ({db:F1} dB)");
            }
            else
            {
                // QUAN TRỌNG: Tìm và set volume cho TẤT CẢ AudioSource đang phát
                AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
                int musicSourcesFound = 0;
                
                foreach (AudioSource source in allAudioSources)
                {
                    // Set volume cho TẤT CẢ AudioSource đang phát (trừ SFX source của AudioManager)
                    if (source.isPlaying && source.clip != null && source != sfxSource && source != ambienceSource)
                    {
                        float finalVolume = volume * currentMasterVolume;
                        source.volume = finalVolume;
                        musicSourcesFound++;
                        Debug.Log($"[AudioManager] Set volume for AudioSource: {source.gameObject.name} (clip: {source.clip.name}, loop: {source.loop}) = {finalVolume:F2}");
                    }
                }
                
                // Set cho musicSource của AudioManager (nếu có và đang phát)
                if (musicSource != null && musicSource.isPlaying)
                {
                    musicSource.volume = volume * currentMasterVolume;
                }
                
                Debug.Log($"[AudioManager] Updated {musicSourcesFound} AudioSource(s) to volume: {volume:F2}");
            }
        }
        
        public void SetSFXVolume(float volume)
        {
            currentSFXVolume = volume;
            
            if (audioMixer != null)
            {
                float db = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
                audioMixer.SetFloat(SFX_VOLUME, db);
                Debug.Log($"[AudioManager] Set SFX volume via AudioMixer: {volume:F2}");
            }
            else
            {
                // Set cho sfxSource của AudioManager
                if (sfxSource != null)
                {
                    sfxSource.volume = volume * currentMasterVolume;
                }
                
                Debug.Log($"[AudioManager] Set SFX volume: {volume:F2} (will affect PlaySFX calls)");
            }
        }
        
        public void SetAmbienceVolume(float volume)
        {
            if (audioMixer != null)
            {
                float db = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
                audioMixer.SetFloat(AMBIENCE_VOLUME, db);
            }
            else if (ambienceSource != null)
            {
                ambienceSource.volume = volume;
            }
        }
        
        #endregion
        
        #region Play Audio
        
        public void PlayMusic(AudioClip clip, bool loop = true, float startVolume = -1f)
        {
            if (musicSource == null || clip == null) return;
            
            musicSource.clip = clip;
            musicSource.loop = loop;
            
            // Nếu startVolume được chỉ định, set volume cho source
            if (startVolume >= 0f && audioMixer == null)
            {
                musicSource.volume = startVolume;
            }
            
            musicSource.Play();
        }
        
        public void StopMusic()
        {
            if (musicSource != null)
            {
                musicSource.Stop();
            }
        }
        
        public void PlaySFX(AudioClip clip, float volumeScale = 1f)
        {
            if (sfxSource == null || clip == null) return;
            
            // Áp dụng volume = currentSFXVolume * currentMasterVolume * volumeScale
            float finalVolume = currentSFXVolume * currentMasterVolume * volumeScale;
            sfxSource.PlayOneShot(clip, finalVolume);
            
            Debug.Log($"[AudioManager] PlaySFX: {clip.name} at volume {finalVolume:F2} (SFX: {currentSFXVolume:F2}, Master: {currentMasterVolume:F2}, Scale: {volumeScale:F2})");
        }
        
        public void PlayAmbience(AudioClip clip, bool loop = true)
        {
            if (ambienceSource == null || clip == null) return;
            
            ambienceSource.clip = clip;
            ambienceSource.loop = loop;
            ambienceSource.Play();
        }
        
        public void StopAmbience()
        {
            if (ambienceSource != null)
            {
                ambienceSource.Stop();
            }
        }
        
        #endregion
    }
}
