using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolfUnit : DiceUnit
{
    protected float BiteDmg = 2.5f;
    protected float DefendDmg = 1;
    protected float MoveSpd = 3f;
    protected float BiteKnockback = 6f;

    protected DiceUnit Target;
    protected bool Attacking;
    protected bool Defending;
    protected float TimeLastWentForTarget;
    protected float GoForTargetInterval = 0.25f;

    public override void Defend()
    {
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

    public override void Attack()
    {
        DiceUnit closest;
        if (Controller.TryGetNearestEnemyUnit(this, out closest))
        {
            Attacking = true;
            Target = closest;
        }

        ExecuteAfterTimer(StandardStepLengthSeconds,
            () => {
                Attacking = false;
                ExecuteNextAction();
            });
    }

    public override void Move()
    {
        transform.forward = GetDirectionToFinishLine();
        Rigidbody.velocity = GetDirectionToFinishLine() * MoveSpd;
        ExecuteAfterTimer(StandardStepLengthSeconds,
            () => {
                EndMove();
                ExecuteNextAction();
            }
        );
    }


    protected void EndMove()
    {
        StopIfOnGround();
    }

    protected override void InheritableUpdate()
    {
        base.InheritableUpdate();
        if((Attacking || Defending) && Target != null && OnGround())
        {
            if(Time.time - TimeLastWentForTarget > GoForTargetInterval)
            {
                TimeLastWentForTarget = Time.time;
                Rigidbody.velocity = (Target.transform.position - transform.position).normalized * MoveSpd * 0.5f;
                transform.forward = (Target.transform.position - transform.position).normalized;
            }
        }
    }

    protected override void InheritableOnTouchedCollider(Collider other)
    {
        base.InheritableOnTouchedCollider(other);
        var unit = other.GetComponent<DiceUnit>();
        if(unit != null && unit.Player1 != Player1)
        {
            if (Attacking)
            {
                Attacking = false;
                unit.TakeDamage(BiteDmg,((unit.transform.position - transform.position).normalized + Vector3.up).normalized * BiteKnockback);
                Target = null;
                StopIfOnGround();
            }

            if (Defending)
            {
                Defending = false;
                unit.TakeDamage(DefendDmg, GetDirectionToFinishLine() + Vector3.up * BiteKnockback);
                Target = null;
                StopIfOnGround();
            }
        }
    }

    public override void AllUnitsStepEnded()
    {
        base.AllUnitsStepEnded();
        Attacking = false;
        Defending = false;
        EndMove();
    }
}
