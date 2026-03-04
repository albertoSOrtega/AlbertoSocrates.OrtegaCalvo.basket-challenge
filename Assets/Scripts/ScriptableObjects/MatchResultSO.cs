using UnityEngine;

[CreateAssetMenu(fileName = "MatchResult", menuName = "BasketballGame/Match Result")]
public class MatchResultSO : ScriptableObject
{
    public int playerScore;
    public int cpuScore;
    public bool hasResult;

    public void SetResult(int player, int cpu)
    {
        playerScore = player;
        cpuScore = cpu;
        hasResult = true;
    }

    public void Clear()
    {
        playerScore = 0;
        cpuScore = 0;
        hasResult = false;
    }
}