using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeerUnit : DiceUnit
{
    protected float Dmg = 1.5f;
    protected float MoveSpd = 3f;
    protected DiceUnit Target;
    protected bool Defending;
    protected float TimeLastWentForTarget;
    protected float GoForTargetInterval = 0.25f;
    protected float BiteKnockback = 4f;

    public DeerAttackVisual attackVisual;

    float RandNeg ()
    {
        return (Random.value - 0.5f)*1f;
    }

    public override void Attack()
    {
        if (!OpponentsExist())
        {
            ExecuteAfterTimer(0.1f, ExecuteNextAction);
            return;
        }

        DiceUnit closest;
        if (Controller.TryGetNearestEnemyUnit(this, out closest))
        {
            Vector3 dir = (closest.transform.position - transform.position).normalized;
            Rigidbody.velocity = dir * MoveSpd * 0.5f;
        }

        ExecuteAfterTimer(StandardStepLengthSeconds * 0.25f,
        () =>
        {
            attackVisual.PlayAnim( StandardStepLengthSeconds * 0.125f );
            var others = Controller.GetAllEnemiesWithin(this, 1.5f);
            foreach (var other in others)
            {
                var vectTo = (other.transform.position - transform.position).normalized;
                var knock = (vectTo + Vector3.up).normalized;
                other.TakeDamage(Dmg, knock * BiteKnockback * 0.75f);
            }
        });
        ExecuteAfterTimer(StandardStepLengthSeconds,
        () => {
            ExecuteNextAction();
        });
    }

    public override void Move()
    {
        var randV = new Vector3(RandNeg(), 0, RandNeg()).normalized;
        transform.forward = randV;
        Rigidbody.velocity = randV * MoveSpd;

        ExecuteAfterTimer(StandardStepLengthSeconds * 0.5f,
        () => {
            if (OnGround())
            {
                Rigidbody.velocity = GetDirectionToFinishLine() * MoveSpd;
                transform.forward = GetDirectionToFinishLine();
            };
        });
        ExecuteAfterTimer(StandardStepLengthSeconds,
        () =>
        {
            ExecuteNextAction();
        });
    }

    public override void Defend()
    {
        if (!OpponentsExist())
        {
            ExecuteAfterTimer(0.1f, ExecuteNextAction);
            return;
        }

        DiceUnit closest;
        if (Controller.TryGetEnemyNearestMyFinishLine(Player1, out closest))
        {
            Defending = true;
            Target = closest;
        }

        ExecuteAfterTimer(StandardStepLengthSeconds,
            () => {
                Defending = false;
                ExecuteNextAction();
            });
    }

    protected override void InheritableUpdate()
    {
        base.InheritableUpdate();
        if (( Defending) && Target != null && OnGround())
        {
            if (Time.time - TimeLastWentForTarget > GoForTargetInterval)
            {
                TimeLastWentForTarget = Time.time;
                Rigidbody.velocity = (Target.transform.position - transform.position).normalized * MoveSpd * 0.7f;
                var v = (Target.transform.position - transform.position);
                transform.forward = new Vector3(v.x,0,v.z).normalized;
            }
        }
    }

    protected override void InheritableOnTouchedCollider(Collider other)
    {
        base.InheritableOnTouchedCollider(other);
        var unit = other.GetComponent<DiceUnit>();
        if (unit != null && unit.Player1 != Player1)
        {
            if (Defending)
            {
                Defending = false;
                unit.TakeDamage(Dmg, GetDirectionToFinishLine() + Vector3.up * BiteKnockback * 1.5f);
                Target = null;
                //StopIfOnGround();
            }
        }
    }

    protected void EndMove()
    {
        StopIfOnGround();
    }
    public override void AllUnitsStepEnded()
    {
        base.AllUnitsStepEnded();
        Defending = false;
        EndMove();
    }

}
