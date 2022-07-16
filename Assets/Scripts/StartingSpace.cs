using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartingSpace : MonoBehaviour
{
    private DiceUnit myUnit;
    private bool canEdit = true;
    public int parentPlayerID;
    public int unitType;
    public ParticleSystem dust;
    public ParticleSystem diePlaced;
    
    // Start is called before the first frame update
    void Start()
    {
        myUnit = null;
        unitType = -1;
        parentPlayerID = transform.parent.parent.GetComponent<PlayerControl>().playerID;
    }

    //Returns whether or not successful
    public bool AssignUnit( DiceUnit t, int type, bool playDust = true )
    {
        if( !canEdit ) return false;
        myUnit = t;
        unitType = type;
        if( playDust )
        {
            dust.Play();
        }
        return true;
    }

    //Returns whether or not successful
    public bool RemoveUnit( bool destroyGameObject = true )
    {
        if( !canEdit ) return false;
        if( destroyGameObject )
        {
            myUnit.DoDestroy();
        }
        myUnit = null;
        unitType = -1;
        return true;
    }

    public void PlayDieEffect()
    {
        diePlaced.Play();
    }

    public DiceUnit GetUnit()
    {
        return myUnit;
    }

    public bool HasUnit()
    {
        return myUnit != null;
    }

    public void LockUnit()
    {
        canEdit = false;
    }

    public void UnlockUnit()
    {
        canEdit = true;
    }
    
}
