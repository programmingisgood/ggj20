using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleController : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("Main");
    }
    
    public void ExitGame()
    {
        Application.Quit();
    }
}
