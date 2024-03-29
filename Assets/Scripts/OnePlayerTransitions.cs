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

    protected int MaxPlayerUnits = 10;
    protected int MaxPlayerDice = 14;

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
        UnitController.EnableLookButton();
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
        int spaces = 10;
        List<int> choices = new List<int>();
        for(int i = 0;i < EnemyUnitIds.Count; i++)
        {
            if (i % spaces == 0)
            {
                choices.Clear();
                while(choices.Count < spaces)
                {
                    var choice = Random.Range(0, 10);
                    if (!choices.Contains(choice))
                    {
                        choices.Add(choice);
                    }
                }
            }
            int pos = choices[i % spaces];
            var g = Instantiate(Player2Control.GetUnitPrefab(EnemyUnitIds[i]));
            g.transform.forward = Vector3.back;
            var u = g.GetComponent<DiceUnit>();
            u.SetDice(EnemyDice[i]);
            u.SetPlayer(1);
            u.DontPlaySpawnSound = true;
            u.transform.position = EnemySpawnPointBase.transform.position + new Vector3(pos * DistanceBetweenEnemies, 0f, (i / 10) * DistanceBetweenEnemies);
        }

        ResetCamera();
    }

    public void ResetCamera()
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
                            while(unitOptions.Count < 3)
                            {
                                var newUnit = allUnits[Random.Range(0, allUnits.Count - 1)]; // -1 so not NONE
                                if (!unitOptions.Contains(newUnit))
                                {
                                    unitOptions.Add(newUnit);
                                }
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
                            int diceNum = 4;

                            List<int> selections = new List<int>();
                            while(selections.Count < diceNum)
                            {
                                int nextDie = Random.Range(0, allDice.Count - 1);
                                if (!selections.Contains(nextDie))
                                {
                                    selections.Add(nextDie);
                                }
                            }

                            for (int i = 0; i < selections.Count; i++)
                            {
                                diceOptions.Add(allDice[selections[i]]);
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
        UnitController.DisanbleLookButton();
        MusicMaster.PlayBattleTheme();
        Player1Control.selectBox.gameObject.SetActive(false);
        UnitController.StartGame();
    }

    protected bool LostThisLevelOnce = false;
    public void GameFinished(UnitController.Winner winner, bool unitsBrokeTie)
    {
        NextIsEndRoundAndGoToNext = true;
        RoundWinner = winner;
        switch (winner)
        {
            case UnitController.Winner.Player1:
                LostThisLevelOnce = false;
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
                if (!LostThisLevelOnce)
                {
                    LostThisLevelOnce = true;
                    RoundWinner = UnitController.Winner.Tie;
                    if (unitsBrokeTie)
                    {
                        ShowAnnouncement($"Failure\nTie broken by number of surviving units\nOne more chance!");
                    }
                    else
                    {
                        ShowAnnouncement($"Failure\nOne more chance!");
                    }
                }
                else
                {
                    if (unitsBrokeTie)
                    {
                        ShowAnnouncement($"Failure\nTie broken by number of surviving units\nReached Level {level}");
                    }
                    else
                    {
                        ShowAnnouncement($"Failure\nReached Level {level}");
                    }
                }
                break;
            case UnitController.Winner.Tie:
                ShowAnnouncement("Stalemate");
                break;
        }
    }
}