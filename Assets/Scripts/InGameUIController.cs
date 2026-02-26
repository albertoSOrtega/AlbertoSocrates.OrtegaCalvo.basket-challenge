using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameUIController : MonoBehaviour
{
    [Header("References")]
    public Slider shootPowerSlider;
    public TextMeshProUGUI shootText;
    public ThrowBallInputHandler throwBallInputHandler;
    public Image perfectZoneImage;
    public ShootingBarZoneController shootingBarZoneController;
    public BallShooterController ballShooterController;

    [Header("Perfect Shooting Zone Colors")]
    public Color normalPerfectZoneColor = new Color(0.75f, 0.6f, 0f, 1f);  
    public Color inPerfectZoneColor = new Color(0.3f, 0.75f, 0f, 1f);

    private void OnEnable()
    {
        // Subscribe to the events from the ThrowBallInputHandler
        throwBallInputHandler.OnSwipeStarted += ResetSlider;
        throwBallInputHandler.OnShootPowerChanged += UpdateSlider;
        throwBallInputHandler.OnShootReleased += UIHandleShoot;
        throwBallInputHandler.OnSwipeCancelled += UIHandleCancelShoot;

        // Suscribe to the eevnts of the shootingBarZoneController to initialize the perfect zone rect when randomized
        shootingBarZoneController.OnShootingZonesInitialized += InitializePerfectZoneRect;

        // Subscribe to the events of the BallShooterController
        ballShooterController.OnShotCompleted += ResetAfterShot;
    }

    private void OnDisable()
    {
        // Unsubscribe to the events from the ThrowBallInputHandler
        throwBallInputHandler.OnSwipeStarted -= ResetSlider;
        throwBallInputHandler.OnShootPowerChanged -= UpdateSlider;
        throwBallInputHandler.OnShootReleased -= UIHandleShoot;
        throwBallInputHandler.OnSwipeCancelled -= UIHandleCancelShoot;

        // Unsubscribe to the eevnts of the shootingBarZoneController
        shootingBarZoneController.OnShootingZonesInitialized -= InitializePerfectZoneRect;

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
                shootText.text = $"Short Shot, You Failed!";
                ResetAfterShot(shotType);
                break;
            case ShotType.PerfectBackboard:
                shootText.text = $"Other Shot, You Failed!";
                ResetAfterShot(shotType);
                break;
            case ShotType.ImperfectBackboard:
                shootText.text = $"Other Shot, You Failed!";
                ResetAfterShot(shotType);
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
        UpdatePerfectZoneColor(shootPower);
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

    public void InitializePerfectZoneRect()
    {
        // Slider rotated 90º in Z axis, we use width instead of height
        float sliderLength = ((RectTransform)shootPowerSlider.transform).rect.width;
        float initialPosX = shootingBarZoneController.perfectZoneStart * sliderLength;
        float zoneWidth = shootingBarZoneController.perfectZoneSize * sliderLength;

        perfectZoneImage.rectTransform.anchoredPosition = new Vector2(initialPosX, 0f);
        perfectZoneImage.rectTransform.sizeDelta = new Vector2(zoneWidth, 0f);
    }

    public void UpdatePerfectZoneColor(float shootPower)
    {
        if (shootingBarZoneController.IsInPerfectZone(shootPower))
        {
            perfectZoneImage.color = inPerfectZoneColor;
        }
        else
        {
            perfectZoneImage.color = normalPerfectZoneColor;
        }
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
