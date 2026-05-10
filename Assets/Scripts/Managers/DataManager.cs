using System.IO;
using UnityEditor.U2D.Tooling.Analyzer;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager instance;
    private const string saveFile = "data.pepillo";

    [HideInInspector] public bool initialized;
    [SerializeField] private string firstRoomName;
    [HideInInspector] public string lastRoomName;

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

    private void Start()
    {
        LoadGameData();
        initialized = true;
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
    public void SaveGameData(GameData data)
    {
        string path = Path.Combine(Application.persistentDataPath, saveFile);
        lastRoomName = data.lastRoomName;
        File.WriteAllText(path, JsonUtility.ToJson(data, true));
    }

    private void LoadGameData()
    {
        string path = Path.Combine(Application.persistentDataPath, saveFile);
        if (File.Exists(path))
        {
            GameData data = JsonUtility.FromJson<GameData>(File.ReadAllText(path));
            lastRoomName = data.lastRoomName;
        }
        else
        {
            lastRoomName = firstRoomName;
            SaveGameData(new(firstRoomName));
        }
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