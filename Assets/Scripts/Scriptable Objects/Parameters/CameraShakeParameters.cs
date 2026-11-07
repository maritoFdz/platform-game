using UnityEngine;

[CreateAssetMenu(fileName = "NewCameraShakeParameters", menuName = "Scriptable Objects/Camera Shake Parameters")]
public class CameraShakeParameters : ScriptableObject
{
    [Header("Impulse Source Settings")]
    public float impactTime;
    public float impactForce;
    public Vector3 velocity;

    [Header("Impulse Listener Settings")]
    public float listenerAmplitude;
    public float listenerFrequency;
    public float listenerDuration;
}
