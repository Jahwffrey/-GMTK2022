using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerUI : MonoBehaviour
{
    public RectTransform TimerObj;

    public void DisplayTime(float pct)
    {
        TimerObj.transform.localScale = new Vector3(pct, 1, 1);
    }
}
