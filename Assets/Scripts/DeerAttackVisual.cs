using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeerAttackVisual : MonoBehaviour
{
    public float animDuration = 0.5f;
    public float startScale = 0f;
    public float endScale = 3.2f;

    private float animProgress;
    private bool playing = false;

    void Start()
    {
        animProgress = 0f;
        SetAlpha(0);
        GetComponent<Renderer>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if( playing && animProgress < animDuration )
        {
            animProgress += Time.deltaTime;
            float interp = Mathf.Clamp( animProgress/animDuration, 0, 1 );
            SetAlpha( Mathf.Lerp( 1, 0, interp ) );
            transform.localScale = Vector3.one * Mathf.Lerp( startScale, endScale, interp );
        }
        else
        {
            playing = false;
            animProgress = 0;
            GetComponent<Renderer>().enabled = false;
        }
    }

    public void PlayAnim( float duration )
    {
        GetComponent<Renderer>().enabled = true;
        playing = true;
        animProgress = 0;
        animDuration = duration;
        transform.localScale = Vector3.one * startScale;
        SetAlpha(1);
    }

    private void SetAlpha( float a )
    {
        Color currentCol = GetComponent<Renderer>().material.GetColor( "_Color");
        GetComponent<Renderer>().material.SetColor( "_Color", new Color( currentCol.r, currentCol.g, currentCol.b, a ) );
    }
}
