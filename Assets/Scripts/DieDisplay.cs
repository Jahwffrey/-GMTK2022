using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieDisplay : MonoBehaviour
{
    public static float RollDurationSecs = 0.5f;

    protected DiceUnit Unit;
    protected float XRotSpd = 0f;
    protected float YRotSpd = 0f;
    protected float ZRotSpd = 0f;

    public Renderer[] SideDisplays;
    public Material AttackMaterial;
    public Material DefendMaterial;
    public Material MoveMaterial;
    public Material BadMaterial;

    protected float TimeStartedAnimation = 0f;
    protected int ResultAfterAnimation = 0;

    protected Vector3[] SideVects = new Vector3 [] { Vector3.up, Vector3.right, Vector3.forward, -Vector3.forward, -Vector3.right, -Vector3.up };

    protected Material GetSideMaterial(DiceSides side)
    {
        switch (side)
        {
            case DiceSides.Attack:
            case DiceSides.DoubleAttack:
                return AttackMaterial;
            case DiceSides.Defend:
            case DiceSides.DoubleDefend:
                return DefendMaterial;
            case DiceSides.Move:
            case DiceSides.DoubleMove:
                return MoveMaterial;
            case DiceSides.Nothing:
            case DiceSides.HalfHealth:
                return BadMaterial;
            default:
                Debug.LogError("No material for this kind of side...");
                return BadMaterial;
        }
    }

    public void Setup(DiceUnit unit)
    {
        Unit = unit;
    }

    protected float RangNed()
    {
        return (Random.value - 0.5f) * 2f;
    }

    public void ShowRoll(DiceSides[] sides, int result) 
    {
        var rollVect = new Vector3(RangNed(), RangNed(), RangNed()).normalized;
        XRotSpd = rollVect.x * 3f;
        YRotSpd = rollVect.y * 3f;
        ZRotSpd = rollVect.z * 3f;

        for (int i = 0;i < DiceUnit.DiceSidesNum; i++)
        {
            SideDisplays[i].material = GetSideMaterial(sides[i]);
        }

        ResultAfterAnimation = result;
        TimeStartedAnimation = Time.time;
    }

    private void Update()
    {
        transform.position = Unit.transform.position + Vector3.up * 2f;
        if (Time.time - TimeStartedAnimation > RollDurationSecs)
        {
            transform.up = -SideVects[ResultAfterAnimation];
        }
        else
        {
            transform.Rotate(Vector3.up, YRotSpd);
            transform.Rotate(Vector3.right, XRotSpd);
            transform.Rotate(Vector3.forward, ZRotSpd);
        }
    }
}
