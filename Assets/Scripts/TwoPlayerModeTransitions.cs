using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TwoPlayerModeTransitions : MonoBehaviour
{
    public Camera MainCamera;
    public GameObject AnnouncementObj;
    public Text AnnouncementText;

    protected void ShowAnnouncement(string text)
    {
        AnnouncementObj.SetActive(true);
        AnnouncementText.text = text;
    }

    public void HideAnnouncement()
    {
        AnnouncementObj.SetActive(false);
    }

    public void StartPlayerOneSetup()
    {
        ShowAnnouncement("Player 1 Setup\nPlayer 2, Don't look!");
    }
}
