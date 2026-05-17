using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [SerializeField] private Sound[] sounds;

    private Transform audioContainer;
    private Dictionary<AudioName, Sound> soundDictionary;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        audioContainer = new GameObject("Audio Container").transform;
        audioContainer.transform.SetParent(transform);
        soundDictionary = new Dictionary<AudioName, Sound>();

        foreach (Sound sound in sounds)
        {
            if (!soundDictionary.ContainsKey(sound.name))
                soundDictionary.Add(sound.name, sound);

            sound.audioSource = audioContainer.gameObject.AddComponent<AudioSource>();
            sound.audioSource.outputAudioMixerGroup = sound.mixerGroup;
            sound.audioSource.playOnAwake = sound.playsOnAwake;
            sound.audioSource.loop = sound.loops;
            sound.audioSource.clip = sound.clip;
            sound.audioSource.volume = sound.volume;
            sound.audioSource.pitch = sound.minPitch;

        }
    }

    public void Play(AudioName name, bool randomize = true)
    {
        if (soundDictionary.TryGetValue(name, out Sound sound))
        {
            if (sound.loops && sound.audioSource.isPlaying)
                return;
            if (randomize) sound.audioSource.pitch = Random.Range(sound.minPitch, sound.maxPitch);
            else sound.audioSource.pitch = sound.defaultPitch;
            sound.audioSource.Play();
        }
    }

    public void PlayRandom(params AudioName[] names)
    {
        AudioName random = names[Random.Range(0, names.Length)];
        Play(random);
    }

    public void StopPlaying(AudioName name)
    {
        if (soundDictionary.TryGetValue(name, out Sound sound))
        {
            sound.audioSource.Stop();
        }
    }
}
