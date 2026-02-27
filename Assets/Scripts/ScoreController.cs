using UnityEngine;

public enum ScoringEntity { Player, CPU }

public class ScoreController : MonoBehaviour
{ 

    [Header("Points Configuration")]
    [SerializeField] private int perfectShotBasePoints = 3;
    [SerializeField] private int perfectBackboardBasePoints = 2;
    [SerializeField] private int imperfectShotBasePoints = 2;

    // Events
    public event System.Action<int, int> OnScoreUpdated;           // playerScore, cpuScore
    public event System.Action<int, ScoringEntity> OnBasketScored; // points, who scored

    // State
    public int PlayerScore  { get; private set; }
    public int CpuScore { get; private set; }
    private int currentBackboardBonus = 0;
    private int currentFireballMultiplier = 1;

    public void ResetScores()
    {
        PlayerScore = 0;
        CpuScore = 0;
        OnScoreUpdated?.Invoke(PlayerScore, CpuScore);
    }

    public void AddScore(ScoringEntity entity, ShotType shotType)
    {
        int points = CalculatePoints(shotType);
        if (points == 0) return;

        if (entity == ScoringEntity.Player)
            PlayerScore += points;
        else
            CpuScore += points;

        OnScoreUpdated?.Invoke(PlayerScore, CpuScore);

        Debug.Log($"[ScoreController] {entity} +{points}pts ({shotType}). " +
                  $"Player: {PlayerScore} | CPU: {CpuScore}");
    }

    private int CalculatePoints(ShotType shotType)
    {
        switch (shotType)
        {
            case ShotType.Perfect:
                return perfectShotBasePoints * currentFireballMultiplier;
            case ShotType.Imperfect:
                return imperfectShotBasePoints * currentFireballMultiplier;
            case ShotType.PerfectBackboard:
                return (perfectBackboardBasePoints + currentBackboardBonus) * currentFireballMultiplier;
            default:
                return 0;
        }
    }
}