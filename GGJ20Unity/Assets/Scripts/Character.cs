using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour, IOutlined
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
    private bool outlined = false;
    private Machine targetMachine = null;
    private Vector3 moveToPoint = default;
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
        UpdateRenderState();
    }
    
    public bool GetSelected()
    {
        return selected;
    }
    
    public void SetMoving(bool setMoving)
    {
        moving = setMoving;
        UpdateRenderState();
    }
    
    public void SetRepairing(bool setRepairing)
    {
        repairing = setRepairing;
        UpdateRenderState();
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
    
    public void SetMoveToPoint(Vector3 setMoveToPoint)
    {
        moveToPoint = setMoveToPoint;
    }
    
    public Vector3 GetMoveToPoint()
    {
        return moveToPoint;
    }
    
    public void SetOutlined(bool setOutlined)
    {
        outlined = setOutlined;
        UpdateRenderState();
    }
    
    private void UpdateRenderState()
    {
        animator.SetBool("moving", moving);
        //animator.SetBool("repairing", repairing);
        
        status.SetSelected(selected);
        status.SetRepairing(repairing);
        
        GetComponentInChildren<cakeslice.Outline>().enabled = outlined || selected;
        GetComponentInChildren<cakeslice.Outline>().color = selected ? 1 : 0;
    }
}
