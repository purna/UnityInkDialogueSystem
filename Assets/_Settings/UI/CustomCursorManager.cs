using UnityEngine;
using System.Collections;

/// <summary>
/// Manages custom cursor appearance and click effects
/// </summary>
public class CustomCursorManager : MonoBehaviour
{
    [Header("Cursor Settings")]
    [SerializeField] private Texture2D _cursorTexture;
    [SerializeField] private Vector2 _cursorHotspot = new Vector2(0, 0);
    [SerializeField] private CursorMode _cursorMode = CursorMode.Auto;
    
    [Header("Click Effect Settings")]
    [SerializeField] private GameObject _clickEffectPrefab;
    [SerializeField] private bool _useParticleEffect = true;
    [SerializeField] private float _effectLifetime = 1f;
    
    [Header("Click Ripple Settings")]
    [SerializeField] private bool _enableRippleEffect = true;
    [SerializeField] private Color _rippleColor = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField] private float _rippleMaxSize = 2f;
    [SerializeField] private float _rippleDuration = 0.5f;
    [SerializeField] private AnimationCurve _rippleSizeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve _rippleAlphaCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    
    [Header("Camera Reference")]
    [SerializeField] private Camera _mainCamera;
    
    private static CustomCursorManager _instance;
    public static CustomCursorManager Instance => _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }
        
        SetCustomCursor();
    }

    private void Update()
    {
        // Detect mouse clicks
        if (Input.GetMouseButtonDown(0))
        {
            OnMouseClick(Input.mousePosition);
        }
    }

    /// <summary>
    /// Sets the custom cursor texture
    /// </summary>
    public void SetCustomCursor()
    {
        if (_cursorTexture != null)
        {
            Cursor.SetCursor(_cursorTexture, _cursorHotspot, _cursorMode);
        }
    }

    /// <summary>
    /// Sets a new cursor texture at runtime
    /// </summary>
    public void SetCustomCursor(Texture2D texture, Vector2 hotspot)
    {
        _cursorTexture = texture;
        _cursorHotspot = hotspot;
        Cursor.SetCursor(_cursorTexture, _cursorHotspot, _cursorMode);
    }

    /// <summary>
    /// Resets to the default system cursor
    /// </summary>
    public void ResetCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    /// <summary>
    /// Called when the mouse is clicked
    /// </summary>
    private void OnMouseClick(Vector3 screenPosition)
    {
        // Spawn prefab effect if available
        if (_clickEffectPrefab != null)
        {
            SpawnClickEffect(screenPosition);
        }
        
        // Create ripple effect if enabled
        if (_enableRippleEffect)
        {
            CreateRippleEffect(screenPosition);
        }
    }

    /// <summary>
    /// Spawns a prefab-based click effect
    /// </summary>
    private void SpawnClickEffect(Vector3 screenPosition)
    {
        Vector3 worldPosition;
        
        if (_mainCamera != null)
        {
            // Convert screen position to world position
            worldPosition = _mainCamera.ScreenToWorldPoint(new Vector3(
                screenPosition.x, 
                screenPosition.y, 
                _mainCamera.nearClipPlane + 1f
            ));
        }
        else
        {
            worldPosition = screenPosition;
        }

        GameObject effect = Instantiate(_clickEffectPrefab, worldPosition, Quaternion.identity);
        
        // Auto-destroy after lifetime
        Destroy(effect, _effectLifetime);
    }

    /// <summary>
    /// Creates a procedural ripple effect at the click position
    /// </summary>
    private void CreateRippleEffect(Vector3 screenPosition)
    {
        // Create a GameObject for the ripple
        GameObject rippleObj = new GameObject("ClickRipple");
        
        // Position it in world space
        if (_mainCamera != null)
        {
            Vector3 worldPosition = _mainCamera.ScreenToWorldPoint(new Vector3(
                screenPosition.x, 
                screenPosition.y, 
                _mainCamera.nearClipPlane + 1f
            ));
            rippleObj.transform.position = worldPosition;
        }
        
        // Add a sprite renderer for the ripple visual
        SpriteRenderer spriteRenderer = rippleObj.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = CreateCircleSprite();
        spriteRenderer.color = _rippleColor;
        spriteRenderer.sortingOrder = 1000; // Render on top
        
        // Start the ripple animation
        StartCoroutine(AnimateRipple(rippleObj, spriteRenderer));
    }

    /// <summary>
    /// Animates the ripple effect
    /// </summary>
    private IEnumerator AnimateRipple(GameObject rippleObj, SpriteRenderer spriteRenderer)
    {
        float elapsedTime = 0f;
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one * _rippleMaxSize;
        
        while (elapsedTime < _rippleDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / _rippleDuration;
            
            // Animate scale
            float sizeT = _rippleSizeCurve.Evaluate(t);
            rippleObj.transform.localScale = Vector3.Lerp(startScale, endScale, sizeT);
            
            // Animate alpha
            float alphaT = _rippleAlphaCurve.Evaluate(t);
            Color color = spriteRenderer.color;
            color.a = _rippleColor.a * alphaT;
            spriteRenderer.color = color;
            
            yield return null;
        }
        
        Destroy(rippleObj);
    }

    /// <summary>
    /// Creates a simple circle sprite for the ripple effect
    /// </summary>
    private Sprite CreateCircleSprite()
    {
        int resolution = 128;
        Texture2D texture = new Texture2D(resolution, resolution);
        Vector2 center = new Vector2(resolution / 2f, resolution / 2f);
        float radius = resolution / 2f;
        
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = 1f - Mathf.Clamp01((distance - radius + 10f) / 10f);
                texture.SetPixel(x, y, new Color(1, 1, 1, alpha));
            }
        }
        
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f));
    }

    /// <summary>
    /// Public method to trigger click effect manually
    /// </summary>
    public void TriggerClickEffect(Vector3 screenPosition)
    {
        OnMouseClick(screenPosition);
    }
}