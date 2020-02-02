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
    private Vector3 initialRendererTransPos = new Vector3(0f, 1f, 0f);
    private bool facingLeft = true;
    private bool facingForward = true;
    private Vector3 facingLeftScale = default;
    private Vector3 facingRightScale = default;
    private Vector3 repairScaleLeft = new Vector3(-1.6f, 3.8f, 1f);
    private Vector3 repairScaleRight = new Vector3(1.6f, 3.8f, 1f);
    private Vector3 repairOffset = new Vector3(0f, 1.9f, 0f);
    
    void Start()
    {
        Vector3 initLocalScale = rendererTrans.localScale;
        facingLeftScale = new Vector3(initLocalScale.x * -1f, initLocalScale.y, initLocalScale.z);
        facingRightScale = new Vector3(initLocalScale.x * 1f, initLocalScale.y, initLocalScale.z);
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
    
    public void SetMoving(bool setMoving, Vector3 movingToPoint)
    {
        moving = setMoving;
        
        if (moving)
        {
            // Determine which side of this character the machine is on.
            Vector3 toPoint = (movingToPoint - transform.position).normalized;
            Vector3 side = transform.right;
            facingLeft = Vector3.Dot(side, toPoint) < 0f;
            
            // Face toward this machine.
            rendererTrans.localScale = facingLeft ? facingLeftScale : facingRightScale;
            
            // Determine if we are moving toward the camera or away.
            Vector3 forward = transform.forward;
            facingForward = Vector3.Dot(forward, toPoint) < 0f;
        }
        
        UpdateRenderState();
    }
    
    public void SetRepairing(bool setRepairing)
    {
        repairing = setRepairing;
        UpdateRenderState();
    }
    
    public bool GetRepairing()
    {
        return repairing;
    }
    
    public void SetTargetMachine(Machine setTargetMachine)
    {
        targetMachine = setTargetMachine;
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
    
    public bool GetOutlined()
    {
        return outlined;
    }
    
    private void UpdateRenderState()
    {
        animator.SetBool("moving", moving);
        animator.SetBool("facing_forward", facingForward);
        animator.SetBool("repairing", repairing);
        
        status.SetSelected(selected);
        status.SetRepairing(repairing);
        
        GetComponentInChildren<cakeslice.Outline>().enabled = outlined || selected;
        GetComponentInChildren<cakeslice.Outline>().color = selected ? 1 : 0;
        //GetComponentInChildren<Renderer>().material.SetColor("Tint", selected ? Color.green : Color.yellow);
        
        if (repairing)
        {
            rendererTrans.localScale = facingLeft ? repairScaleLeft : repairScaleRight;
        }
        else
        {
            // Reset the offset when not repairing.
            rendererTrans.localPosition = initialRendererTransPos;
        }
    }
    
    private void LateUpdate()
    {
        if (repairing)
        {
            rendererTrans.localScale = facingLeft ? repairScaleLeft : repairScaleRight;
            rendererTrans.localPosition = repairOffset;
        }
    }
}
