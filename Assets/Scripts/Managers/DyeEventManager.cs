using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DyeEventManager : MonoBehaviour
{
    public static DyeEventManager instance;

    [Header("References")]
    [SerializeField] private Light2D flashLightPrefab;
    [SerializeField] private Volume vigneteNBloomVolume;

    [Header("Parameters")]
    [SerializeField] private float flashMaxVolumeIntensity;
    [SerializeField] private float innerRetraso;

    [Header("Ratios")]
    [SerializeField, Range(0f, 1f)] private float startPauseRatio;
    [SerializeField, Range(0f, 1f)] private float riseRatio;
    [SerializeField, Range(0f, 1f)] private float holdRatio;
    [SerializeField, Range(0f, 1f)] private float fallRatio;
    [SerializeField, Range(0f, 1f)] private float endPauseRatio;

    private Light2D flashLight;
    private Coroutine eventRoutine;
    private float flashAmount;
    private float baseVolumeWeight;
    private float flashRadius;
    private float baseLightIntensity;

    public static event Action DyeEventStart;
    public static event Action<float> DyeEventAnimation;
    public static event Action DyeEventPeak;
    public static event Action<float> DyeEventPostPeak;
    public static event Action DyeEventEnd;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        baseVolumeWeight = vigneteNBloomVolume.weight;
        baseLightIntensity = flashLightPrefab.intensity;
    }

    public void StartDyeEvent(Transform target, float duration)
    {
        if (eventRoutine != null) return;
        eventRoutine = StartCoroutine(DyeEventCo(target, duration));
    }

    private IEnumerator DyeEventCo(Transform target, float duration)
    {
        DyeEventStart?.Invoke(); // player lo coge para quedarse parado

        if (flashLight == null) flashLight = Instantiate(flashLightPrefab);
        flashLight.transform.position = new Vector3(target.position.x, target.position.y, 0f);
        flashLight.gameObject.SetActive(true);

        yield return new WaitForSeconds(duration * startPauseRatio);

        DyeEventAnimation?.Invoke(duration * riseRatio); // lo coge el player para la animacion y la camara para el tembleque
        yield return InterpolateFlashCo(0f, 1f, duration * riseRatio, true);

        yield return new WaitForSeconds(duration * holdRatio * 0.5f);
        DyeEventPeak?.Invoke(); // lo coge settings manager para el cambio de paleta
        yield return new WaitForSeconds(duration * holdRatio * 0.5f);

        yield return InterpolateFlashCo(1f, 0f, duration * fallRatio, false);
        flashRadius = 0f;
        flashLight.gameObject.SetActive(false);

        DyeEventPostPeak?.Invoke(duration * endPauseRatio); // el tembleque termina
        yield return new WaitForSeconds(duration * endPauseRatio);

        DyeEventEnd?.Invoke(); // player se mueve
        eventRoutine = null;
    }

    private IEnumerator InterpolateFlashCo(float start, float target, float time, bool affectRadius)
    {
        float timer = 0f;
        while (timer < time)
        {
            timer += Time.deltaTime;
            flashAmount = Mathf.Lerp(start, target, Mathf.SmoothStep(0f, 1f, timer / time));
            if (affectRadius) flashRadius = flashAmount;
            ApplyFlash();
            yield return null;
        }
        flashAmount = target;
        if (affectRadius) flashRadius = target;
        ApplyFlash();
    }

    private void ApplyFlash()
    {
        float height = Camera.main.orthographicSize * 2f;
        float width = height * Camera.main.aspect;
        float diagonal = Mathf.Sqrt(width * width + height * height);

        float maxRadius = diagonal * 1.5f;
        flashLight.pointLightOuterRadius = maxRadius * flashRadius;
        flashLight.pointLightInnerRadius = maxRadius * Mathf.Pow(flashRadius, innerRetraso);
        flashLight.volumeIntensity = flashMaxVolumeIntensity * flashAmount;
        flashLight.intensity = baseLightIntensity * flashAmount;
        vigneteNBloomVolume.weight = Mathf.Lerp(baseVolumeWeight, 0f, flashAmount);
    }
}
