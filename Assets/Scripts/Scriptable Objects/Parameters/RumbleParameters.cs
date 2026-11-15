using UnityEngine;

[CreateAssetMenu(fileName = "New Rumble Parameters", menuName = "Scriptable Objects/Rumble Parameters")]
public class RumbleParameters : ScriptableObject
{
    [Range(0f, 1f)] public float lowFreq;
    [Range(0f, 1f)] public float highFreq;
    [Min(0.01f)] public float duration;
    public bool fadeOut;
    public int priority;
}
