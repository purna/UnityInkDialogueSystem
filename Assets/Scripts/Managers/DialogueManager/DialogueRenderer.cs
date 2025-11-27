using UnityEngine;
using UnityEngine.UI;

public class DialogueRenderer : MonoBehaviour {
    [SerializeField] private Image _characterIcon;
    [SerializeField] private LocalizedText _characterName;
    [SerializeField] private AnimatedText _text;
    [SerializeField] private DialogueTextAudio _audio;

    public bool IsAnimated => _text.IsAnimated;

    public void RenderDialogue(Dialogue dialogue) {
        _characterIcon.sprite = dialogue.Character.GetEmotionSprite(dialogue.Emotion);
        _characterName.SetText(dialogue.Character.Name);

        _audio.SetAudioInfo(dialogue.Character.VoiceInfo);
        _text.SetText(dialogue.Text);
    }

    public void StopTextAnimation() {
        _text.StopAnimation();
    }
}
