using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhichPlayerIndicator : MonoBehaviour
{
    public Material Player1Material;
    public Material Player2Material;

    protected DiceUnit Unit;
    public void Setup(DiceUnit unit)
    {
        Unit = unit;
        if (unit.Player1)
        {
            GetComponent<Renderer>().material = Player1Material;
        }
        else
        {
            GetComponent<Renderer>().material = Player2Material;
        }
    }

    private void Update()
    {
        if (Unit != null)
        {
            transform.position = Unit.transform.position + Vector3.up * 2f;
        }
    }
}
