using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CurrentSessionCurrency", menuName = "BasketballGame/CurrentSessionCurrency")]
public class CurrentSessionCurrencySO : ScriptableObject
{
    [Header("Current Session Currency State")]
    public int money;
    public int gold;
}