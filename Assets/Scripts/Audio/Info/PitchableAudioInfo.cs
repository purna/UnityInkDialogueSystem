using System;
using UnityEngine;

[Serializable]
public class PitchableAudioInfo : AudioInfo {
    [SerializeField] private RangeFloat _pitch;

    public RangeFloat Pitch => _pitch;
}
