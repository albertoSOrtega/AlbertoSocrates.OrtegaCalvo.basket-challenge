using UnityEngine;

public class GameTimerController : MonoBehaviour
{
    [Header("Timer Configuration")]
    [SerializeField] private float matchDuration = 60f;

    [Header("Backboard Bonus Configuration")]
    [SerializeField] private float backboardBonusTimeInterval = 10f;

    // Events
    public event System.Action OnMatchStarted;
    public event System.Action OnMatchEnded;
    public event System.Action<float> OnTimerTick; // for updating UI with remaining time
    public event System.Action OnBonusIntervalStarted;

    // State
    private float remainingTime; // proper timer, not using Coroutines to ensure accuracy and better control over the timer
    private bool isClockRunning = false;
    [SerializeField]
    private float bonusTimer;
    private bool isBonusTimerPaused = false;

    public int RemainingTime { get; private set; }

    public void StartMatch()
    {
        remainingTime = matchDuration;
        bonusTimer = backboardBonusTimeInterval;
        isBonusTimerPaused = false;
        isClockRunning = true;
        OnMatchStarted?.Invoke();
    }

    public void StopMatch()
    {
        isClockRunning = false;
        remainingTime = 0f;
        OnMatchEnded?.Invoke();
    }

    public void PauseBonusTimer()
    {
        isBonusTimerPaused = true;
        Debug.Log("[GameTimerController] Bonus timer paused.");
    }

    public void ResumeBonusTimer()
    {
        bonusTimer = backboardBonusTimeInterval; 
        isBonusTimerPaused = false;
        Debug.Log("[GameTimerController] Bonus timer resumed.");
    }

    private void UpdateBonusTimer()
    {
        bonusTimer -= Time.deltaTime;
        if (bonusTimer <= 0f)
        {
            bonusTimer = backboardBonusTimeInterval;
            Debug.Log("[GameTimerController] OnBonusIntervalTick fired");
            isBonusTimerPaused = true; // pause until bonus is collected or expires
            OnBonusIntervalStarted?.Invoke();
        }
    }

    private void Update()
    {
        if (!isClockRunning) return;

        remainingTime -= Time.deltaTime;
        OnTimerTick?.Invoke(remainingTime);

        if (!isBonusTimerPaused)
            UpdateBonusTimer();

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            isClockRunning = false;
            OnMatchEnded?.Invoke();
        }
    }
}