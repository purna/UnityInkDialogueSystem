using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PitchableAudioSource : MonoBehaviour {
    [SerializeField] private RangeFloat _pitch;
    protected AudioSource _audioSource;

    private void Awake() {
        _audioSource = GetComponent<AudioSource>();
    }

    public virtual void Play() {
        _audioSource.pitch = _pitch.RandomValue;
        _audioSource.Play();
    }

    public virtual void Play(float pitch) {
        _audioSource.pitch = pitch;
        _audioSource.Play();
    }

    public void ChangeState(bool newState) {
        _audioSource.pitch = _pitch.RandomValue;
        _audioSource.ChangeState(newState);
    }

    public void ForceChangeState(bool newState) {
        _audioSource.pitch = _pitch.RandomValue;
        _audioSource.ForceChangeState(newState);
    }

    public void SetAudioInfo(PitchableAudioInfo audioInfo) {
        SetPitch(audioInfo.Pitch);
        SetAudioInfo(audioInfo as AudioInfo);
    }

    public void SetPitch(RangeFloat rangeFloat) {
        _pitch = rangeFloat;
    }

    public void SetAudioInfo(AudioInfo audioInfo) {
        _audioSource.SetAudioInfo(audioInfo);
    }
}
