//using UnityEditor.iOS;
using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameTimerController gameTimerController;
    [SerializeField] private ScoreController scoreController;
    [SerializeField] private ThrowBallInputHandler throwBallInputHandler;
    [SerializeField] private BasketballDetectorController basketDetectorController;
    [SerializeField] private ShootingPositionController playerShootingPositionController;
    [SerializeField] private ShootingPositionController cpuShootingPositionController;
    [SerializeField] private BallShooterController playerBallShooterController;
    [SerializeField] private CPUController cpuController;
    [SerializeField] private FireballController fireballController;
    [SerializeField] private MatchResultSO matchResult;

    // Events
    public event System.Action OnBackboardBonusActivated;
    public event System.Action OnBackboardBonusReset;
    public event System.Action<ShotType, int> OnPlayerScored;

    [Header("Bonus Configuration")]
    [SerializeField] private int[] backboardBonusValues = { 2, 4, 6 };

    // State
    private bool isBonusActive = false;
    private bool isBonusReady = false; // timer fired, waiting for next basket to activate

    private void OnEnable()
    {
        gameTimerController.OnMatchEnded += HandleMatchEnded;
        gameTimerController.OnBonusIntervalStarted += HandleBonusIntervalStarted;
        basketDetectorController.OnBasketballScored += HandleBasketScored;
        playerBallShooterController.OnShotCompleted += HandlePlayerShotCompleted;
    }

    private void OnDisable()
    {
        gameTimerController.OnMatchEnded -= HandleMatchEnded;
        gameTimerController.OnBonusIntervalStarted -= HandleBonusIntervalStarted;
        basketDetectorController.OnBasketballScored -= HandleBasketScored;
        playerBallShooterController.OnShotCompleted -= HandlePlayerShotCompleted;
    }

    private void Start()
    {
        InitializeMatch();
    }

    private void InitializeMatch()
    {
        scoreController.ResetScores();

        // Randomly decide player orientation - CPU starts on the opposite side
        bool playerStartsRight = UnityEngine.Random.value > 0.5f;
        playerShootingPositionController.GenerateNewRound(playerStartsRight);
        cpuShootingPositionController.GenerateNewRound(!playerStartsRight);

        gameTimerController.StartMatch();
    }

    private void HandleBonusIntervalStarted()
    {
        isBonusReady = true;
    }

    private void HandleBasketScored(ShotType shotType, GameEntity scoredEntity)
    {

        scoreController.AddScore(scoredEntity, shotType);

        if (scoredEntity == GameEntity.Player)
        {
            fireballController.HandlePlayerBasketScored(shotType);
            OnPlayerScored?.Invoke(shotType, scoreController.LastScoredPoints);
        }

        if (isBonusActive && shotType == ShotType.PerfectBackboard)
            ResetBackboardBonus();
        else if (isBonusReady && !isBonusActive)
            ActivateBonus();
    }

    // For detecting possible missed shots after the player completes a shot - It would drain the fireball bar
    private void HandlePlayerShotCompleted(ShotType shotType)
    {
        if (shotType == ShotType.Short || shotType == ShotType.LowerBackboard || shotType == ShotType.UpperBackboard)
        {
            fireballController.HandlePlayerPossibleMissedShot();
        }
    }

    private void ActivateBonus()
    {
        isBonusReady = false;
        isBonusActive = true;

        int bonus = backboardBonusValues[UnityEngine.Random.Range(0, backboardBonusValues.Length)];
        scoreController.SetBackboardBonus(bonus);
        BackboardVisualFeedbackController.instance.StartBonusGlow(bonus);

        // Notify CPU so it can adjust its shot selection
        OnBackboardBonusActivated?.Invoke();

        Debug.Log($"[GameController] Bonus activated: +{bonus}");
    }

    private void ResetBackboardBonus()
    {
        isBonusActive = false;
        scoreController.SetBackboardBonus(0);
        BackboardVisualFeedbackController.instance.StopBonusGlow();
        gameTimerController.ResumeBonusTimer();

        // Notify CPU so it returns to normal shot selection
        OnBackboardBonusReset?.Invoke();

        Debug.Log("[GameController] Reset BackboardBonus, timer resumed.");
    }

    private void HandleMatchEnded()
    {
        throwBallInputHandler.enabled = false;
        cpuController.enabled = false;

        if (isBonusActive)
        {
            isBonusActive = false;
            BackboardVisualFeedbackController.instance.StopBonusGlow();
        }

        isBonusReady = false;

        Debug.Log($"[GameController] Match ended! " +
                  $"Player: {scoreController.PlayerScore} | CPU: {scoreController.CpuScore}");

        // Load 
        StartCoroutine(LoadMainMenuSceneResults());
    }

    private IEnumerator LoadMainMenuSceneResults()
    {
        yield return new WaitForSeconds(2f);
        DOTween.KillAll();

        // Store results in ScriptableObject for end game UI to display - in here so the information passed to the other scene is correct
        matchResult.SetResult(scoreController.PlayerScore, scoreController.CpuScore);

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu"); 
    }
}