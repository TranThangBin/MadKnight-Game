using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSetting : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider musicSlider;

    public string parameterName = "MasterVol";

    public void SetVolume(float v)
    {
        float dB = v <= 0.0001f ? -80f : Mathf.Log10(Mathf.Clamp(v, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat(parameterName, dB);
    }
}
