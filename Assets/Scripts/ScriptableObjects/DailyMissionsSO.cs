using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DailyMissions", menuName = "BasketballGame/DailyMissions")]
public class DailyMissionsSO : ScriptableObject
{
    [Header("Daily Missions State")]
    public List<bool> missionsDone;
    public List<bool> rewardsClaimed;
    public bool bagRewardClaimed;
}
