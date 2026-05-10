using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager instance;
    private const string saveFile = "data.pepillo";
    private const string encryptionKey = "m7Kp2Xq9Lz4Nv8Rt1By6Hd3Fs0Wu5QeA";

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
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, Encrypt(json));
    }

    private void LoadGameData()
    {
        string path = Path.Combine(Application.persistentDataPath, saveFile);
        if (File.Exists(path))
        {
            string dataText = File.ReadAllText(path);
            GameData data = JsonUtility.FromJson<GameData>(Decrypt(dataText));
            lastRoomName = data.lastRoomName;
        }
        else
        {
            lastRoomName = firstRoomName;
            SaveGameData(new(firstRoomName));
        }
    }
    #endregion

    #region Encryption
    private string Encrypt(string original)
    {
        byte[] key = Encoding.UTF8.GetBytes(encryptionKey);
        using Aes aesAlg = Aes.Create();
        aesAlg.Key = key;
        aesAlg.GenerateIV();
        ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

        using MemoryStream msEncrypt = new();
        msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);
        using (CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write))
        {
            using StreamWriter swEncrypt = new(csEncrypt);
            swEncrypt.Write(original);
        }
        return Convert.ToBase64String(msEncrypt.ToArray());
    }

    private string Decrypt(string cipheredData)
    {
        byte[] fullCipher = Convert.FromBase64String(cipheredData);
        byte[] iv = new byte[16];
        byte[] cipher = new byte[fullCipher.Length - 16];

        Array.Copy(fullCipher, iv, iv.Length);
        Array.Copy(fullCipher, 16, cipher, 0, cipher.Length);

        byte[] key = Encoding.UTF8.GetBytes(encryptionKey);
        using Aes aesAlg = Aes.Create();
        aesAlg.Key = key;
        aesAlg.IV = iv;
        ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
        using MemoryStream msDecrypt = new(cipher);
        using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);
        using StreamReader srDecrypt = new(csDecrypt);
        return srDecrypt.ReadToEnd();
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