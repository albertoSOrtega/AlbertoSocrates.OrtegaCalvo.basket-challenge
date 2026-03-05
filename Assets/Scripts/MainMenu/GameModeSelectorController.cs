using DG.Tweening;
using UnityEngine;

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
        MenuAudioController.instance?.PlayConfirmSound();
        DOTween.KillAll();
        matchResult?.Clear();
        selectedDifficulty.config = config;
        UnityEngine.SceneManagement.SceneManager.LoadScene("BasketballGame");
    }
}