using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class LootboxOpenerController : MonoBehaviour
{
    [Header("Bag References")]
    [SerializeField] private GameObject openBag;
    [SerializeField] private GameObject closedBag;

    [Header("Popup Reference")]
    [SerializeField] private GameObject lootboxRewardsPopup;
    [SerializeField] private GameObject overlayObject;

    [Header("Cards")]
    [SerializeField] private RectTransform[] cards; 

    [Header("Card Animation")]
    [SerializeField] private float cardAnimationDuration = 0.5f;
    [SerializeField] private Ease cardAnimationEase = Ease.OutBack;
    [SerializeField] private float delayBetweenCards = 0.08f;

    [Header("Other References")]
    [SerializeField] private GameObject mainMenuRef;

    private void OnEnable()
    {
        GetComponent<Button>().enabled = true;
        ResetBag();
        ResetCards();
    }

    public void OnLootboxPressed()
    {
        OpenBag();
        ShowRewardsPopup();
        GetComponent<Button>().enabled = false;
    }

    private void OpenBag()
    {
        closedBag.SetActive(false);
        openBag.SetActive(true);

        // remove the bag from the main menu
        mainMenuRef.SetActive(true);
        BagSlotController selectedSlot = mainMenuRef.GetComponent<MainMenuController>().SelectedSlot;
        selectedSlot.DeactivateBagSlot();
        mainMenuRef.SetActive(false);
    }

    private void ShowRewardsPopup()
    {
        GameAudioController.instance?.PlayOpenLootboxSound();
        MenuNavigationController.instance.OpenPopupInstant(MenuNavigationController.PopupType.LootboxRewards);
        AnimateCards();
    }

    private void AnimateCards()
    {
        for (int i = 0; i < cards.Length; i++)
        {
            RectTransform card = cards[i];

            CardController cardController = card.GetComponent<CardController>();
            cardController.ResetCard(); 

            card.localScale = Vector3.zero;
            card.DOKill();
            card.DOScale(Vector3.one, cardAnimationDuration)
                .SetDelay(i * delayBetweenCards)
                .SetEase(cardAnimationEase);
        }
    }

    private void ResetBag()
    {
        openBag.SetActive(false);
        closedBag.SetActive(true);
    }

    private void ResetCards()
    {
        foreach (RectTransform card in cards)
        {
            card.DOKill();
            card.localScale = Vector3.zero; 
        }
    }
}