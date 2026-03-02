using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public enum GameEntity { Player, CPU }

public class ScoreController : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private FireballController fireballController;

    [Header("Points Configuration")]
    [SerializeField] private int perfectShotBasePoints = 3;
    [SerializeField] private int perfectBackboardBasePoints = 2;
    [SerializeField] private int imperfectShotBasePoints = 2;

    // Events
    public event System.Action<int, int> OnScoreUpdated; // playerScore, cpuScore

    // State
    public int PlayerScore  { get; private set; }
    public int CpuScore { get; private set; }
    private int currentBackboardBonus = 0;

    public void ResetScores()
    {
        PlayerScore = 0;
        CpuScore = 0;
        OnScoreUpdated?.Invoke(PlayerScore, CpuScore);
    }

    public void AddScore(GameEntity entity, ShotType shotType)
    {
        int points = CalculatePoints(entity, shotType);
        if (points == 0) return;

        if (entity == GameEntity.Player)
            PlayerScore += points;
        else
            CpuScore += points;

        OnScoreUpdated?.Invoke(PlayerScore, CpuScore);

        Debug.Log($"[ScoreController] {entity} +{points}pts ({shotType}). " +
                  $"Player: {PlayerScore} | CPU: {CpuScore}");
    }

    private int CalculatePoints(GameEntity gameEntity, ShotType shotType)
    {
        // Calculate Fireball multiplier that only applies to the player
        int fireballMultiplier;
        if (gameEntity == GameEntity.Player)
        {
            fireballMultiplier = fireballController.GetScoreFireballBonusMultiplier();
        }
        else
        {
            fireballMultiplier = 1; // CPU does not get Fireball multiplier
        }

        switch (shotType)
        {
            case ShotType.Perfect:
                return perfectShotBasePoints * fireballMultiplier;
            case ShotType.Imperfect:
                return imperfectShotBasePoints * fireballMultiplier;
            case ShotType.PerfectBackboard:
                return (perfectBackboardBasePoints + currentBackboardBonus) * fireballMultiplier;
            default:
                return 0;
        }
    }

    public void SetBackboardBonus(int bonus)
    {
        currentBackboardBonus = bonus;
        Debug.Log($"[ScoreController] Backboard bonus set to: +{bonus}");
    }
}