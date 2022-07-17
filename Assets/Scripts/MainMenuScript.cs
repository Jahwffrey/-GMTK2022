using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    public int OnePlayerSceneInt;
    public int TwoPlayerSceneInt;
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}
