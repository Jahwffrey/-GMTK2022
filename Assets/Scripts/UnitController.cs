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

    public float Player1WinZ => Player1ZCutoffObj.transform.position.z;
    public float Player2WinZ => Player2ZCutoffObj.transform.position.z;

    // All active units
    protected List<DiceUnit> Units = new List<DiceUnit>();
    
    // Units that have passed the finish line
    protected List<DiceUnit> UnitThatPassedFinishLine = new List<DiceUnit>();

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
        Units.Remove(unit);
    }

    private void Update()
    {
        if (GameActive)
        {
            TimerUI.DisplayTime(GetGamePercentLeft());

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
        foreach (var unit in UnitThatPassedFinishLine)
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
        DuringGameStep = false;
        if (IsGameFinished())
        {
            EndGame();
        }
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

    public void GetNearestEnemyUnit()
    {

    }

    public void UnitPassedFinishLine(DiceUnit unit)
    {
        UnitThatPassedFinishLine.Add(unit);
    }

    public void StartGame()
    {
        GameStepsTaken = 0;
        GameActive = true;
    }
}
