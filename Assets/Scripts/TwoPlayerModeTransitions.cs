using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TwoPlayerModeTransitions : MonoBehaviour
{
    public Camera MainCamera;
    public GameObject Player1ReadyButton;
    public GameObject Player2ReadyButton;
    public GameObject AnnouncementObj;
    public GameObject HidePlayerOneObj;
    public Text AnnouncementText;

    public UnitController UnitController;

    protected Vector3 CameraOrigPosition;
    protected float TimeSwitchedToPlayer2 = -1000f;
    protected float DurationToSwingCameraAround = 5f;
    protected bool SwingingCameraAround;

    int TwoPlayerModeState = 0;

    private void Start()
    {
        CameraOrigPosition = MainCamera.transform.position;
        Player1ReadyButton.SetActive(false);
        Player2ReadyButton.SetActive(false);
    }

    protected void ShowAnnouncement(string text)
    {
        AnnouncementObj.SetActive(true);
        AnnouncementText.text = text;
    }

    public void HideAnnouncement()
    {
        AnnouncementObj.SetActive(false);
        if (TwoPlayerModeState == 0)
        {
            Player1ReadyButton.SetActive(true);
            TwoPlayerModeState = 1;
        }
        else if(TwoPlayerModeState == 1)
        {
            Player2ReadyButton.SetActive(true);
            TwoPlayerModeState = 2;
        }
        else
        {
            UnitController.StartGame();
        }
    }

    public void StartPlayerOneSetup()
    {
        TwoPlayerModeState = 0;
        HidePlayerOneObj.SetActive(false);
        ShowAnnouncement("Player 1 Setup\nPlayer 2, Don't look!");
    }

    public void SwitchToPlayerTwoSetup()
    {
        ShowAnnouncement("Player 2 Setup\nPlayer 1, Get lost!");
        TimeSwitchedToPlayer2 = Time.time;
        SwingingCameraAround = true;
        Player1ReadyButton.SetActive(false);
        HidePlayerOneObj.SetActive(true);
    }

    public void SwitchToGameSetupIsReady()
    {
        ShowAnnouncement("Begin Game?");
        Player2ReadyButton.SetActive(false);
        HidePlayerOneObj.SetActive(false);
    }

    private void Update()
    {
        // Swing the camera around which switching from player 1 to player 2
        if (SwingingCameraAround)
        {
            float swingPct = (Time.time - TimeSwitchedToPlayer2) / DurationToSwingCameraAround;
            var finalCameraPos = new Vector3(CameraOrigPosition.x, CameraOrigPosition.y, -CameraOrigPosition.z);
            if (swingPct < 1)
            {
                var origPointVect = new Vector3(CameraOrigPosition.x - 0.001f, 0, CameraOrigPosition.z);
                
                var finalPointVect = new Vector3(finalCameraPos.x, 0, finalCameraPos.z).normalized;
                var newVect = Vector3.Slerp(origPointVect.normalized, finalPointVect, swingPct) * origPointVect.magnitude ;
                MainCamera.transform.position = new Vector3(newVect.x, CameraOrigPosition.y, newVect.z);
                MainCamera.transform.LookAt(Vector3.zero);
            }
            else
            {
                MainCamera.transform.position = finalCameraPos;
                MainCamera.transform.LookAt(Vector3.zero);
                SwingingCameraAround = false;
            }
        }
    }
}
