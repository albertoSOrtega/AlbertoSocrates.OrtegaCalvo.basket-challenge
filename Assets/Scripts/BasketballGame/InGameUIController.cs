using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameUIController : MonoBehaviour
{
    [Header("Landscape UI References")]
    [SerializeField] private Slider shootPowerSlider_LS;
    [SerializeField] private Slider fireballSlider_LS;
    [SerializeField] private Image sliderFire_LS;
    [SerializeField] private GameObject timesTwo_LS;
    [SerializeField] private TextMeshProUGUI shootText_LS;
    [SerializeField] private TextMeshProUGUI playerScoreText_LS;
    [SerializeField] private TextMeshProUGUI cpuScoreText_LS;
    //[SerializeField] private TextMeshProUGUI timerText_LS;
    [SerializeField] private Image perfectZoneImage_LS;
    [SerializeField] private Image backboardZoneImage_LS;
    [SerializeField] private Image playerTimerFillImage_LS;
    [SerializeField] private Image cpuTimerFillImage_LS;
    [SerializeField] private TextMeshProUGUI moneyQuantity_LS;

    [Header("Portrait UI References")]
    [SerializeField] private Slider shootPowerSlider_PT;
    [SerializeField] private Slider fireballSlider_PT;
    [SerializeField] private Image sliderFire_PT;
    [SerializeField] private GameObject timesTwo_PT;
    [SerializeField] private TextMeshProUGUI shootText_PT;
    [SerializeField] private TextMeshProUGUI playerScoreText_PT;
    [SerializeField] private TextMeshProUGUI cpuScoreText_PT;
    //[SerializeField] private TextMeshProUGUI timerText_PT;
    [SerializeField] private Image perfectZoneImage_PT;
    [SerializeField] private Image backboardZoneImage_PT;
    [SerializeField] private Image playerTimerFillImage_PT;
    [SerializeField] private Image cpuTimerFillImage_PT;
    [SerializeField] private TextMeshProUGUI moneyQuantity_PT;

    [Header("Controller References")]
    public ShootingBarZoneController shootingBarZoneController;
    public BallShooterController ballShooterController;
    public ScoreController scoreController;
    public GameTimerController gameTimerController;
    public FireballController fireballController;
    public SelectedDifficultySO selectedDifficulty;
    public ThrowBallInputHandler throwBallInputHandler;
    public PauseController pauseController;
    public GameController gameController;

    [Header("Perfect Shooting Zone Colors")]
    public Color normalPerfectZoneColor = new Color(0.75f, 0.6f, 0f, 1f);  
    public Color inPerfectZoneColor = new Color(0.3f, 0.75f, 0f, 1f);
    public Color normalBackboardZoneColor = new Color(0.7f, 0f, 1f, 1f);
    public Color inBackboardZoneColor = new Color(1f, 0f, 0.7f, 1f);

    // Intern active references (point to the correct orientation ones at runtime)
    private Slider shootPowerSlider;
    private Slider fireballSlider;
    private Image sliderFire;
    private GameObject timesTwo;
    private TextMeshProUGUI shootText;
    private TextMeshProUGUI playerScoreText;
    private TextMeshProUGUI cpuScoreText;
    //public TextMeshProUGUI timerText;
    private Image perfectZoneImage;
    private Image backboardZoneImage;
    private Image playerTimerFillImage;
    private Image cpuTimerFillImage;
    private TextMeshProUGUI moneyQuantity;

    private static readonly Color ColorPerfect = new Color(0.2f, 0.9f, 0.2f);
    private static readonly Color ColorImperfect = new Color(1f, 0.85f, 0f);
    private static readonly Color ColorBackboardBonus = new Color(0f, 0.9f, 0.85f);

    private void Awake()
    {
        bool isPortrait = Screen.height > Screen.width;

        shootPowerSlider = isPortrait ? shootPowerSlider_PT : shootPowerSlider_LS;
        fireballSlider = isPortrait ? fireballSlider_PT : fireballSlider_LS;
        shootText = isPortrait ? shootText_PT : shootText_LS;
        playerScoreText = isPortrait ? playerScoreText_PT : playerScoreText_LS;
        cpuScoreText = isPortrait ? cpuScoreText_PT : cpuScoreText_LS;
        //timerText = isPortrait ? timerText_PT : timerText_LS;
        perfectZoneImage = isPortrait ? perfectZoneImage_PT : perfectZoneImage_LS;
        backboardZoneImage = isPortrait ? backboardZoneImage_PT : backboardZoneImage_LS;
        playerTimerFillImage = isPortrait ? playerTimerFillImage_PT : playerTimerFillImage_LS;
        cpuTimerFillImage = isPortrait ? cpuTimerFillImage_PT : cpuTimerFillImage_LS;
        moneyQuantity = isPortrait ? moneyQuantity_PT : moneyQuantity_LS;
        sliderFire = isPortrait ? sliderFire_PT : sliderFire_LS;
        timesTwo = isPortrait ? timesTwo_PT : timesTwo_LS;
    }

    private void OnEnable()
    {
        // Subscribe to the events from the ThrowBallInputHandler
        throwBallInputHandler.OnInputEnabledNextFrame += ResetAfterShot;
        throwBallInputHandler.OnInputEnabledNextFrame += InitializeZoneRects;
        throwBallInputHandler.OnShootPowerChanged += UpdateSlider;

        // Subscribe to the events of the ScoreController
        scoreController.OnScoreUpdated += UpdateScore;

        // Subscribe to the events of the GameTimerController
        gameTimerController.OnTimerTick += UpdateTimer;

        fireballController.OnBarValueChanged += UpdateFireballBar;
        fireballController.OnFireballBonusActivated += ActivateFireballBonusVisuals;
        fireballController.OnFireballBonusDeactivated += DeactivateFireballBonusVisuals;

        gameController.OnPlayerScored += HandlePlayerScored;
    }

    private void OnDisable()
    {
        // Unsubscribe to the events from the ThrowBallInputHandler
        throwBallInputHandler.OnInputEnabledNextFrame -= ResetAfterShot;
        throwBallInputHandler.OnInputEnabledNextFrame -= InitializeZoneRects;
        throwBallInputHandler.OnShootPowerChanged -= UpdateSlider;

        // Unsubscribe to the events of the ScoreController
        scoreController.OnScoreUpdated -= UpdateScore;

        // Unsscribe to the events of the GameTimerController
        gameTimerController.OnTimerTick -= UpdateTimer;

        fireballController.OnBarValueChanged -= UpdateFireballBar;
        fireballController.OnFireballBonusActivated -= ActivateFireballBonusVisuals;
        fireballController.OnFireballBonusDeactivated -= DeactivateFireballBonusVisuals;

        gameController.OnPlayerScored -= HandlePlayerScored;
    }

    public void UpdateSlider(float shootPower)
    {
        shootPowerSlider.value = shootPower;
        UpdateZoneColors(shootPower);
    }

    public void ResetSlider()
    {
        shootPowerSlider.value = 0f;
    }

    public void ResetAfterShot()
    {
        ResetSlider();
        UpdateSlider(0f);
    }

    // Positions and sizes both zone images based on the computed boundaries
    public void InitializeZoneRects()
    {
        RectTransform sliderRect = (RectTransform)shootPowerSlider.transform;
        bool isVertical = sliderRect.rect.height > sliderRect.rect.width;
        float sliderLength = isVertical ? sliderRect.rect.height : sliderRect.rect.width;

        if (isVertical)
        {
            // Vertical slider — zones positioned on Y axis
            float perfectPosY = shootingBarZoneController.perfectZoneStart * sliderLength;
            float perfectHeight = shootingBarZoneController.perfectZoneSize * sliderLength;
            perfectZoneImage.rectTransform.anchoredPosition = new Vector2(0f, perfectPosY);
            perfectZoneImage.rectTransform.sizeDelta = new Vector2(0f, perfectHeight);

            float backboardPosY = shootingBarZoneController.backboardStart * sliderLength;
            float backboardHeight = shootingBarZoneController.backboardZoneSize * sliderLength;
            backboardZoneImage.rectTransform.anchoredPosition = new Vector2(0f, backboardPosY);
            backboardZoneImage.rectTransform.sizeDelta = new Vector2(0f, backboardHeight);
        }
        else
        {
            // Horizontal slider — zones positioned on X axis
            float perfectPosX = shootingBarZoneController.perfectZoneStart * sliderLength;
            float perfectWidth = shootingBarZoneController.perfectZoneSize * sliderLength;
            perfectZoneImage.rectTransform.anchoredPosition = new Vector2(perfectPosX, 0f);
            perfectZoneImage.rectTransform.sizeDelta = new Vector2(perfectWidth, 0f);

            float backboardPosX = shootingBarZoneController.backboardStart * sliderLength;
            float backboardWidth = shootingBarZoneController.backboardZoneSize * sliderLength;
            backboardZoneImage.rectTransform.anchoredPosition = new Vector2(backboardPosX, 0f);
            backboardZoneImage.rectTransform.sizeDelta = new Vector2(backboardWidth, 0f);
        }
    }

    // Updates both zone image colors independently and always resets the inactive one
    public void UpdateZoneColors(float shootPower)
    {
        perfectZoneImage.color = shootingBarZoneController.IsInPerfectZone(shootPower)
            ? inPerfectZoneColor
            : normalPerfectZoneColor;

        backboardZoneImage.color = shootingBarZoneController.IsInBackboardZone(shootPower)
            ? inBackboardZoneColor
            : normalBackboardZoneColor;
    }

    public void UpdateScore(int PlayerScore, int CPUScore)
    {
        playerScoreText.text = $"{PlayerScore}";
        cpuScoreText.text = $"{CPUScore}";
    }

    public void UpdateTimer(float currentTimeNormalized)
    {
        playerTimerFillImage.fillAmount = Mathf.Clamp01(currentTimeNormalized);
        cpuTimerFillImage.fillAmount = Mathf.Clamp01(currentTimeNormalized);

        if (currentTimeNormalized < 0.2f)
        {
            playerTimerFillImage.color = Color.red; // Change color to red when time is running out
            cpuTimerFillImage.color = Color.red;
        }
    }

    private void UpdateFireballBar(float value)
    {
        fireballSlider.value = value;
    }

    private void ActivateFireballBonusVisuals()
    {
        Color orangeColor = new Color32(238, 137, 0, 255);
        fireballSlider.fillRect.GetComponent<Image>().color = orangeColor; // Change color to indicate bonus
        sliderFire.color = orangeColor; // Make the fire icon more vibrant
        timesTwo.SetActive(true); // Show the "x2" icon

        // Animate x2
        RectTransform timesTwoRect = timesTwo.GetComponent<RectTransform>();

        // Resetea antes de lanzar por si quedó en un estado intermedio
        timesTwoRect.localScale = Vector3.one;
        timesTwoRect.localRotation = Quaternion.identity;

        // Vibration
        timesTwoRect.localRotation = Quaternion.Euler(0f, 0f, -15f);
        timesTwoRect.DORotate(new Vector3(0f, 0f, 15f), 0.4f)
                   .SetEase(Ease.InOutSine)
                   .SetLoops(-1, LoopType.Yoyo);

        // Scaling
        timesTwoRect.DOScale(1.2f, 0.4f)
                   .SetEase(Ease.InOutSine)
                   .SetLoops(-1, LoopType.Yoyo);
    }

    private void DeactivateFireballBonusVisuals()
    {
        fireballSlider.fillRect.GetComponent<Image>().color = new Color32(122, 123, 125, 255); // Revert to normal color
        sliderFire.color = Color.white; // Revert fire icon color

        RectTransform timesTwoRect = timesTwo.GetComponent<RectTransform>();

        // Mata todos los tweens del objeto y devuelve al estado original
        timesTwoRect.DOKill();
        timesTwoRect.localScale = Vector3.one;
        timesTwoRect.localRotation = Quaternion.identity;

        timesTwo.SetActive(false); // hide the "x2" icon
    }

    public void OnPauseButtonPressed()
    {
        pauseController.TogglePause();
    }

    private void HandlePlayerScored(ShotType shotType, int points)
    {
        shootText.DOKill();
        shootText.alpha = 1f;

        switch (shotType)
        {
            case ShotType.Perfect:
                shootText.text = $"Perfect Shot! +{points} Points";
                shootText.color = ColorPerfect;
                break;

            case ShotType.Imperfect:
                shootText.text = $"2 Point Shot! +{points} Points";
                shootText.color = ColorImperfect;
                break;

            case ShotType.PerfectBackboard:
                bool hasBonus = points > 2;
                shootText.text = hasBonus
                    ? $"Bonus Shot! +{points} Points"
                    : $"2 Point Shot! +{points} Points";
                shootText.color = hasBonus ? ColorBackboardBonus : ColorImperfect;
                break;
        }

        shootText.DOFade(0f, 2f);
    }

    // Start is called before the first frame update
    void Start()
    {
        perfectZoneImage.color = normalPerfectZoneColor;
        UpdateScore(0, 0);
        moneyQuantity.text = selectedDifficulty.config.moneyReward.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
