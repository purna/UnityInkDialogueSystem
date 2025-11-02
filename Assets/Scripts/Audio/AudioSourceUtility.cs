using UnityEngine;

public static class AudioSourceUtility {
    public static void ChangeState(this AudioSource audioSource, bool newState) {
        if (newState)
            audioSource.UnPause();
        else
            audioSource.Pause();
    }

    public static void ForceChangeState(this AudioSource audioSource, bool newState) {
        if (newState)
            audioSource.Play();
        else
            audioSource.Stop();
    }

    public static void SetAudioInfo(this AudioSource audioSource, AudioInfo info) {
        if (info == null) {
            audioSource.clip = null;
            return;
        }

        audioSource.clip = info.Audio;
        audioSource.volume = info.Volume;
    }

    public static void PlayIfNot(this AudioSource audioSource) {
        if (audioSource.isPlaying)
            return;

        audioSource.Play();
    }
}
