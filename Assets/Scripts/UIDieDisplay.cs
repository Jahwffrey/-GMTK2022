using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDieDisplay : MonoBehaviour
{
    public Renderer[] SideDisplays;
    public Material AttackMaterial;
    public Material DefendMaterial;
    public Material MoveMaterial;
    public Material BadMaterial;
    
    private Dice die;

    protected Material GetSideMaterial(DiceSides side)
    {
        switch (side)
        {
            case DiceSides.Attack:
            case DiceSides.DoubleAttack:
                return AttackMaterial;
            case DiceSides.Defend:
            //case DiceSides.DoubleDefend:
                return DefendMaterial;
            case DiceSides.Move:
            case DiceSides.DoubleMove:
                return MoveMaterial;
            case DiceSides.Nothing:
            case DiceSides.Lose1Hp:
            case DiceSides.Lose2Hp:
                return BadMaterial;
            default:
                Debug.LogError("No material for this kind of side...");
                return BadMaterial;
        }
    }

    public void SetDie( Dice d )
    {
        die = d;
        for ( int i = 0; i < DiceUnit.DiceSidesNum; i++ )
        {
            SideDisplays[i].material = GetSideMaterial( die.GetSides()[i] );
        }
    }

    public Dice GetDie()
    {
        return die;
    }

    void Start()
    {
        die = null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
