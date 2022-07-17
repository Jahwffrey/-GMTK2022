using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SideResultDispScript : MonoBehaviour
{
    public Sprite[] Sprites;

    protected float RiseSpeed = 0.7f;
    protected float RiseSpeedDeceleration = 0.95f;
    protected float GrowDuration = 0.1f;
    protected float RiseDuration = 1f;
    protected float TimeCreated;
    protected float eventualSize = 3f;

    public void SetDisp(DiceSides side)
    {
        transform.localScale = Vector3.one * 0.01f;
        GetComponent<SpriteRenderer>().sprite = Sprites[(int)side];
        TimeCreated = Time.time;
        RiseSpeedDeceleration = RiseSpeedDeceleration * (0.95f + 0.05f * Random.value);
        RiseSpeed = RiseSpeed + 0.1f * Random.value;
    }

    private void Update()
    {
        transform.forward = Camera.main.transform.forward;

        if(Time.time - TimeCreated < GrowDuration)
        {
            transform.localScale = Vector3.one * ((Time.time - TimeCreated)/ GrowDuration) * eventualSize;
        }
        else
        {
            transform.localScale = Vector3.one * eventualSize;
        }

        if(Time.time - TimeCreated < RiseDuration)
        {
            transform.position += Vector3.up * RiseSpeed * Time.deltaTime;
            RiseSpeed *= 0.99f;
            //RiseSpeed *= RiseSpeedDeceleration;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
