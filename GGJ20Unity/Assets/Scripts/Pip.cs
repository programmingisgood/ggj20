using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pip : MonoBehaviour
{
    [SerializeField]
    private Image icon = null;
    
    [SerializeField]
    private Color brokenColor = default;
    
    [SerializeField]
    private Color workingColor = default;
    
    public void SetBroken(bool setBroken)
    {
        icon.color = setBroken ? brokenColor : workingColor;
    }
}
