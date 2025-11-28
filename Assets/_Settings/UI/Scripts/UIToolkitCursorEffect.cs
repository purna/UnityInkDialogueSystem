using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

/// <summary>
/// Manages custom cursor and click effects specifically for UI Toolkit
/// Attach this to the same GameObject as your UIDocument
/// </summary>
public class UIToolkitCursorEffect : MonoBehaviour
{
    [Header("Cursor Settings")]
    [SerializeField] private Texture2D _cursorTexture;
    [SerializeField] private Vector2 _cursorHotspot = new Vector2(0, 0);
    
    [Header("Click Effect Settings")]
    [SerializeField] private bool _enableClickEffect = true;
    [SerializeField] private Color _clickColor = new Color(1f, 1f, 1f, 0.8f);
    [SerializeField] private float _clickEffectSize = 50f;
    [SerializeField] private float _clickEffectDuration = 0.3f;
    [SerializeField] private AnimationCurve _sizeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve _alphaCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    
    [Header("Click Effect Sprite (Optional)")]
    [SerializeField] private Texture2D _clickEffectTexture;
    
    private UIDocument _uiDocument;
    private VisualElement _root;
    private VisualElement _clickEffectContainer;

    private void Awake()
    {
        _uiDocument = GetComponent<UIDocument>();
    }

    private void OnEnable()
    {
        if (_uiDocument != null)
        {
            _root = _uiDocument.rootVisualElement;
            SetupClickEffectContainer();
            RegisterClickEvents();
        }
        
        SetCustomCursor();
    }

    private void SetupClickEffectContainer()
    {
        // Create a container for click effects that covers the entire screen
        _clickEffectContainer = new VisualElement();
        _clickEffectContainer.name = "ClickEffectContainer";
        _clickEffectContainer.style.position = Position.Absolute;
        _clickEffectContainer.style.left = 0;
        _clickEffectContainer.style.top = 0;
        _clickEffectContainer.style.right = 0;
        _clickEffectContainer.style.bottom = 0;
        _clickEffectContainer.style.width = Length.Percent(100);
        _clickEffectContainer.style.height = Length.Percent(100);
        _clickEffectContainer.pickingMode = PickingMode.Ignore; // Don't block clicks
        
        _root.Add(_clickEffectContainer);
    }

    private void RegisterClickEvents()
    {
        if (_root == null) return;
        
        // Register click event on the root element
        _root.RegisterCallback<PointerDownEvent>(OnPointerDown);
    }

    private void OnPointerDown(PointerDownEvent evt)
    {
        if (_enableClickEffect)
        {
            CreateClickEffect(evt.position);
        }
    }

    private void SetCustomCursor()
    {
        if (_cursorTexture != null)
        {
            UnityEngine.Cursor.SetCursor(_cursorTexture, _cursorHotspot, CursorMode.Auto);
        }
    }

    private void CreateClickEffect(Vector2 position)
    {
        // Create the click effect visual element
        var clickEffect = new VisualElement();
        clickEffect.name = "ClickEffect";
        
        // Style the click effect
        clickEffect.style.position = Position.Absolute;
        clickEffect.style.left = position.x - (_clickEffectSize / 2);
        clickEffect.style.top = position.y - (_clickEffectSize / 2);
        clickEffect.style.width = _clickEffectSize;
        clickEffect.style.height = _clickEffectSize;
        clickEffect.pickingMode = PickingMode.Ignore;
        
        // Set background (use custom texture or create a circle)
        if (_clickEffectTexture != null)
        {
            clickEffect.style.backgroundImage = new StyleBackground(_clickEffectTexture);
        }
        else
        {
            clickEffect.style.borderTopLeftRadius = _clickEffectSize / 2;
            clickEffect.style.borderTopRightRadius = _clickEffectSize / 2;
            clickEffect.style.borderBottomLeftRadius = _clickEffectSize / 2;
            clickEffect.style.borderBottomRightRadius = _clickEffectSize / 2;
            clickEffect.style.borderTopWidth = 3;
            clickEffect.style.borderRightWidth = 3;
            clickEffect.style.borderBottomWidth = 3;
            clickEffect.style.borderLeftWidth = 3;
            clickEffect.style.borderTopColor = _clickColor;
            clickEffect.style.borderRightColor = _clickColor;
            clickEffect.style.borderBottomColor = _clickColor;
            clickEffect.style.borderLeftColor = _clickColor;
        }
        
        clickEffect.style.unityBackgroundImageTintColor = _clickColor;
        
        // Add to container
        _clickEffectContainer.Add(clickEffect);
        
        // Animate the effect
        StartCoroutine(AnimateClickEffect(clickEffect, position));
    }

    private IEnumerator AnimateClickEffect(VisualElement effect, Vector2 startPosition)
    {
        float elapsedTime = 0f;
        float startSize = _clickEffectSize;
        float endSize = _clickEffectSize * 2f;
        
        while (elapsedTime < _clickEffectDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / _clickEffectDuration;
            
            // Animate size
            float sizeT = _sizeCurve.Evaluate(t);
            float currentSize = Mathf.Lerp(startSize, endSize, sizeT);
            effect.style.width = currentSize;
            effect.style.height = currentSize;
            
            // Keep centered
            effect.style.left = startPosition.x - (currentSize / 2);
            effect.style.top = startPosition.y - (currentSize / 2);
            
            // Update border radius if using circle
            if (_clickEffectTexture == null)
            {
                float radius = currentSize / 2;
                effect.style.borderTopLeftRadius = radius;
                effect.style.borderTopRightRadius = radius;
                effect.style.borderBottomLeftRadius = radius;
                effect.style.borderBottomRightRadius = radius;
            }
            
            // Animate alpha
            float alphaT = _alphaCurve.Evaluate(t);
            Color color = _clickColor;
            color.a = _clickColor.a * alphaT;
            
            effect.style.unityBackgroundImageTintColor = color;
            if (_clickEffectTexture == null)
            {
                effect.style.borderTopColor = color;
                effect.style.borderRightColor = color;
                effect.style.borderBottomColor = color;
                effect.style.borderLeftColor = color;
            }
            
            yield return null;
        }
        
        // Remove the effect
        effect.RemoveFromHierarchy();
    }

    private void OnDisable()
    {
        if (_root != null)
        {
            _root.UnregisterCallback<PointerDownEvent>(OnPointerDown);
        }
        
        // Reset cursor
        UnityEngine.Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    /// <summary>
    /// Change the cursor texture at runtime
    /// </summary>
    public void SetCursor(Texture2D texture, Vector2 hotspot)
    {
        _cursorTexture = texture;
        _cursorHotspot = hotspot;
        UnityEngine.Cursor.SetCursor(_cursorTexture, _cursorHotspot, CursorMode.Auto);
    }

    /// <summary>
    /// Reset to default system cursor
    /// </summary>
    public void ResetCursor()
    {
        UnityEngine.Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}