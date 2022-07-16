using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public GameObject Bar;

    protected DiceUnit Unit;

    public void Setup(DiceUnit unit)
    {
        Unit = unit;
    }


    public void DisplayAmt(float pct)
    {
        Bar.transform.localScale = new Vector3(pct, 1f, 1f);
    }

    private void Update()
    {
        transform.position = Unit.transform.position + Vector3.up * 0.75f;
        transform.forward = -Camera.main.transform.forward;
    }
}
