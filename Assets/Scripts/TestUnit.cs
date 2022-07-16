using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUnit : DiceUnit
{
    public GameObject Shield;
    public GameObject Projectile;

    protected bool Invulnerable;
    protected Vector3 InvulnPos;

    protected override void InheritableUpdate()
    {
        base.InheritableUpdate();
        if (Invulnerable)
        {
            transform.position = InvulnPos;
        }
    }

    public override void Attack()
    {
        DiceUnit closest;
        if (Controller.TryGetNearestEnemyUnit(this, out closest))
        {
            Vector3 dir = (closest.transform.position - transform.position).normalized;
            Rigidbody.velocity = dir * 0.5f;
            var g = Instantiate(Projectile);
            g.transform.position = transform.position + dir * 0.5f;
            g.transform.forward = new Vector3(Random.value, Random.value, Random.value).normalized;
            g.GetComponent<Rigidbody>().velocity = dir * 20f + Vector3.up * 4f;
            g.GetComponent<Projectile>().Setup(dir + Vector3.up, this);
        }

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
                TurnOffShield();
                Invulnerable = false;
                ExecuteNextAction();
            }
        );
    }

    protected void TurnOnShield()
    {
        Shield.SetActive(true);
    }

    protected void TurnOffShield()
    {
        Shield.SetActive(false);
    }

    public override void Move()
    {
        Rigidbody.velocity = (transform.forward + new Vector3((Random.value - 0.5f) * 0.2f, 0f, (Random.value - 0.5f) * 0.2f)).normalized * 4f;
        ExecuteAfterTimer(StandardStepLengthSeconds, 
            () => {
                EndMove();
                ExecuteNextAction();
            }
        );
    }

    protected void EndMove()
    {
        Rigidbody.velocity = Vector3.zero;
    }

    protected override void InheritableStepEnded()
    {
        base.InheritableStepEnded();
        Invulnerable = false;
    }
}
