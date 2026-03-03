using DG.Tweening;
using UnityEngine;
using static UnityEngine.InputSystem.InputControlScheme;

public class GameModeSelectorController : MonoBehaviour
{
    [SerializeField] private SelectedDifficultySO selectedDifficulty;
    [SerializeField] private MatchResultSO matchResult;
    [SerializeField] private GameDifficultyConfigSO easyConfig;
    [SerializeField] private GameDifficultyConfigSO mediumConfig;
    [SerializeField] private GameDifficultyConfigSO hardConfig;

    public void StartEasyGame() => StartGame(easyConfig);
    public void StartNormalGame() => StartGame(mediumConfig);
    public void StartHardGame() => StartGame(hardConfig);

    private void StartGame(GameDifficultyConfigSO config)
    {
        DOTween.KillAll();
        matchResult?.Clear();
        selectedDifficulty.Select(config);
        UnityEngine.SceneManagement.SceneManager.LoadScene("BasketballGame");
    }
}