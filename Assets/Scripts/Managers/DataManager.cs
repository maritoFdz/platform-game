using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        DontDestroyOnLoad(gameObject);
    }

    #region Settings Data
    public SettingsData GetSettingsData()
    {
        return new SettingsData(PlayerPrefs.GetInt("Fullscreen", 1) != 0, PlayerPrefs.GetInt("ResolutionIndex", -1), PlayerPrefs.GetFloat("Volume", 1f));
    }

    public void SaveResolutionSetting(int resolution)
    {
        PlayerPrefs.SetInt("ResolutionIndex", resolution);
    }

    public void SaveFullscreenSetting(bool fullscreen)
    {
        PlayerPrefs.SetInt("Fullscreen", fullscreen ? 1 : 0);
    }

    public void SaveVolumeSetting(float volume)
    {
        PlayerPrefs.SetFloat("Volume", volume);
    }
    #endregion

    #region Game Data
    private void SaveGameData()
    {

    }

    private void LoadGameData()
    {

    }
    #endregion
}

public struct SettingsData
{
    public bool isFullscreen;
    public int resolution;
    public float volume;

    public SettingsData(bool isFullscreen, int resolution, float volume)
    {
        this.isFullscreen = isFullscreen;
        this.resolution = resolution;
        this.volume = volume;
    }
}