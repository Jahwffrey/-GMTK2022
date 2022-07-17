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

    protected override void InheritableOnTriggerStay(Collider other)
    {
        // Ducks are immune to water
    }

    public override void Move()
    {
        transform.forward = GetDirectionToFinishLine().normalized;
        Rigidbody.velocity += GetDirectionToFinishLine().normalized * MoveSpd * 0.75f + Vector3.up * JumpVel;
        ExecuteAfterTimer(StandardStepLengthSeconds,
        () => {
            ExecuteNextAction();
        });
    }

    public override void Attack()
    {

        Attacking = true;
        Rigidbody.velocity += Vector3.up * JumpVel;
        ExecuteAfterTimer(0.33f, () =>
        {
            DiceUnit closest;
            if (Controller.TryGetNearestEnemyUnit(this, out closest))
            {
                var vectTo = (closest.transform.position - transform.position).normalized;
                transform.forward = vectTo;
                Rigidbody.velocity = vectTo * MoveSpd * 2f + Vector3.up;
            }
            ExecuteAfterTimer(StandardStepLengthSeconds - 0.33f,
            () =>
            {
                Attacking = false;
                transform.forward = GetDirectionToFinishLine();
                StopIfOnGround();
                ExecuteNextAction();
            });
        });
    }

    public override void Defend()
    {
        GameObject gust = Instantiate(Gust);
        gust.transform.position = transform.position + 0.5f * GetDirectionToFinishLine();
        gust.transform.forward = GetDirectionToFinishLine();

        System.Action actn = () =>
        {
            if (gust != null)
            {
                var fwd = GetDirectionToFinishLine();
                var capsule = gust.GetComponentInChildren<CapsuleCollider>();
                var hits = Physics.CapsuleCastAll(capsule.center - fwd * capsule.height, capsule.center + fwd * capsule.height, capsule.radius, fwd, 0.01f);
                foreach (var hit in hits)
                {
                    if (hit.collider != null && hit.collider.gameObject != null)
                    {
                        var unit = hit.collider.GetComponent<DiceUnit>();
                        if (unit != null && unit != this)
                        {
                            unit.TakeDamage(0f, GetDirectionToFinishLine() * MainKockback * 0.25f + Vector3.up * 0.25f);
                        }
                    }
                }
            }
        };


        ExecuteAfterTimer(StandardStepLengthSeconds * 0.125f, actn);
        ExecuteAfterTimer(StandardStepLengthSeconds * 0.25f, actn);
        ExecuteAfterTimer(StandardStepLengthSeconds * 0.375f, actn);
        ExecuteAfterTimer(StandardStepLengthSeconds * 0.5f,
        () =>
        {
            actn();
            Destroy(gust);
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
                StopIfOnGround();
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
