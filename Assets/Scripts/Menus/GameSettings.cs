using System;
using UnityEngine;

public static class GameSettings
{
    const string MouseKey = "sens_mouse";
    const string StickKey = "sens_stick";
    const string MasterVolumeKey = "master_volume";
    const string AmbientVolumeKey = "ambient_volume";
    const string MusicVolumeKey = "music_volume";
    const string SFXVolumeKey = "sfx_volume";

    public static event Action OnChanged;

    public static float MouseSensitivity
    {
        get => PlayerPrefs.GetFloat(MouseKey, 1f);
        set
        {
            PlayerPrefs.SetFloat(MouseKey, value);
            PlayerPrefs.Save();
            OnChanged?.Invoke();
        }
    }

    public static float StickSensitivity
    {
        get => PlayerPrefs.GetFloat(StickKey, 1f);
        set
        {
            PlayerPrefs.SetFloat(StickKey, value);
            PlayerPrefs.Save();
            OnChanged?.Invoke();
        }
    }

    public static float MasterVolume
    {
        get => PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
        set
        {
            PlayerPrefs.SetFloat(MasterVolumeKey, value);
            PlayerPrefs.Save();
            OnChanged?.Invoke();
        }
    }

    public static float AmbientVolume
    {
        get => PlayerPrefs.GetFloat(AmbientVolumeKey, 1f);
        set
        {
            PlayerPrefs.SetFloat(AmbientVolumeKey, value);
            PlayerPrefs.Save();
            OnChanged?.Invoke();
        }
    }

    public static float MusicVolume
    {
        get => PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        set
        {
            PlayerPrefs.SetFloat(MusicVolumeKey, value);
            PlayerPrefs.Save();
            OnChanged?.Invoke();
        }
    }

    public static float SFXVolume
    {
        get => PlayerPrefs.GetFloat(SFXVolumeKey, 1f);
        set
        {
            PlayerPrefs.SetFloat(SFXVolumeKey, value);
            PlayerPrefs.Save();
            OnChanged?.Invoke();
        }
    }
}