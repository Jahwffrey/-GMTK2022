using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuckUnit : DiceUnit
{
    public GameObject Gust;

    protected float Dmg = 1.5f;
    protected float MoveSpd = 1.5f;
    protected float MainKockback = 3f;
    protected float JumpVel = 7f;

    protected bool Attacking;


    float RandNeg()
    {
        return (Random.value - 0.5f) * 2f;
    }

    protected override void InheritableOnTriggerStay(Collider other)
    {
        // Ducks are immune to water
    }

    public override void Move()
    {
        transform.forward = GetDirectionToFinishLine().normalized;
        Rigidbody.velocity += GetDirectionToFinishLine().normalized * MoveSpd * 0.85f + Vector3.up * JumpVel;
        ExecuteAfterTimer(StandardStepLengthSeconds,
        () => {
            ExecuteNextAction();
        });
    }

    public override void Attack()
    {
        if (!OpponentsExist())
        {
            ExecuteAfterTimer(0.1f, ExecuteNextAction);
            return;
        }


        Attacking = true;
        Rigidbody.velocity = Vector3.up * JumpVel;
        ExecuteAfterTimer(0.33f, () =>
        {
            DiceUnit closest;
            if (Controller.TryGetNearestEnemyUnit(this, out closest))
            {
                var vectTo = (closest.transform.position - transform.position).normalized;
                transform.forward = vectTo;
                Rigidbody.velocity = vectTo * MoveSpd * 2f + Vector3.up;
            }
            ExecuteAfterTimer(StandardStepLengthSeconds - 0.35f,
            () =>
            {
                Attacking = false;
                transform.forward = GetDirectionToFinishLine();
                StopIfOnGround();
                ExecuteAfterTimer(0.01f, ExecuteNextAction);
            });
        });
    }

    public override void Defend()
    {
        if (!OpponentsExist())
        {
            ExecuteAfterTimer(0.1f, ExecuteNextAction);
            return;
        }

        System.Action actn = () =>
        {
            Vector3 dir = GetDirectionToFinishLine();
            var g = Instantiate(Gust);
            g.transform.position = transform.position + dir * 0.25f;
            g.transform.forward = new Vector3(Random.value, Random.value, Random.value).normalized;
            g.GetComponent<Rigidbody>().velocity = dir * 10f + Vector3.up * Random.value * 3f + Vector3.right * RandNeg() * 4f;
            g.GetComponent<Projectile>().Setup(dir + Vector3.up, this);
        };


        ExecuteAfterTimer(StandardStepLengthSeconds * 0.125f, actn);
        ExecuteAfterTimer(StandardStepLengthSeconds * 0.25f, actn);
        ExecuteAfterTimer(StandardStepLengthSeconds * 0.375f, actn);
        ExecuteAfterTimer(StandardStepLengthSeconds * 0.5f, actn);
        ExecuteAfterTimer(StandardStepLengthSeconds * 0.625f, actn);
        ExecuteAfterTimer(StandardStepLengthSeconds * 0.75f, actn);
        ExecuteAfterTimer(StandardStepLengthSeconds * 0.875f, actn);
        ExecuteAfterTimer(StandardStepLengthSeconds * 1f, actn);
        ExecuteAfterTimer(StandardStepLengthSeconds * 1.125f, actn);
        ExecuteAfterTimer(StandardStepLengthSeconds * 1.25f, actn);
        ExecuteAfterTimer(StandardStepLengthSeconds * 1.375f, actn);
        ExecuteAfterTimer(StandardStepLengthSeconds * 1.5f, actn);
        ExecuteAfterTimer(StandardStepLengthSeconds * 1.625f, actn);
        ExecuteAfterTimer(StandardStepLengthSeconds * 1.75f, actn);
        ExecuteAfterTimer(StandardStepLengthSeconds * 1.875f, actn);
        ExecuteAfterTimer(StandardStepLengthSeconds * 2f,
        () =>
        {
            actn();
            ExecuteNextAction();
        });
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
                unit.TakeDamage(Dmg, ((unit.transform.position - transform.position).normalized + Vector3.up).normalized * MainKockback);
                //StopIfOnGround();
            }
        }
    }


    public override void AllUnitsStepEnded()
    {
        base.AllUnitsStepEnded();
        Attacking = false;
        StopIfOnGround();
    }
}
