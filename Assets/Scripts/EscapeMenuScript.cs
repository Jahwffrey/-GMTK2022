using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EscapeMenuScript : MonoBehaviour
{
    public void backToMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
