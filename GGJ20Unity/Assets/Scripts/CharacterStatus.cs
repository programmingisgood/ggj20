using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterStatus : MonoBehaviour
{
    [SerializeField]
    private Image icon = null;
    
    [SerializeField]
    private Sprite selectedSprite = null;
    
    [SerializeField]
    private Sprite repairingSprite = null;
    
    private bool selected = false;
    private bool repairing = false;
    
    public void SetSelected(bool setSelected)
    {
        selected = setSelected;
        UpdateUI();
    }
    
    public void SetRepairing(bool setRepairing)
    {
        repairing = setRepairing;
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        gameObject.SetActive(selected || repairing);
        
        if (repairing)
        {
            icon.sprite = repairingSprite;
        }
        else if (selected)
        {
            icon.sprite = selectedSprite;
        }
    }
}
