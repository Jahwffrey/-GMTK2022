using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime;
using System;

public enum DiceSides
{
    Attack = 0,
    Defend = 1,
    Move = 2,
    DoubleAttack = 3,
    DoubleMove = 4,
    Nothing = 5,
    Lose1Hp = 6,
    Lose2Hp = 7,
    Heal1Hp = 8,
}

public class Dice
{
    protected DiceSides[] Sides;
    public string Name;

    public Dice(string name, List<DiceSides> sides)
    {
        Name = name;
        Sides = sides.ToArray();
    }

    public int Roll()
    {
        return UnityEngine.Random.Range(0, Sides.Length);//Sides[UnityEngine.Random.Range(0, Sides.Length - 1)];
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
    public static float StandardStepLengthSeconds = 2f;
    public static int DiceSidesNum = 6;

    public string UnitName;
    public string AttackDesc;
    public string MoveDesc;
    public string DefendDesc;
    
    public GameObject UnitModel;

    public float MaxHealth;
    protected float Health;

    public DieDisplay DieDisplay;
    public Collider MainCollider;

    public bool Player1;
    public bool Player2 => !Player1;

    public Color player1Color;
    public Color player2Color;

    protected Dice Brain;
    protected Rigidbody Rigidbody;
    protected UnitController Controller;
    protected HealthBar HealthBar;
    protected WhichPlayerIndicator WhichPlayerIndicator;
    protected bool DuringStep;
    protected bool StopNextOnGound;

    protected AudioSource AudioSource;

    // Game steps work by units adding several actions to a stack
    // Once 'ExecuteNextAction' is called, the next item on the
    // stack is executed. When there are no actions left, EndStep
    // is called and the unit becomes idle until the next Step begins
    protected List<Action> CurrentActions = new List<Action>();
    protected List<ActionAfterTime> Timers = new List<ActionAfterTime>();

    protected float TimeWhenPassedFinishLine;
    protected bool PassedFinishLine;
    protected Vector3 PositionWhenPassedFinishLine;

    protected bool Dead;

    public AudioClip SpawnSound;
    public AudioClip HurtSound;
    public AudioClip DeathSound;
    public AudioClip VictorySound;

    public string GetInfoText()
    {
        return $"{UnitName} {MaxHealth}<sprite=\"heart\" index=0> :\n<sprite=\"attack\" index=0> {AttackDesc}\n<sprite=\"defend\" index=0> {DefendDesc}\n<sprite=\"move\" index=0> {MoveDesc}";
    }

    protected virtual void InheritableAwake()
    {

    }

    protected virtual void InheritableStart()
    {

    }

    protected virtual void InheritableUpdate()
    {

    }


    protected Vector3 GetDirectionToFinishLine()
    {
        if (Player1)
        {
            return Vector3.forward;
        }
        else
        {
            return Vector3.back;
        }
    }

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
        HealthBar = GetComponentInChildren<HealthBar>();
        WhichPlayerIndicator = GetComponentInChildren<WhichPlayerIndicator>();
        AudioSource = GetComponentInChildren<AudioSource>();
        Health = MaxHealth;
        InheritableAwake();
    }

    protected bool DiceSet = false;
    void Start()
    {
        PlaySound(SpawnSound);
        if (!DiceSet)
        {
            Brain = UnitController.NothingDie();
        }
        Controller = GameObject.Find("GameController").GetComponent<UnitController>();
        Controller.AddUnit(this);
        DieDisplay.transform.parent = transform.parent;
        HealthBar.transform.parent = transform.parent;
        WhichPlayerIndicator.transform.parent = transform.parent;
        DieDisplay.Setup(this);
        HealthBar.Setup(this);
        DieDisplay.gameObject.SetActive(false);
        HealthBar.gameObject.SetActive(false);
        WhichPlayerIndicator.gameObject.SetActive(false);
        InheritableStart();
    }

    public void SetPlayer(int playerId)
    {
        Player1 = playerId == 0;
        UnitModel.GetComponent<Renderer>().material.SetColor( "_Color", Player1 ? player1Color : player2Color );
        //WhichPlayerIndicator.Setup(this);
    }

