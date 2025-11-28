using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

/// <summary>
/// Reusable UI animation presets and utilities
/// </summary>
public static class UIAnimationPresets
{
    public enum AnimationType
    {
        FadeOnly,
        FadeAndScale,
        FadeAndSlideUp,
        FadeAndSlideDown,
        FadeAndSlideLeft,
        FadeAndSlideRight
    }

    /// <summary>
    /// Animate a VisualElement in with various effects
    /// </summary>
    public static IEnumerator AnimateIn(VisualElement element, AnimationType type, float duration, AnimationCurve curve = null)
    {
        if (element == null) yield break;
        if (curve == null) curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        element.style.display = DisplayStyle.Flex;
        element.style.opacity = 0;

        Vector3 startPosition = Vector3.zero;
        Vector3 endPosition = Vector3.zero;
        float startScale = 1f;
        float endScale = 1f;

        // Setup animation based on type
        switch (type)
        {
            case AnimationType.FadeOnly:
                break;

            case AnimationType.FadeAndScale:
                startScale = 0.95f;
                endScale = 1f;
                element.style.scale = new Scale(new Vector3(startScale, startScale, 1));
                break;

            case AnimationType.FadeAndSlideUp:
                startPosition = new Vector3(0, 50, 0);
                element.style.translate = new Translate(0, 50, 0);
                break;

            case AnimationType.FadeAndSlideDown:
                startPosition = new Vector3(0, -50, 0);
                element.style.translate = new Translate(0, -50, 0);
                break;

            case AnimationType.FadeAndSlideLeft:
                startPosition = new Vector3(50, 0, 0);
                element.style.translate = new Translate(50, 0, 0);
                break;

            case AnimationType.FadeAndSlideRight:
                startPosition = new Vector3(-50, 0, 0);
                element.style.translate = new Translate(-50, 0, 0);
                break;
        }

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float curveValue = curve.Evaluate(t);

            // Animate opacity
            element.style.opacity = curveValue;

            // Animate scale
            if (type == AnimationType.FadeAndScale)
            {
                float scale = Mathf.Lerp(startScale, endScale, curveValue);
                element.style.scale = new Scale(new Vector3(scale, scale, 1));
            }

            // Animate position
            if (type != AnimationType.FadeOnly && type != AnimationType.FadeAndScale)
            {
                Vector3 position = Vector3.Lerp(startPosition, endPosition, curveValue);
                element.style.translate = new Translate(position.x, position.y, position.z);
            }

            yield return null;
        }

        // Ensure final state
        element.style.opacity = 1;
        if (type == AnimationType.FadeAndScale)
        {
            element.style.scale = new Scale(Vector3.one);
        }
        else if (type != AnimationType.FadeOnly)
        {
            element.style.translate = new Translate(0, 0, 0);
        }
    }

    /// <summary>
    /// Animate a VisualElement out with various effects
    /// </summary>
    public static IEnumerator AnimateOut(VisualElement element, AnimationType type, float duration, AnimationCurve curve = null)
    {
        if (element == null) yield break;
        if (curve == null) curve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        Vector3 startPosition = Vector3.zero;
        Vector3 endPosition = Vector3.zero;
        float startScale = 1f;
        float endScale = 1f;

        // Setup animation based on type
        switch (type)
        {
            case AnimationType.FadeOnly:
                break;

            case AnimationType.FadeAndScale:
                startScale = 1f;
                endScale = 0.95f;
                break;

            case AnimationType.FadeAndSlideUp:
                endPosition = new Vector3(0, -50, 0);
                break;

            case AnimationType.FadeAndSlideDown:
                endPosition = new Vector3(0, 50, 0);
                break;

            case AnimationType.FadeAndSlideLeft:
                endPosition = new Vector3(-50, 0, 0);
                break;

            case AnimationType.FadeAndSlideRight:
                endPosition = new Vector3(50, 0, 0);
                break;
        }

        float elapsedTime = 0f;
        float startOpacity = element.resolvedStyle.opacity;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float curveValue = curve.Evaluate(t);

            // Animate opacity (fade out)
            element.style.opacity = startOpacity * (1f - curveValue);

            // Animate scale
            if (type == AnimationType.FadeAndScale)
            {
                float scale = Mathf.Lerp(startScale, endScale, curveValue);
                element.style.scale = new Scale(new Vector3(scale, scale, 1));
            }

            // Animate position
            if (type != AnimationType.FadeOnly && type != AnimationType.FadeAndScale)
            {
                Vector3 position = Vector3.Lerp(startPosition, endPosition, curveValue);
                element.style.translate = new Translate(position.x, position.y, position.z);
            }

            yield return null;
        }

        // Final state
        element.style.opacity = 0;
        element.style.display = DisplayStyle.None;

        // Reset transforms
        if (type == AnimationType.FadeAndScale)
        {
            element.style.scale = new Scale(Vector3.one);
        }
        else if (type != AnimationType.FadeOnly)
        {
            element.style.translate = new Translate(0, 0, 0);
        }
    }

    /// <summary>
    /// Staggered animation for multiple elements
    /// </summary>
    public static IEnumerator AnimateInStaggered(VisualElement[] elements, AnimationType type, float duration, float staggerDelay, AnimationCurve curve = null)
    {
        if (elements == null || elements.Length == 0) yield break;

        for (int i = 0; i < elements.Length; i++)
        {
            if (elements[i] != null)
            {
                // Start animation for this element
                CoroutineRunner.Instance.StartCoroutine(AnimateIn(elements[i], type, duration, curve));
                
                // Wait for stagger delay before starting next element
                if (i < elements.Length - 1)
                {
                    yield return new WaitForSecondsRealtime(staggerDelay);
                }
            }
        }
    }

    /// <summary>
    /// Simple fade transition between two elements
    /// </summary>
    public static IEnumerator CrossFade(VisualElement fromElement, VisualElement toElement, float duration)
    {
        if (toElement == null) yield break;

        toElement.style.display = DisplayStyle.Flex;
        toElement.style.opacity = 0;

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);

            if (fromElement != null)
            {
                fromElement.style.opacity = 1f - t;
            }
            
            toElement.style.opacity = t;

            yield return null;
        }

        if (fromElement != null)
        {
            fromElement.style.opacity = 0;
            fromElement.style.display = DisplayStyle.None;
        }
        
        toElement.style.opacity = 1;
    }
}

/// <summary>
/// Helper MonoBehaviour for running coroutines from static contexts
/// </summary>
public class CoroutineRunner : MonoBehaviour
{
    private static CoroutineRunner _instance;
    
    public static CoroutineRunner Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("CoroutineRunner");
                _instance = go.AddComponent<CoroutineRunner>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }
}