using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class SettingsMenuUI : MonoBehaviour
{
    public TMP_Dropdown resolutionsDropdown;
    public TMP_Text paletteText;
    [SerializeField] private string mainSceneName;

    private Resolution[] resolutions;
    private ColorPallete[] palletes;

    private int palleteIndex;

    private void Start()
    {
        if (SettingsManager.instance == null) return;

        resolutions = SettingsManager.instance.resolutions;
        palletes = SettingsManager.instance.palletes;
        palleteIndex = SettingsManager.instance.currentPalleteIndex;
        RefreshPalleteText(palletes[palleteIndex].name);
        resolutionsDropdown.ClearOptions();
        List<string> resolutionsStrings = new();
        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            Resolution resolution = resolutions[i];
            string resolutionString = resolution.width + "x" + resolution.height + " @" + Mathf.RoundToInt((float)resolution.refreshRateRatio.value) + "Hz";
            resolutionsStrings.Add(resolutionString);

            if (resolution.width == Screen.width && resolution.height == Screen.height)
                currentResolutionIndex = resolutionsStrings.Count - 1;
        }

        resolutionsDropdown.AddOptions(resolutionsStrings);
        resolutionsDropdown.value = currentResolutionIndex;
        resolutionsDropdown.RefreshShownValue();
    }

    public void GoBack()
    {
        SceneManager.LoadScene(mainSceneName);
    }

    public void OnSliderReleased()
    {
        if (AudioManager.instance != null)
            AudioManager.instance.Play(AudioName.Jump);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        if (SettingsManager.instance != null)
            SettingsManager.instance.SetFullscreen(isFullscreen);
    }

    public void SetResolution(int resolutionIndex)
    {
        if (SettingsManager.instance != null)
            SettingsManager.instance.SetResolution(resolutionIndex);
    }

    public void SetVolume(float volume)
    {
        if (SettingsManager.instance != null)
            SettingsManager.instance.SetVolume(volume);
    }

    public void SelectButton(GameObject button)
    {
        EventSystem.current.SetSelectedGameObject(button);
    }

    public void RefreshPalleteText(string text)
    {
        paletteText.text = text;
    }

    public void GoNextPallete()
    {
        palleteIndex = (palleteIndex + 1) % palletes.Length;
        if (SettingsManager.instance != null)
            SettingsManager.instance.SetColorPallete(palleteIndex);
        RefreshPalleteText(palletes[palleteIndex].name);
    }

    public void GoBackPallete()
    {
        palleteIndex = (palleteIndex - 1 + palletes.Length) % palletes.Length;
        if (SettingsManager.instance != null)
            SettingsManager.instance.SetColorPallete(palleteIndex);
        RefreshPalleteText(palletes[palleteIndex].name);
    }
}