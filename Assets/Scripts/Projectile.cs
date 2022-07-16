using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float LifetimeSeconds;
    public float Damage;
    public float Knockback;
    public DiceUnit Parent;

    protected float TimeSpawned;
    protected Vector3 KnockbackDir;

    private void Start()
    {
        TimeSpawned = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time - TimeSpawned > LifetimeSeconds)
        {
            Destroy(gameObject);
        }
    }

    public void Setup(Vector3 knockbackDir, DiceUnit parent)
    {
        KnockbackDir = knockbackDir.normalized;
        Parent = parent;
    }

    public virtual float GetDamage()
    {
        return Damage;
    }

    public virtual Vector3 GetKnockback()
    {
        return KnockbackDir * Knockback;
    }

    public virtual void TouchedUnit(DiceUnit unit)
    {
        Destroy(gameObject);
    }

}
