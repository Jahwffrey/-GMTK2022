using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OnePlayerTransitions : MonoBehaviour
{
    public Camera MainCamera;
    protected Vector3 CameraOrigPosition;

    public BoardGen BoardGenerator;

    public GameObject EnemySpawnPointBase;
    protected float DistanceBetweenEnemies = 1;

    public UnitController UnitController;
    public PlayerControl Player1Control;
    public PlayerControl Player2Control;
    protected bool WaitingForFirstUpdate = true;
    protected bool DecidedUnits = false;

    protected int PlayerStartingUnits = 3;
    protected int PlayerStartingDice = 5;
    protected int EnemyStartingUnits = 1;

    protected int MaxPlayerUnits = 8;
    protected int MaxPlayerDice = 8;

    protected List<PlayerControl.UnitID> PlayerUnitIds;
    protected List<Dice> PlayerDice;

    protected List<PlayerControl.UnitID> EnemyUnitIds;
    protected List<Dice> EnemyDice;


    public GameObject AnnouncementObj;
    public Text AnnouncementText;
    protected int level = 0;
    protected UnitController.Winner RoundWinner;

    protected bool NewUnitRound = true;

    public MusicController MusicMaster;

    public bool AnnouncementShowing;

    private void Start()
    {
        CameraOrigPosition = MainCamera.transform.position;
    }

    protected bool NextPregameSetup = false;
    protected bool NextIsEndRoundAndGoToNext = false;

    protected void AddAnotherEnemyUnit()
    {
        var allUnits = System.Enum.GetValues(typeof(PlayerControl.UnitID)).Cast<PlayerControl.UnitID>().ToList();
        var allDice = UnitController.GetAllDice();
        EnemyUnitIds.Add(allUnits[Random.Range(0, allUnits.Count - 1)]); // -1 so not NONE
        EnemyDice.Add(allDice[Random.Range(0, allDice.Count)]);
    }

    public void NewUnitSelected(PlayerControl.UnitID id)
    {
        PlayerUnitIds.Add(id);
        MoveToNextLevel();
    }

    public void NewDieSelected(Dice die)
    {
        PlayerDice.Add(die);
        MoveToNextLevel();
    }

    public void SetupGame()
    {
        if (!DecidedUnits)
        {
            MusicMaster.PlayChoosingTheme();
            DecidedUnits = true;
            PlayerUnitIds = new List<PlayerControl.UnitID>();
            PlayerDice = new List<Dice>();
            EnemyUnitIds = new List<PlayerControl.UnitID>();
            EnemyDice = new List<Dice>();
            var allUnits = System.Enum.GetValues(typeof(PlayerControl.UnitID)).Cast<PlayerControl.UnitID>().ToList();
            var allDice = UnitController.GetAllDice();
            for (int i = 0; i < PlayerStartingUnits; i++)
            {
                PlayerUnitIds.Add(allUnits[Random.Range(0, allUnits.Count - 1)]); // -1 so not NONE
            }
            for (int i = 0; i < PlayerStartingDice; i++)
            {
                PlayerDice.Add(allDice[Random.Range(0, allDice.Count)]);
            }
            for (int i = 0; i < EnemyStartingUnits; i++)
            {
                AddAnotherEnemyUnit();
            }
        }

        Player1Control.SetInventories(PlayerUnitIds, PlayerDice);
        Player1Control.infoCanvas.gameObject.SetActive(true);

        // Place the enemy units
        for(int i = 0;i < EnemyUnitIds.Count; i++)
        {
            var g = Instantiate(Player2Control.GetUnitPrefab(EnemyUnitIds[i]));
            g.transform.forward = Vector3.back;
            var u = g.GetComponent<DiceUnit>();
            u.SetDice(EnemyDice[i]);
            u.SetPlayer(1);
            u.transform.position = EnemySpawnPointBase.transform.position + new Vector3((i % 10) * DistanceBetweenEnemies, 0f, (i / 10) * DistanceBetweenEnemies);
        }

        ResetCamera();
    }

    void ResetCamera()
    {
        MainCamera.transform.position = CameraOrigPosition;
        MainCamera.transform.LookAt(Vector3.zero);
    }

    protected void ShowAnnouncement(string text)
    {
        AnnouncementShowing = true;
        AnnouncementObj.SetActive(true);
        AnnouncementText.text = text;
    }

    public void MoveToNextLevel()
    {
        BoardGenerator.Cleanup();
        BoardGenerator.PlaceObstacles();
        UnitController.PostGameCleanup();
        BeginNewGame();
    }

    public void HideAnnouncement()
    {
        AnnouncementShowing = false;
        AnnouncementObj.SetActive(false);

        if (NextPregameSetup)
        {
            NextPregameSetup = false;
        }

        if (NextIsEndRoundAndGoToNext)
        {
            MusicMaster.PlayChoosingTheme();
            UnitController.HideTimer();
            NextIsEndRoundAndGoToNext = false;
            switch (RoundWinner)
            {
                case UnitController.Winner.Player1:
                    // Continue
                    if (NewUnitRound)
                    {
                        NewUnitRound = !NewUnitRound;
                        if (PlayerUnitIds.Count < MaxPlayerUnits)
                        {
                            ShowAnnouncement("Select A New Creature");
                            ResetCamera();

                            // Choose two units to be able to select
                            var allUnits = System.Enum.GetValues(typeof(PlayerControl.UnitID)).Cast<PlayerControl.UnitID>().ToList();
                            var unitOptions = new List<PlayerControl.UnitID>();
                            for (int i = 0; i < 3; i++)
                            {
                                unitOptions.Add(allUnits[Random.Range(0, allUnits.Count - 1)]); // -1 so not NONE
                            }
                            Player1Control.SetInventories(unitOptions, new List<Dice>());

                            Player1Control.BeginSelectNewUnit();
                        }
                        else
                        {
                            MoveToNextLevel();
                        }
                    }
                    else
                    {
                        NewUnitRound = !NewUnitRound;
                        if (PlayerDice.Count < MaxPlayerDice)
                        {
                            ShowAnnouncement("Select A New Die");
                            ResetCamera();

                            var allDice = UnitController.GetAllDice();
                            var diceOptions = new List<Dice>();
                            for (int i = 0; i < 4; i++)
                            {
                                diceOptions.Add(allDice[Random.Range(0, allDice.Count - 1)]); // -1 so not NONE
                            }
                            Player1Control.SetInventories(new List<PlayerControl.UnitID>(), diceOptions);
                            Player1Control.BeginSelectNewDie();
                        }
                        else
                        {
                            MoveToNextLevel();
                        }
                    }
                    break;
                case UnitController.Winner.Player2:
                    // Lose
                    //Scene scene = SceneManager.GetActiveScene();
                    SceneManager.LoadScene(0);//scene.name);
                    break;
                case UnitController.Winner.Tie:
                    // Retry
                    level -= 1; // It will increment again in StartPlayerSetup
                    UnitController.PostGameCleanup();
                    BeginNewGame();
                    break;
            }
        }
    }


    private void Update()
    {
        if (WaitingForFirstUpdate)
        {
            WaitingForFirstUpdate = false;
            BeginNewGame();
        }
    }

    public void BeginNewGame()
    {
        UnitController.PregameSetup();
    }

    public void StartPlayerSetup()
    {
        level += 1;
        NextPregameSetup = true;
        ShowAnnouncement($"Level {level}");
    }

    public void GameSetupFinished()
    {
        MusicMaster.PlayBattleTheme();
        Player1Control.selectBox.gameObject.SetActive(false);
        UnitController.StartGame();
    }

    public void GameFinished(UnitController.Winner winner, bool unitsBrokeTie)
    {
        NextIsEndRoundAndGoToNext = true;
        RoundWinner = winner;
        switch (winner)
        {
            case UnitController.Winner.Player1:
                if (unitsBrokeTie)
                {
                    ShowAnnouncement("Victory!\nTie broken by number of surviving units");
                }
                else
                {
                    ShowAnnouncement("Victory!");
                }
                AddAnotherEnemyUnit();
                MusicMaster.PlayVictory();
                break;
            case UnitController.Winner.Player2:
                if (unitsBrokeTie)
                {
                    ShowAnnouncement($"Failure\nTie broken by number of surviving units\nReached Level {level}");
                }
                else
                {
                    ShowAnnouncement($"Failure\nReached Level {level}");
                }
                break;
            case UnitController.Winner.Tie:
                ShowAnnouncement("Stalemate");
                break;
        }
    }
}