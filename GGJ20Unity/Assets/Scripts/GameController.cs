using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private const float AtTargetDistance = 1f;
    private const float HammerVolumeChangeRate = 0.5f;
    
    [SerializeField]
    private Camera mainCamera = null;
    
    [SerializeField]
    private GameObject characterPrefab = null;
    
    [SerializeField]
    private Transform spawnPoint = null;
    
    [SerializeField]
    private Transform enterToPoint = null;
    
    [SerializeField]
    private AudioSource hammerAudioSource = null;
    
    [SerializeField]
    private float charMoveSpeed = 1f;
    
    private List<Character> characters = new List<Character>();
    private List<Machine> machines = new List<Machine>();
    // A list of machines that have been repaired at least once.
    private List<Machine> repairedMachines = new List<Machine>();
    // The outlined object under the cursor.
    private IOutlined outlined = null;
    
    void Start()
    {
        CharacterEntersFactory();
        
        machines = new List<Machine>(FindObjectsOfType<Machine>());
        
        ClearSelection();
    }
    
    private void CharacterEntersFactory()
    {
        GameObject newCharGO = Instantiate(characterPrefab, spawnPoint.position, Quaternion.identity);
        Character newChar = newCharGO.GetComponent<Character>();
        characters.Add(newChar);
        
        newChar.SetMoveToPoint(enterToPoint.position);
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
                    AssignSelectedTargetMachine(hitMachine);
                }
            }
            else
            {
                ClearSelection();
            }
        }
        
        UpdateOutlines();
        
        UpdateCharacterMovement();
        
        UpdateMachineBreaking();
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
                if (outlined is Machine)
                {
                    if (GetNumSelectedCharacters() > 0)
                    {
                        outlined.SetOutlined(true);
                    }
                }
                else
                {
                    outlined.SetOutlined(true);
                }
            }
        }
    }
    
    private void UpdateCharacterMovement()
    {
        bool atLeastOneRepairerActive = false;
        int numCharsToAdd = 0;
        
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
                        if (!targetMachine.GetIsBroken() && !repairedMachines.Contains(targetMachine))
                        {
                            repairedMachines.Add(targetMachine);
                            // Reward with a new character.
                            numCharsToAdd++;
                        }
                        atLeastOneRepairerActive = true;
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
                    character.SetRepairing(false);
                }
            }
            else
            {
                character.SetRepairing(false);
                if (Vector3.Distance(character.GetMoveToPoint(), character.transform.position) <= AtTargetDistance)
                {
                    character.SetMoving(false);
                }
                else
                {
                    character.transform.position += (character.GetMoveToPoint() - character.transform.position).normalized * Time.deltaTime * charMoveSpeed;
                }
            }
        }
        
        for (int i = 0; i < numCharsToAdd; i++)
        {
            CharacterEntersFactory();
        }
        
        float volumeDir = atLeastOneRepairerActive ? 1f : -1f;
        hammerAudioSource.volume = atLeastOneRepairerActive ? 1f : 0f;//hammerAudioSource.volume = Mathf.Clamp(hammerAudioSource.volume + volumeDir * Time.deltaTime * HammerVolumeChangeRate, 0f, 1f);
    }
    
    private void UpdateMachineBreaking()
    {
        foreach (Machine machine in machines)
        {
            
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
    
    private void AssignSelectedTargetMachine(Machine machine)
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
