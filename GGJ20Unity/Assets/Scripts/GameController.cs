using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private const float AtTargetDistance = 1f;
    private const float HammerVolumeChangeRate = 0.5f;
    private const float RepelDistance = 1f;
    
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
    private AudioSource selectedAudioSource = null;
    
    [SerializeField]
    private AudioSource confirmAudioSource = null;
    
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
            GameObject hitObject = FindObjectUnderMouse();
            if (hitObject != null)
            {
                if (hitObject.tag == "Character")
                {
                    ClearSelection();
                    Character hitChar = hitObject.GetComponentInParent<Character>();
                    hitChar.SetSelected(true);
                    selectedAudioSource.Play();
                }
                else if (hitObject.tag == "Machine")
                {
                    Machine hitMachine = hitObject.GetComponentInParent<Machine>();
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
        RepelCharacters();
    }
    
    private void UpdateOutlines()
    {
        // Assume nothing is outlined by default.
        if (outlined != null)
        {
            outlined.SetOutlined(false);
        }
        
        GameObject hitObject = FindObjectUnderMouse();
        if (hitObject != null)
        {
            outlined = hitObject.GetComponentInParent<IOutlined>();
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
    
    private GameObject FindObjectUnderMouse()
    {
        GameObject returnHit = null;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray);
        
        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.gameObject.tag == "Character")
            {
                // Character is always the priority.
                return hit.transform.gameObject;
            }
            else if (hit.transform.gameObject.tag == "Machine")
            {
                returnHit = hit.transform.gameObject;
            }
        }
        
        return returnHit;
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
                character.SetMoving(!atMachine, targetMachine.transform.position);
                if (atMachine)
                {
                    if (targetMachine.GetIsBroken())
                    {
                        float repairAmount = character.GetRepairRate() * Time.deltaTime;
                        targetMachine.Repair(repairAmount);
                        if (!targetMachine.GetIsBroken())
                        {
                            // Check if this was the first time this specific machine was repaired.
                            if (!repairedMachines.Contains(targetMachine))
                            {
                                repairedMachines.Add(targetMachine);
                                // Reward with a new character.
                                numCharsToAdd++;
                            }
                            // Find all characters that are working on this machine and clear their target machine.
                            foreach (Character checkMachineChacacter in characters)
                            {
                                if (checkMachineChacacter.GetTargetMachine() == targetMachine)
                                {
                                    checkMachineChacacter.SetTargetMachine(null);
                                }
                            }
                        }
                        atLeastOneRepairerActive = true;
                        character.SetRepairing(atMachine);
                    }
                    else
                    {
                        character.SetRepairing(false);
                        character.SetMoving(false, Vector3.zero);
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
                    character.SetMoving(false, Vector3.zero);
                }
                else
                {
                    character.SetMoving(true, character.GetMoveToPoint());
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
    
    private void RepelCharacters()
    {
        foreach (Character character in characters)
        {
            Vector3 repelDir = Vector3.zero;
            // Check if this character is too close to another character and add to a repel direction.
            foreach (Character closeCharacter in characters)
            {
                if (character != closeCharacter)
                {
                    float dist = Vector3.Distance(character.transform.position, closeCharacter.transform.position);
                    if (dist <= RepelDistance)
                    {
                        repelDir += character.transform.position - closeCharacter.transform.position;
                    }
                }
            }
            
            repelDir.Normalize();
            character.transform.position += repelDir * Time.deltaTime * (charMoveSpeed / 2f);
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
                character.SetSelected(false);
                confirmAudioSource.Play();
            }
        }
    }
}