    public virtual void StartGame()
    {
        //WhichPlayerIndicator.gameObject.SetActive(true);
    }

    public void SetDice(Dice d)
    {
        DiceSet = true;
        Brain = d;
    }

    public Dice GetDice()
    {
        return Brain;
    }

    public DiceSides[] GetCurrentDiceSides()
    {
        return Brain.GetSides();
    }

    private void Update()
    {
        // Soft forward/backward boundaris
        if(transform.position.z < -5.5 && Rigidbody.velocity.z < 0)
        {
            Rigidbody.velocity = new Vector3(Rigidbody.velocity.x, Rigidbody.velocity.y, -Rigidbody.velocity.z);
        }
        if(transform.position.z > 5.5 && Rigidbody.velocity.z > 0)
        {
            Rigidbody.velocity = new Vector3(Rigidbody.velocity.x, Rigidbody.velocity.y, -Rigidbody.velocity.z);
        }

        if (Dead)
        {
            transform.up = Vector3.Slerp(Vector3.down, transform.up, Mathf.Min(1f, Time.deltaTime));
            transform.position -= Vector3.down * Time.deltaTime;
            DisplayHealth();
        }

        if(StopNextOnGound && OnGround())
        {
            Rigidbody.velocity = Vector3.zero;
            StopNextOnGound = false;
        }

        // Execute any waiting actions
        float secondsPassed = Time.deltaTime;
        List<ActionAfterTime> completedActions = new List<ActionAfterTime>();
        for (int i = 0; i < Timers.Count; i++)
        {
            var timer = Timers[i];
            if (timer.ExecuteIfPassTime(secondsPassed))
            {
                completedActions.Add(timer);
            }
        }

        // Remove any actions that just happened from the list
        foreach (var completed in completedActions)
        {
            Timers.Remove(completed);
        }

        // Passing finish line
        if (!PassedFinishLine && !Dead)
        {
            if (CheckIfPassedFinishLine())
            {
                PlaySound(VictorySound);
                PassedFinishLine = true;
                Controller.UnitPassedFinishLine(this);
                PositionWhenPassedFinishLine = transform.position;
                TimeWhenPassedFinishLine = Time.time;
                RemoveFromConsideration();
            }
        }

        InheritableUpdate();

        // Animate if passed finish line
        if (PassedFinishLine)
        {
            Rigidbody.velocity = Vector3.zero;
            transform.position = PositionWhenPassedFinishLine + Vector3.up * (Mathf.Sin(Time.time - TimeWhenPassedFinishLine) + 1f) * 0.5f;
            transform.Rotate(Vector3.up, 90f * Time.deltaTime );
        }
    }

    public void DoDestroy()
    {
        Controller.UnitFullyDestroyed(this);
        RemoveFromConsideration();
        Destroy(gameObject);
        Destroy(DieDisplay.gameObject);
        Destroy(HealthBar.gameObject);
        Destroy(WhichPlayerIndicator.gameObject);
    }

    public void RemoveFromConsideration()
    {
        var collider = GetComponent<Collider>();
        if (collider != null) collider.enabled = false;
        foreach (var coll in GetComponentsInChildren<Collider>())
        {
            coll.enabled = false;
        }
        EndStep();
        Controller.RemoveUnitFromConsideration(this);
    }

