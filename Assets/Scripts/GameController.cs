//using UnityEditor.iOS;
using System;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameTimerController gameTimerController;
    [SerializeField] private ScoreController scoreController;
    [SerializeField] private ThrowBallInputHandler throwBallInputHandler;
    [SerializeField] private BasketballDetectorController basketDetectorController;

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
    }

    private void OnDisable()
    {
        gameTimerController.OnMatchEnded -= HandleMatchEnded;
        gameTimerController.OnBonusIntervalStarted -= HandleBonusIntervalStarted;
        basketDetectorController.OnBasketballScored -= HandleBasketScored;
    }

    private void Start()
    {
        scoreController.ResetScores();
        gameTimerController.StartMatch();
    }

    private void HandleBonusIntervalStarted()
    {
        isBonusReady = true;
    }

    private void HandleBasketScored(ShotType shotType)
    {
        // this is just if we don't want to count the last basket scored after the match has ended
        //if (!gameTimerController.IsMatchActive) return;
        scoreController.AddScore(ScoringEntity.Player, shotType);

        if (isBonusActive && shotType == ShotType.PerfectBackboard)
        {
            ResetBackboardBonus();
        }
        else if (isBonusReady && !isBonusActive)
        {
            ActivateBonus();
        }
    }

    private void ActivateBonus()
    {
        isBonusReady = false;
        isBonusActive = true;

        int bonus = backboardBonusValues[UnityEngine.Random.Range(0, backboardBonusValues.Length)];
        scoreController.SetBackboardBonus(bonus);
        BackboardVisualFeedbackController.instance.StartBonusGlow(bonus);

        Debug.Log($"[GameController] Bonus activated: +{bonus}");
    }

    private void ResetBackboardBonus()
    {
        isBonusActive = false;
        scoreController.SetBackboardBonus(0);
        BackboardVisualFeedbackController.instance.StopBonusGlow();
        gameTimerController.ResumeBonusTimer();

        Debug.Log("[GameController] Reset BackboardBonus, timer resumed.");
    }

    private void HandleMatchEnded()
    {
        throwBallInputHandler.enabled = false;

        if (isBonusActive)
        {
            isBonusActive = false;
            BackboardVisualFeedbackController.instance.StopBonusGlow();
        }

        isBonusReady = false;

        Debug.Log($"[GameController] Match ended! " +
                  $"Player: {scoreController.PlayerScore} | CPU: {scoreController.CpuScore}");

        // TODO: trigger end game UI
    }
}