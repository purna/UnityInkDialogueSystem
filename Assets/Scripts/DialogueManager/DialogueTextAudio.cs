using UnityEngine;

[RequireComponent(typeof(PitchableAudioSource))]
public class DialogueTextAudio : MonoBehaviour {
    [SerializeField] private AnimatedText _text;
    private PitchableAudioSource _audioSource;

    private void Awake() {
        _audioSource = GetComponent<PitchableAudioSource>();
        _text.TextUpdated += _audioSource.Play;
    }

    public void SetAudioInfo(PitchableAudioInfo audioInfo) {
        _audioSource.SetAudioInfo(audioInfo);
    }
}
