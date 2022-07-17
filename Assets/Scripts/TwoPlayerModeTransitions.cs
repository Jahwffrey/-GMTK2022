using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TwoPlayerModeTransitions : MonoBehaviour
{
    public Camera MainCamera;
    public GameObject AnnouncementObj;
    public GameObject HidePlayerOneObj;
    public Text AnnouncementText;

    public UnitController UnitController;
    public PlayerControl Player1Control;
    public PlayerControl Player2Control;

    protected Vector3 CameraOrigPosition;
    protected float TimeSwitchedToPlayer2 = -1000f;
    protected float DurationToSwingCameraAround = 5f;
    protected bool SwingingCameraAround;

    int TwoPlayerModeState = 0;

    protected List<PlayerControl.UnitID> UnitIds;
    protected List<Dice> Dice;

    protected bool WaitingForFirstUpdate = true;
    protected bool DecidedUnits = false;
    protected bool ReloadScene = false;

    private void Start()
    {
        CameraOrigPosition = MainCamera.transform.position;
    }
    
    public void BeginNewGame()
    {
        UnitController.PregameSetup();
    }

    public void SetupGame()
    {
        if (!DecidedUnits)
        {
            DecidedUnits = true;
            UnitIds = new List<PlayerControl.UnitID>();
            Dice = new List<Dice>();
            var numUnits = Random.Range(3, 9);
            var numDice = numUnits + Random.Range(0, 4);
            if (numDice > 8) numDice = 8;
            var allUnits = System.Enum.GetValues(typeof(PlayerControl.UnitID)).Cast<PlayerControl.UnitID>().ToList();
            var allDice = UnitController.GetAllDice();
            for (int i = 0; i < numUnits; i++)
            {
                UnitIds.Add(allUnits[Random.Range(0, allUnits.Count - 1)]); // -1 so not NONE
            }
            for (int i = 0; i < numDice; i++)
            {
                Dice.Add(allDice[Random.Range(0, allDice.Count)]);
            }
        }
        MainCamera.transform.position = CameraOrigPosition;
        MainCamera.transform.LookAt(Vector3.zero);
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
            TwoPlayerModeState = 1;
        }
        else if(TwoPlayerModeState == 1)
        {
            TwoPlayerModeState = 2;
        }
        else if (TwoPlayerModeState == 2)
        {
            TwoPlayerModeState = 3;
            UnitController.StartGame();
        }
        else
        {
            if (ReloadScene)
            {
                //Scene scene = SceneManager.GetActiveScene(); 
                SceneManager.LoadScene(0);// scene.name);
            }
            else
            {
                TwoPlayerModeState = 0;
                UnitController.PostGameCleanup();
                BeginNewGame();
            }
        }
    }

    public void StartPlayerOneSetup()
    {
        Player1Control.SetInventories(UnitIds, Dice);
        Player2Control.SetInventories(UnitIds, Dice);
        TwoPlayerModeState = 0;
        HidePlayerOneObj.SetActive(false);
        ShowAnnouncement("Player 1 Setup\nPlayer 2, Don't look!");
    }

    public void SwitchToPlayerTwoSetup()
    {
        ShowAnnouncement("Player 2 Setup\nPlayer 1, Get lost!");
        TimeSwitchedToPlayer2 = Time.time;
        SwingingCameraAround = true;
        HidePlayerOneObj.SetActive(true);
    }

    public void SwitchToGameSetupIsReady()
    {
        ShowAnnouncement("Begin Game?");
        HidePlayerOneObj.SetActive(false);
    }

    protected int Player1Wins = 0;
    protected int Player2Wins = 0;
    public void GameFinished(UnitController.Winner winner)
    {
        string winnerStr = "";
        switch (winner)
        {
            case UnitController.Winner.Player1:
                Player1Wins += 1;
                winnerStr = "Player 1 Wins!";
                break;
            case UnitController.Winner.Player2:
                Player2Wins += 1;
                winnerStr = "Player 2 Wins!";
                break;
            case UnitController.Winner.Tie:
                winnerStr = "Tie!";
                break;
        }

        if (Player1Wins >= 2)
        {
            ShowAnnouncement("Player 1 Is Victorious!");
            ReloadScene = true;
        }
        else if (Player2Wins >= 2)
        {
            ShowAnnouncement("Player 2 Is Victorious!");
            ReloadScene = true;
        }
        else
        {
            ShowAnnouncement($"{winnerStr}\nScore:{Player1Wins} - {Player2Wins}");
        }
    }

    private void Update()
    {
        if (WaitingForFirstUpdate)
        {
            WaitingForFirstUpdate = false;
            BeginNewGame();
        }

        // Swing the camera around which switching from player 1 to player 2
        if (SwingingCameraAround)
        {
            float swingPct = (Time.time - TimeSwitchedToPlayer2) / DurationToSwingCameraAround;
            var finalCameraPos = new Vector3(CameraOrigPosition.x, CameraOrigPosition.y, -CameraOrigPosition.z);
            if (swingPct < 1)
            {
                var origPointVect = new Vector3(CameraOrigPosition.x + 0.001f, 0, CameraOrigPosition.z);
                
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
