using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialScript : MonoBehaviour
{
    public GameObject[] Screenshots;

    int whichShot = 0;
    public void NextClick()
    {
        Screenshots[whichShot].SetActive(false);
        whichShot += 1;
        if(whichShot >= Screenshots.Length)
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            Screenshots[whichShot].SetActive(true);
        }
    }
}
