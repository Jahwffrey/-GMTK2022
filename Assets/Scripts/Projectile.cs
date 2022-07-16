using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float LifetimeSeconds;
    public float Damage;
    public float Knockback;

    protected float TimeSpawned;
    protected Vector3 KnockbackDir;

    // Update is called once per frame
    void Update()
    {
        if(Time.time - TimeSpawned > LifetimeSeconds)
        {
            Destroy(gameObject);
        }
    }

    public void SetKnockbackDir(Vector3 v)
    {
        KnockbackDir = v.normalized;
    }

    public virtual void TouchedUnit(DiceUnit unit)
    {
        Destroy(gameObject);
    }

}
