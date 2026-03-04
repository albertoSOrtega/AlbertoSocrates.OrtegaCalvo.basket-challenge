using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class MatchResultsPanelController : MonoBehaviour
{
    [Header("ScriptableObject")]
    [SerializeField] private MatchResultSO matchResult;
    [SerializeField] private SelectedDifficultySO selectedDifficultySO;
    //[SerializeField] private DailyMissionsSO currentDailyMissionsSO;
    //[SerializeField] private CurrentSessionCurrencySO currentSessionCurrencySO;

    [Header("Score UI References")]
    [SerializeField] private TextMeshProUGUI playerScoreText;
    [SerializeField] private TextMeshProUGUI cpuScoreText;

    [Header("Result Images")]
    [SerializeField] private GameObject playerWinnerImage;
    [SerializeField] private GameObject cpuWinnerImage;
    [SerializeField] private GameObject drawImage;

    [Header("Entrance Animation")]
    [SerializeField] private float animationDelay = 0.3f;
    [SerializeField] private float animationDuration = 0.5f;

    [Header("Rewards Reference")]
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private List<BagSlotController> bags;
    [SerializeField] private TextMeshProUGUI rewardMoneyText;
    [SerializeField] private GameObject rewardBag;

    [Header("Other References")]
    [SerializeField] private GameObject mainMenuRef;

    private void OnEnable()
    {
        if (matchResult == null || !matchResult.hasResult) return;
        DisplayResult();
    }

    private void DisplayResult()
    {
        var daily = SessionState.I.daily;
        var currency = SessionState.I.currency;

        // Scores
        playerScoreText.text = matchResult.playerScore.ToString();
        cpuScoreText.text = matchResult.cpuScore.ToString();

        UpdateRewards();

        moneyText.text = currency.money.ToString();
        goldText.text = currency.gold.ToString();

        playerWinnerImage.SetActive(false);
        cpuWinnerImage.SetActive(false);
        drawImage.SetActive(false);

        // Activate correct image
        if (matchResult.playerScore > matchResult.cpuScore)
            AnimateWinnerImage(playerWinnerImage);
        else if (matchResult.playerScore < matchResult.cpuScore)
            AnimateWinnerImage(cpuWinnerImage);
        else
            AnimateWinnerImage(drawImage);
    }

    private void AnimateWinnerImage(GameObject imageObject)
    {
        imageObject.SetActive(true);

        Transform imageTransform = imageObject.transform;
        imageTransform.localScale = Vector3.zero;

        imageTransform
            .DOScale(1f, animationDuration)
            .SetDelay(animationDelay)
            .SetEase(Ease.OutBack);
    }

    private void UpdateRewards()
    {
        var daily = SessionState.I.daily;
        var currency = SessionState.I.currency;

        GameDifficultyConfigSO selectedGameDifficulty = selectedDifficultySO.config;

        rewardMoneyText.text = selectedGameDifficulty.moneyReward.ToString();

        currency.money += selectedGameDifficulty.moneyReward;
        moneyText.text = currency.money.ToString();

        // Enable main menu momentarily to ensure bag reward is displayed if applicable, then disable it again to show the results panel
        mainMenuRef.SetActive(true);
        rewardBag.SetActive(selectedGameDifficulty.bagReward ? true : false);
        mainMenuRef.SetActive(false);

        if (selectedGameDifficulty.bagReward)
        {
            foreach (BagSlotController bag in bags)
            {
                if (!bag.IsActive())
                {
                    bag.ActivateBagSlot();
                    break;
                }
            }
        }

        // Daily mission section
        for (int i = 0; i < daily.missionsDone.Count; i++)
        {
            if (!daily.missionsDone[i])
            {
                daily.missionsDone[i] = true;
                break;
            }
        }
    }
}