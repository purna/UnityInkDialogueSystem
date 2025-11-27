using UnityEngine;
using System.Collections.Generic;
using System;

#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

[CreateAssetMenu(fileName = "DialogueCharacter", menuName = "Dialogue System/Dialogue Character")]
public class DialogueCharacter : ScriptableObject 
{
    [Header("Basic Info")]
    [SerializeField] private Sprite _defaultSprite;
    [SerializeField] private string _defaultName;
    
    [Header("Emotion Sprites (Graph-Based System)")]
    [SerializeField] private DialogueCharacterEmotionSprite[] _emotionSprites;
    
    [Header("Animator Controller (Ink System)")]
    [Tooltip("Animator controller containing portrait animation states")]
    [SerializeField] private RuntimeAnimatorController _portraitAnimatorController;
    
    [Tooltip("Map emotions to animation states")]
    [SerializeField] private List<EmotionAnimationMapping> _emotionAnimationMappings = new List<EmotionAnimationMapping>();
    
    [Header("Available Animation States")]
    [Tooltip("All available animation states from the animator controller")]
    [SerializeField] private List<string> _availableAnimationStates = new List<string>();
    
    [Header("Layout")]
    [Tooltip("Default layout position for this character")]
    [SerializeField] private CharacterLayout _defaultLayout = CharacterLayout.Right;
    
    [Header("Voice")]
    [SerializeField] private PitchableAudioInfo _voiceInfo;

    // Properties
    public PitchableAudioInfo VoiceInfo => _voiceInfo;
    public string Name => !string.IsNullOrEmpty(_defaultName) ? _defaultName : name;
    public Sprite Icon => _defaultSprite;
    public RuntimeAnimatorController PortraitAnimatorController => _portraitAnimatorController;
    public List<string> AvailableAnimationStates => _availableAnimationStates;
    public CharacterLayout DefaultLayout => _defaultLayout;
    public string DefaultLayoutString => _defaultLayout.ToString().ToLower();

    /// <summary>
    /// Get emotion sprite for graph-based dialogue system
    /// </summary>
    public Sprite GetEmotionSprite(DialogueCharacterEmotion emotion) 
    {
        // If no emotion or emotion is None, return default sprite
        if (emotion == DialogueCharacterEmotion.None) 
        {
            return _defaultSprite;
        }

        // Search through emotion sprites
        if (_emotionSprites != null && _emotionSprites.Length > 0) 
        {
            foreach (var emotionSprite in _emotionSprites) 
            {
                if (emotionSprite != null && emotionSprite.Emotion == emotion) 
                {
                    if (emotionSprite.Sprite != null) 
                    {
                        return emotionSprite.Sprite;
                    } 
                    else 
                    {
                        Debug.LogWarning($"Character '{Name}': Emotion sprite for '{emotion}' is assigned but the sprite is null!");
                        break;
                    }
                }
            }
            
            Debug.LogWarning($"Character '{Name}': No sprite found for emotion '{emotion}', using default sprite");
        } 
        else 
        {
            Debug.LogWarning($"Character '{Name}': No emotion sprites array assigned, using default sprite");
        }
        
        if (_defaultSprite == null) 
        {
            Debug.LogError($"Character '{Name}': Default sprite is null! Please assign a default sprite.");
        }
        
        return _defaultSprite;
    }

    /// <summary>
    /// Get animation state name for a given emotion (for Ink system)
    /// </summary>
    public string GetAnimationStateForEmotion(DialogueCharacterEmotion emotion)
    {
        if (_emotionAnimationMappings == null || _emotionAnimationMappings.Count == 0)
        {
            Debug.LogWarning($"Character '{Name}': No emotion-animation mappings configured!");
            return GetDefaultAnimationState();
        }

        // Find the mapping for this emotion
        foreach (var mapping in _emotionAnimationMappings)
        {
            if (mapping.emotion == emotion && !string.IsNullOrEmpty(mapping.animationStateName))
            {
                return mapping.animationStateName;
            }
        }

        // Fallback to default
        Debug.LogWarning($"Character '{Name}': No animation state found for emotion '{emotion}', using default");
        return GetDefaultAnimationState();
    }

