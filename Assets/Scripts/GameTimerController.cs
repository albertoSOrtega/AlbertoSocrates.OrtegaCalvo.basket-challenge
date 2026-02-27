using UnityEngine;

public class GameTimerController : MonoBehaviour
{
    [Header("Timer Configuration")]
    [SerializeField] private float matchDuration = 60f; 

    // Events
    public event System.Action OnMatchStarted;
    public event System.Action OnMatchEnded;
    public event System.Action<float> OnTimerTick; // for updating UI with remaining time

    // State
    private float remainingTime; // proper timer, not using Coroutines to ensure accuracy and better control over the timer
    private bool isClockRunning = false;

    public int RemainingTime { get; private set; }

    public void StartMatch()
    {
        remainingTime = matchDuration;
        isClockRunning = true;
        OnMatchStarted?.Invoke();
    }

    public void StopMatch()
    {
        isClockRunning = false;
        remainingTime = 0f;
        OnMatchEnded?.Invoke();
    }

    private void Update()
    {
        if (!isClockRunning) return;

        remainingTime -= Time.deltaTime;
        OnTimerTick?.Invoke(remainingTime);

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            isClockRunning = false;
            OnMatchEnded?.Invoke();
        }
    }
}