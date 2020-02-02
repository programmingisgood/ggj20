using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private const float AtTargetDistance = 1f;
    private const float HammerVolumeChangeRate = 0.5f;
    private const float RepelDistance = 1f;
    private const int MaxTownsfolk = 5;
    private const float VictoryTime = 30f;
    
    [SerializeField]
    private Camera mainCamera = null;
    
    [SerializeField]
    private GameObject characterPrefab = null;
    
    [SerializeField]
    private Transform spawnPoint = null;
    
    [SerializeField]
    private List<Transform> enterToPoints = null;
    
    [SerializeField]
    private VictoryStatus victoryStatus = null;
    
    [SerializeField]
    private AudioSource hammerAudioSource = null;
    
    [SerializeField]
    private AudioSource selectedAudioSource = null;
    
    [SerializeField]
    private AudioSource confirmAudioSource = null;
    
    [SerializeField]
    private AudioSource entranceAudioSource = null;
    
    [SerializeField]
    private AudioSource highlightAudioSource = null;
    
    [SerializeField]
    private float charMoveSpeed = 1f;
    
    private List<Character> characters = new List<Character>();
    private List<Machine> machines = new List<Machine>();
    // A list of machines that have been repaired at least once.
    private List<Machine> repairedMachines = new List<Machine>();
    // The outlined object under the cursor.
    private IOutlined outlined = null;
    private float victoryTimeRemaining = 0f;
    
    void Start()
    {
        CharacterEntersFactory();
        
        machines = new List<Machine>(FindObjectsOfType<Machine>());
        victoryTimeRemaining = VictoryTime;
        
        ClearSelection();
    }
    
    private void CharacterEntersFactory()
    {
        if (characters.Count < MaxTownsfolk)
        {
            GameObject newCharGO = Instantiate(characterPrefab, spawnPoint.position, Quaternion.identity);
            Character newChar = newCharGO.GetComponent<Character>();
            characters.Add(newChar);
            
            Vector3 closestEnterPoint = FindClosestEnterPoint(newChar.transform.position);
            newChar.SetMoveToPoint(closestEnterPoint);
            
            entranceAudioSource.Play();
        }
    }
    
    void Update()
    {
        if (victoryTimeRemaining <= 0f)
        {
            return;
        }
        
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
        
        UpdateVictoryProgress();
    }
    
    private void UpdateOutlines()
    {
        IOutlined prevOutlined = outlined;
        
        GameObject hitObject = FindObjectUnderMouse();
        if (hitObject != null)
        {
            outlined = hitObject.GetComponentInParent<IOutlined>();
            if (outlined != null)
            {
                // We don't want to re-outline what is already outlined.
                if (outlined == prevOutlined)
                {
                    return;
                }
                
                if (outlined is Machine)
                {
                    if (GetNumSelectedCharacters() > 0 && !outlined.GetOutlined())
                    {
                        outlined.SetOutlined(true);
                        highlightAudioSource.Play();
                    }
                }
                else if (!outlined.GetOutlined())
                {
                    outlined.SetOutlined(true);
                    highlightAudioSource.Play();
                }
            }
            else
            {
                outlined = null;
            }
        }
        else
        {
            outlined = null;
        }
        
        // Assume nothing is outlined by default.
        if (prevOutlined != null)
        {
            prevOutlined.SetOutlined(false);
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
                // Only find characters if we don't have a character already selected.
                // And we cannot select characters that are busy repairing.
                if (GetNumSelectedCharacters() == 0 && !hit.transform.gameObject.GetComponentInParent<Character>().GetRepairing())
                {
                    // Character is always the priority.
                    return hit.transform.gameObject;
                }
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
                            foreach (Character checkMachineCharacter in characters)
                            {
                                if (checkMachineCharacter.GetTargetMachine() == targetMachine)
                                {
                                    checkMachineCharacter.SetTargetMachine(null);
                                    Vector3 closestEnterPoint = FindClosestEnterPoint(checkMachineCharacter.transform.position);
                                    checkMachineCharacter.SetMoveToPoint(closestEnterPoint);
                                }
                            }
                        }
                        atLeastOneRepairerActive = true;
                        character.SetRepairing(atMachine);
                        character.SetOutlined(false);
                        character.SetSelected(false);
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
    
    private void UpdateVictoryProgress()
    {
        bool allMachinesWorking = true;
        foreach (Machine machine in machines)
        {
            if (machine.GetIsBroken())
            {
                allMachinesWorking = false;
            }
        }
        
        if (allMachinesWorking)
        {
            victoryTimeRemaining -= Time.deltaTime;
        }
        
        victoryTimeRemaining = Mathf.Max(0f, victoryTimeRemaining);
        
        if (victoryTimeRemaining <= 0f || Time.realtimeSinceStartup > 20f)
        {
            victoryTimeRemaining = 0f;
            
            foreach (Machine machine in machines)
            {
                machine.SetBreakageAllowed(false);
            }
            
            // Dance time!
            foreach (Character character in characters)
            {
                character.SetSelected(false);
                character.SetOutlined(false);
                character.SetRepairing(false);
                character.SetMoving(false, Vector3.zero);
                character.SetDancing(true);
            }
        }
        
        victoryStatus.SetStatus(allMachinesWorking, Mathf.CeilToInt(victoryTimeRemaining));
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
    
    private Vector3 FindClosestEnterPoint(Vector3 fromPoint)
    {
        Vector3 closestPoint = default;
        float closestDist = float.MaxValue;
        foreach (Transform enterToPoint in enterToPoints)
        {
            float dist = Vector3.Distance(enterToPoint.position, fromPoint);
            if (dist < closestDist)
            {
                closestPoint = enterToPoint.position;
                closestDist = dist;
            }
        }
        
        return closestPoint;
    }
}
