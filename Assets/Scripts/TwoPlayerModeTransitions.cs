using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TwoPlayerModeTransitions : MonoBehaviour
{
    public Camera MainCamera;
    public GameObject AnnouncementObj;
    public Text AnnouncementText;

    protected Vector3 CameraOrigPosition;
    protected float TimeSwitchedToPlayer2 = -1000f;
    protected float DurationToSwingCameraAround = 5f;
    protected bool SwingingCameraAround;

    private void Start()
    {
        CameraOrigPosition = MainCamera.transform.position;
    }

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

    public void SwitchToPlayerTwoSetup()
    {
        ShowAnnouncement("Player 2 Setup\nPlayer 1, Get lost!");
        TimeSwitchedToPlayer2 = Time.time;
        SwingingCameraAround = true;
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
