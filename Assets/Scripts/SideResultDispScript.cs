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
    protected float riseHeight = 0.4f;
    protected float riseEase = 0f;
    protected float TimeCreated;
    protected float eventualSize = 3f;

    protected DiceUnit Unit;

    public void SetDisp(DiceSides side, DiceUnit unit)
    {
        transform.localScale = Vector3.one * 0.01f;
        GetComponent<SpriteRenderer>().sprite = Sprites[(int)side];
        TimeCreated = Time.time;
        RiseSpeedDeceleration = RiseSpeedDeceleration * (0.95f + 0.05f * Random.value);
        RiseSpeed = RiseSpeed + 0.1f * Random.value;
        Unit = unit;
    }

    Vector3 risenDelta = Vector3.zero;

    private void Update()
    {
        if(Unit == null)
        {
            Destroy(gameObject);
            return;
        }

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
            riseEase += Time.deltaTime / RiseDuration;
            float targetY = EaseOut( riseEase, 5 ) * riseHeight;
            risenDelta = Vector3.up * targetY;
        }
        transform.position = Unit.transform.position + Vector3.up * 1.33f + risenDelta;
    }

    float Flip( float x )
    {
        return 1 - x;
    }

    float EaseOut( float x, float power )
    {
        return Flip( Mathf.Pow( Flip(x), power) );
    }
}