    /// <summary>
    /// Get emotion for a given animation state name (reverse lookup)
    /// </summary>
    public DialogueCharacterEmotion GetEmotionForAnimationState(string stateName)
    {
        if (_emotionAnimationMappings == null || _emotionAnimationMappings.Count == 0)
            return DialogueCharacterEmotion.None;

        foreach (var mapping in _emotionAnimationMappings)
        {
            if (mapping.animationStateName == stateName)
            {
                return mapping.emotion;
            }
        }

        return DialogueCharacterEmotion.None;
    }

    /// <summary>
    /// Check if an animation state exists in the animator controller
    /// </summary>
    public bool HasAnimationState(string stateName)
    {
        if (_availableAnimationStates == null || _availableAnimationStates.Count == 0)
            return false;
            
        return _availableAnimationStates.Contains(stateName);
    }

    /// <summary>
    /// Get the default animation state (usually the first one or "default")
    /// </summary>
    public string GetDefaultAnimationState()
    {
        // Try to find a state named "default" or "idle"
        if (_availableAnimationStates != null)
        {
            if (_availableAnimationStates.Contains("default"))
                return "default";
            if (_availableAnimationStates.Contains("idle"))
                return "idle";
            if (_availableAnimationStates.Contains("neutral"))
                return "neutral";
            if (_availableAnimationStates.Count > 0)
                return _availableAnimationStates[0];
        }
            
        return "default";
    }

    /// <summary>
    /// Validation method to check if character is properly configured
    /// </summary>
    public bool IsValid() 
    {
        if (_defaultSprite == null) 
        {
            Debug.LogError($"Character '{name}': Missing default sprite!");
            return false;
        }
        
        if (string.IsNullOrEmpty(_defaultName)) 
        {
            Debug.LogWarning($"Character '{name}': Missing default name!");
        }
        
        return true;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Refresh the list of available animation states from the animator controller
    /// </summary>
    [ContextMenu("Refresh Animation States")]
    public void RefreshAnimationStates()
    {
        _availableAnimationStates.Clear();
        
        if (_portraitAnimatorController == null)
        {
            Debug.LogWarning($"Character '{name}': No animator controller assigned!");
            return;
        }

        AnimatorController controller = _portraitAnimatorController as AnimatorController;
        if (controller == null)
        {
            Debug.LogWarning($"Character '{name}': Animator controller is not an AnimatorController type!");
            return;
        }

        // Iterate through all layers
        foreach (var layer in controller.layers)
        {
            // Iterate through all states in the layer's state machine
            foreach (var state in layer.stateMachine.states)
            {
                string stateName = state.state.name;
                
                // Avoid duplicates
                if (!_availableAnimationStates.Contains(stateName))
                {
                    _availableAnimationStates.Add(stateName);
                }
            }
        }

        Debug.Log($"Character '{name}': Found {_availableAnimationStates.Count} animation states");
        UnityEditor.EditorUtility.SetDirty(this);
    }

    /// <summary>
    /// Auto-create emotion mappings based on available animation states
    /// </summary>
    [ContextMenu("Auto-Map Emotions to Animation States")]
    public void AutoMapEmotionsToAnimations()
    {
        if (_availableAnimationStates == null || _availableAnimationStates.Count == 0)
        {
            Debug.LogWarning($"Character '{name}': No animation states available. Run 'Refresh Animation States' first!");
            return;
        }

        // Get all emotion values
        var allEmotions = (DialogueCharacterEmotion[])Enum.GetValues(typeof(DialogueCharacterEmotion));

        foreach (var emotion in allEmotions)
        {
            // Skip "None"
            if (emotion == DialogueCharacterEmotion.None)
                continue;

            // Check if mapping already exists
            bool mappingExists = false;
            foreach (var mapping in _emotionAnimationMappings)
            {
                if (mapping.emotion == emotion)
                {
                    mappingExists = true;
                    break;
                }
            }

            if (mappingExists)
                continue;

            // Try to find a matching animation state (case-insensitive)
            string emotionName = emotion.ToString().ToLower();
            string matchingState = null;

            foreach (var stateName in _availableAnimationStates)
            {
                if (stateName.ToLower() == emotionName)
                {
                    matchingState = stateName;
                    break;
                }
            }

            // If found, create the mapping
            if (!string.IsNullOrEmpty(matchingState))
            {
                _emotionAnimationMappings.Add(new EmotionAnimationMapping
                {
                    emotion = emotion,
                    animationStateName = matchingState
                });
                Debug.Log($"Character '{name}': Auto-mapped {emotion} -> {matchingState}");
            }
        }

        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"Character '{name}': Auto-mapping complete. Created {_emotionAnimationMappings.Count} mappings.");
    }

