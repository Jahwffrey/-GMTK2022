using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UnitController : MonoBehaviour
{
    public enum Winner
    {
        Player1,
        Player2,
        Tie
    }

    public static int MaxNumberGameSteps = 20;

    public GameObject Player1ZCutoffObj;
    public GameObject Player2ZCutoffObj;
    public WinnerUI WinnerUI;
    public TimerUI TimerUI;
    public GameObject StartGameBtn;
    public PlayerControl PlayerControl1;
    public PlayerControl PlayerControl2;

    public float Player1WinZ => Player1ZCutoffObj.transform.position.z;
    public float Player2WinZ => Player2ZCutoffObj.transform.position.z;

    // All active units
    protected List<DiceUnit> Units = new List<DiceUnit>();
    
    // Units that have passed the finish line
    protected List<DiceUnit> UnitsThatPassedFinishLine = new List<DiceUnit>();

    // Is the game currently simulating and all we should do is wait?
    protected bool DuringGameStep = false;
    protected bool GameActive = false;
    protected int GameStepsTaken;

    public void AddUnit(DiceUnit unit)
    {
        Units.Add(unit);
    }

    public void RemoveUnitFromConsideration(DiceUnit unit)
    {
        if (unit == null) return;
        if (Units.Contains(unit))
        {
            Units.Remove(unit);
        }
    }

    private void Start()
    {
        PregameSetup();
    }

    private void Update()
    {
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
        if (player1Wins > player2Wins) winner = Winner.Player1;
        if (player2Wins > player1Wins) winner = Winner.Player2;

        ShowWinner(winner);
    }

    protected void ShowWinner(Winner winner)
    {
        switch (winner)
        {
            case Winner.Player1: 
                WinnerUI.ShowWinner("Player 1 Wins!");
                break;
            case Winner.Player2:
                WinnerUI.ShowWinner("Player 2 Wins!");
                break;
            case Winner.Tie:
                WinnerUI.ShowWinner("Tie!");
                break;

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

    public void UnitPassedFinishLine(DiceUnit unit)
    {
        UnitsThatPassedFinishLine.Add(unit);
    }


    public void PregameSetup()
    {
        StartGameBtn.SetActive(true);
        TimerUI.gameObject.SetActive(false);
    }

    public void StartGame()
    {
        StartGameBtn.SetActive(false);
        TimerUI.gameObject.SetActive(true);
        GameStepsTaken = 0;
        GameActive = true;
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
    }
}
