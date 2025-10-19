using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace MadKnight.UI
{
    /// <summary>
    /// Audio Manager - Quản lý âm thanh toàn game
    /// Singleton pattern
    /// Quản lý các GameObject chứa AudioSource và list âm thanh tương ứng
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }
        
        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer audioMixer;
        
        [Header("Audio GameObjects")]
        [SerializeField] private GameObject musicGameObject;
        [SerializeField] private GameObject sfxGameObject;
        [SerializeField] private GameObject ambienceGameObject;
        
        [Header("Music Clips")]
        [SerializeField] private List<AudioClip> musicClips = new List<AudioClip>();
        
        [Header("SFX Clips")]
        [SerializeField] private List<AudioClip> sfxClips = new List<AudioClip>();
        
        [Header("Ambience Clips")]
        [SerializeField] private List<AudioClip> ambienceClips = new List<AudioClip>();
        
        [Header("Volume Control")]
        [Tooltip("Current music volume (0-1)")]
        [SerializeField] private float currentMusicVolume = 0.7f;
        [SerializeField] private float currentSFXVolume = 0.8f;
        [SerializeField] private float currentAmbienceVolume = 0.7f;
        [SerializeField] private float currentMasterVolume = 0.8f;
        
        // Audio sources
        private AudioSource musicSource;
        private AudioSource sfxSource;
        private AudioSource ambienceSource;
        
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
            
            // Setup Music GameObject and AudioSource
            if (musicGameObject != null)
            {
                musicSource = musicGameObject.GetComponent<AudioSource>();
                if (musicSource == null)
                {
                    musicSource = musicGameObject.AddComponent<AudioSource>();
                }
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }
            else
            {
                Debug.LogWarning("[AudioManager] Music GameObject is not assigned!");
            }
            
            // Setup SFX GameObject and AudioSource
            if (sfxGameObject != null)
            {
                sfxSource = sfxGameObject.GetComponent<AudioSource>();
                if (sfxSource == null)
                {
                    sfxSource = sfxGameObject.AddComponent<AudioSource>();
                }
                sfxSource.playOnAwake = false;
            }
            else
            {
                Debug.LogWarning("[AudioManager] SFX GameObject is not assigned!");
            }
            
            // Setup Ambience GameObject and AudioSource
            if (ambienceGameObject != null)
            {
                ambienceSource = ambienceGameObject.GetComponent<AudioSource>();
                if (ambienceSource == null)
                {
                    ambienceSource = ambienceGameObject.AddComponent<AudioSource>();
                }
                ambienceSource.loop = true;
                ambienceSource.playOnAwake = false;
            }
            else
            {
                Debug.LogWarning("[AudioManager] Ambience GameObject is not assigned!");
            }
            
            // Load saved volumes
            GameSettings settings = GameSettings.Load();
            currentMusicVolume = settings.musicVolume;
            currentSFXVolume = settings.sfxVolume;
            currentMasterVolume = settings.masterVolume;
            
            // Apply initial volumes
            ApplyVolumes();
            
            Debug.Log($"[AudioManager] Initialized with volumes - Music: {currentMusicVolume:F2}, SFX: {currentSFXVolume:F2}, Master: {currentMasterVolume:F2}");
            Debug.Log($"[AudioManager] Loaded {musicClips.Count} music clips, {sfxClips.Count} SFX clips, {ambienceClips.Count} ambience clips");
        }
        
        #region Volume Control
        
        /// <summary>
        /// Áp dụng tất cả các volume settings
        /// </summary>
        private void ApplyVolumes()
        {
            SetMasterVolume(currentMasterVolume);
            SetMusicVolume(currentMusicVolume);
            SetSFXVolume(currentSFXVolume);
            SetAmbienceVolume(currentAmbienceVolume);
        }
        
        public void SetMasterVolume(float volume)
        {
            currentMasterVolume = Mathf.Clamp01(volume);
            
            if (audioMixer != null)
            {
                // Convert 0-1 to decibels (-80 to 0)
                float db = currentMasterVolume > 0 ? Mathf.Log10(currentMasterVolume) * 20 : -80f;
                audioMixer.SetFloat(MASTER_VOLUME, db);
                Debug.Log($"[AudioManager] Set master volume via AudioMixer: {currentMasterVolume:F2} ({db:F1} dB)");
            }
            else
            {
                // Re-apply all volumes with new master volume
                UpdateSourceVolume(musicSource, currentMusicVolume);
                UpdateSourceVolume(sfxSource, currentSFXVolume);
                UpdateSourceVolume(ambienceSource, currentAmbienceVolume);
                Debug.Log($"[AudioManager] Set master volume: {currentMasterVolume:F2} (re-applied all volumes)");
            }
        }
        
        public void SetMusicVolume(float volume)
        {
            currentMusicVolume = Mathf.Clamp01(volume);
            
            if (audioMixer != null)
            {
                float db = currentMusicVolume > 0 ? Mathf.Log10(currentMusicVolume) * 20 : -80f;
                audioMixer.SetFloat(MUSIC_VOLUME, db);
                Debug.Log($"[AudioManager] Set music volume via AudioMixer: {currentMusicVolume:F2} ({db:F1} dB)");
            }
            else
            {
                UpdateSourceVolume(musicSource, currentMusicVolume);
                Debug.Log($"[AudioManager] Set music volume: {currentMusicVolume:F2}");
            }
        }
        
        public void SetSFXVolume(float volume)
        {
            currentSFXVolume = Mathf.Clamp01(volume);
            
            if (audioMixer != null)
            {
                float db = currentSFXVolume > 0 ? Mathf.Log10(currentSFXVolume) * 20 : -80f;
                audioMixer.SetFloat(SFX_VOLUME, db);
                Debug.Log($"[AudioManager] Set SFX volume via AudioMixer: {currentSFXVolume:F2} ({db:F1} dB)");
            }
            else
            {
                UpdateSourceVolume(sfxSource, currentSFXVolume);
                Debug.Log($"[AudioManager] Set SFX volume: {currentSFXVolume:F2}");
            }
        }
        
        public void SetAmbienceVolume(float volume)
        {
            currentAmbienceVolume = Mathf.Clamp01(volume);
            
            if (audioMixer != null)
            {
                float db = currentAmbienceVolume > 0 ? Mathf.Log10(currentAmbienceVolume) * 20 : -80f;
                audioMixer.SetFloat(AMBIENCE_VOLUME, db);
                Debug.Log($"[AudioManager] Set ambience volume via AudioMixer: {currentAmbienceVolume:F2} ({db:F1} dB)");
            }
            else
            {
                UpdateSourceVolume(ambienceSource, currentAmbienceVolume);
                Debug.Log($"[AudioManager] Set ambience volume: {currentAmbienceVolume:F2}");
            }
        }
        
        /// <summary>
        /// Cập nhật volume cho AudioSource
        /// </summary>
        private void UpdateSourceVolume(AudioSource source, float volume)
        {
            if (source != null && audioMixer == null)
            {
                source.volume = volume * currentMasterVolume;
            }
        }
        
        #endregion
        
        #region Play Audio
        
        /// <summary>
        /// Phát nhạc từ list music clips theo index
        /// </summary>
        public void PlayMusic(int clipIndex, bool loop = true, float startVolume = -1f)
        {
            if (musicSource == null)
            {
                Debug.LogWarning("[AudioManager] Music source is null!");
                return;
            }
            
            if (clipIndex < 0 || clipIndex >= musicClips.Count)
            {
                Debug.LogWarning($"[AudioManager] Music clip index {clipIndex} out of range! (0-{musicClips.Count - 1})");
                return;
            }
            
            AudioClip clip = musicClips[clipIndex];
            if (clip == null)
            {
                Debug.LogWarning($"[AudioManager] Music clip at index {clipIndex} is null!");
                return;
            }
            
            musicSource.clip = clip;
            musicSource.loop = loop;
            
            // Set volume nếu được chỉ định
            if (startVolume >= 0f && audioMixer == null)
            {
                musicSource.volume = startVolume * currentMasterVolume;
            }
            else
            {
                UpdateSourceVolume(musicSource, currentMusicVolume);
            }
            
            musicSource.Play();
            Debug.Log($"[AudioManager] Playing music: {clip.name}");
        }
        
        /// <summary>
        /// Phát nhạc từ AudioClip
        /// </summary>
        public void PlayMusic(AudioClip clip, bool loop = true, float startVolume = -1f)
        {
            if (musicSource == null || clip == null)
            {
                Debug.LogWarning("[AudioManager] Music source or clip is null!");
                return;
            }
            
            musicSource.clip = clip;
            musicSource.loop = loop;
            
            // Set volume nếu được chỉ định
            if (startVolume >= 0f && audioMixer == null)
            {
                musicSource.volume = startVolume * currentMasterVolume;
            }
            else
            {
                UpdateSourceVolume(musicSource, currentMusicVolume);
            }
            
            musicSource.Play();
            Debug.Log($"[AudioManager] Playing music: {clip.name}");
        }
        
        public void StopMusic()
        {
            if (musicSource != null)
            {
                musicSource.Stop();
                Debug.Log("[AudioManager] Music stopped");
            }
        }
        
        public void PauseMusic()
        {
            if (musicSource != null)
            {
                musicSource.Pause();
                Debug.Log("[AudioManager] Music paused");
            }
        }
        
        public void ResumeMusic()
        {
            if (musicSource != null)
            {
                musicSource.UnPause();
                Debug.Log("[AudioManager] Music resumed");
            }
        }
        
        /// <summary>
        /// Phát SFX từ list sfx clips theo index
        /// </summary>
        public void PlaySFX(int clipIndex, float volumeScale = 1f)
        {
            if (sfxSource == null)
            {
                Debug.LogWarning("[AudioManager] SFX source is null!");
                return;
            }
            
            if (clipIndex < 0 || clipIndex >= sfxClips.Count)
            {
                Debug.LogWarning($"[AudioManager] SFX clip index {clipIndex} out of range! (0-{sfxClips.Count - 1})");
                return;
            }
            
            AudioClip clip = sfxClips[clipIndex];
            if (clip == null)
            {
                Debug.LogWarning($"[AudioManager] SFX clip at index {clipIndex} is null!");
                return;
            }
            
            // Áp dụng volume = currentSFXVolume * currentMasterVolume * volumeScale
            float finalVolume = audioMixer == null 
                ? currentSFXVolume * currentMasterVolume * volumeScale 
                : volumeScale;
            
            sfxSource.PlayOneShot(clip, finalVolume);
            Debug.Log($"[AudioManager] PlaySFX: {clip.name} at volume scale {volumeScale:F2}");
        }
        
        /// <summary>
        /// Phát SFX từ AudioClip
        /// </summary>
        public void PlaySFX(AudioClip clip, float volumeScale = 1f)
        {
            if (sfxSource == null || clip == null)
            {
                Debug.LogWarning("[AudioManager] SFX source or clip is null!");
                return;
            }
            
            // Áp dụng volume = currentSFXVolume * currentMasterVolume * volumeScale
            float finalVolume = audioMixer == null 
                ? currentSFXVolume * currentMasterVolume * volumeScale 
                : volumeScale;
            
            sfxSource.PlayOneShot(clip, finalVolume);
            Debug.Log($"[AudioManager] PlaySFX: {clip.name} at volume scale {volumeScale:F2}");
        }
        
        /// <summary>
        /// Phát ambience từ list ambience clips theo index
        /// </summary>
        public void PlayAmbience(int clipIndex, bool loop = true)
        {
            if (ambienceSource == null)
            {
                Debug.LogWarning("[AudioManager] Ambience source is null!");
                return;
            }
            
            if (clipIndex < 0 || clipIndex >= ambienceClips.Count)
            {
                Debug.LogWarning($"[AudioManager] Ambience clip index {clipIndex} out of range! (0-{ambienceClips.Count - 1})");
                return;
            }
            
            AudioClip clip = ambienceClips[clipIndex];
            if (clip == null)
            {
                Debug.LogWarning($"[AudioManager] Ambience clip at index {clipIndex} is null!");
                return;
            }
            
            ambienceSource.clip = clip;
            ambienceSource.loop = loop;
            UpdateSourceVolume(ambienceSource, currentAmbienceVolume);
            ambienceSource.Play();
            Debug.Log($"[AudioManager] Playing ambience: {clip.name}");
        }
        
        /// <summary>
        /// Phát ambience từ AudioClip
        /// </summary>
        public void PlayAmbience(AudioClip clip, bool loop = true)
        {
            if (ambienceSource == null || clip == null)
            {
                Debug.LogWarning("[AudioManager] Ambience source or clip is null!");
                return;
            }
            
            ambienceSource.clip = clip;
            ambienceSource.loop = loop;
            UpdateSourceVolume(ambienceSource, currentAmbienceVolume);
            ambienceSource.Play();
            Debug.Log($"[AudioManager] Playing ambience: {clip.name}");
        }
        
        public void StopAmbience()
        {
            if (ambienceSource != null)
            {
                ambienceSource.Stop();
                Debug.Log("[AudioManager] Ambience stopped");
            }
        }
        
        public void PauseAmbience()
        {
            if (ambienceSource != null)
            {
                ambienceSource.Pause();
                Debug.Log("[AudioManager] Ambience paused");
            }
        }
        
        public void ResumeAmbience()
        {
            if (ambienceSource != null)
            {
                ambienceSource.UnPause();
                Debug.Log("[AudioManager] Ambience resumed");
            }
        }
        
        #endregion
        
        #region Getters
        
        /// <summary>
        /// Lấy AudioClip từ list music theo index
        /// </summary>
        public AudioClip GetMusicClip(int index)
        {
            if (index >= 0 && index < musicClips.Count)
                return musicClips[index];
            return null;
        }
        
        /// <summary>
        /// Lấy AudioClip từ list SFX theo index
        /// </summary>
        public AudioClip GetSFXClip(int index)
        {
            if (index >= 0 && index < sfxClips.Count)
                return sfxClips[index];
            return null;
        }
        
        /// <summary>
        /// Lấy AudioClip từ list ambience theo index
        /// </summary>
        public AudioClip GetAmbienceClip(int index)
        {
            if (index >= 0 && index < ambienceClips.Count)
                return ambienceClips[index];
            return null;
        }
        
        /// <summary>
        /// Lấy tổng số music clips
        /// </summary>
        public int GetMusicClipCount() => musicClips.Count;
        
        /// <summary>
        /// Lấy tổng số SFX clips
        /// </summary>
        public int GetSFXClipCount() => sfxClips.Count;
        
        /// <summary>
        /// Lấy tổng số ambience clips
        /// </summary>
        public int GetAmbienceClipCount() => ambienceClips.Count;
        
        /// <summary>
        /// Check xem music có đang phát không
        /// </summary>
        public bool IsMusicPlaying() => musicSource != null && musicSource.isPlaying;
        
        /// <summary>
        /// Check xem ambience có đang phát không
        /// </summary>
        public bool IsAmbiencePlaying() => ambienceSource != null && ambienceSource.isPlaying;
        
        #endregion
    }
}