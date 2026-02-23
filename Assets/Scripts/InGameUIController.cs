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
    //public RectTransform perfectZoneRectTransform;
    public Image perfectZoneImage;
    public PerfectZoneController perfectZoneController;

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

        // Suscribe to the eevnts of the PerfectZoneController to initialize the perfect zone rect when randomized
        perfectZoneController.OnPerfectZoneRandomized += InitializePerfectZoneRect;
    }

    private void OnDisable()
    {
        // Unsubscribe to the events from the ThrowBallInputHandler
        throwBallInputHandler.OnSwipeStarted -= ResetSlider;
        throwBallInputHandler.OnShootPowerChanged -= UpdateSlider;
        throwBallInputHandler.OnShootReleased -= UIHandleShoot;
        throwBallInputHandler.OnSwipeCancelled -= UIHandleCancelShoot;

        // Suscribe to the eevnts of the PerfectZoneController
        perfectZoneController.OnPerfectZoneRandomized -= InitializePerfectZoneRect;
    }

    public void UIHandleShoot(float shootPower)
    {
        shootText.alpha = 1f; // Reset alpha to fully visible
        shootText.text = $"Shooting with this power: {shootPower}";
        shootText.DOFade(0, 2f);
    }

    public void UIHandleCancelShoot(float shootPower)
    {
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

    public void InitializePerfectZoneRect()
    {
        // Slider rotated 90º in Z axis, we use width instead of height
        float sliderLength = ((RectTransform)shootPowerSlider.transform).rect.width;
        float initialPosX = perfectZoneController.perfectZoneStart * sliderLength;
        float zoneWidth = perfectZoneController.perfectZoneSize * sliderLength;

        perfectZoneImage.rectTransform.anchoredPosition = new Vector2(initialPosX, 0f);
        perfectZoneImage.rectTransform.sizeDelta = new Vector2(zoneWidth, 0f);
    }

    public void UpdatePerfectZoneColor(float shootPower)
    {
        if (perfectZoneController.IsInPerfectZone(shootPower))
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
