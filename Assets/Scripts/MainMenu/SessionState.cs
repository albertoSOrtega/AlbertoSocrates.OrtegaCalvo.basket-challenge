using UnityEngine;

public class SessionState : MonoBehaviour
{
    public static SessionState I { get; private set; }

    [SerializeField] private CurrentSessionCurrencySO defaultCurrency;
    [SerializeField] private DailyMissionsSO defaultDaily;

    public CurrentSessionCurrencySO currency { get; private set; }
    public DailyMissionsSO daily { get; private set; }

    private void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        currency = Instantiate(defaultCurrency);
        daily = Instantiate(defaultDaily);
    }
}