using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class PlayerUI : MonoBehaviour
{
    public static PlayerUI instance;

    [SerializeField] private Material UIMaterial;
    [SerializeField] private RawImage UI;
    [SerializeField] private float fadeTime;
    [SerializeField] private float actualizeTime;
    [SerializeField] private float startFlashThreeshold;
    [SerializeField] private float flashWait;
    [SerializeField] private float flashTime;

    [Header("Dark fade")]
    [SerializeField] private float darkThreshold;
    [SerializeField] private float darkMax;
    [SerializeField] private float darkFadeTime;

    private static readonly int FillHash = Shader.PropertyToID("_Fill");
    private int darkHash;
    private bool isDark;
    private int flashHash;
    private float currentFill;
    private float currentFlash;
    private float targetFill;
    private float velocity;
    private Coroutine fadeRoutine;
    private Coroutine flashRoutine;
    private Coroutine darkRoutine;


    private void Awake()
    {
        if (instance != null) Destroy(gameObject);
        else instance = this;

        targetFill = 1f;
        currentFill = UIMaterial.GetFloat(FillHash);
        flashHash = Shader.PropertyToID("_PulseAmount");
        darkHash = Shader.PropertyToID("_DarkAmount");
        currentFlash = 0f;
        UIMaterial.SetFloat(flashHash, currentFlash);
        targetFill = currentFill;
        isDark = currentFill < darkThreshold;
        UIMaterial.SetFloat(darkHash, isDark ? darkMax : 0f);
    }

    private void Start()
    {
        if (DyeEventManager.instance != null)
        {
            DyeEventManager.DyeEventStart += FadeOut;
            DyeEventManager.DyeEventPostPeak += FadeIn;
        }
    }

    private void Update()
    {
        if ((currentFill < darkThreshold) != isDark)
        {
            isDark = currentFill < darkThreshold;
            if (darkRoutine != null) StopCoroutine(darkRoutine);
            darkRoutine = StartCoroutine(DarkFadeCo(isDark ? darkMax : 0f));
        }

        if (currentFill <= startFlashThreeshold)
            flashRoutine ??= StartCoroutine(FlashCo());
        else if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
            currentFlash = 0f;
            UIMaterial.SetFloat(flashHash, currentFlash);
        }

        if (Mathf.Abs(currentFill - targetFill) < 0.0001f)
        {
            if (currentFill != targetFill)
            {
                currentFill = targetFill;
                UIMaterial.SetFloat(FillHash, currentFill);
            }
            return;
        }

        currentFill = Mathf.SmoothDamp(currentFill, targetFill, ref velocity, actualizeTime, Mathf.Infinity, Time.deltaTime);
        UIMaterial.SetFloat(FillHash, currentFill);
    }

    private IEnumerator DarkFadeCo(float target)
    {
        float start = UIMaterial.GetFloat(darkHash);
        float time = 0f;
        while (time < darkFadeTime)
        {
            time += Time.deltaTime;
            float currentDark = Mathf.SmoothStep(0f, 1f, time / darkFadeTime);
            UIMaterial.SetFloat(darkHash, Mathf.Lerp(start, target, currentDark));
            yield return null;
        }
        UIMaterial.SetFloat(darkHash, target);
        darkRoutine = null;
    }

    public IEnumerator FlashCo()
    {
        while (true)
        {
            float time = 0f;
            while (time < flashTime)
            {
                time += Time.deltaTime;
                currentFlash = Mathf.Lerp(0f, 1f, Mathf.Clamp01(time / flashTime));
                UIMaterial.SetFloat(flashHash, currentFlash);
                yield return null;
            }
            yield return new WaitForSeconds(flashWait);
            time = 0f;
            while (time < flashTime)
            {
                time += Time.deltaTime;
                currentFlash = Mathf.Lerp(1f, 0f, Mathf.Clamp01(time / flashTime));
                UIMaterial.SetFloat(flashHash, currentFlash);
                yield return null;
            }
            yield return new WaitForSeconds(flashWait);
        }
    }

    public void SetFill(float fill)
    {
        targetFill = Mathf.Clamp01(fill);
    }

    public void FadeOut()
    {
        Fade(0f);
    }

    public void FadeIn(float rellenoXD)
    {
        Fade(1f);
    }

    private void Fade(float target)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeCo(target));
    }

    private IEnumerator FadeCo(float target)
    {
        float start = UI.color.a;
        float time = 0f;
        while (time < fadeTime)
        {
            time += Time.deltaTime;
            Color color = UI.color;
            color.a = Mathf.Lerp(start, target, time / fadeTime);
            UI.color = color;
            yield return null;
        }

        Color final = UI.color;
        final.a = target;
        UI.color = final;
        fadeRoutine = null;
    }

    private void OnDestroy()
    {
        DyeEventManager.DyeEventStart -= FadeOut;
        DyeEventManager.DyeEventPostPeak -= FadeIn;
    }
}
