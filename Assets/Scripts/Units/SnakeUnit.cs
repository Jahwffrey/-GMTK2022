using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeUnit : DiceUnit
{
    protected float MoveSpd = 1f;

    public GameObject Shield;
    public GameObject Projectile;

    protected bool Invulnerable;
    protected Vector3 InvulnPos;

    protected bool Moving;
    protected float LastTimeChangedMoveSpd;
    protected float IntervalChangeMoveSpd = 0.1f;


    protected override void InheritableUpdate()
    {
        base.InheritableUpdate();
        if (Invulnerable)
        {
            transform.position = InvulnPos;
        }

        if (Moving)
        {
            if(Time.time - LastTimeChangedMoveSpd > IntervalChangeMoveSpd)
            {
                LastTimeChangedMoveSpd = Time.time;
                float horizontal = Mathf.Sin(Time.time * 2f) * MoveSpd;
                if (OnGround())
                {
                    Rigidbody.velocity = GetDirectionToFinishLine() * MoveSpd + Vector3.right * horizontal;
                }
            }
        }
    }

    public override void Attack()
    {
        System.Action actn = () =>
        {
            DiceUnit closest;
            if (Controller.TryGetNearestEnemyUnit(this, out closest))
            {
                Vector3 dir = (closest.transform.position - transform.position).normalized;
                Rigidbody.velocity = dir * 0.5f;
                var g = Instantiate(Projectile);
                g.transform.position = transform.position + dir * 0.25f;
                g.transform.forward = new Vector3(Random.value, Random.value, Random.value).normalized;
                g.GetComponent<Rigidbody>().velocity = dir * 8f + Vector3.up * 3f;
                g.GetComponent<Projectile>().Setup(dir + Vector3.up, this);
            }
        };

        // Spit twice
        actn();
        ExecuteAfterTimer(StandardStepLengthSeconds * 0.5f, actn);

        ExecuteAfterTimer(StandardStepLengthSeconds, ExecuteNextAction);
    }

    public override void TakeDamage(float amt, Vector3 knockback)
    {
        if (!Invulnerable)
        {
            base.TakeDamage(amt, knockback);
        }
    }



    public override void Defend()
    {
        TurnOnShield();
        InvulnPos = transform.position;
        Invulnerable = true;
        ExecuteAfterTimer(StandardStepLengthSeconds * 2f,
            () => {
                ExecuteNextAction();
            }
        );
    }

    public override void Move()
    {
        Moving = true;
        ExecuteAfterTimer(StandardStepLengthSeconds,
            () => {
                Moving = false;
                ExecuteNextAction();
            });
    }

    protected void TurnOnShield()
    {
        Shield.SetActive(true);
    }

    protected void TurnOffShield()
    {
        Shield.SetActive(false);
    }


    protected void EndMove()
    {
        Moving = false;
        StopIfOnGround();
    }

    public override void AllUnitsStepEnded()
    {
        base.AllUnitsStepEnded();
        Invulnerable = false;
        Moving = false;
        TurnOffShield();
        EndMove();
    }

}
