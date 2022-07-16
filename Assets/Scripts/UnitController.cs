using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UnitController : MonoBehaviour
{
    // All active units
    protected List<DiceUnit> Units = new List<DiceUnit>();

    // Is the game currently simulating and all we should do is wait?
    protected bool DuringGameStep = false;

    public bool GameActive = true;

    public void AddUnit(DiceUnit unit)
    {
        Units.Add(unit);
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

    public void StartGameStep()
    {
        DuringGameStep = true;
        foreach(var unit in Units)
        {
            unit.StartGameStep();
        }
    }

    public void EndGameStep()
    {
        DuringGameStep = false;
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
}
