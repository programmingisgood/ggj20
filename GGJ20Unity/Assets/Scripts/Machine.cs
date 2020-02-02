using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Machine : MonoBehaviour, IOutlined
{
    private const float MachineVolumeChangeRate = 0.5f;
    
    // The number of seconds this machine will run peacefully before starting to break.
    private const float RepairBreakageSafetyBuffer = 12f;
    
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
    
    [SerializeField, Tooltip("How many repair units break per second after being repaired.")]
    private float breakRate = 1f;
    
    private bool broken = true;
    private float currentRepairLevel = 0f;
    // A buffer where the machine will not start to break.
    private float breakageSafetyBufferRemaining = 0f;
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
            breakageSafetyBufferRemaining = RepairBreakageSafetyBuffer;
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
    
    public bool GetOutlined()
    {
        return outlined;
    }
    
    private void Update()
    {
        if (!broken)
        {
            breakageSafetyBufferRemaining = Mathf.Max(0f, breakageSafetyBufferRemaining - Time.deltaTime);
            if (breakageSafetyBufferRemaining == 0f)
            {
                currentRepairLevel -= breakRate * Time.deltaTime;
                if (currentRepairLevel <= 0f)
                {
                    currentRepairLevel = 0f;
                    broken = true;
                }
                UpdateRenderState();
            }
        }
        
        float volumeDir = broken ? -1f : 1f;
        audioSource.volume = broken ? 0f : 1f;//Mathf.Clamp(audioSource.volume + volumeDir * Time.deltaTime * MachineVolumeChangeRate, 0f, 1f);
    }
    
    private void UpdateRenderState()
    {
        //animator.SetBool("broken", broken);
        
        status.SetStatus(broken, currentRepairLevel, repairNeeded);
        GetComponentInChildren<cakeslice.Outline>().enabled = outlined;
    }
}
