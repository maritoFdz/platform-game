using UnityEngine;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance;

    [SerializeField] private AudioMixer audioMixer;
    public Resolution[] resolutions;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        resolutions = Screen.resolutions;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SettingsData settingsData = DataManager.instance.GetSettingsData();
        SetFullscreen(settingsData.isFullscreen);
        SetResolution(settingsData.resolution);
        SetVolume(settingsData.volume);

    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        if (DataManager.instance != null)
            DataManager.instance.SaveFullscreenSetting(isFullscreen);
    }

    public void SetResolution(int resolutionIndex)
    {
        resolutionIndex = resolutionIndex == -1 ? GetHighestResolution() : Mathf.Clamp(resolutionIndex, 0, resolutions.Length - 1);
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        if (DataManager.instance != null)
            DataManager.instance.SaveResolutionSetting(resolutionIndex);
    }

    public void SetVolume(float volume)
    {
        float dB = 20 * Mathf.Log10(Mathf.Max(volume, 0.0001f));
        audioMixer.SetFloat("Volume", dB);
        if (DataManager.instance != null)
            DataManager.instance.SaveVolumeSetting(volume);
    }

    private int GetHighestResolution()
    {
        int highestIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
            if (resolutions[i].width * resolutions[i].height > resolutions[highestIndex].width * resolutions[highestIndex].height)
                highestIndex = i;
        return highestIndex;
    }
}