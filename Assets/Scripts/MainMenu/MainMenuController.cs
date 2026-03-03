using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public BagSlotController SelectedSlot { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        SelectedSlot = null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
