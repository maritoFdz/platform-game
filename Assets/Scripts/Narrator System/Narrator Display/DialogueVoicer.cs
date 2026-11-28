using System.Collections.Generic;
using UnityEngine;

public class DialogueVoicer : MonoBehaviour
{
    [SerializeField] private Sound[] mumbleSounds;

    public void PlayMumble(char current)
    {
        if (AudioManager.instance == null || mumbleSounds == null || mumbleSounds.Length == 0) return;
        current = char.ToLowerInvariant(current);
        int hash = current.GetHashCode();
        Sound sound = mumbleSounds[Mathf.Abs(hash % mumbleSounds.Length)];
        int minPitchInt = (int)(sound.minPitch * 100);
        int maxPitchInt = (int)(sound.maxPitch * 100);
        int pitchRange = maxPitchInt - minPitchInt;
        float pitch;
        if (pitchRange != 0) pitch = (Mathf.Abs((hash % pitchRange)) + minPitchInt) / 100f;
        else pitch = sound.minPitch;
        AudioManager.instance.PlaySound(sound, pitch);
    }
}