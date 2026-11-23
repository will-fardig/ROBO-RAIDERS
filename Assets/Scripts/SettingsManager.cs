using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("Volume Sliders")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider soundEffectsSlider;

    private const string MusicKey = "MusicVolume";
    private const string SFXKey = "SFXVolume";

    private void Start()
    {
        float savedMusicVolume = PlayerPrefs.GetFloat(MusicKey, 1f);
        float savedSFXVolume = PlayerPrefs.GetFloat(SFXKey, 1f);

        musicVolumeSlider.SetValueWithoutNotify(savedMusicVolume);
        soundEffectsSlider.SetValueWithoutNotify(savedSFXVolume);

        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        soundEffectsSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    public void SetMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat(MusicKey, volume);
        PlayerPrefs.Save();

        Debug.Log("Music Volume: " + volume);
    }

    public void SetSFXVolume(float volume)
    {
        PlayerPrefs.SetFloat(SFXKey, volume);
        PlayerPrefs.Save();

        Debug.Log("Sound Effects Volume: " + volume);
    }

    private void OnDestroy()
    {
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.RemoveListener(SetMusicVolume);

        if (soundEffectsSlider != null)
            soundEffectsSlider.onValueChanged.RemoveListener(SetSFXVolume);
    }
}