using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartingSpace : MonoBehaviour
{
    private Transform myUnit;   //TODO: change type to DiceUnit
    private bool canEdit = true;
    public int parentPlayerID;
    public int unitType;
    public ParticleSystem dust;
    
    // Start is called before the first frame update
    void Start()
    {
        myUnit = null;
        unitType = -1;
        parentPlayerID = transform.parent.parent.GetComponent<PlayerControl>().playerID;
    }

    //Returns whether or not successful
    public bool AssignUnit( Transform t, int type, bool playDust = true ) //TODO: change Transform t to a DiceUnit
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
            Destroy( myUnit.gameObject );
        }
        myUnit = null;
        unitType = -1;
        return true;
    }

    public Transform GetUnit() //TODO: change Transform to DiceUnit
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
