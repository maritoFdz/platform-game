using NUnit.Framework.Constraints;
using System;
using UnityEngine;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance;

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private int frameRate;

    [Header("Pallete Shader")]
    [SerializeField] private Material palleteMaterial;
    [SerializeField] private Material iceMaterial;
    [SerializeField] private ColorPallete defaultPallete;
    [SerializeField] private float tolerance;
    public ColorPallete[] palletes;

    [HideInInspector] public Resolution[] resolutions;
    [HideInInspector] public int currentPalleteIndex;
    [HideInInspector] public bool initialized;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        resolutions = Screen.resolutions;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = frameRate;
        SetDefaultPalleteShader();
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SettingsData settingsData = DataManager.instance.GetSettingsData();
        currentPalleteIndex = settingsData.currentPallete;
        if (!SetColorPallete(currentPalleteIndex))
            SetColorPallete(0);
        SetFullscreen(settingsData.isFullscreen);
        SetResolution(settingsData.resolution);
        SetVolume(settingsData.volume);
        initialized = true;
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

    public bool SetColorPallete(int palleteIndex) // returns true if seted
    {
        palleteIndex = palleteIndex == -1 ? 0 : Mathf.Clamp(palleteIndex, 0, palletes.Length - 1);
        ColorPallete pallete = palletes[palleteIndex];
        if (pallete == null)
        {
            pallete = defaultPallete;
            palleteIndex = 0;
        }
        currentPalleteIndex = palleteIndex;
        if (DataManager.instance != null)
        {
            if (!DataManager.instance.unlockedPalletes.Contains(pallete.palleteId))
                return false;
        }
        else
            return false;
        palleteMaterial.SetColor("_New1", pallete.color1);
        palleteMaterial.SetColor("_New2", pallete.color2);
        palleteMaterial.SetColor("_New3", pallete.color3);
        palleteMaterial.SetColor("_New4", pallete.color4);
        palleteMaterial.SetColor("_New5", pallete.color5);
        palleteMaterial.SetColor("_New6", pallete.color6);
        palleteMaterial.SetColor("_New7", pallete.color7);
        iceMaterial.SetColor("_Ice_Color", pallete.color7);
        iceMaterial.SetFloat("_Opacity", pallete.iceOpacity);
        if (DataManager.instance != null)
            DataManager.instance.SavePalleteSetting(palleteIndex);
        return true;
    }

    public void SetPalleteById(int id)
    {
        if (DataManager.instance == null) return;
        if (DataManager.instance.unlockedPalletes.Contains(id))
        {
            int value = GetPalleteIndex(id);
            currentPalleteIndex = value == -1 ? currentPalleteIndex : value;
            SetColorPallete(currentPalleteIndex);
        }
    }

    private int GetPalleteIndex(int id)
    {
        for (int i = 0; i < palletes.Length; i++)
            if (palletes[i].palleteId == id) return i;
        return -1;
    }

    private void SetDefaultPalleteShader()
    {
        palleteMaterial.SetColor("_Original1", defaultPallete.color1);
        palleteMaterial.SetColor("_Original2", defaultPallete.color2);
        palleteMaterial.SetColor("_Original3", defaultPallete.color3);
        palleteMaterial.SetColor("_Original4", defaultPallete.color4);
        palleteMaterial.SetColor("_Original5", defaultPallete.color5);
        palleteMaterial.SetColor("_Original6", defaultPallete.color6);
        palleteMaterial.SetColor("_Original7", defaultPallete.color7);
        iceMaterial.SetColor("_Ice_Color", defaultPallete.color7);
        iceMaterial.SetFloat("_Opacity", defaultPallete.iceOpacity);
        palleteMaterial.SetFloat("_Tolerance", tolerance);
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