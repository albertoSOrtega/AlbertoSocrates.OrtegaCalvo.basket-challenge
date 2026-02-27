using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameUIController : MonoBehaviour
{
    [Header("UI References")]
    public Slider shootPowerSlider;
    public TextMeshProUGUI shootText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public ThrowBallInputHandler throwBallInputHandler;
    public Image perfectZoneImage;
    public Image backboardZoneImage;

    [Header("Controller References")]
    public ShootingBarZoneController shootingBarZoneController;
    public BallShooterController ballShooterController;
    public ScoreController scoreController;
    public GameTimerController gameTimerController;

    [Header("Perfect Shooting Zone Colors")]
    public Color normalPerfectZoneColor = new Color(0.75f, 0.6f, 0f, 1f);  
    public Color inPerfectZoneColor = new Color(0.3f, 0.75f, 0f, 1f);
    public Color normalBackboardZoneColor = new Color(0.7f, 0f, 1f, 1f);
    public Color inBackboardZoneColor = new Color(1f, 0f, 0.7f, 1f);

    private void OnEnable()
    {
        // Subscribe to the events from the ThrowBallInputHandler
        throwBallInputHandler.OnSwipeStarted += ResetSlider;
        throwBallInputHandler.OnShootPowerChanged += UpdateSlider;
        throwBallInputHandler.OnShootReleased += UIHandleShoot;
        throwBallInputHandler.OnSwipeCancelled += UIHandleCancelShoot;

        // Suscribe to the eevnts of the shootingBarZoneController to initialize the perfect zone rect when randomized
        shootingBarZoneController.OnShootingZonesInitialized += InitializeZoneRects;

        // Subscribe to the events of the BallShooterController
        ballShooterController.OnShotCompleted += ResetAfterShot;

        // Subscribe to the events of the ScoreController
        scoreController.OnScoreUpdated += UpdateScore;

        // Subscribe to the events of the GameTimerController
        gameTimerController.OnTimerTick += UpdateTimer;
    }

    private void OnDisable()
    {
        // Unsubscribe to the events from the ThrowBallInputHandler
        throwBallInputHandler.OnSwipeStarted -= ResetSlider;
        throwBallInputHandler.OnShootPowerChanged -= UpdateSlider;
        throwBallInputHandler.OnShootReleased -= UIHandleShoot;
        throwBallInputHandler.OnSwipeCancelled -= UIHandleCancelShoot;

        // Unsubscribe to the eevnts of the shootingBarZoneController
        shootingBarZoneController.OnShootingZonesInitialized -= InitializeZoneRects;

        // Unsubscribe to the events of the BallShooterController
        ballShooterController.OnShotCompleted -= ResetAfterShot;
    }

    public void UIHandleShoot(float shootPower)
    {
        shootText.DOKill();
        shootText.alpha = 1f; // Reset alpha to fully visible

        ShotType shotType = shootingBarZoneController.GetShotType(shootPower);

        switch (shotType)
        {
            case ShotType.Perfect:
                shootText.text = $"Shooting Perfect Shot with this power: {shootPower}";
                break;
            case ShotType.Imperfect:
                shootText.text = $"Shooting Imperfect Shot with this power: {shootPower}";
                break;
            case ShotType.Short:
                shootText.text = $"Short Shot, You Failed! {shootPower}";
                ResetAfterShot(shotType);
                break;
            case ShotType.PerfectBackboard:
                shootText.text = $"Perfect backboard shot! {shootPower}";
                ResetAfterShot(shotType);
                break;
            case ShotType.LowerBackboard:
                shootText.text = $"Lower backboad shot, you failed {shootPower}";
                ResetAfterShot(shotType);
                break;
            case ShotType.UpperBackboard:
                shootText.text = $"upper backboard shot, you failed {shootPower}";
                ResetAfterShot(shotType);
                break;
            default:
                shootText.text = $"Shooting Perfect Shot with this power: {shootPower}";
                break;
        }
        
        shootText.DOFade(0, 2f);
    }

    public void UIHandleCancelShoot(float shootPower)
    {
        shootText.DOKill();
        shootText.alpha = 1f; // Reset alpha to fully visible
        shootText.text = $"Shoot cancelled with this power: {shootPower}";
        shootText.DOFade(0, 2f);
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

    public void ResetAfterShot(ShotType shotType)
    {
        ResetSlider();
        UpdateSlider(0f);
    }

    // Positions and sizes both zone images based on the computed boundaries
    public void InitializeZoneRects()
    {
        // Slider is rotated 90º in Z, so we use rect.width as the bar length
        float sliderLength = ((RectTransform)shootPowerSlider.transform).rect.width;

        // Perfect zone
        float perfectPosX = shootingBarZoneController.perfectZoneStart * sliderLength;
        float perfectWidth = shootingBarZoneController.perfectZoneSize * sliderLength;
        perfectZoneImage.rectTransform.anchoredPosition = new Vector2(perfectPosX, 0f);
        perfectZoneImage.rectTransform.sizeDelta = new Vector2(perfectWidth, 0f);

        // Backboard zone
        float backboardPosX = shootingBarZoneController.backboardStart * sliderLength;
        float backboardWidth = shootingBarZoneController.backboardZoneSize * sliderLength;
        backboardZoneImage.rectTransform.anchoredPosition = new Vector2(backboardPosX, 0f);
        backboardZoneImage.rectTransform.sizeDelta = new Vector2(backboardWidth, 0f);
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
        scoreText.text = $"Score\n{PlayerScore}";
    }

    public void UpdateTimer(float currentTime)
    {
        timerText.text = $"{currentTime:0.00}s";
    }

    // Start is called before the first frame update
    void Start()
    {
        perfectZoneImage.color = normalPerfectZoneColor;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
