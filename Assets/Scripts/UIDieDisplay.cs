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
    public Material HealMaterial;

    public Sprite AttackSprite;
    public Sprite DefendSprite;
    public Sprite MoveSprite;
    public Sprite DoubleAttackSprite;
    public Sprite DoubleMoveSprite;
    public Sprite DoNothingSprite;
    public Sprite HealSprite;
    public Sprite HurtSprite;
    public Sprite DoubleHurtSprite;

    public Sprite RedBox;
    public Sprite BlueBox;
    public Sprite GreenBox;
    public Sprite PinkBox;
    public Sprite BlackBox;

    public GameObject toolTipSprite;
    public Transform toolTip;
    public float toolTipLineSpacing = 0.2f;
    public float toolTipIconSpacing = 0.2f;

    private Dice die = null;
    private Vector3 tooltipPos;
    private float lineOn = 0;
    private float columnOn = 0;
    private int[] sideCounts;

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
            case DiceSides.Heal1Hp:
                return HealMaterial;
            default:
                Debug.LogError("No material for this kind of side...");
                return BadMaterial;
        }
    }

    public void SetDie( Dice d )
    {
        die = d;

        sideCounts = new int[9];
        for( int i = 0; i < sideCounts.Length; i++ )
        {
            sideCounts[i] = 0;
        }

        for ( int i = 0; i < DiceUnit.DiceSidesNum; i++ )
        {
            SideDisplays[i].material = GetSideMaterial( die.GetSides()[i] );
            sideCounts[(int)die.GetSides()[i]]++;
        }

        for( int i = 0; i < sideCounts.Length; i++ )
        {
            if( sideCounts[i] > 0 )
            {
                GameObject spr = Instantiate( toolTipSprite );
                spr.GetComponent<SpriteRenderer>().sprite = GetSideSprite( (DiceSides)i );
                spr.transform.parent = toolTip;
                spr.transform.localPosition = new Vector3( toolTipIconSpacing * columnOn, -toolTipLineSpacing * lineOn, 0 );
                spr.transform.localRotation = Quaternion.identity;
                columnOn++;
                for( int j = 0; j < sideCounts[i]; j++ )
                {
                    spr = Instantiate( toolTipSprite );
                    spr.GetComponent<SpriteRenderer>().sprite = GetSideColorSprite( (DiceSides)i );
                    spr.transform.parent = toolTip;
                    spr.transform.localPosition = new Vector3( toolTipIconSpacing * columnOn, -toolTipLineSpacing * lineOn, 0 );
                    spr.transform.localRotation = Quaternion.identity;
                    columnOn++;
                }
                columnOn = 0;
                lineOn++;
            }
        }
    }

    Sprite GetSideSprite( DiceSides d )
    {
        switch (d)
        {
            case DiceSides.Attack:
                return AttackSprite;
            case DiceSides.DoubleAttack:
                return DoubleAttackSprite;
            case DiceSides.Defend:
                return DefendSprite;
            case DiceSides.Move:
                return MoveSprite;
            case DiceSides.DoubleMove:
                return DoubleMoveSprite;
            case DiceSides.Nothing:
                return DoNothingSprite;
            case DiceSides.Lose1Hp:
                return HurtSprite;
            case DiceSides.Lose2Hp:
                return DoubleHurtSprite;
            case DiceSides.Heal1Hp:
                return HealSprite;
            default:
                Debug.LogError("No sprite for this kind of side...");
                return null;
        }
    }

    Sprite GetSideColorSprite( DiceSides d )
    {
        switch (d)
        {
            case DiceSides.Attack:
            case DiceSides.DoubleAttack:
                return RedBox;
            case DiceSides.Defend:
                return GreenBox;
            case DiceSides.Move:
            case DiceSides.DoubleMove:
                return BlueBox;
            case DiceSides.Nothing:
            case DiceSides.Lose1Hp:
            case DiceSides.Lose2Hp:
                return BlackBox;
            case DiceSides.Heal1Hp:
                return PinkBox;
            default:
                Debug.LogError("No sprite for this kind of side...");
                return null;
        }
    }

    public Dice GetDie()
    {
        return die;
    }

    public void ShowToolTip()
    {
        toolTip.gameObject.SetActive(true);
    }

    public void HideToolTip()
    {
        toolTip.gameObject.SetActive(false);
    }

    void Start()
    {
        tooltipPos = toolTip.localPosition;
        HideToolTip();
    }

    // Update is called once per frame
    void Update()
    {
        toolTip.position = transform.position + tooltipPos;
        toolTip.rotation = Quaternion.identity;
    }
}
