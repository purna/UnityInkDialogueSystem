using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerBombCountdown : MonoBehaviour
{
    private float countdownTime;
    private float currentTime;
    private bool isActive = false;
    private bool isFinished = false;
    private System.Action onCountdownFinished;

    public bool IsCountdownActive => isActive;
    public bool IsCountdownFinished => isFinished;

    public void Initialize(float time, System.Action onFinished = null)
    {
        countdownTime = time;
        currentTime = time;
        onCountdownFinished = onFinished;
    }

    public void StartCountdown()
    {
        if (!isActive && !isFinished)
        {
            isActive = true;
            currentTime = countdownTime;
            Debug.Log($"Bomb countdown started! Time remaining: {countdownTime} seconds");
        }
    }

    private void Update()
    {
        if (isActive && !isFinished)
        {
            currentTime -= Time.deltaTime;
            
            if (currentTime <= 0f)
            {
                FinishCountdown();
            }
        }
    }

    private void FinishCountdown()
    {
        isActive = false;
        isFinished = true;
        currentTime = 0f;
        onCountdownFinished?.Invoke();
    }

    public float GetRemainingTime()
    {
        return Mathf.Max(0f, currentTime);
    }

    public float GetProgress()
    {
        return 1f - (currentTime / countdownTime);
    }

    // Method to reset the countdown if needed
    public void ResetCountdown()
    {
        isActive = false;
        isFinished = false;
        currentTime = countdownTime;
    }
}