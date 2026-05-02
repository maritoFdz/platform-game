using System;
using UnityEngine;

public enum AudioName {
    DashOne,
    DashTwo,
    Death,
    FallHeavy,
    FallWeak,
    Jump,
    Movement,
    Sliding,
    SplashLoud,
    SplashOut,
    SplashWeak,
    Split
}

[Serializable]
public class Sound
{
    public AudioName name;
    public AudioClip clip;

    [HideInInspector] public AudioSource audioSource;

    [Range (0, 1)]
    public float volume;
    [Range (-3, 3)]
    public float pitch;
    public bool loops;
    public bool playsOnAwake;
}
