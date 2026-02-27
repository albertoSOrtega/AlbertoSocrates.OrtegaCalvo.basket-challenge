//using UnityEditor.iOS;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameTimerController gameTimerController;
    [SerializeField] private ScoreController scoreController;
    [SerializeField] private ThrowBallInputHandler throwBallInputHandler;
    [SerializeField] private BasketballDetectorController basketDetectorController;

    private void OnEnable()
    {
        gameTimerController.OnMatchEnded += HandleMatchEnded;
        basketDetectorController.OnBasketballScored += HandleBasketScored;
    }

    private void OnDisable()
    {
        gameTimerController.OnMatchEnded -= HandleMatchEnded;
        basketDetectorController.OnBasketballScored -= HandleBasketScored;
    }

    private void Start()
    {
        scoreController.ResetScores();
        gameTimerController.StartMatch();
    }

    private void HandleBasketScored(ShotType shotType)
    {
        // this is just if we don't want to count the last basket scored after the match has ended
        //if (!gameTimerController.IsMatchActive) return;
        scoreController.AddScore(ScoringEntity.Player, shotType);
    }

    private void HandleMatchEnded()
    {
        throwBallInputHandler.enabled = false;

        Debug.Log($"[GameController] Match ended! " +
                  $"Player: {scoreController.PlayerScore} | CPU: {scoreController.CpuScore}");

        // TODO: trigger end game UI
    }
}