using System;
using UnityEngine;

public class AnimatedText : LocalizedText {
    [SerializeField, Min(0)] private float _pauseTime;

    private string _targetText;
    private int _lastCharIndex;
    private bool _isAnimated;
    private float _nowTime;

    public bool IsAnimated => _isAnimated;

    public event Action TextUpdated;

    public override void UpdateText() {
        base.UpdateText();
        _targetText = _text.text;
        _text.text = string.Empty;
    }

    public override void SetText(string text) {
        base.SetText(text);
        if (_isAnimated)
            ForceStopAnimation();
        StartAnimation();
    }

    private void StartAnimation() {
        _isAnimated = true;
        _lastCharIndex = 0;
        _nowTime = 0;
    }

    private void Update() {
        _nowTime += Time.deltaTime;

        if (_nowTime < _pauseTime)
            return;

        if (_lastCharIndex >= _targetText.Length) {
            StopAnimation();
            return;
        }

        _lastCharIndex++;
        _text.text = _targetText[.._lastCharIndex];
        _nowTime = 0;
        TextUpdated?.Invoke();
    }

    public void StopAnimation() {
        if (string.IsNullOrEmpty(_targetText) || !_isAnimated)
            return;

        _isAnimated = false;
        _text.text = _targetText;
        TextUpdated?.Invoke();
    }

    private void ForceStopAnimation() {
        _isAnimated = false;
        TextUpdated?.Invoke();
    }
}
