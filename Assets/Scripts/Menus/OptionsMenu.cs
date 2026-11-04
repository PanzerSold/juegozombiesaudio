using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class OptionsMenu : MonoBehaviour
{
    [Header("Mouse")]
    [SerializeField] private Slider mouseSlider;
    [SerializeField] private TMP_Text mouseLabel;

    [Header("Joystick")]
    [SerializeField] private Slider stickSlider;
    [SerializeField] private TMP_Text stickLabel;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Audio")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider ambientSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider voicesSlider;

    [SerializeField] private TMP_Text masterPercent;
    [SerializeField] private TMP_Text ambientPercent;
    [SerializeField] private TMP_Text musicPercent;
    [SerializeField] private TMP_Text sfxPercent;
    [SerializeField] private TMP_Text voicesPercent;

    private void Start()
    {
        SetupSlider(mouseSlider, mouseLabel, GameSettings.MouseSensitivity, v => GameSettings.MouseSensitivity = v);
        SetupSlider(stickSlider, stickLabel, GameSettings.StickSensitivity, v => GameSettings.StickSensitivity = v);
        SetupVolumeSlider(masterSlider, masterPercent, GameSettings.MasterVolume, v => GameSettings.MasterVolume = v, "MasterVolume");
        SetupVolumeSlider(ambientSlider, ambientPercent, GameSettings.AmbientVolume, v => GameSettings.AmbientVolume = v, "AmbientVolume");
        SetupVolumeSlider(musicSlider, musicPercent, GameSettings.MusicVolume, v => GameSettings.MusicVolume = v, "MusicVolume");
        SetupVolumeSlider(sfxSlider, sfxPercent, GameSettings.SFXVolume, v => GameSettings.SFXVolume = v, "SFXVolume");
        SetupVolumeSlider(voicesSlider, voicesPercent, GameSettings.VoicesVolume, v => GameSettings.VoicesVolume = v, "VoicesVolume");
    }

    private void SetupSlider(Slider slider, TMP_Text label, float current, System.Action<float> onChange)
    {
        slider.minValue = 0.1f;
        slider.maxValue = 3f;
        slider.SetValueWithoutNotify(current);
        label.text = current.ToString("0.0") + "x";
        slider.onValueChanged.AddListener(v => {onChange(v); label.text = v.ToString("0.0") + "x";});
    }

    private void SetupVolumeSlider(Slider slider, TMP_Text label, float current, System.Action<float> onChange, string mixerParameter)
    {
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.SetValueWithoutNotify(current);
        label.text = Mathf.RoundToInt(current * 100f) + "%";
        SetVolume(mixerParameter, current);
        slider.onValueChanged.AddListener(v => {onChange(v); SetVolume(mixerParameter, v); label.text = Mathf.RoundToInt(v * 100f) + "%";});
    }

    private void SetVolume(string parameterName, float volume)
    {
        GameSettings.ApplyMixerVolume(audioMixer, parameterName, volume);
    }
}