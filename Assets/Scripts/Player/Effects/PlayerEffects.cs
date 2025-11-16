using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// * Summary:
/// The PlayerEffects script manages visual and audio effects for a player character in a Unity game.
/// It primarily controls shader-based effects, such as invisibility and collection effects, and manages audio cues for interactions like collecting coins or triggering explosions.
/// * Key Features:
/// Shader Property Control:
/// _Negative and _NegativeAmount: Likely used for an invisibility effect or color inversion.
/// _HitEffect, _HitEffectBlend, and _HitEffectColor: Handles visual feedback for events like item collection.
/// Collection Effect (PlayCollectionEffect)
/// Gradually blends a shader effect when an item is collected.
/// Stops any ongoing effect before starting a new one.
/// Can change color dynamically based on the collected item.
/// Calls an audio clip (disabled in this script but can be uncommented).
/// Coroutine-Based Effect Handling (CollectionEffect)
/// Uses smooth interpolation (Mathf.Lerp) to animate shader properties over time.
/// Ensures smooth transition in and out of collection effects.
/// Uses _isCollectEffecting to prevent overlapping effects.
/// </summary>
/*
 * Example Usage in Another Script (e.g., Coin Collection)

public class Coin : MonoBehaviour
{
    public PlayerEffects playerEffects;
    public Color coinEffectColor = Color.yellow;
    public AudioClip coinSound;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerEffects.PlayCollectionEffect(0.5f, coinEffectColor, coinSound);
            Destroy(gameObject); // Remove coin after collection
        }
    }
}
*/


public class PlayerEffects : MonoBehaviour
{
    [SerializeField] private AudioClip _invisibilityClip;
    [SerializeField] private AudioClip _coinCollectClip;
    [SerializeField] private AudioClip _explosionClip;

    private SpriteRenderer[] _spriteRenderers;
    private Material[] _materials;

    private Coroutine _invisibilityCoroutine;
    private Coroutine _effectCollectCoroutine;

    public bool IsChanging { get; private set; }

    private int _negativeBool = Shader.PropertyToID("_Negative");
    private int _negativeAmount = Shader.PropertyToID("_NegativeAmount");
    private int _hitEffect = Shader.PropertyToID("_HitEffect");
    private int _hitEffectBlend = Shader.PropertyToID("_HitEffectBlend");
    private int _hitEffectColor = Shader.PropertyToID("_HitEffectColor");
    private bool _isCollectEffecting;

    private void Awake()
    {
        _spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        _materials = new Material[_spriteRenderers.Length];
        for (int i = 0; i < _materials.Length; i++)
        {
            _materials[i] = _spriteRenderers[i].material;
        }

        // Check and set _Negative
        if (_materials[0].HasProperty(_negativeBool) && _materials[0].GetFloat(_negativeBool) == 0)
        {
            for (int i = 0; i < _materials.Length; i++)
            {
                _materials[i].SetFloat(_negativeBool, 1f);
            }
        }
        

       // Check and set _HitEffect
        if (_materials[0].HasProperty(_hitEffect) && _materials[0].GetFloat(_hitEffect) == 0)
        {
            for (int i = 0; i < _materials.Length; i++)
            {
                _materials[i].SetFloat(_hitEffect, 1f);
            }
        }
    }

    #region Invisibility



    #endregion

    #region Collection Effects

    public void PlayCollectionEffect(float time, Color color, AudioClip clip)
    {
        if (_isCollectEffecting)
        {
            StopCoroutine(_effectCollectCoroutine);
            _isCollectEffecting = false;

            for (int i = 0; i < _materials.Length; i++)
            {
                _materials[i].SetColor(_hitEffectColor, color);
            }

            _effectCollectCoroutine = StartCoroutine(CollectionEffect(_materials[0].GetFloat(_hitEffectBlend), 1f, time));

            //AudioManager.PlayClip(clip, 0.65f);
        }
    }

    private IEnumerator CollectionEffect(float startValue, float endValue, float time)
    {
        _isCollectEffecting = true;

        float elapsedTime = 0f;
        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;

            float lerpedAmount = Mathf.Lerp(startValue, endValue, (elapsedTime / time));

            for (int i = 0; i < _materials.Length; i++)
            {
                _materials[i].SetFloat(_hitEffectBlend, lerpedAmount);
            }

            yield return null;
        }

        elapsedTime = 0f;
        while(elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;

            float lerpedAmount = Mathf.Lerp(endValue, 0f, (elapsedTime / time));

            for (int i = 0; i < _materials.Length; i++)
            {
                _materials[i].SetFloat(_hitEffectBlend, lerpedAmount);
            }

            yield return null;
        }

        _isCollectEffecting = false;
    }

    #endregion
}
