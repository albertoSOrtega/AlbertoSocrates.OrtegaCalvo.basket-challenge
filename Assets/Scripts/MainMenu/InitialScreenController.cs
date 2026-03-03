using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InitialScreenController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform logo;
    [SerializeField] private CanvasGroup continueText;

    [Header("Blink Configuration")]
    [SerializeField] private float logoScaleDuration = 0.8f;
    [SerializeField] private float pulseScaleAmount = 1.15f;
    [SerializeField] private float pulseDuration = 0.6f;
    [SerializeField] private float textFadeDuration = 1.2f;

    private bool inputEnabled = false;

    private void OnEnable()
    {
        inputEnabled = false;

        // Scale logo to 0 and hide text
        logo.localScale = Vector3.zero;
        continueText.alpha = 0;

        Sequence introSequence = DOTween.Sequence();

        // Logo scales up with bounce effect
        introSequence.Append(logo.DOScale(1f, logoScaleDuration).SetEase(Ease.OutBack));

        // Once the entrance is done, start the infinite loops
        introSequence.OnComplete(() =>
        {
            inputEnabled = true;
            StartLoopAnimations();
        });
    }

    private void OnDisable()
    {
        DOTween.Kill(logo);
        DOTween.Kill(continueText);
    }

    private void Update()
    {
        if (!inputEnabled) return;

        // Any touch (mobile) or any mouse click (PC)
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            Advance();
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            Advance();
    }

    void StartLoopAnimations()
    {
        // Logo heart-beat effect
        logo.DOScale(pulseScaleAmount, pulseDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        // Text Fade In / Fade Out effect
        continueText.DOFade(1f, textFadeDuration)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void Advance()
    {
        inputEnabled = false;

        logo.DOKill();
        continueText.DOKill();

        Sequence finishSequence = DOTween.Sequence();

        // Logo "pops" to the pulse size quickly
        // Text snaps to full opacity (white/visible)
        finishSequence.Append(logo.DOScale(pulseScaleAmount, 0.1f).SetEase(Ease.OutQuad));
        finishSequence.Join(continueText.DOFade(1f, 0.1f));

        finishSequence.AppendInterval(0.4f);

        // Call the transition
        finishSequence.OnComplete(() =>
        {
            MenuNavigationController.instance.NavigateAndClearStackMainMenu();
        });
    }
}