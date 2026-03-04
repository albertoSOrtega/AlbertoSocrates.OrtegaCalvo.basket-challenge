using UnityEngine;

// Single Responsibility: distributes difficulty configuration to all controllers
// before the match starts. If no config is provided (e.g. Play from Editor),
// each controller keeps its Inspector-serialized default values.
public class GameInitializer : MonoBehaviour
{
    [Header("Difficulty Config (optional - leave null to use Inspector defaults)")]
    [SerializeField] private GameDifficultyConfigSO difficultyConfig;

    [Header("References")]
    [SerializeField] private CPUController cpuController;
    [SerializeField] private ShootingBarZoneController shootingBarZoneController;
    [SerializeField] private GameTimerController gameTimerController;
    [SerializeField] private FireballController fireballController;

    // SO from menu
    [SerializeField] private SelectedDifficultySO selectedDifficulty;

    private void Awake()
    {
        // If there is a selected difficulty from the menu, use its config (overriding any Inspector assignment)
        if (selectedDifficulty != null && selectedDifficulty.config != null)
        {
            difficultyConfig = selectedDifficulty.config;
        }

        // If no config is assigned (either from menu or Inspector), log a warning and keep using defaults
        if (difficultyConfig == null)
        {
            Debug.Log("[GameInitializer] No difficulty config found - using Inspector defaults.");
            return;
        }

        ApplyDifficultyConfig();
    }

    private void ApplyDifficultyConfig()
    {
        cpuController.ApplyConfig(difficultyConfig);
        shootingBarZoneController.ApplyConfig(difficultyConfig);
        gameTimerController.ApplyConfig(difficultyConfig);
        fireballController.ApplyConfig(difficultyConfig);

        Debug.Log($"[GameInitializer] Config applied: {difficultyConfig.name}");
    }
}