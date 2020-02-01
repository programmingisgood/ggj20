using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MachineStatus : MonoBehaviour
{
    private const float PipsPerRepair = 5f;
    
    [SerializeField]
    private Image icon = null;
    
    [SerializeField]
    private Sprite workingSprite = null;
    
    [SerializeField]
    private Sprite brokenSprite = null;
    
    [SerializeField]
    private Color workingColor = default;
    
    [SerializeField]
    private Color brokenColor = default;
    
    [SerializeField]
    private GameObject pipPrefab = null;
    
    [SerializeField]
    private Transform pipsContainer = null;
    
    private List<Pip> pips = new List<Pip>();
    
    public void SetStatus(bool setBroken, float currentRepairLevel, float repairNeeded)
    {
        icon.sprite = setBroken ? brokenSprite : workingSprite;
        icon.color = setBroken ? brokenColor : workingColor;
        
        int numPips = (int) (repairNeeded / PipsPerRepair);
        while (pips.Count < numPips)
        {
            GameObject newPip = Instantiate(pipPrefab, pipsContainer);
            pips.Add(newPip.GetComponent<Pip>());
        }
        
        while (pips.Count > numPips)
        {
            Destroy(pips[pips.Count - 1].gameObject);
            pips.RemoveAt(pips.Count - 1);
        }
        
        float percentRepaired = currentRepairLevel / repairNeeded;
        int numRepairedPips = (int) (pips.Count * percentRepaired);
        for (int p = 0; p < pips.Count; p++)
        {
            bool pipWorking = p < numRepairedPips;
            Pip.State pipState = pipWorking ? Pip.State.Working : (setBroken ? Pip.State.Broken : Pip.State.Breaking);
            pips[p].SetState(pipState);
        }
    }
}