    protected bool CheckIfPassedFinishLine()
    {
        if (Player1)
        {
            return transform.position.z >= Controller.Player1WinZ;
        }
        else
        {
            return transform.position.z <= Controller.Player2WinZ;
        }
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

    public bool OnGround()
    {
        // https://answers.unity.com/questions/196381/how-do-i-check-if-my-rigidbody-player-is-grounded.html
        return Physics.Raycast(transform.position, -Vector3.up, MainCollider.bounds.extents.y + 0.1f);
    }

    public void StopIfOnGround()
    {
        StopNextOnGound = true;
    }

    public virtual void StartGameStep()
    {
        StopIfOnGround();
        DuringStep = true;
        int res = Brain.Roll();
        switch (Brain.GetResult(res))
        {
            case DiceSides.Attack: AddActionLast(Attack); break;
            case DiceSides.Defend: AddActionLast(Defend); break;
            case DiceSides.Move: AddActionLast(Move); break;
            case DiceSides.DoubleAttack: AddActionLast(Attack); AddActionLast(Attack); break;
            //case DiceSides.DoubleDefend: AddActionLast(Defend); AddActionLast(Defend); break;
            case DiceSides.DoubleMove: AddActionLast(Move); AddActionLast(Move); break;
            case DiceSides.Nothing: AddActionFirst(ExecuteNextAction); break;
            case DiceSides.Lose1Hp: AddActionFirst(
                    () =>
                    {
                        TakeDamage(1f, Vector3.zero);
                        ExecuteNextAction();
                    }
                );
                break;
            case DiceSides.Lose2Hp:
                AddActionFirst(
                () =>
                {
                    TakeDamage(2f, Vector3.zero);
                    ExecuteNextAction();
                }
                );
                break;
            case DiceSides.Heal1Hp: AddActionFirst(
                    () =>
                    {
                        HealDamage(1f);
                        ExecuteNextAction();
                    }
                );
                break;
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
        if(!Dead && !PassedFinishLine && CurrentActions.Count > 0)
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


    protected virtual void InheritableMyStepEnded()
    {

    }

    public virtual void AllUnitsStepEnded()
    {

    }

    public void EndStep()
    {
        StopIfOnGround();
        DuringStep = false;
        InheritableMyStepEnded();
        Controller.UnitEndedStep(this);
    }

    public virtual void HealDamage(float amt)
    {
        Health += amt;
        if (Health > MaxHealth) Health = MaxHealth;
        DisplayHealth();
    }

    public virtual void TakeDamage(float amt, Vector3 knockback)
    {
        if(amt > 0)
        {
            PlaySound(HurtSound);
        }

        Health -= amt;
        Rigidbody.velocity += knockback;
        DisplayHealth();

        if (Health <= 0)
        {
            Die();
        }
    }

    protected void DisplayHealth()
    {
        if (Health < MaxHealth && MaxHealth > 0)
        {
            if (Dead)
            {
                HealthBar.DisplayAmt(0);
            }
            else
            {
                HealthBar.gameObject.SetActive(true);
                HealthBar.DisplayAmt(Health / MaxHealth);
            }
        }
        else
        {
            HealthBar.gameObject.SetActive(false);
        }
    }

    public void Die()
    {
        PlaySound(DeathSound);
        DisplayHealth();
        RemoveFromConsideration();
        Dead = true;
    }

    protected void TouchingCollider(Collider other)
    {
        if (other != null && other.gameObject != null)
        {
            InheritableOnTouchedCollider(other);
            var proj = other.gameObject.GetComponent<Projectile>();
            if (proj != null && proj.Parent != this)
            {
                GotHitByProjectile(proj);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision != null && collision.collider != null)
        {
            TouchingCollider(collision.collider);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision != null && collision.collider != null)
        {
            TouchingCollider(collision.collider);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        TouchingCollider(other);
    }

    private void OnTriggerStay(Collider other)
    {
        InheritableOnTriggerStay(other);
        TouchingCollider(other);
    }

    protected virtual void InheritableOnTouchedCollider(Collider other)
    {
        
    }

    public virtual void GotHitByProjectile(Projectile p)
    {
        if (p.Parent.Player1 != Player1 || p.HitsBothSides) // Dont git hit by your own team's attacks
        {
            TakeDamage(p.GetDamage(), p.GetKnockback());
            p.TouchedUnit(this);
        }
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


    protected virtual void InheritableOnTriggerStay(Collider other)
    {
        if (other != null && other.gameObject != null)
        {
            if (other.GetComponent<PuddleScript>() != null)
            {
                Rigidbody.velocity = Rigidbody.velocity * 0.9f;
            }
        }
    }

    protected void PlaySound(AudioClip clip)
    {
        if(clip != null && AudioSource != null)
        {
            AudioSource.clip = clip;
            AudioSource.Play();
        }
    }

    protected bool OpponentsExist()
    {
        DiceUnit tmp;
        return Controller.TryGetNearestEnemyUnit(this, out tmp);
    }
}
