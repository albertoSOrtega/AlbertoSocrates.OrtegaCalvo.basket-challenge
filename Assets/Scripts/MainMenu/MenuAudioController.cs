using UnityEngine;

public class MenuAudioController : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("UI Sounds")]
    [SerializeField] private AudioClip backSound;
    [SerializeField] private AudioClip confirmSound;
    [SerializeField] private AudioClip cancelSound;
    [SerializeField] private AudioClip firstTapSound; // Initial Screen
    [SerializeField] private AudioClip popupSlideInSound;
    [SerializeField] private AudioClip popupSlideOutSound;
    [SerializeField] private AudioClip redeemDailyMissionSound;

    [Header("Game Result Sounds")]
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip loseDrawSound;

    [Header("Lootbox Sounds")]
    [SerializeField] private AudioClip openLootboxSound;
    [SerializeField] private AudioClip cardFlipSound;

    [Header("Music")]
    [SerializeField] private AudioClip backgroundMusic;

    [Header("Volume")]
    [SerializeField][Range(0f, 1f)] private float sfxVolume = 0.8f;
    [SerializeField][Range(0f, 1f)] private float musicVolume = 0.4f;

    public static MenuAudioController instance { get; private set; }

    private void Awake()
    {
        // Don't destroy between scenes
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource.volume = musicVolume;
        musicSource.loop = true;
    }

    private void Start()
    {
        PlayBackgroundMusic();
    }

    // UI
    public void PlayBackSound() => Play(backSound);
    public void PlayConfirmSound() => Play(confirmSound);
    public void PlayCancelSound() => Play(cancelSound);
    public void PlayFirstTapSound() => Play(firstTapSound);
    public void PlayPopupSlideInSound() => Play(popupSlideInSound);
    public void PlayPopupSlideOutSound() => Play(popupSlideOutSound);
    public void PlayRedeemDailyMissionSound() => Play(redeemDailyMissionSound);

    // Game Resul
    public void PlayWinSound() => Play(winSound);
    public void PlayLoseDrawSound() => Play(loseDrawSound);

    // Lootbox
    public void PlayOpenLootboxSound() => Play(openLootboxSound);
    public void PlayCardFlipSound() => Play(cardFlipSound);

    // Music
    public void PlayBackgroundMusic()
    {
        if (backgroundMusic == null || musicSource == null) return;
        if (musicSource.clip == backgroundMusic && musicSource.isPlaying) return;
        musicSource.clip = backgroundMusic;
        musicSource.Play();
    }

    public void StopBackgroundMusic() => musicSource.Stop();

    // General sound play method
    private void Play(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }
}