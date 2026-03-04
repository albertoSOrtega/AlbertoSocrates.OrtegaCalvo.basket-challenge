using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private Slider dailyMissionsSlider;
    [SerializeField] private TextMeshProUGUI dailyMissionsSliderText;

    public BagSlotController SelectedSlot { get; set; }

    private void OnEnable()
    {
        var currency = SessionState.I.currency;
        moneyText.text = currency.money.ToString();
        goldText.text = currency.gold.ToString();

        var daily = SessionState.I.daily;

        int count = 0;
        foreach (bool val in daily.rewardsClaimed)
            if (val) count++;

        float ratioMissionsDone = (float)count / 3f;
        dailyMissionsSlider.value = ratioMissionsDone;
        dailyMissionsSliderText.text = $"{count}/3";
    }

    // Start is called before the first frame update
    void Start()
    {
        SelectedSlot = null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
