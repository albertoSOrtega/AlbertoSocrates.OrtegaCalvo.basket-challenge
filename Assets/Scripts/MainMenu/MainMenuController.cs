using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI goldText;

    [Header("SO")]
    [SerializeField] private CurrentSessionCurrencySO currentSessionCurrencySO;

    public BagSlotController SelectedSlot { get; set; }

    private void OnEnable()
    {
        var currency = SessionState.I.currency;
        moneyText.text = currency.money.ToString();
        goldText.text = currency.gold.ToString();
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
