using System;
using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Centralized typewriter effect for dialogue text.
/// Handles character-by-character display with rich text support.
/// Supports both ScreenSpace and WorldSpace canvases.
/// </summary>
public class TypewriterEffect : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float typingSpeed = 0.04f;

    [Header("Unified DialogueUI Reference")]
    [SerializeField] private DialogueUI dialogueUI;
    
    // Active text component (set dynamically based on mode)
    private TextMeshProUGUI activeTextComponent;
    
    private Coroutine typewriterCoroutine;
    private bool isTyping = false;
    private bool skipRequested = false;
    
    // Callbacks
    private Action<int, char> onCharacterTyped; // Called for each character (for audio)
    private Action onTypingComplete; // Called when typing finishes
    
    public bool IsTyping => isTyping;
    public float TypingSpeed => typingSpeed;

    private void Awake()
    {
        // Don't set textComponent in Awake - it will be set when DialogueUI switches modes
        if (dialogueUI == null)
        {
            dialogueUI = GetComponent<DialogueUI>();
            if (dialogueUI == null)
            {
                Debug.LogError("[TypewriterEffect] No DialogueUI component found!");
            }
        }
    }
    
    /// <summary>
    /// Update the active text component when mode changes
    /// Called by DialogueUI when switching between ScreenSpace/WorldSpace
    /// </summary>
    public void UpdateTextComponent()
    {
        if (dialogueUI != null)
        {
            activeTextComponent = dialogueUI.GetDialogueTextComponent();
            
            if (activeTextComponent == null)
            {
                Debug.LogWarning("[TypewriterEffect] Failed to get text component from DialogueUI");
            }
            else
            {
                Debug.Log($"[TypewriterEffect] Updated text component to: {activeTextComponent.gameObject.name}");
            }
        }
    }

    /// <summary>
    /// Initialize the typewriter with a specific text component
    /// </summary>
    public void Initialize(TextMeshProUGUI target)
    {
        activeTextComponent = target;
    }

    /// <summary>
    /// Set the typing speed
    /// </summary>
    public void SetTypingSpeed(float speed)
    {
        typingSpeed = Mathf.Max(0.001f, speed);
    }

    /// <summary>
    /// Start typing text with optional callbacks
    /// </summary>
    public void TypeText(string text, Action<int, char> onCharTyped = null, Action onComplete = null)
    {
        // Update text component reference before typing
        UpdateTextComponent();
        
        if (activeTextComponent == null)
        {
            Debug.LogError("[TypewriterEffect] TextMeshProUGUI component is null!");
            return;
        }

        // Stop any existing typewriter
        Stop();

        // Set callbacks
        onCharacterTyped = onCharTyped;
        onTypingComplete = onComplete;

        // Start typing
        typewriterCoroutine = StartCoroutine(TypeTextCoroutine(text));
    }

    /// <summary>
    /// Skip to the end of the current typing animation
    /// </summary>
    public void Skip()
    {
        skipRequested = true;
    }

    /// <summary>
    /// Stop the typewriter effect completely
    /// </summary>
    public void Stop()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }
        
        isTyping = false;
        skipRequested = false;
    }

    /// <summary>
    /// Check if currently typing or finished
    /// </summary>
    public bool IsComplete()
    {
        return !isTyping;
    }

    /// <summary>
    /// Main typewriter coroutine
    /// </summary>
    private IEnumerator TypeTextCoroutine(string text)
    {
        if (activeTextComponent == null || string.IsNullOrEmpty(text))
        {
            onTypingComplete?.Invoke();
            yield break;
        }

        isTyping = true;
        skipRequested = false;

        // Set the full text but hide all characters
        activeTextComponent.text = text;
        activeTextComponent.maxVisibleCharacters = 0;

        int visibleCharCount = 0;
        int totalCharacters = text.Length;

        // Type each character
        while (visibleCharCount < totalCharacters)
        {
            // Check if skip was requested
            if (skipRequested)
            {
                // Instantly show all text
                activeTextComponent.maxVisibleCharacters = totalCharacters;
                break;
            }

            char currentChar = text[visibleCharCount];

            // Handle rich text tags (e.g., <color=red>, <b>, etc.)
            if (currentChar == '<')
            {
                int tagEndIndex = text.IndexOf('>', visibleCharCount);
                
                if (tagEndIndex != -1)
                {
                    // Skip to the end of the tag
                    int tagLength = tagEndIndex - visibleCharCount + 1;
                    
                    // Advance past the entire tag
                    visibleCharCount = tagEndIndex + 1;
                    
                    // Update visible characters to include the tag
                    activeTextComponent.maxVisibleCharacters = visibleCharCount;
                    
                    // Don't wait for tags, continue immediately
                    continue;
                }
            }

            // Show the next character
            activeTextComponent.maxVisibleCharacters = visibleCharCount + 1;

            // Trigger character typed callback (for audio)
            onCharacterTyped?.Invoke(visibleCharCount, currentChar);

            // Move to next character
            visibleCharCount++;

            // Wait for typing speed
            yield return new WaitForSeconds(typingSpeed);
        }

        // Ensure all text is visible
        activeTextComponent.maxVisibleCharacters = totalCharacters;

        // Mark as complete
        isTyping = false;
        skipRequested = false;
        typewriterCoroutine = null;

        // Trigger completion callback
        onTypingComplete?.Invoke();
    }

    /// <summary>
    /// Instantly display text without typing effect
    /// </summary>
    public void DisplayInstantly(string text)
    {
        Stop();
        
        UpdateTextComponent();
        
        if (activeTextComponent != null)
        {
            activeTextComponent.text = text;
            activeTextComponent.maxVisibleCharacters = text.Length;
        }
        
        onTypingComplete?.Invoke();
    }

    /// <summary>
    /// Clear the text
    /// </summary>
    public void Clear()
    {
        Stop();
        
        UpdateTextComponent();
        
        if (activeTextComponent != null)
        {
            activeTextComponent.text = "";
            activeTextComponent.maxVisibleCharacters = 0;
        }
    }
}