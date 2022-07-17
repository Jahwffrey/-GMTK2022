using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    public int OnePlayerSceneInt;
    public int TwoPlayerSceneInt;

    public void GoToOnePlayer()
    {
        SceneManager.LoadScene(OnePlayerSceneInt);
    }
    public void GoToTwoPlayer()
    {
        SceneManager.LoadScene(TwoPlayerSceneInt);
    }
}
