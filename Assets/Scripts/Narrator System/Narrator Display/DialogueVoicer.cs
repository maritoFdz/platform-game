using System.Collections.Generic;
using UnityEngine;

public class DialogueVoicer : MonoBehaviour
{
    [SerializeField] private Sound[] voiceSounds;
    private Dictionary<AudioName, Sound> voiceSoundDictionary;

    private void Awake()
    {
        voiceSoundDictionary = new Dictionary<AudioName, Sound>();
        foreach (Sound sound in voiceSounds)
            voiceSoundDictionary.TryAdd(sound.name, sound);
    }

    public void PlayMumble()
    {
        if (AudioManager.instance == null || voiceSounds == null || voiceSounds.Length == 0) return;
        Sound sound = voiceSounds[Random.Range(0, voiceSounds.Length)];
        AudioManager.instance.PlaySound(sound);
    }
}