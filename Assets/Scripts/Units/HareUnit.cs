using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HareUnit : DiceUnit
{
    protected float Dmg = 1f;
    protected float MoveSpd = 5f;
    protected float MainKockback = 3f;
    protected float JumpVel = 4f;
    protected bool Attacking;
    protected float PushBackMaxDist = 1f;

    public override void Defend()
    {
        DiceUnit closest;
        if (Controller.TryGetEnemyNearestMyFinishLine(Player1, out closest))
        {
            Attacking = true;
            var vectToOther = (closest.transform.position - transform.position);
            transform.forward = vectToOther.normalized;
            if (vectToOther.magnitude < PushBackMaxDist)
            {
                closest.TakeDamage(Dmg, (vectToOther + Vector3.up).normalized * MainKockback);
            }
            Rigidbody.velocity += -vectToOther.normalized * MoveSpd * 0.5f + Vector3.up * JumpVel;
        }
        ExecuteAfterTimer(StandardStepLengthSeconds,
            () => {
                Attacking = false;
                ExecuteNextAction();
            });
    }

    public override void Attack()
    {
        DiceUnit closest;
        if (Controller.TryGetEnemyNearestMyFinishLine(Player1, out closest))
        {
            Attacking = true;
            var vectToOther = (closest.transform.position - transform.position).normalized;
            transform.forward = vectToOther.normalized;
            Rigidbody.velocity += vectToOther.normalized * MoveSpd * 0.5f + Vector3.up * JumpVel;
        }
        ExecuteAfterTimer(StandardStepLengthSeconds,
            () => {
                Attacking = false;
                ExecuteNextAction();
            });
    }

    public override void Move()
    {
        Rigidbody.velocity = GetDirectionToFinishLine() * MoveSpd + Vector3.right * ((Random.value - 0.5f)*6f);
        transform.forward = Rigidbody.velocity.normalized;
        ExecuteAfterTimer(StandardStepLengthSeconds,
            () => {
                EndMove();
                ExecuteNextAction();
            }
        );
    }


    protected override void InheritableOnTouchedCollider(Collider other)
    {
        base.InheritableOnTouchedCollider(other);
        var unit = other.GetComponent<DiceUnit>();
        if (unit != null && unit.Player1 != Player1)
        {
            if (Attacking)
            {
                Attacking = false;
                unit.TakeDamage(Dmg, (unit.transform.position - transform.position).normalized * MainKockback);
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
        transform.forward = GetDirectionToFinishLine();
        Attacking = false;
        EndMove();
    }
}
