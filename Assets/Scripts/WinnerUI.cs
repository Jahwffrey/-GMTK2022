using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinnerUI : MonoBehaviour
{
    public Text WinnerText;

    public void ShowWinner (string text)
    {
        WinnerText.text = text;
    }
}
