using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime;
using System;

public enum DiceSides
{
    Attack,
    Defend,
    Move,
    DoubleAttack,
    DoubleMove,
    DoubleDefend,
    Nothing,
    HalfHealth, // Halves your Unit's health
}

public class Dice
{
    protected DiceSides[] Sides;

    public Dice(List<DiceSides> sides)
    {
        Sides = sides.ToArray();
    }

    public int Roll()
    {
        return UnityEngine.Random.Range(0, Sides.Length - 1);//Sides[UnityEngine.Random.Range(0, Sides.Length - 1)];
    }

    public DiceSides GetResult(int side)
    {
        return Sides[side];
    }

    public DiceSides[] GetSides() 
    {
        return Sides;
    }
}

public class ActionAfterTime
{
    protected float TimeLeft;
    protected Action Action;

    public ActionAfterTime(float seconds, Action action)
    {
        TimeLeft = seconds;
        Action = action;
    }

    public bool ExecuteIfPassTime(float seconds)
    {
        TimeLeft -= seconds;
        if(TimeLeft <= 0)
        {
            Action();
            return true;
        }

        return false;
    }

}

public class DiceUnit : MonoBehaviour
{
    public static float StandardStepLengthSeconds = 1f;
    public static int DiceSidesNum = 6;

    public string AttackDesc;
    public string MoveDesc;
    public string DefendDesc;

    public DieDisplay DieDisplay;

    public bool Player1;
    public bool Player2 => !Player1;

    protected Dice Brain;
    protected Rigidbody Rigidbody;
    protected UnitController Controller;
    protected bool DuringStep;

    // Game steps work by units adding several actions to a stack
    // Once 'ExecuteNextAction' is called, the next item on the
    // stack is executed. When there are no actions left, EndStep
    // is called and the unit becomes idle until the next Step begins
    protected List<Action> CurrentActions = new List<Action>();
    protected List<ActionAfterTime> Timers = new List<ActionAfterTime>();

    protected virtual void InheritableAwake()
    {
        
    }

    protected virtual void InheritableStart()
    {

    }

    protected virtual void InheritableUpdate()
    {

    }

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
        InheritableAwake();
    }

    // Start is called before the first frame update
    void Start()
    {
        Brain = new Dice(new List<DiceSides>() { DiceSides.Attack, DiceSides.Move, DiceSides.Defend, DiceSides.DoubleAttack, DiceSides.DoubleMove, DiceSides.DoubleDefend });
        Controller = GameObject.Find("GameController").GetComponent<UnitController>();
        Controller.AddUnit(this);
        DieDisplay.transform.parent = transform.parent;
        DieDisplay.Setup(this);
        DieDisplay.gameObject.SetActive(false);
        InheritableStart();
    }

    public DiceSides[] GetCurrentDiceSides()
    {
        return Brain.GetSides();
    }

    private void Update()
    {
        // Execute any waiting actions
        float secondsPassed = Time.deltaTime;
        List<ActionAfterTime> completedActions = new List<ActionAfterTime>();
        for(int i = 0;i < Timers.Count;i++)
        {
            var timer = Timers[i];
            if (timer.ExecuteIfPassTime(secondsPassed))
            {
                completedActions.Add(timer);
            }
        }
        
        // Remove any actions that just happened from the list
        foreach(var completed in completedActions)
        {
            Timers.Remove(completed);
        }

        InheritableUpdate();
    }


    protected void AddActionLast(Action action)
    {
        CurrentActions.Add(action);
    }

    protected void AddActionFirst(Action action)
    {
        CurrentActions.Insert(0, action);
    }

    // Have I completed the current step?
    public bool StepComplete()
    {
        return !DuringStep;
    }

    public virtual void StartGameStep()
    {
        DuringStep = true;
        int res = Brain.Roll();
        switch (Brain.GetResult(res))
        {
            case DiceSides.Attack: AddActionLast(Attack); break;
            case DiceSides.Defend: AddActionLast(Defend); break;
            case DiceSides.Move: AddActionLast(Move); break;
            case DiceSides.DoubleAttack: AddActionLast(Attack); AddActionLast(Attack); break;
            case DiceSides.DoubleDefend: AddActionLast(Defend); AddActionLast(Defend); break;
            case DiceSides.DoubleMove: AddActionLast(Move); AddActionLast(Move); break;
        }
        DieDisplay.gameObject.SetActive(true);
        DieDisplay.ShowRoll(Brain.GetSides(), res);
        ExecuteAfterTimer(DieDisplay.RollDurationSecs + 0.5f,
            () =>
            {
                ExecuteNextAction();
            }
        );
        ExecuteAfterTimer(DieDisplay.RollDurationSecs + 1.5f,
            () =>
            {
                DieDisplay.gameObject.SetActive(false);
            }
        );
    }

    protected void ExecuteAfterTimer(float seconds, Action action)
    {
        Timers.Add(new ActionAfterTime(seconds, action));
    }

    protected void ExecuteNextAction()
    {
        if(CurrentActions.Count > 0)
        {
            var action = CurrentActions[0];
            CurrentActions.RemoveAt(0);
            action();
        }
        else
        {
            EndStep();
        }
    }

    public virtual void EndStep()
    {
        Debug.Log("Turn ended");
        Rigidbody.velocity = Vector3.zero;
        DuringStep = false;
        Controller.UnitEndedStep(this);
    }

    public virtual void Attack()
    {

    }

    public virtual void Move()
    {

    }

    public virtual void Defend()
    {

    }
}
