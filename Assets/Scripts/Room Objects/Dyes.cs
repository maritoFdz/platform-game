using System;
using System.Collections;
using UnityEngine;

[RequireComponent (typeof(BoxCollider2D))]
[RequireComponent (typeof(Rigidbody2D))]
public class Dyes : MonoBehaviour
{
    [SerializeField] private ColorPallete palleteUnlocked;
    [SerializeField] private float grabbedEventDuration;

    private bool triggered;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (triggered) return;
        if (!collision.TryGetComponent<Player>(out var player) || !player.IsActive) return;
        triggered = true;
        DyeEventManager.DyeEventPeak += ApplyPallete;
        DyeEventManager.DyeEventEnd += Finish;
        DyeEventManager.instance.StartDyeEvent(player.transform, grabbedEventDuration);
    }

    private void ApplyPallete()
    {
        if (DataManager.instance != null)
            DataManager.instance.AddUnlockedPallete(palleteUnlocked);
        if (SettingsManager.instance != null)
            SettingsManager.instance.SetPalleteById(palleteUnlocked.palleteId);
    }

    private void Finish()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        DyeEventManager.DyeEventPeak -= ApplyPallete;
        DyeEventManager.DyeEventEnd -= Finish;
    }
}
