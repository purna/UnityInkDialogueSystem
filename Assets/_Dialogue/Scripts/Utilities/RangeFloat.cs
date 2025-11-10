using System;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class RangeFloat {
    [SerializeField] private float _min;
    [SerializeField] private float _max;
    [SerializeField] private bool _isGreaterZero = true;

    public float RandomValue {
        get {
            CheckValues();
            return Random.Range(_min, _max);
        }
    }

    public float InverseLerp(float value) {
        return Mathf.InverseLerp(_min, _max, value);
    }

    public float Lerp(float value) {
        return Mathf.Lerp(_min, _max, value);
    }

    private void CheckValues() {
        if (_isGreaterZero) {
            _min = Mathf.Max(0, _min);
            _max = Mathf.Max(0, _max);
        }

        if (_min > _max)
            (_min, _max) = (_max, _min);
    }
}
