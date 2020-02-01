using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Machine : MonoBehaviour, IOutlined
{
    private const float MachineVolumeChangeRate = 0.5f;
    
    [SerializeField]
    private Animator animator = null;
    
    [SerializeField]
    private MachineStatus status = null;
    
    [SerializeField]
    private Transform moveToPoint = null;
    
    [SerializeField]
    private AudioSource audioSource = null;
    
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
    
    public void SetOutlined(bool setOutlined)
    {
        outlined = setOutlined;
        UpdateRenderState();
    }
    
    private void Update()
    {
        float volumeDir = broken ? -1f : 1f;
        audioSource.volume = broken ? 0f : 1f;//Mathf.Clamp(audioSource.volume + volumeDir * Time.deltaTime * MachineVolumeChangeRate, 0f, 1f);
    }
    
    private void UpdateRenderState()
    {
        //animator.SetBool("broken", broken);
        
        status.SetBroken(broken);
        status.SetRepair(currentRepairLevel, repairNeeded);
        GetComponentInChildren<cakeslice.Outline>().enabled = outlined;
    }
}
