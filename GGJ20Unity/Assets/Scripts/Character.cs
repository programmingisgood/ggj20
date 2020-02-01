using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField]
    private Animator animator = null;
    
    [SerializeField]
    private CharacterStatus status = null;
    
    [SerializeField]
    private Transform rendererTrans = null;
    
    [SerializeField]
    private float repairRate = 10f;
    
    private bool selected = false;
    private bool moving = false;
    private bool repairing = false;
    private Machine targetMachine = null;
    private Vector3 initialLocalScale = default;
    
    void Start()
    {
        initialLocalScale = rendererTrans.localScale;
    }
    
    public float GetRepairRate()
    {
        return repairRate;
    }
    
    public void SetSelected(bool setSelected)
    {
        selected = setSelected;
        UpdateAnimationState();
    }
    
    public bool GetSelected()
    {
        return selected;
    }
    
    public void SetMoving(bool setMoving)
    {
        moving = setMoving;
        UpdateAnimationState();
    }
    
    public void SetRepairing(bool setRepairing)
    {
        repairing = setRepairing;
        UpdateAnimationState();
    }
    
    public void SetTargetMachine(Machine setTargetMachine)
    {
        targetMachine = setTargetMachine;
        
        // Determine which side of this character the machine is on.
        Vector3 toMachine = (setTargetMachine.transform.position - transform.position).normalized;
        Vector3 side = transform.right;
        bool faceLeft = Vector3.Dot(side, toMachine) < 0f;
        
        // Face toward this machine.
        rendererTrans.localScale = new Vector3(initialLocalScale.x * (faceLeft ? -1f : 1f), initialLocalScale.y, initialLocalScale.z);
    }
    
    public Machine GetTargetMachine()
    {
        return targetMachine;
    }
    
    private void UpdateAnimationState()
    {
        animator.SetBool("moving", moving);
        //animator.SetBool("repairing", repairing);
        
        status.SetSelected(selected);
        status.SetRepairing(repairing);
    }
}
