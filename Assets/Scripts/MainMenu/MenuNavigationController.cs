using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuNavigationController : MonoBehaviour
{

    // Panel configuration - This panels will be used for all the menus and submenus in the game, so we can easily change the configuration of all the panels by changing this configuration

    public enum MenuPanelType
    {
        InitialScreen,
        MainMenu,
        GameModeSelector,
        DailyMissions,
        LootboxOpener,
        Results
    }

    public enum SlideDirection
    {
        Left, Right, Up, Down
    }

    [System.Serializable]
    public class MenuPanel
    {
        public MenuPanelType panelType;
        public GameObject panelObject;
        public SlideDirection slideFrom = SlideDirection.Right;

        [Header("Transition Overrides - 0 to use global defaults")]
        [Min(0f)] public float durationOverride = 0f;
        [Min(0f)] public float slideDistanceOverride = 0f;
    }

    [Header("Panel Configuration")]
    [SerializeField] private List<MenuPanel> landscapePanels;
    [SerializeField] private List<MenuPanel> portraitPanels;
    [SerializeField] private MenuPanelType initialPanel = MenuPanelType.InitialScreen;

    [Header("Default Panel Transition Configuration")]
    [SerializeField] private float defaultDuration = 0.35f;
    [SerializeField] private float defaultSlideDistance = 800f;
    [SerializeField] private Ease defaultSlideEase = Ease.OutCubic;
    [SerializeField] private Ease defaultFadeEase = Ease.OutCubic;

    [Header("References")]
    [SerializeField] private MenuAudioController audioController;

    // state
    private List<MenuPanel> panels; // Reference to the currently active panel configuration (landscape or portrait) based on device orientation
    private Stack<MenuPanelType> navigationStack = new Stack<MenuPanelType>(); // Navigation stack — enables automatic Back behaviour and Android back gesture
    private MenuPanelType currentPanel;
    private bool isTransitioning = false;

    // Input System device reference
    private Keyboard keyboard;

    // Built once in Awake for O(1) panel access during transitions
    private Dictionary<MenuPanelType, MenuPanel> panelAccessDict;

    // Singleton - any button in the scene can reach the controller without a reference
    public static MenuNavigationController instance;


    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        panels = Screen.height > Screen.width ? portraitPanels : landscapePanels;

        BuildAccessDict();
        HideAllPanels();
    }

    private void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChanged;
        RefreshDevices();
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChanged;
    }

    private void Start()
    {
        // Show the initial panel immediately, with no transition
        ShowImmediate(initialPanel);
        currentPanel = initialPanel;
    }

    private void Update()
    {
        HandleBackInput();
    }

    // Input handling for back navigation, supporting both PC (Escape key) and Android (back gesture / button)
    private void HandleBackInput()
    {
        // PC - New Input System - Escape key
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            NavigateBack();
            return;
        }

        // Android - New Input System - back gesture / button
        #if UNITY_ANDROID
            if (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame)
            {
                NavigateBack();
                return;
            }
        #endif
    }

    private void RefreshDevices()
    {
        keyboard = Keyboard.current;
    }

    private void OnDeviceChanged(InputDevice device, InputDeviceChange change)
    {
        RefreshDevices();
    }

    // Navigate to a new panel, pushing the current one onto the stack
    public void NavigateToNewPanel(MenuPanelType target)
    {
        if (isTransitioning || target == currentPanel) return;

        audioController?.PlayNavigationSound();
        navigationStack.Push(currentPanel);
        PerformTransition(currentPanel, target, true);
        currentPanel = target;
    }

    // Pop the stack and return to the previous panel
    public void NavigateBack()
    {
        if (isTransitioning || navigationStack.Count == 0) return;

        audioController?.PlayBackSound();
        MenuPanelType previousPanel = navigationStack.Pop();
        PerformTransition(currentPanel, previousPanel, false);
        currentPanel = previousPanel;
    }

    // Navigate without pushing to stack — used for Results -> MainMenu so the
    // player cannot back into the results screen
    public void NavigateAndClearStack(MenuPanelType targetPanel)
    {
        if (isTransitioning) return;

        audioController?.PlayNavigationSound();
        navigationStack.Clear();
        PerformTransition(currentPanel, targetPanel, false);
        currentPanel = targetPanel;
    }

    // Wrapper methods for UnityEvents in buttons — these can be assigned directly in the Inspector without needing a separate script for each button
    public void NavigateToMainMenu() => NavigateToNewPanel(MenuPanelType.MainMenu);
    public void NavigateToGameModeSelector() => NavigateToNewPanel(MenuPanelType.GameModeSelector);
    public void NavigateToDailyMissions() => NavigateToNewPanel(MenuPanelType.DailyMissions);
    public void NavigateToLootboxOpener() => NavigateToNewPanel(MenuPanelType.LootboxOpener);
    public void NavigateToResults() => NavigateToNewPanel(MenuPanelType.Results);
    public void NavigateAndClearStackMainMenu() => NavigateAndClearStack(MenuPanelType.MainMenu);


    // Performs Panel Trasitioning using DOTween for smooth sliding and fading. The "forward" parameter determines the direction of the slide based on the panel's configured SlideDirection.
    private void PerformTransition(MenuPanelType from, MenuPanelType to, bool forward)
    {
        isTransitioning = true;

        MenuPanel fromPanel = panelAccessDict[from];
        MenuPanel toPanel = panelAccessDict[to];

        RectTransform fromRect = fromPanel.panelObject.GetComponent<RectTransform>();
        RectTransform toRect = toPanel.panelObject.GetComponent<RectTransform>();
        CanvasGroup fromCG = GetOrAddCanvasGroup(fromPanel.panelObject);
        CanvasGroup toCG = GetOrAddCanvasGroup(toPanel.panelObject);

        // Resolve duration and distance - use panel override if set, else global default
        float duration = ResolveValue(toPanel.durationOverride, defaultDuration);
        float slideDistance = ResolveValue(toPanel.slideDistanceOverride, defaultSlideDistance);

        // Incoming panel starts offset in the slide direction
        Vector2 incomingStart = GetSlideVector(toPanel.slideFrom, forward) * slideDistance;
        // Outgoing panel exits in the opposite direction
        Vector2 outgoingEnd = -incomingStart;

        // Prepare incoming panel before activating it
        toRect.anchoredPosition = incomingStart;
        toCG.alpha = 0f;
        toPanel.panelObject.SetActive(true);

        // Kill any tweens already running on both panels
        fromRect.DOKill();
        toRect.DOKill();
        fromCG.DOKill();
        toCG.DOKill();

        // Outgoing panel - slide out + fade out
        fromRect.DOAnchorPos(outgoingEnd, duration).SetEase(defaultSlideEase);
        fromCG.DOFade(0f, duration).SetEase(defaultFadeEase);

        // Incoming panel - slide in + fade in
        toRect.DOAnchorPos(Vector2.zero, duration).SetEase(defaultSlideEase);
        toCG.DOFade(1f, duration).SetEase(defaultFadeEase)
            .OnComplete(() =>
            {
                // Deactivate outgoing panel and reset its state for next use
                fromPanel.panelObject.SetActive(false);
                fromRect.anchoredPosition = Vector2.zero;
                fromCG.alpha = 1f;
                isTransitioning = false;
            });
    }

    // Helpers
    private void BuildAccessDict()
    {
        panelAccessDict = new Dictionary<MenuPanelType, MenuPanel>();
        foreach (MenuPanel config in panels)
            panelAccessDict[config.panelType] = config;
    }

    private void HideAllPanels()
    {
        foreach (MenuPanel config in panels)
        {
            config.panelObject.SetActive(false);
            CanvasGroup cg = GetOrAddCanvasGroup(config.panelObject);
            cg.alpha = 1f;
        }
    }

    private void ShowImmediate(MenuPanelType panel)
    {
        MenuPanel config = panelAccessDict[panel];
        config.panelObject.SetActive(true);
        GetOrAddCanvasGroup(config.panelObject).alpha = 1f;
        config.panelObject.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }

    private CanvasGroup GetOrAddCanvasGroup(GameObject go)
    {
        CanvasGroup cg = go.GetComponent<CanvasGroup>();
        if (cg == null) cg = go.AddComponent<CanvasGroup>();
        return cg;
    }

    // Returns override if greater than zero, otherwise returns the global default
    private float ResolveValue(float overrideValue, float defaultValue)
    {
        return overrideValue > 0f ? overrideValue : defaultValue;
    }

    // Converts a SlideDirection to a normalized Vector2.
    // forward = true -> natural direction (Navigate to new panel)
    // forward = false -> reversed direction (Navigate back)
    private Vector2 GetSlideVector(SlideDirection direction, bool forward)
    {
        Vector2 v = direction switch
        {
            SlideDirection.Left => Vector2.left,
            SlideDirection.Right => Vector2.right,
            SlideDirection.Up => Vector2.up,
            SlideDirection.Down => Vector2.down,
            _ => Vector2.right
        };
        return forward ? v : -v;
    }
}