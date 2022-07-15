using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieDisplay : MonoBehaviour
{
    protected static float RollDurationSecs = 0.5f;

    protected DiceUnit Unit;
    protected float XRotSpd = 0f;
    protected float YRotSpd = 0f;
    protected float ZRotSpd = 0f;

    public Renderer[] SideDisplays;
    public Material AttackMaterial;
    public Material DefendMaterial;
    public Material MoveMaterial;
    public Material BadMaterial;

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
        ShowRoll(unit.GetCurrentDiceSides(),0);
    }

    public void ShowRoll(DiceSides[] sides, int result) 
    {
        XRotSpd = Random.value + 1f;
        YRotSpd = Random.value + 1f;
        ZRotSpd = Random.value + 1f;
        
        for (int i = 0;i < DiceUnit.DiceSidesNum; i++)
        {
            SideDisplays[i].material = GetSideMaterial(sides[i]);
        }
    }

    private void Update()
    {
        transform.position = Unit.transform.position + Vector3.up * 2f;
        transform.Rotate(Vector3.up, YRotSpd);
        transform.Rotate(Vector3.right, XRotSpd);
        transform.Rotate(Vector3.forward, ZRotSpd);
    }
}
