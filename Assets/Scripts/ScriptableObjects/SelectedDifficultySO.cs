using UnityEngine;

[CreateAssetMenu(fileName = "SelectedDifficulty", menuName = "BasketballGame/Selected Difficulty")]
public class SelectedDifficultySO : ScriptableObject
{
    public GameDifficultyConfigSO config;

    public void Select(GameDifficultyConfigSO selectedConfig)
    {
        config = selectedConfig;
    }

    public void Clear()
    {
        config = null;
    }
}