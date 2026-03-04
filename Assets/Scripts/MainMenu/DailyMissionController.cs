using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyMissionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI currencyText;
    [SerializeField] private List<BagSlotController> bags;
    [SerializeField] private Slider dailyMissionSlider;
    [SerializeField] private Slider dailyMissionsGeneralSlider;
    [SerializeField] private TextMeshProUGUI dailyMissionSliderText;
    [SerializeField] private TextMeshProUGUI dailyMissionsGeneralSliderText;
    [SerializeField] private DailyMissionsSO currentDailyMissionsSO;
    [SerializeField] private Button dailyMissionButton;
    [SerializeField] private GameObject rewardImage;
    [SerializeField] private CurrentSessionCurrencySO currentSessionCurrencySO;

    [Header("Daily Mission Configuration")]
    [SerializeField] private int currencyAmount;
    [SerializeField] private int missionIndex;
    [SerializeField] private bool isMoney;


    private void OnEnable()
    {
        var daily = SessionState.I.daily;

        dailyMissionButton.enabled = false;

        if (daily.missionsDone[missionIndex])
        {
            dailyMissionSlider.value = 1f;
            dailyMissionSliderText.text = "1/1";

            if (!daily.rewardsClaimed[missionIndex])
            {
                dailyMissionButton.enabled = true;
                rewardImage.SetActive(true);
            }
            else
            {
                rewardImage.SetActive(false);
            }
        }
    }

    public void OnRewardClicked()
    {
        var daily = SessionState.I.daily;
        var currency = SessionState.I.currency;

        daily.rewardsClaimed[missionIndex] = true;

        if (isMoney)
        {
            currency.money += currencyAmount;
            currencyText.text = currency.money.ToString();
        }
        else
        {
            currency.gold += currencyAmount;
            currencyText.text = currency.gold.ToString();
        }

        int count = 0;
        foreach (bool val in daily.rewardsClaimed)
            if (val) count++;

        float ratioMissionsDone = (float)count / 3f;
        dailyMissionsGeneralSlider.value = ratioMissionsDone;
        dailyMissionsGeneralSliderText.text = $"{count}/3";

        if (ratioMissionsDone == 1f)
        {
           ActivateBag();
        }

        dailyMissionButton.enabled = false;
        rewardImage.SetActive(false);
    }

    private void ActivateBag()
    {

        var daily = SessionState.I.daily;
        daily.bagRewardClaimed = true;

        foreach (BagSlotController bag in bags)
        {
            if (!bag.IsActive())
            {
                bag.ActivateBagSlot();
                break;
            }
        }
    }

}
