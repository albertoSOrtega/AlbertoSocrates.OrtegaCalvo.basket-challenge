using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CardController : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite cardBackSprite;   // initial sprite
    [SerializeField] private Sprite cardFrontSprite;  // sprite to reveal
    [Header("Flip Configuration")]
    [SerializeField] private float flipHalfDuration = 0.2f;
    [SerializeField] private Ease flipEase = Ease.InOutSine;

    // References
    private Image cardImage;

    // State
    private bool isFlipped = false;
    private bool isFlipping = false;

    private void Awake()
    {
        cardImage = GetComponent<Image>();
        cardImage.sprite = cardBackSprite;
    }

    public void OnCardPressed()
    {
        if (isFlipping || isFlipped) return;
        FlipCard();
    }

    private void FlipCard()
    {
        isFlipping = true;

        // first half: scale X from 1 to 0
        transform.DOScaleX(0f, flipHalfDuration)
            .SetEase(flipEase)
            .OnComplete(() =>
            {
                // change sprite in the middle of the animation
                cardImage.sprite = cardFrontSprite;

                // second half: scale X from 0 to 1
                transform.DOScaleX(1f, flipHalfDuration)
                    .SetEase(flipEase)
                    .OnComplete(() =>
                    {
                        isFlipped = true;
                        isFlipping = false;
                    });
            });
    }

    public void ResetCard()
    {
        transform.DOKill();
        isFlipped = false;
        isFlipping = false;
        transform.localScale = Vector3.one;
        cardImage.sprite = cardBackSprite;
    }
}