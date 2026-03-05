using UnityEngine;

public class GameAudioController : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource fireLoopSource;
    [SerializeField] private AudioSource tictacSource;

    // Menu Scene

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

    // BasketballGame Scene
    [Header("Basketball Game sounds")]
    [SerializeField] private AudioClip netNormalSound;
    [SerializeField] private AudioClip netPerfectSound;
    [SerializeField] private AudioClip niceShotSound;
    [SerializeField] private AudioClip rimBounceSound;
    [SerializeField] private AudioClip backboardBounceSound;
    [SerializeField] private AudioClip floorBounceSound;
    [SerializeField] private AudioClip playerMoanSound;
    [SerializeField] private AudioClip tictacSound;

    [Header("Fire Game sounds")]
    [SerializeField] private AudioClip fireSound;
    [SerializeField] private AudioClip fireOverSound;

    [Header("Music")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip basketGameBackgroundSound;

    [Header("Volume")]
    [SerializeField][Range(0f, 1f)] private float sfxVolume = 0.8f;
    [SerializeField][Range(0f, 1f)] private float musicVolume = 0.4f;



    public static GameAudioController instance { get; private set; }

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

    // Game Result
    public void PlayWinSound() => Play(winSound);
    public void PlayLoseDrawSound() => Play(loseDrawSound);

    // Lootbox
    public void PlayOpenLootboxSound() => Play(openLootboxSound);
    public void PlayCardFlipSound() => Play(cardFlipSound);

    // Basketball Game SFX
    public void PlayNetNormalSound() => Play(netNormalSound);
    public void PlayNetPerfectSound() => Play(netPerfectSound);
    public void PlayNiceShotSound() => Play(niceShotSound);
    public void PlayRimBounceSound() => Play(rimBounceSound);
    public void PlayBackboardBounceSound() => Play(backboardBounceSound);
    public void PlayFloorBounceSound() => Play(floorBounceSound);
    public void PlayPlayerMoanSound() => Play(playerMoanSound);

    // Tic Tac
    public void PlayTictacSound()
    {
        if (tictacSound == null || tictacSource == null) return;
        if (tictacSource.isPlaying) return;
        tictacSource.clip = tictacSound;
        tictacSource.loop = false;
        tictacSource.Play();
    }

    public void StopTictacSound()
    {
        if (tictacSource == null) return;
        tictacSource.Stop();
    }

    // Fire
    public void PlayFireSound()
    {
        if (fireSound == null || fireLoopSource == null) return;
        fireLoopSource.clip = fireSound;
        fireLoopSource.loop = true;
        fireLoopSource.Play();
    }

    public void PlayFireOverSound()
    {
        StopFireSound();
        Play(fireOverSound);
    }

    public void StopFireSound()
    {
        if (fireLoopSource != null)
        {
            fireLoopSource.Stop();
            fireLoopSource.loop = false;
        }
    }

    // — Basketball Game Music —
    public void PlayMenuMusic()
    {
        SwitchMusic(backgroundMusic);
    }

    public void PlayGameMusic()
    {
        SwitchMusic(basketGameBackgroundSound);
    }

    private void SwitchMusic(AudioClip clip)
    {
        if (clip == null || musicSource == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;
        musicSource.clip = clip;
        musicSource.Play();
    }

    // Music
    public void PlayBackgroundMusic()
    {
        if (backgroundMusic == null || musicSource == null) return;
        if (musicSource.clip == backgroundMusic && musicSource.isPlaying) return;
        musicSource.clip = backgroundMusic;
        musicSource.Play();
    }

    public void StopBackgroundMusic() => musicSource.Stop();

    public void StopAllSFX()
    {
        if (sfxSource != null) sfxSource.Stop();
        if (fireLoopSource != null) fireLoopSource.Stop();
        if (tictacSource != null) tictacSource.Stop();
    }

    public void PauseMusic()
    {
        if (musicSource == null) return;
        musicSource.Pause();
    }

    public void ResumeMusic()
    {
        if (musicSource == null) return;
        musicSource.UnPause();
    }

    // General sound play method
    private void Play(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }
}