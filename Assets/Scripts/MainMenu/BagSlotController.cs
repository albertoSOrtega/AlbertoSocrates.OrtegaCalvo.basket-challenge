using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BagSlotController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject bagImage;
    [SerializeField] private GameObject emptySlotText;

    // State
    [SerializeField] private bool isActive = false;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public bool IsActive()
    {
        return isActive;
    }

    public void ActivateBagSlot()
    {
        button.enabled = true;
        isActive = true;
        bagImage.SetActive(true);
        emptySlotText.SetActive(false);
    }

    public void DeactivateBagSlot()
    {
        button.enabled = false;
        isActive = false;
        bagImage.SetActive(false);
        emptySlotText.SetActive(true);
    }

}
