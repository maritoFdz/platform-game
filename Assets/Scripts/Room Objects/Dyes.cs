using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent (typeof(BoxCollider2D))]
[RequireComponent (typeof(Rigidbody2D))]
public class Dyes : MonoBehaviour
{
    [SerializeField] private ColorPallete palleteUnlocked;
    [SerializeField] private float grabbedEventDuration;
    [SerializeField] private CinemachineImpulseSource impulseSource;
    [SerializeField] private CameraShakeParameters rumbleParameters;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Light2D dyeLight;

    public static Player Toucher { get; private set; }

    private bool triggered;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (triggered) return;
        if (!collision.TryGetComponent<Player>(out var player)) return;
        triggered = true;
        Toucher = player;
        if (AudioManager.instance != null) AudioManager.instance.Play(AudioName.GrabbedDye, false);
        DyeEventManager.DyeEventPeak += ApplyPallete;
        DyeEventManager.DyeEventEnd += Finish;
        DyeEventManager.DyeEventAnimation += Rumble;
        DyeEventManager.instance.StartDyeEvent(player.transform, grabbedEventDuration);
    }

    private void ApplyPallete()
    {
        spriteRenderer.enabled = false; // yo se que esto no va aqui pero basta ya de crear metodos
        dyeLight.gameObject.SetActive(false);
        if (DataManager.instance != null)
            DataManager.instance.AddUnlockedPallete(palleteUnlocked);
        if (SettingsManager.instance != null)
            SettingsManager.instance.SetPalleteById(palleteUnlocked.palleteId);
    }

    private void Finish()
    {
        Toucher = null;
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
