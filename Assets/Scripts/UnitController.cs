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

    public void AddUnit(DiceUnit unit)
    {
        Units.Add(unit);
    }

    private void Update()
    {
        if (!DuringGameStep)
        {
            StartGameStep();
        }
    }

    public void StartGameStep()
    {
        Debug.Log("Starting next step");
        DuringGameStep = true;
        foreach(var unit in Units)
        {
            unit.StartGameStep();
        }
    }

    public void EndGameStep()
    {
        Debug.Log("All units finished");
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
