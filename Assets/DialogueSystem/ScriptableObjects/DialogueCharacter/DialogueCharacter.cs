using UnityEngine;

[CreateAssetMenu(fileName = "DialogueCharacter", menuName = "Dialogue System/Dialogue Character")]

public class DialogueCharacter : ScriptableObject {
    [SerializeField] private Sprite _defaultSprite;
    [SerializeField] private string _defaultName;
    [SerializeField] private DialogueCharacterEmotionSprite[] _emotionSprites;
    [SerializeField] private PitchableAudioInfo _voiceInfo;

    public PitchableAudioInfo VoiceInfo => _voiceInfo;
    
    // Return the actual character name, not the asset name
    public string Name => !string.IsNullOrEmpty(_defaultName) ? _defaultName : name;
    
    public Sprite Icon => _defaultSprite;

    public Sprite GetEmotionSprite(DialogueCharacterEmotion emotion) {
        // If no emotion or emotion is None, return default sprite
        if (emotion == DialogueCharacterEmotion.None) {
            return _defaultSprite;
        }

        // Search through emotion sprites
        if (_emotionSprites != null && _emotionSprites.Length > 0) {
            foreach (var emotionSprite in _emotionSprites) {
                if (emotionSprite != null && emotionSprite.Emotion == emotion) {
                    // Make sure the sprite itself isn't null
                    if (emotionSprite.Sprite != null) {
                        return emotionSprite.Sprite;
                    } else {
                        Debug.LogWarning($"Character '{Name}': Emotion sprite for '{emotion}' is assigned but the sprite is null!");
                        break;
                    }
                }
            }
            
            // If we get here, emotion wasn't found
            Debug.LogWarning($"Character '{Name}': No sprite found for emotion '{emotion}', using default sprite");
        } else {
            Debug.LogWarning($"Character '{Name}': No emotion sprites array assigned, using default sprite");
        }
        
        // Return default sprite if emotion not found or if default sprite is null
        if (_defaultSprite == null) {
            Debug.LogError($"Character '{Name}': Default sprite is null! Please assign a default sprite.");
        }
        
        return _defaultSprite;
    }

    // Validation method to check if character is properly configured
    public bool IsValid() {
        if (_defaultSprite == null) {
            Debug.LogError($"Character '{name}': Missing default sprite!");
            return false;
        }
        
        if (string.IsNullOrEmpty(_defaultName)) {
            Debug.LogWarning($"Character '{name}': Missing default name!");
        }
        
        return true;
    }

#if UNITY_EDITOR
    // Editor validation
    private void OnValidate() {
        if (_defaultSprite == null) {
            Debug.LogWarning($"DialogueCharacter '{name}': Default sprite is not assigned!");
        }
        
        if (_emotionSprites != null) {
            for (int i = 0; i < _emotionSprites.Length; i++) {
                if (_emotionSprites[i] != null && _emotionSprites[i].Sprite == null) {
                    Debug.LogWarning($"DialogueCharacter '{name}': Emotion sprite at index {i} ({_emotionSprites[i].Emotion}) has no sprite assigned!");
                }
            }
        }
    }
#endif
}