using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    public int OnePlayerSceneInt;
    public int TwoPlayerSceneInt;
    protected int TutorialSceneInt = 3;
    protected int FreePlaySceneInt = 4;
    public MusicController MusicMaster;

    private void Start()
    {
        MusicMaster.PlayMenuTheme();
    }

    public void GoToOnePlayer()
    {
        SceneManager.LoadScene(OnePlayerSceneInt);
    }
    public void GoToTwoPlayer()
    {
        SceneManager.LoadScene(TwoPlayerSceneInt);
    }
    public void GoToTutorial()
    {
        SceneManager.LoadScene(TutorialSceneInt);
    }
    public void GoToFreePlay()
    {
        SceneManager.LoadScene(FreePlaySceneInt);
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}
