using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent (typeof(BoxCollider2D))]
[RequireComponent (typeof(Rigidbody2D))]
public class Dyes : MonoBehaviour
{
    [SerializeField] private ColorPallete palleteUnlocked;
    [SerializeField] private float grabbedEventDuration;
    [SerializeField] private CinemachineImpulseSource impulseSource;
    [SerializeField] private CameraShakeParameters rumbleParameters;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private bool triggered;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (triggered) return;
        if (!collision.TryGetComponent<Player>(out var player) || !player.IsActive) return;
        triggered = true;
        if (AudioManager.instance != null) AudioManager.instance.Play(AudioName.GrabbedDye, false);
        DyeEventManager.DyeEventPeak += ApplyPallete;
        DyeEventManager.DyeEventEnd += Finish;
        DyeEventManager.DyeEventAnimation += Rumble;
        DyeEventManager.instance.StartDyeEvent(player.transform, grabbedEventDuration);
    }

    private void ApplyPallete()
    {
        spriteRenderer.enabled = false; // yo se que esto no va aqui pero basta ya de crear metodos
        if (DataManager.instance != null)
            DataManager.instance.AddUnlockedPallete(palleteUnlocked);
        if (SettingsManager.instance != null)
            SettingsManager.instance.SetPalleteById(palleteUnlocked.palleteId);
    }

    private void Finish()
    {
        Destroy(gameObject);
    }

    private void Rumble(float riseTime)
    {
        DyeEventManager.DyeEventAnimation -= Rumble;
        if (CameraManager.instance != null) CameraManager.instance.ShakaCameraWithParameters(rumbleParameters, impulseSource);
    }

    private void OnDestroy()
    {
        DyeEventManager.DyeEventPeak -= ApplyPallete;
        DyeEventManager.DyeEventEnd -= Finish;
        DyeEventManager.DyeEventAnimation -= Rumble;
    }
}
