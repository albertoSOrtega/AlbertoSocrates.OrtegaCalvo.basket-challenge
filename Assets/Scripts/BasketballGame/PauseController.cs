using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject pauseMenuUI_LS;
    [SerializeField] private GameObject overlayObject_LS;
    [SerializeField] private GameObject pauseMenuUI_PT;
    [SerializeField] private GameObject overlayObject_PT;
    [SerializeField] private ThrowBallInputHandler throwBallInputHandler;

    [Header("Overlay Configuration")]
    [SerializeField] private float overlayFadeDuration = 0.2f;
    [SerializeField][Range(0f, 1f)] private float overlayMaxAlpha = 0.6f;

    private GameObject pauseMenuUI;
    private GameObject overlayObject;

    // State
    private bool isPaused = false;

    // Input System device references
    private Keyboard keyboard;

    private void Awake()
    {
        bool isPortrait = Screen.height > Screen.width;
        pauseMenuUI = isPortrait ? pauseMenuUI_PT : pauseMenuUI_LS;
        overlayObject = isPortrait ? overlayObject_PT : overlayObject_LS;

        // Aseguramos estado inicial limpio
        CanvasGroup overlayCG = GetOrAddCanvasGroup(overlayObject);
        overlayCG.alpha = 0f;
        overlayObject.SetActive(false);
        pauseMenuUI.SetActive(false);
    }

    private void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChanged;
        RefreshDevices();
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChanged;

        // Safety net: si el objeto se desactiva con el juego pausado
        if (isPaused)
        {
            Time.timeScale = 1f;
            DOTween.PlayAll();
        }
    }

    private void Update()
    {
        // PC - Escape
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
            return;
        }

        // Android - back gesture / button
        #if UNITY_ANDROID
            if (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame)
            {
                TogglePause();
                return;
            }
        #endif
    }

    public void TogglePause()
    {
        if (isPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        DOTween.PauseAll();
        throwBallInputHandler.enabled = false;
        pauseMenuUI.SetActive(true);

        // Overlay fade in
        CanvasGroup overlayCG = GetOrAddCanvasGroup(overlayObject);
        overlayCG.alpha = 0f;
        overlayObject.SetActive(true);
        overlayCG.DOFade(overlayMaxAlpha, overlayFadeDuration)
                 .SetUpdate(true); 
    }

    public void Resume()
    {
        isPaused = false;
        pauseMenuUI.SetActive(false);

        // Overlay fade out
        CanvasGroup overlayCG = GetOrAddCanvasGroup(overlayObject);
        overlayCG.DOFade(0f, overlayFadeDuration)
                 .SetUpdate(true)
                 .OnComplete(() => overlayObject.SetActive(false));

        Time.timeScale = 1f;
        DOTween.PlayAll();
        throwBallInputHandler.enabled = true;
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        DOTween.KillAll();
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        DOTween.KillAll();
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private CanvasGroup GetOrAddCanvasGroup(GameObject go)
    {
        CanvasGroup cg = go.GetComponent<CanvasGroup>();
        if (cg == null) cg = go.AddComponent<CanvasGroup>();
        return cg;
    }

    private void RefreshDevices()
    {
        keyboard = Keyboard.current;
    }

    private void OnDeviceChanged(InputDevice device, InputDeviceChange change)
    {
        RefreshDevices();
    }
}