using UnityEngine;

public class FireballController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] BallShooterController playerBallShooterController;

    [Header("Fill Rates")]
    [SerializeField] private float perfectShotFillRate = 0.25f;
    [SerializeField] private float otherScoredShotFillRate = 0.15f; // Imperfect + PerfectBackboard

    [Header("Drain Rates — passive over time")]
    [SerializeField] private float drainRatePassive = 0.03f;  // While filling, not yet active
    [SerializeField] private float drainRateBonusActive = 0.08f;   // While Fireball bonus is active

    // Events
    public event System.Action OnFireballBonusActivated;
    public event System.Action OnFireballBonusDeactivated;
    public event System.Action<float> OnBarValueChanged;

    // State
    private float fireballBarValue = 0f;
    private bool isFireballBonusActive = false;

    private void OnEnable()
    {
        playerBallShooterController.OnShotCompleted += HandlePossibleMissedShot;
    }

    private void OnDisable()
    {
        playerBallShooterController.OnShotCompleted -= HandlePossibleMissedShot;
    }

    public bool GetIsFireballBonusActive()
    {
        return isFireballBonusActive;
    }
    
    public int GetScoreFireballBonusMultiplier()
    {
        return isFireballBonusActive ? 2 : 1;
    }

    private void Update()
    {
        if (fireballBarValue <= 0f) return;

        float drain = isFireballBonusActive ? drainRateBonusActive : drainRatePassive;
        SetFireballBarValue(fireballBarValue - drain * Time.deltaTime);

        if (isFireballBonusActive && fireballBarValue <= 0f)
        {
            EmptyFireballBonusBar();
        }
    }

    // Called by GameController when the player scores
    public void HandlePlayerBasketScored(ShotType shotType)
    {
        if (isFireballBonusActive) return; // No filling while bonus is active

        if (shotType == ShotType.Perfect)
        {
            FillFireballBonusBar(perfectShotFillRate);
        }
        else // Imperfect and PerfectBackboard are the only options here
        {
            FillFireballBonusBar(otherScoredShotFillRate);
        }
    }

    // Called when OnShotCompleted event is raised, to check for missed shots and empty the bar if needed
    public void HandlePossibleMissedShot(ShotType shotType, GameEntity gameEntity)
    {
        if (gameEntity != GameEntity.Player || !isFireballBonusActive) return; // Only handle player's missed shots when bonus is active

        if (shotType == ShotType.Short || shotType == ShotType.LowerBackboard || shotType == ShotType.UpperBackboard)
        {
            EmptyFireballBonusBar();
        }
    }

    private void FillFireballBonusBar(float amount)
    {
        SetFireballBarValue(fireballBarValue + amount);

        if (!isFireballBonusActive && fireballBarValue >= 1f)
            ActivateFireball();
    }

    private void EmptyFireballBonusBar()
    {
        SetFireballBarValue(0f);

        if (isFireballBonusActive)
            DeactivateFireball();
    }

    private void ActivateFireball()
    {
        isFireballBonusActive = true;
        OnFireballBonusActivated?.Invoke(); // visuals
    }

    private void DeactivateFireball()
    {
        isFireballBonusActive = false;
        OnFireballBonusDeactivated?.Invoke(); // visuals
    }

    private void SetFireballBarValue(float value)
    {
        fireballBarValue = Mathf.Clamp01(value);
        OnBarValueChanged?.Invoke(fireballBarValue);
    }
}