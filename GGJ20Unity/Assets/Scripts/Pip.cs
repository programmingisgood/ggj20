using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pip : MonoBehaviour
{
    public enum State
    {
        Working,
        Breaking,
        Broken
    };
    
    [SerializeField]
    private Image icon = null;
    
    [SerializeField]
    private Color brokenColor = default;
    
    [SerializeField]
    private Color workingColor = default;
    
    [SerializeField]
    private Color breakingColor = default;
    
    public void SetState(State setState)
    {
        switch (setState)
        {
            case State.Working:
                icon.color = workingColor;
                break;
            
            case State.Breaking:
                icon.color = breakingColor;
                break;
            
            case State.Broken:
                icon.color = brokenColor;
                break;
        }
    }
}
