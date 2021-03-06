using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UnitController : MonoBehaviour
{
    public static Dice NothingDie() {
        return new Dice("Nothing", new List<DiceSides>() { DiceSides.Nothing, DiceSides.Nothing, DiceSides.Nothing, DiceSides.Nothing, DiceSides.Nothing, DiceSides.Nothing });
    }
    public static List<Dice> GetAllDice()
    {
        var allSides = System.Enum.GetValues(typeof(DiceSides)).Cast<DiceSides>().ToList();
        return new List<Dice>()
        {
            new Dice("Reckless",new List<DiceSides>() { DiceSides.DoubleAttack, DiceSides.DoubleAttack, DiceSides.DoubleMove, DiceSides.DoubleMove, DiceSides.Lose1Hp, DiceSides.Lose2Hp }),
            new Dice("Knight", new List<DiceSides>() { DiceSides.Attack, DiceSides.Move, DiceSides.Defend, DiceSides.DoubleAttack, DiceSides.DoubleMove, DiceSides.Lose1Hp }),
            new Dice("Scout", new List<DiceSides>() { DiceSides.Move, DiceSides.Move, DiceSides.Move, DiceSides.Move, DiceSides.Attack, DiceSides.Attack }),
            new Dice("Sprinter", new List<DiceSides>() { DiceSides.DoubleMove, DiceSides.DoubleMove, DiceSides.DoubleMove, DiceSides.Move, DiceSides.Nothing, DiceSides.Lose2Hp }),
            new Dice("Great Defender", new List<DiceSides>() { DiceSides.Defend, DiceSides.Defend, DiceSides.Defend, DiceSides.Defend, DiceSides.Move, DiceSides.Heal1Hp }),
            new Dice("Balanced", new List<DiceSides>() { DiceSides.Attack, DiceSides.Defend, DiceSides.Move, DiceSides.Attack, DiceSides.Defend, DiceSides.Move }),
            new Dice("Go For Blood", new List<DiceSides>() { DiceSides.DoubleAttack, DiceSides.DoubleAttack, DiceSides.DoubleAttack, DiceSides.Move, DiceSides.Lose1Hp, DiceSides.Lose1Hp }),
            new Dice("Rogue", new List<DiceSides>() { DiceSides.DoubleAttack, DiceSides.Defend, DiceSides.Move, DiceSides.Move, DiceSides.Nothing, DiceSides.Heal1Hp }),
            new Dice("Gladiator", new List<DiceSides>() { DiceSides.DoubleAttack, DiceSides.DoubleAttack, DiceSides.Attack, DiceSides.Defend, DiceSides.Move, DiceSides.Lose1Hp }),

            new Dice("Paladin", new List<DiceSides>() { DiceSides.DoubleAttack, DiceSides.Attack, DiceSides.Heal1Hp, DiceSides.Move, DiceSides.Defend, DiceSides.Nothing }),
            new Dice("Fencer", new List<DiceSides>() { DiceSides.Defend, DiceSides.Defend, DiceSides.Defend, DiceSides.DoubleAttack, DiceSides.Attack, DiceSides.Nothing }),
            new Dice("Aggressive", new List<DiceSides>() { DiceSides.DoubleAttack, DiceSides.Attack, DiceSides.DoubleMove, DiceSides.Move, DiceSides.Nothing, DiceSides.Lose1Hp }),
            new Dice("Careful", new List<DiceSides>() { DiceSides.Move, DiceSides.Move, DiceSides.Defend, DiceSides.Defend, DiceSides.Heal1Hp, DiceSides.Heal1Hp }),
            new Dice("Intermittent", new List<DiceSides>() { DiceSides.DoubleAttack, DiceSides.DoubleMove, DiceSides.Defend, DiceSides.Heal1Hp, DiceSides.Nothing, DiceSides.Nothing }),

            new Dice("Attacker", new List<DiceSides>() { DiceSides.Attack, DiceSides.Attack, DiceSides.Attack, DiceSides.Defend, DiceSides.Defend, DiceSides.Move }),
            new Dice("Defender", new List<DiceSides>() { DiceSides.Defend, DiceSides.Defend, DiceSides.Defend, DiceSides.Move, DiceSides.Move, DiceSides.Attack }),
            new Dice("Runner", new List<DiceSides>() { DiceSides.Move, DiceSides.Move, DiceSides.Move, DiceSides.Attack, DiceSides.Attack, DiceSides.Defend }),
            new Dice("Attacker Plus", new List<DiceSides>() { DiceSides.DoubleAttack, DiceSides.Attack, DiceSides.Attack, DiceSides.Defend, DiceSides.Defend, DiceSides.Move }),
            new Dice("Defender Plus", new List<DiceSides>() { DiceSides.Defend, DiceSides.Defend, DiceSides.Defend, DiceSides.Move, DiceSides.DoubleMove, DiceSides.DoubleAttack }),
            new Dice("Runner Plus", new List<DiceSides>() { DiceSides.DoubleMove, DiceSides.Move, DiceSides.Move, DiceSides.Attack, DiceSides.Attack, DiceSides.Defend }),

            new Dice("Commoner", new List<DiceSides>() { DiceSides.Move, DiceSides.Move, DiceSides.Move, DiceSides.Move, DiceSides.Attack, DiceSides.Nothing }),
            new Dice("Squire", new List<DiceSides>() { DiceSides.Attack, DiceSides.Move, DiceSides.Defend, DiceSides.Attack, DiceSides.DoubleMove, DiceSides.Nothing }),

            new Dice("????", new List<DiceSides>(){ allSides[Random.Range(0,allSides.Count)],allSides[Random.Range(0,allSides.Count)],allSides[Random.Range(0,allSides.Count)],allSides[Random.Range(0,allSides.Count)],allSides[Random.Range(0,allSides.Count)],allSides[Random.Range(0,allSides.Count)] }),
            new Dice("????", new List<DiceSides>(){ allSides[Random.Range(0,allSides.Count)],allSides[Random.Range(0,allSides.Count)],allSides[Random.Range(0,allSides.Count)],allSides[Random.Range(0,allSides.Count)],allSides[Random.Range(0,allSides.Count)],allSides[Random.Range(0,allSides.Count)] }),
        };
    }

    public enum Winner
    {
        Player1,
        Player2,
        Tie
    }

    public static int MaxNumberGameSteps = 16;//20;

    public bool TwoPlayerMode;

    public GameObject Player1ZCutoffObj;
    public GameObject Player2ZCutoffObj;
    public TimerUI TimerUI;
    public PlayerControl PlayerControl1;
    public PlayerControl PlayerControl2;

    public GameObject FreeLookBtn;
    public GameObject FreeLookExitText;

    public TwoPlayerModeTransitions TwoPlayerModeTransitions;
    public OnePlayerTransitions OnePlayerTransitions;

    protected int CurrentPlayerId = 0;
    public int GetCurrentPlayerId => CurrentPlayerId;

    public float Player1WinZ => Player1ZCutoffObj.transform.position.z;
    public float Player2WinZ => Player2ZCutoffObj.transform.position.z;

    // All active units
    protected List<DiceUnit> Units = new List<DiceUnit>();
    
    // Units that have passed the finish line
    protected List<DiceUnit> UnitsThatPassedFinishLine = new List<DiceUnit>();

    protected CameraController CamControl;

    // Is the game currently simulating and all we should do is wait?
    protected bool DuringGameStep = false;
    protected bool GameActive = false;
    protected int GameStepsTaken;

    public GameObject EscapeMenu;

    private void Awake()
    {
        CamControl = GetComponent<CameraController> ();
    }

    public void AddUnit(DiceUnit unit)
    {
        Units.Add(unit);
    }

    public void HideTimer()
    {
        TimerUI.gameObject.SetActive(false);
    }

    public bool AnnouncementShowing()
    {
        if(OnePlayerTransitions != null)
        {
            return OnePlayerTransitions.AnnouncementShowing;
        }
        
        if(TwoPlayerModeTransitions != null)
        {
            return TwoPlayerModeTransitions.AnnouncementShowing;
        }

        return false;
    }

    public void RemoveUnitFromConsideration(DiceUnit unit)
    {
        if (unit == null) return;
        if (Units.Contains(unit))
        {
            Units.Remove(unit);
        }
    }

    protected bool FreeLookBoardBeforeGameActive = false;

    public void EnableLookButton()
    {
        FreeLookBtn.SetActive(true);
    }

    public void DisanbleLookButton()
    {
        FreeLookBtn.SetActive(false);
    }

    public void BeginLookAtBoard()
    {
        FreeLookExitText.SetActive(true);
        FreeLookBoardBeforeGameActive = true;
    }

    public void EndLookAtBoard()
    {
        FreeLookBoardBeforeGameActive = false;
        CamControl.UnlockCursor();
        if (!GameActive)
        {
            if (OnePlayerTransitions != null) OnePlayerTransitions.ResetCamera();
            if (TwoPlayerModeTransitions != null) TwoPlayerModeTransitions.ResetCamera();
        }
        FreeLookExitText.SetActive(false);
    }

    private void Update()
    {
        if (AnnouncementShowing() || GameActive || FreeLookBoardBeforeGameActive)
        {
            FreeLookBtn.SetActive(false);
        }
        else
        {
            FreeLookBtn.SetActive(true);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (FreeLookBoardBeforeGameActive)
            {
                EndLookAtBoard();
            }
            else
            {
                EscapeMenu.SetActive(!EscapeMenu.activeInHierarchy);
            }
        }

        if(Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButton(1))
        {
            EndLookAtBoard();
        }

        if (GameActive || FreeLookBoardBeforeGameActive)
        {
            CamControl.ControlCam();
        }

        if (GameActive)
        {

            if (!DuringGameStep)
            {
                StartGameStep();
            }
        }
    }

    protected bool IsGameFinished()
    {
        return GameActive && (GameStepsTaken >= MaxNumberGameSteps || Units.Count == 0);
    }

    protected float GetGamePercentLeft()
    {
        return Mathf.Max(0f, 1f - (((float)GameStepsTaken) / ((float)MaxNumberGameSteps)));
    }

    protected void EndGame()
    {
        GameActive = false;

        CamControl.UnlockCursor();

        int player1Wins = 0;
        int player2Wins = 0;
        foreach (var unit in UnitsThatPassedFinishLine)
        {
            if (unit.Player1)
            {
                player1Wins += 1;
            }
            else
            {
                player2Wins += 1;
            }
        }

        Winner winner = Winner.Tie;
        bool unitsBrokeTie = false;
        if (player1Wins != player2Wins)
        {
            // If one player got more units across the finish line, that player wins
            if (player1Wins > player2Wins) winner = Winner.Player1;
            if (player2Wins > player1Wins) winner = Winner.Player2;
        }
        else
        {
            // Otherwise, the player with more suriving units wins
            int player1Units = 0;
            int player2Units = 0;
            foreach(var unit in Units)
            {
                if (unit.Player1)
                {
                    player1Units += 1;
                } 
                else
                {
                    player2Units += 1;
                }
            }

            if (player1Units != player2Units)
            {
                unitsBrokeTie = true;
                if (player1Units > player2Units) winner = Winner.Player1;
                if (player2Units > player1Units) winner = Winner.Player2;
            }
        }

        ShowWinner(winner, unitsBrokeTie);
    }

    protected void ShowWinner(Winner winner, bool unitsBrokeTie)
    {
        if (TwoPlayerMode)
        {
            TwoPlayerModeTransitions.GameFinished(winner, unitsBrokeTie);
        }
        else
        {
            OnePlayerTransitions.GameFinished(winner, unitsBrokeTie);
        }
    }

    public void StartGameStep()
    {
        DuringGameStep = true;
        foreach(var unit in Units)
        {
            unit.StartGameStep();
        }
        GameStepsTaken += 1;
    }

    public void EndGameStep()
    {
        // Update the timer ui before update # of steps so that it stays full
        // for all of the first turn and goes full empty the moment the game ends
        TimerUI.DisplayTime(GetGamePercentLeft());
        DuringGameStep = false;
        foreach(var unit in Units)
        {
            unit.AllUnitsStepEnded();
        }
        if (IsGameFinished())
        {
            EndGame();
        }
    }

    public void UnitFullyDestroyed(DiceUnit unit)
    {
        if (unit == null) return;
        if (Units.Contains(unit))
        {
            Units.Remove(unit);
        }
        if (UnitsThatPassedFinishLine.Contains(unit))
        {
            UnitsThatPassedFinishLine.Remove(unit);
        }
        PlayerControl1.UnitWasFullyDestroyed(unit);
        PlayerControl2.UnitWasFullyDestroyed(unit);
    }

    public void UnitEndedStep(DiceUnit unit) 
    {
        // If all units have finished, end the step
        bool anyUnitStillActing = Units.Any(x => !x.StepComplete());

        if (!anyUnitStillActing)
        {
            EndGameStep();
        }
    }

    public List<DiceUnit> GetAllEnemiesWithin(DiceUnit unit, float dist)
    {
        List<DiceUnit> others = new List<DiceUnit>();
        foreach (var otherUnit in Units)
        {
            if (unit.Player1 != otherUnit.Player1)
            {
                if((unit.transform.position - otherUnit.transform.position).magnitude < dist)
                {
                    others.Add(otherUnit);
                }
            }
        }
        return others;
    }

    public bool TryGetNearestEnemyUnit(DiceUnit unit, out DiceUnit closest)
    {
        float minDist = float.PositiveInfinity;
        closest = null;
        foreach(var otherUnit in Units)
        {
            if(unit.Player1 != otherUnit.Player1)
            {
                var dist = (otherUnit.transform.position - unit.transform.position).magnitude;
                if(dist < minDist)
                {
                    minDist = dist;
                    closest = otherUnit;
                }
            }
        }

        return closest != null;
    }

    public bool TryGetEnemyNearestMyFinishLine(bool player1, out DiceUnit closest)
    {
        float minZ = float.PositiveInfinity;
        float maxZ = float.NegativeInfinity;
        closest = null;
        foreach (var otherUnit in Units)
        {
            if (player1 != otherUnit.Player1)
            {
                float z = otherUnit.transform.position.z;
                if (player1)
                {
                    if (z < minZ)
                    {
                        minZ = z;
                        closest = otherUnit;
                    }
                }
                else
                {
                    if(z > maxZ)
                    {
                        maxZ = z;
                        closest = otherUnit;
                    }
                }
            }
        }

        return closest != null;
    }

    public void UnitPassedFinishLine(DiceUnit unit)
    {
        UnitsThatPassedFinishLine.Add(unit);
    }

    public void PlayerOneReady()
    {
        if (TwoPlayerMode)
        {
            CurrentPlayerId = 1;
            TwoPlayerModeTransitions.SwitchToPlayerTwoSetup();
        } 
        else
        {
            OnePlayerTransitions.GameSetupFinished();
        }
    }
    public void PlayerTwoReady()
    {
        if (TwoPlayerMode)
        {
            CurrentPlayerId = 1;
            TwoPlayerModeTransitions.SwitchToGameSetupIsReady();
        }
    }

    public void PregameSetup()
    {
        CurrentPlayerId = 0;
        TimerUI.gameObject.SetActive(false);
        PlayerControl1.PregameSetup();
        PlayerControl2.PregameSetup();

        if (TwoPlayerMode)
        {
            TwoPlayerModeTransitions.SetupGame();
            TwoPlayerModeTransitions.StartPlayerOneSetup();
        }
        else
        {
            OnePlayerTransitions.SetupGame();
            OnePlayerTransitions.StartPlayerSetup();
        }
    }

    public void NewUnitSelected(PlayerControl.UnitID id)
    {
        OnePlayerTransitions.NewUnitSelected(id);
    }


    public void NewDieSelected(Dice die)
    {
        OnePlayerTransitions.NewDieSelected(die);
    }

    public void StartGame()
    {
        PlayerControl1.BeginGameplay();
        PlayerControl2.BeginGameplay();
        TimerUI.gameObject.SetActive(true);
        GameStepsTaken = 0;
        GameActive = true;
        TimerUI.DisplayTime(1);
        foreach(var unit in Units)
        {
            unit.StartGame();
        }
        if(Units.Count == 0)
        {
            EndGame();
        }
    }

    public void PostGameCleanup()
    {
        while(Units.Count > 0)
        {
            Units[0].DoDestroy();
        }
        while(UnitsThatPassedFinishLine.Count > 0)
        {
            UnitsThatPassedFinishLine[0].DoDestroy();
        }
        PlayerControl1.PostgameCleanup();
        PlayerControl2.PostgameCleanup();
    }
}
