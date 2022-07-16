using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUnit : DiceUnit
{
    public GameObject Shield;
    public GameObject Projectile;

    public override void Attack()
    {
        var g = Instantiate(Projectile);
        g.transform.position = transform.position + transform.forward * 0.5f;
        g.transform.forward = new Vector3(Random.value, Random.value, Random.value).normalized;
        g.GetComponent<Rigidbody>().velocity = transform.forward * 20f;

        ExecuteAfterTimer(StandardStepLengthSeconds, ExecuteNextAction);
    }


    public override void Defend()
    {
        TurnOnShield();
        ExecuteAfterTimer(StandardStepLengthSeconds * 2f,
            () => {
                TurnOffShield();
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
        Rigidbody.velocity = transform.forward * 20f;
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
}
