using System;
using UnityEngine;

[Serializable]
public class DialogueCharacterEmotionSprite {
    [SerializeField] private DialogueCharacterEmotion _emotion;
    [SerializeField] private Sprite _sprite;

    public DialogueCharacterEmotion Emotion => _emotion;
    public Sprite Sprite => _sprite;
}
