using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VictoryStatus : MonoBehaviour
{
    [SerializeField]
    private Image icon = null;
    
    [SerializeField]
    private TMP_Text text = null;
    
    public void SetStatus(bool active, int timeRemaining)
    {
        text.SetText(timeRemaining.ToString());
        text.color = active ? Color.green : Color.red;
        icon.color = active ? Color.green : Color.red;
    }
}
