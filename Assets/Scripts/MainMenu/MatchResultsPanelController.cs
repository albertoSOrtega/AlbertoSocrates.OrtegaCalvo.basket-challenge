using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MatchResultsPanelController : MonoBehaviour
{
    [Header("ScriptableObject")]
    [SerializeField] private MatchResultSO matchResult;

    [Header("Score UI References")]
    [SerializeField] private TextMeshProUGUI playerScoreText;
    [SerializeField] private TextMeshProUGUI cpuScoreText;

    [Header("Result Images")]
    [SerializeField] private GameObject playerWinnerImage;
    [SerializeField] private GameObject cpuWinnerImage;
    [SerializeField] private GameObject drawImage;

    [Header("Entrance Animation")]
    [SerializeField] private float animationDelay = 0.3f;
    [SerializeField] private float animationDuration = 0.5f;

    private void OnEnable()
    {
        if (matchResult == null || !matchResult.hasResult) return;
        DisplayResult();
    }

    private void DisplayResult()
    {
        // Scores
        playerScoreText.text = matchResult.playerScore.ToString();
        cpuScoreText.text = matchResult.cpuScore.ToString();

        playerWinnerImage.SetActive(false);
        cpuWinnerImage.SetActive(false);
        drawImage.SetActive(false);

        // Activate correct image
        if (matchResult.playerScore > matchResult.cpuScore)
            AnimateWinnerImage(playerWinnerImage);
        else if (matchResult.playerScore < matchResult.cpuScore)
            AnimateWinnerImage(cpuWinnerImage);
        else
            AnimateWinnerImage(drawImage);
    }

    private void AnimateWinnerImage(GameObject imageObject)
    {
        imageObject.SetActive(true);

        Transform imageTransform = imageObject.transform;
        imageTransform.localScale = Vector3.zero;

        imageTransform
            .DOScale(1f, animationDuration)
            .SetDelay(animationDelay)
            .SetEase(Ease.OutBack);
    }
}