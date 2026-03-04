using UnityEngine;

[CreateAssetMenu(fileName = "DifficultyConfig", menuName = "BasketballGame/Difficulty Config")]
public class GameDifficultyConfigSO : ScriptableObject
{
    [Header("CPU - Shooting Timing")]
    public float cpuShootingRateMin = 2f;
    public float cpuShootingRateMax = 4f;

    [Header("CPU - Shot Weights")]
    public float weightPerfect = 0.50f;
    public float weightImperfect = 0.20f;
    public float weightShort = 0.10f;
    public float weightPerfectBackboard = 0.10f;
    public float weightLowerBackboard = 0.05f;
    public float weightUpperBackboard = 0.05f;

    [Header("CPU - Backboard Bonus")]
    [Range(0f, 1f)]
    public float perfectBackboardBonusProbability = 0.5f;

    [Header("Player - Shooting Bar Zones")]
    [Range(0.1f, 0.2f)]
    public float perfectZoneSize = 0.1f;
    [Range(0.05f, 0.1f)]
    public float imperfectZoneSize = 0.1f;
    [Range(0.05f, 0.1f)]
    public float lowerBackboardSize = 0.1f;
    [Range(0.05f, 0.2f)]
    public float backboardZoneSize = 0.1f;

    [Header("Game - Timer")]
    public float matchDuration = 60f;

    [Header("Fireball")]
    public float perfectShotFillRate = 0.25f;
    public float otherScoredShotFillRate = 0.15f;

    [Header("Difficulty Rewards")]
    public int moneyReward = 50;
    public bool bagReward = true;
}