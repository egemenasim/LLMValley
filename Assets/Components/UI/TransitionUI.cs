using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

/// <summary>
/// Handles screen transitions (fade in/out) using a UI Image.
/// Optimized with a manual easing function to simulate tweening without external libraries.
/// </summary>
[RequireComponent(typeof(Image))]
public class TransitionUI : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────────────
    public static TransitionUI Instance { get; private set; }

    // ── Inspector ─────────────────────────────────────────────────────────
    [Header("Settings")]
    [Tooltip("The Image component used for the fade overlay.")]
    [SerializeField] private Image transitionImage;

    [Tooltip("Default time taken to fade in or out.")]
    [SerializeField] private float defaultDuration = 1.0f;

    [Tooltip("If true, the screen will fade out (to clear) as soon as the game starts.")]
    [SerializeField] private bool fadeOutOnStart = true;

    // ── Private State ─────────────────────────────────────────────────────
    private Coroutine _fadeCoroutine;

    // ── Unity Lifecycle ───────────────────────────────────────────────────
    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Auto-assign if missing
        if (transitionImage == null)
            transitionImage = GetComponent<Image>();
    }

    private void Start()
    {
        if (fadeOutOnStart)
        {
            // Start the scene by fading out the black screen
            fade_out(defaultDuration);
        }
    }

    // ── Public API ────────────────────────────────────────────────────────

    /// <summary>
    /// Fades the overlay to fully opaque (usually black).
    /// </summary>
    /// <param name="duration">Duration in seconds. Uses default if < 0.</param>
    /// <param name="onComplete">Callback when animation finished.</param>
    public void fade_in(float duration = -1f, Action onComplete = null)
    {
        float dur = duration < 0 ? defaultDuration : duration;
        StartFade(1f, dur, onComplete);
    }

    /// <summary>
    /// Fades the overlay to fully transparent.
    /// </summary>
    /// <param name="duration">Duration in seconds. Uses default if < 0.</param>
    /// <param name="onComplete">Callback when animation finished.</param>
    public void fade_out(float duration = -1f, Action onComplete = null)
    {
        float dur = duration < 0 ? defaultDuration : duration;
        StartFade(0f, dur, onComplete);
    }

    // ── Internal Logic ────────────────────────────────────────────────────

    private void StartFade(float targetAlpha, float duration, Action onComplete)
    {
        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);

        _fadeCoroutine = StartCoroutine(FadeRoutine(targetAlpha, duration, onComplete));
    }

    private IEnumerator FadeRoutine(float targetAlpha, float duration, Action onComplete)
    {
        if (transitionImage == null)
        {
            Debug.LogError("[TransitionUI] Image component is missing!", this);
            yield break;
        }

        Color color = transitionImage.color;
        float startAlpha = color.a;
        float elapsed = 0f;

        // Ensure duration isn't exactly zero to avoid issues
        duration = Mathf.Max(duration, 0.001f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            
            // "Tween" Easing: SmoothStep (Ease In Out)
            float easedT = t * t * (3f - 2f * t);
            
            color.a = Mathf.Lerp(startAlpha, targetAlpha, easedT);
            transitionImage.color = color;
            
            yield return null;
        }

        // Snap to final value
        color.a = targetAlpha;
        transitionImage.color = color;
        _fadeCoroutine = null;
        
        onComplete?.Invoke();
    }
}