    /// <summary>
    /// Sync emotion sprites with animation mappings
    /// </summary>
    [ContextMenu("Sync Emotion Sprites with Mappings")]
    public void SyncEmotionSpritesWithMappings()
    {
        if (_emotionAnimationMappings == null || _emotionAnimationMappings.Count == 0)
        {
            Debug.LogWarning($"Character '{name}': No emotion-animation mappings to sync!");
            return;
        }

        // Create a list to hold new emotion sprites
        List<DialogueCharacterEmotionSprite> newEmotionSprites = new List<DialogueCharacterEmotionSprite>();

        foreach (var mapping in _emotionAnimationMappings)
        {
            // Check if emotion sprite already exists
            DialogueCharacterEmotionSprite existingSprite = null;
            
            if (_emotionSprites != null)
            {
                foreach (var sprite in _emotionSprites)
                {
                    if (sprite != null && sprite.Emotion == mapping.emotion)
                    {
                        existingSprite = sprite;
                        break;
                    }
                }
            }

            // Keep existing sprite or create placeholder
            if (existingSprite != null)
            {
                newEmotionSprites.Add(existingSprite);
            }
            else
            {
                // Create new emotion sprite entry using constructor
                newEmotionSprites.Add(new DialogueCharacterEmotionSprite(mapping.emotion, _defaultSprite));
            }
        }

        _emotionSprites = newEmotionSprites.ToArray();
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"Character '{name}': Synced {_emotionSprites.Length} emotion sprites with animation mappings.");
    }

    /// <summary>
    /// Editor validation
    /// </summary>
    private void OnValidate() 
    {
        if (_defaultSprite == null) 
        {
            Debug.LogWarning($"DialogueCharacter '{name}': Default sprite is not assigned!");
        }
        
        if (_emotionSprites != null) 
        {
            for (int i = 0; i < _emotionSprites.Length; i++) 
            {
                if (_emotionSprites[i] != null && _emotionSprites[i].Sprite == null) 
                {
                    Debug.LogWarning($"DialogueCharacter '{name}': Emotion sprite at index {i} ({_emotionSprites[i].Emotion}) has no sprite assigned!");
                }
            }
        }

        // Auto-refresh animation states when animator controller changes
        if (_portraitAnimatorController != null && (_availableAnimationStates == null || _availableAnimationStates.Count == 0))
        {
            RefreshAnimationStates();
        }
    }
#endif
}

/// <summary>
/// Maps an emotion to an animation state name
/// </summary>
[System.Serializable]
public class EmotionAnimationMapping
{
    [Tooltip("The emotion this mapping represents")]
    public DialogueCharacterEmotion emotion;
    
    [Tooltip("The animation state name in the animator controller")]
    public string animationStateName;
}

/// <summary>
/// Character layout position options
/// </summary>
public enum CharacterLayout
{
    Left,
    Right
}