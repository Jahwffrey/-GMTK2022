using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TwoPlayerModeTransitions : MonoBehaviour
{   
    public MusicController MusicMaster;
    public Camera MainCamera;
    public GameObject AnnouncementObj;
    public GameObject HidePlayerOneObj;
    public Text AnnouncementText;
    public BoardGen BoardGenerator;

    public bool AnnouncementShowing;

    public UnitController UnitController;
    public PlayerControl Player1Control;
    public PlayerControl Player2Control;

    protected Vector3 CameraOrigPosition;
    protected float TimeSwitchedToPlayer2 = -1000f;
    protected float DurationToSwingCameraAround = 2f;
    protected bool SwingingCameraAround;

    int TwoPlayerModeState = 0;

    protected List<PlayerControl.UnitID> UnitIds;
    protected List<Dice> Dice;

    protected bool WaitingForFirstUpdate = true;
    protected bool DecidedUnits = false;
    protected bool ReloadScene = false;
    protected bool RegenerateBoard = false;

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
        MusicMaster.PlayChoosingTheme();
        if (!DecidedUnits)
        {
            DecidedUnits = true;
            UnitIds = new List<PlayerControl.UnitID>();
            Dice = new List<Dice>();
            var numUnits = Random.Range(3, 9); // 3 - 8 units feels nice 
            var numDice = numUnits + Random.Range(1, 5);
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
        AnnouncementShowing = true;
        AnnouncementObj.SetActive(true);
        AnnouncementText.text = text;
    }

    public void HideAnnouncement()
    {
        AnnouncementShowing = false;
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
            MusicMaster.PlayBattleTheme();
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
                if (RegenerateBoard)
                {
                    BoardGenerator.Cleanup();
                    BoardGenerator.PlaceObstacles();
                    RegenerateBoard = false;
                }
                TwoPlayerModeState = 0;
                UnitController.PostGameCleanup();
                BeginNewGame();
            }
        }
    }

    public void StartPlayerOneSetup()
    {
        UnitController.EnableLookButton();
        Player1Control.SetInventories(UnitIds, Dice);
        Player1Control.infoCanvas.gameObject.SetActive(true);
        Player2Control.infoCanvas.gameObject.SetActive(false);
        Player2Control.SetInventories(UnitIds, Dice);
        Player1Control.FreePlayModeHack();
        Player2Control.FreePlayModeHack();
        TwoPlayerModeState = 0;
        HidePlayerOneObj.SetActive(false);
        ShowAnnouncement("Player 1 Setup\nPlayer 2, Don't look!");
    }

    protected bool DuringPlayerTwoSetup = false;
    public void SwitchToPlayerTwoSetup()
    {
        ShowAnnouncement("Player 2 Setup\nPlayer 1, Get lost!");
        Player1Control.infoCanvas.gameObject.SetActive(false);
        Player2Control.infoCanvas.gameObject.SetActive(true);
        TimeSwitchedToPlayer2 = Time.time;
        SwingingCameraAround = true;
        DuringPlayerTwoSetup = true;
        HidePlayerOneObj.SetActive(true);
    }

    public void SwitchToGameSetupIsReady()
    {
        DuringPlayerTwoSetup = false;
        ShowAnnouncement("Begin Game?");
        UnitController.DisanbleLookButton();
        Player1Control.infoCanvas.gameObject.SetActive(false);
        Player1Control.selectBox.gameObject.SetActive(false);
        Player2Control.infoCanvas.gameObject.SetActive(false);
        Player2Control.selectBox.gameObject.SetActive(false);
        HidePlayerOneObj.SetActive(false);
    }

    protected int Player1Wins = 0;
    protected int Player2Wins = 0;
    public void GameFinished(UnitController.Winner winner, bool unitsBrokeTie)
    {
        string winnerStr = "";
        string tieStr = "";
        switch (winner)
        {
            case UnitController.Winner.Player1:
                Player1Wins += 1;
                if (unitsBrokeTie)
                {
                    tieStr = "Tie, but Player 1 has more surviving units!\n";
                }
                winnerStr = "Player 1 Wins!";
                MusicMaster.PlayVictory();
                RegenerateBoard = true;
                break;
            case UnitController.Winner.Player2:
                Player2Wins += 1;
                if (unitsBrokeTie)
                {
                    tieStr = "Tie, but Player 2 has more surviving units!\n";
                }
                winnerStr = "Player 2 Wins!";
                MusicMaster.PlayVictory();
                RegenerateBoard = true;
                break;
            case UnitController.Winner.Tie:
                winnerStr = "Stalemate";
                break;
        }

        if (Player1Wins >= 2)
        {
            ShowAnnouncement($"{tieStr}Player 1 Is Victorious!");
            ReloadScene = true;
        }
        else if (Player2Wins >= 2)
        {
            ShowAnnouncement($"{tieStr}Player 2 Is Victorious!");
            ReloadScene = true;
        }
        else
        {
            ShowAnnouncement($"{tieStr}{winnerStr}\nScore:{Player1Wins} to {Player2Wins}");
        }
    }

    public void ResetCamera()
    {
        if (DuringPlayerTwoSetup)
        {
            var finalCameraPos = new Vector3(CameraOrigPosition.x, CameraOrigPosition.y, -CameraOrigPosition.z);
            MainCamera.transform.position = finalCameraPos;
            MainCamera.transform.LookAt(Vector3.zero);
        }
        else
        {
            MainCamera.transform.position = CameraOrigPosition;
            MainCamera.transform.LookAt(Vector3.zero);
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
                var origPointVect = new Vector3(CameraOrigPosition.x - 0.001f, 0, CameraOrigPosition.z);
                
                var finalPointVect = new Vector3(finalCameraPos.x, 0, finalCameraPos.z).normalized;
                var newVect = Vector3.Slerp(origPointVect.normalized, finalPointVect, swingPct) * origPointVect.magnitude ;
                MainCamera.transform.position = new Vector3(-newVect.x, CameraOrigPosition.y, newVect.z);
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
