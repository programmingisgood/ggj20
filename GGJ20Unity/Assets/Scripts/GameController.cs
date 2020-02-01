using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private const float AtTargetDistance = 1f;
    
    [SerializeField]
    private Camera mainCamera = null;
    
    [SerializeField]
    private GameObject characterPrefab = null;
    
    [SerializeField]
    private float charMoveSpeed = 1f;
    
    private List<Character> characters = new List<Character>();
    private List<Machine> machines = new List<Machine>();
    // The outlined object under the cursor.
    private IOutlined outlined = null;
    
    void Start()
    {
        GameObject newChar = Instantiate(characterPrefab);
        characters.Add(newChar.GetComponent<Character>());
        
        machines = new List<Machine>(FindObjectsOfType<Machine>());
        
        ClearSelection();
    }
    
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.gameObject.tag == "Character")
                {
                    ClearSelection();
                    Character hitChar = hit.transform.gameObject.GetComponentInParent<Character>();
                    hitChar.SetSelected(true);
                }
                else if (hit.transform.gameObject.tag == "Machine")
                {
                    Machine hitMachine = hit.transform.gameObject.GetComponentInParent<Machine>();
                    // Move selected characters to this machine.
                    MoveSelectedToMachine(hitMachine);
                }
            }
        }
        
        UpdateOutlines();
        
        UpdateCharacterMovement();
    }
    
    private void UpdateOutlines()
    {
        // Assume nothing is outlined by default.
        if (outlined != null)
        {
            outlined.SetOutlined(false);
        }
        
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            outlined = hit.transform.gameObject.GetComponentInParent<IOutlined>();
            if (outlined != null)
            {
                if (outlined is Machine && GetNumSelectedCharacters() > 0)
                {
                    outlined.SetOutlined(true);
                }
            }
        }
    }
    
    private void UpdateCharacterMovement()
    {
        foreach (Character character in characters)
        {
            Machine targetMachine = character.GetTargetMachine();
            if (targetMachine != null)
            {
                bool atMachine = targetMachine.Distance(character) <= AtTargetDistance;
                character.SetMoving(!atMachine);
                if (atMachine)
                {
                    if (targetMachine.GetIsBroken())
                    {
                        float repairAmount = character.GetRepairRate() * Time.deltaTime;
                        targetMachine.Repair(repairAmount);
                        character.SetRepairing(atMachine);
                    }
                    else
                    {
                        character.SetRepairing(false);
                    }
                }
                else
                {
                    character.transform.position += (targetMachine.GetMoveToPoint() - character.transform.position).normalized * Time.deltaTime * charMoveSpeed;
                }
            }
            else
            {
                character.SetMoving(false);
                character.SetRepairing(false);
            }
        }
    }
    
    private void ClearSelection()
    {
        foreach (Character character in characters)
        {
            character.SetSelected(false);
        }
    }
    
    private int GetNumSelectedCharacters()
    {
        int selected = 0;
        foreach (Character character in characters)
        {
            if (character.GetSelected())
            {
                selected++;
            }
        }
        return selected;
    }
    
    private void MoveSelectedToMachine(Machine machine)
    {
        foreach (Character character in characters)
        {
            if (character.GetSelected())
            {
                character.SetTargetMachine(machine);
            }
        }
    }
}
