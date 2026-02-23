using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameUIController : MonoBehaviour
{
    public Slider shootPowerSlider;
    public TextMeshProUGUI shootText;
    public ThrowBallInputHandler throwBallInputHandler;

    private void OnEnable()
    {
        // Subscribe to the events from the ThrowBallInputHandler
        throwBallInputHandler.OnSwipeStarted += ResetSlider;
        throwBallInputHandler.OnShootPowerChanged += UpdateSlider;
        throwBallInputHandler.OnShootReleased += UIHandleShoot;
        throwBallInputHandler.OnSwipeCancelled += UIHandleCancelShoot;
    }

    private void OnDisable()
    {
        // Unsubscribe to the events from the ThrowBallInputHandler
        throwBallInputHandler.OnSwipeStarted -= ResetSlider;
        throwBallInputHandler.OnShootPowerChanged -= UpdateSlider;
        throwBallInputHandler.OnShootReleased -= UIHandleShoot;
        throwBallInputHandler.OnSwipeCancelled -= UIHandleCancelShoot;
    }

    public void UIHandleShoot(float shootPower)
    {
        //shootText.text = $"Shooting with this power: {shootPower}";
        //shootText.DOFade(0, 2f);
    }

    public void UIHandleCancelShoot(float shootPower)
    {
        //shootText.text = $"Shoot cancelled with this power: {shootPower}";
    }

    public void UpdateSlider(float power)
    {
        shootPowerSlider.value = power;
    }

    public void ResetSlider()
    {
        shootPowerSlider.value = 0f;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
