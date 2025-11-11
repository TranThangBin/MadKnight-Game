using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [Header("Mixer")]
    [SerializeField] private AudioMixer mixer;        // Kéo MainMixer

    [Header("Sliders")]
    [SerializeField] private Slider masterSlider;     // MV (âm lượng tổng)
    [SerializeField] private Slider musicSlider;      // BGM
    [SerializeField] private Slider sfxSlider;        // SFX

    [Header("Mute tổng (tuỳ chọn)")]
    [SerializeField] private Toggle masterMuteToggle; // nếu có UI toggle

    [Header("Exposed Parameters (trùng trong Mixer)")]
    [SerializeField] private string masterParam = "MVol";
    [SerializeField] private string musicParam  = "BGMVol";
    [SerializeField] private string sfxParam    = "SFXVol";

    const float MIN = 0.0001f;           // tránh log10(0)
    const float MUTE_DB = -80f;          // coi như tắt hẳn
    float lastMasterDb = 0f;             // nhớ mức trước khi mute

    void Start()
    {
        // Load
        if (masterSlider) masterSlider.value = PlayerPrefs.GetFloat("vol."+masterParam, 1f);
        if (musicSlider)  musicSlider.value  = PlayerPrefs.GetFloat("vol."+musicParam,  1f);
        if (sfxSlider)    sfxSlider.value    = PlayerPrefs.GetFloat("vol."+sfxParam,    1f);
        bool isMuted = PlayerPrefs.GetInt("vol."+masterParam+".muted", 0) == 1;

        // Apply ngay
        ApplyMaster(masterSlider ? masterSlider.value : 1f, isMuted);
        ApplyParam(musicParam,  musicSlider ? musicSlider.value : 1f);
        ApplyParam(sfxParam,    sfxSlider   ? sfxSlider.value   : 1f);

        // Hook sự kiện
        if (masterSlider) masterSlider.onValueChanged.AddListener(v => { ApplyMaster(v, GetMuteFlag()); Save("vol."+masterParam, v); });
        if (musicSlider)  musicSlider.onValueChanged.AddListener(v => { ApplyParam(musicParam, v);     Save("vol."+musicParam,  v); });
        if (sfxSlider)    sfxSlider.onValueChanged.AddListener(v => { ApplyParam(sfxParam, v);         Save("vol."+sfxParam,    v); });
        if (masterMuteToggle) {
            masterMuteToggle.isOn = isMuted;
            masterMuteToggle.onValueChanged.AddListener(MuteMaster);
        }
    }

    // ====== Public API nếu muốn gọi từ Button/Slider trong Inspector ======
    public void SetMasterVolume(float v) { if (masterSlider) masterSlider.value = v; else ApplyMaster(v, GetMuteFlag()); }
    public void SetMusicVolume(float v)  { if (musicSlider)  musicSlider.value  = v; else { ApplyParam(musicParam, v); Save("vol."+musicParam, v); } }
    public void SetSFXVolume(float v)    { if (sfxSlider)    sfxSlider.value    = v; else { ApplyParam(sfxParam, v);   Save("vol."+sfxParam,   v); } }
    public void MuteMaster(bool on)      { ApplyMaster(masterSlider ? masterSlider.value : 1f, on); PlayerPrefs.SetInt("vol."+masterParam+".muted", on ? 1 : 0); }

    // ====== Core ======
    void ApplyMaster(float value01, bool muted)
    {
        if (!mixer) return;

        // nếu đang mute thì set -80 dB, nếu không thì áp dB theo slider
        if (muted) {
            // lưu lại mức hiện tại (nếu có) để unmute trả lại
            if (mixer.GetFloat(masterParam, out float cur)) lastMasterDb = cur;
            mixer.SetFloat(masterParam, MUTE_DB);
            return;
        }

        float v  = Mathf.Clamp(value01, 0f, 1f);
        float dB = v <= MIN ? MUTE_DB : Mathf.Log10(Mathf.Clamp(v, MIN, 1f)) * 20f;
        mixer.SetFloat(masterParam, dB);
        Save("vol."+masterParam, v);
    }

    void ApplyParam(string param, float value01)
    {
        if (!mixer) return;
        float v  = Mathf.Clamp(value01, 0f, 1f);
        float dB = v <= MIN ? MUTE_DB : Mathf.Log10(Mathf.Clamp(v, MIN, 1f)) * 20f;
        if (!mixer.SetFloat(param, dB))
            Debug.LogWarning($"[VolumeSettings] Param '{param}' không tồn tại trong {mixer.name}");
    }

    bool GetMuteFlag() => masterMuteToggle ? masterMuteToggle.isOn : (PlayerPrefs.GetInt("vol."+masterParam+".muted", 0) == 1);
    void Save(string key, float v) { PlayerPrefs.SetFloat(key, v); }
}
