using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsMenuUI : MonoBehaviour
{
    public TMP_Dropdown resolutionsDropdown;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string mainSceneName;

    private Resolution[] resolutions;

    private void Awake()
    {
        resolutions = Screen.resolutions;
        resolutionsDropdown.ClearOptions();
        List<string> resolutionsStrings = new();
        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            Resolution resolution = resolutions[i];
            resolutionsStrings.Add(resolution.width + "x" + resolution.height);
            if (resolution.width == Screen.currentResolution.width && resolution.height == Screen.currentResolution.height)
                currentResolutionIndex = i;
        }
        resolutionsDropdown.AddOptions(resolutionsStrings);
        resolutionsDropdown.value = currentResolutionIndex;
        resolutionsDropdown.RefreshShownValue();
    }

    public void GoBack()
    {
        SceneManager.LoadScene(mainSceneName);
    }

    public void SetVolume(float volume)
    {
        float dB = 20 * Mathf.Log10(volume);
        audioMixer.SetFloat("Volume", dB);
    }

    public void OnSliderReleased()
    {
        if (AudioManager.instance != null)
            AudioManager.instance.Play(AudioName.Jump);
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }
}