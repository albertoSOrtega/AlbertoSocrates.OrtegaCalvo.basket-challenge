using UnityEngine;

public class MenuAudioController : MonoBehaviour
{
    [Header("Audio Source")]
    [SerializeField] private AudioSource audioSource;

    [Header("UI Sounds")]
    [SerializeField] private AudioClip navigationSound;
    [SerializeField] private AudioClip backSound;
    [SerializeField] private AudioClip confirmSound;
    [SerializeField] private AudioClip cancelSound;

    [Header("Volume")]
    [SerializeField][Range(0f, 1f)] private float uiVolume = 0.8f;

    public void PlayNavigationSound()
    {
        Play(navigationSound);
    }

    public void PlayBackSound()
    {
        Play(backSound);
    } 

    public void PlayConfirmSound()
    {
        Play(confirmSound);
    }

    public void PlayCancelSound()
    {
        Play(cancelSound);
    }

    private void Play(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;
        audioSource.PlayOneShot(clip, uiVolume);
    }
}