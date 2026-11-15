using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class RumbleManager : MonoBehaviour
{
    public static RumbleManager instance;
    public float intensity;
    private Gamepad currentPad;
    private Coroutine rumbleRoutine;
    private int currentPriority;
    private bool hasContinuosRumble;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        currentPriority = int.MinValue;
        instance = this;
    }

    public void Rumble(RumbleParameters parameters)
    {
        if (parameters == null || intensity <= 0f || hasContinuosRumble) return;
        if (DeviceManager.instance == null) return;

        Gamepad pad = DeviceManager.instance.ActiveGamepad;
        if (pad == null) return;
        if (rumbleRoutine != null && parameters.priority < currentPriority) return;
        if (rumbleRoutine != null) StopCoroutine(rumbleRoutine);

        currentPad = pad;
        currentPriority = parameters.priority;
        pad?.SetMotorSpeeds(0f, 0f);
        rumbleRoutine = StartCoroutine(RumbleCo(parameters, currentPad));
    }

    private IEnumerator RumbleCo(RumbleParameters parameters, Gamepad pad)
    {
        float time = 0f;
        while (time < parameters.duration)
        {
            float ease = parameters.fadeOut ? 1f - (time / parameters.duration) : 1f;
            pad?.SetMotorSpeeds(parameters.lowFreq * ease * intensity, parameters.highFreq * ease * intensity);
            time += Time.unscaledDeltaTime; // otra mousequerramienta que nos servira mas tarde
            yield return null;
        }
        currentPad?.SetMotorSpeeds(0f, 0f);
        rumbleRoutine = null;
        currentPriority = int.MinValue;
    }

    public void StopRumble()
    {
        if (rumbleRoutine != null) StopCoroutine(rumbleRoutine);
        rumbleRoutine = null;
        currentPriority = int.MinValue;
        currentPad?.SetMotorSpeeds(0f, 0f);
        hasContinuosRumble = false;
    }

    public void RumbleContinuous(RumbleParameters parameters)
    {
        if (parameters == null || intensity <= 0f || DeviceManager.instance == null) return;
        Gamepad pad = DeviceManager.instance.ActiveGamepad;
        if (pad == null) return;
        if (rumbleRoutine != null) StopCoroutine(rumbleRoutine);
        rumbleRoutine = null;

        currentPad = pad;
        hasContinuosRumble = true;
        pad.SetMotorSpeeds(parameters.lowFreq * intensity, parameters.highFreq * intensity);
    }

    public void StopContinuous()
    {
        if (!hasContinuosRumble) return;
        hasContinuosRumble = false;
        currentPad?.SetMotorSpeeds(0f, 0f);
    }

    private void OnDisable()
    {
        StopRumble();
    }
}