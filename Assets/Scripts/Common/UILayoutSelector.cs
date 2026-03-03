using UnityEngine;


// Activates the correct UI Canvas based on screen orientation at startup.
// Landscape -> PC/tablet horizontal.
// Portrait -> mobile vertical.
public class UILayoutSelector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject landscapeCanvas;
    [SerializeField] private GameObject portraitCanvas;

    private void Awake()
    {
        if (Screen.height > Screen.width)
        {
            landscapeCanvas.SetActive(false);
            portraitCanvas.SetActive(true);
        }
        else
        {
            landscapeCanvas.SetActive(true);
            portraitCanvas.SetActive(false);
        }
    }
}