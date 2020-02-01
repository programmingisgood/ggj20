using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Machine : MonoBehaviour, IOutlined
{
    [SerializeField]
    private Animator animator = null;
    
    [SerializeField]
    private MachineStatus status = null;
    
    [SerializeField]
    private Transform moveToPoint = null;
    
    [SerializeField]
    private float repairNeeded = 100f;
    
    private bool broken = true;
    private float currentRepairLevel = 0f;
    private bool outlined = false;
    
    void Start()
    {
        UpdateRenderState();
    }
    
    public void Repair(float repairAmount)
    {
        currentRepairLevel += repairAmount;
        if (currentRepairLevel >= repairNeeded)
        {
            currentRepairLevel = repairNeeded;
            broken = false;
        }
        
        UpdateRenderState();
    }
    
    public bool GetIsBroken()
    {
        return broken;
    }
    
    public float Distance(Character toCharacter)
    {
        return Vector3.Distance(moveToPoint.position, toCharacter.transform.position);
    }
    
    public Vector3 GetMoveToPoint()
    {
        return moveToPoint.position;
    }
    
    private void UpdateRenderState()
    {
        //animator.SetBool("broken", broken);
        
        status.SetBroken(broken);
        status.SetRepair(currentRepairLevel, repairNeeded);
        GetComponentInChildren<cakeslice.Outline>().enabled = outlined;
    }
    
    public void SetOutlined(bool setOutlined)
    {
        outlined = setOutlined;
        UpdateRenderState();
    }
}
