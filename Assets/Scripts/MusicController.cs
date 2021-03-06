using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    AudioSource speaker;

    public AudioClip battleIntro;
    public AudioClip battleLoop;

    public AudioClip placementIntro;
    public AudioClip placementLoop;

    public AudioClip menuIntro;
    public AudioClip menuLoop;

    public AudioClip victory;
    public AudioClip placement;
    public AudioClip blip;

    private AudioClip currentClip;
    private AudioClip nextClip;
    
    // Start is called before the first frame update
    void Awake()
    {
        speaker = GetComponent<AudioSource>();
        currentClip = null;
        nextClip = null;

        //PlayBattleTheme();
    }

    void Update()
    {
        if( !speaker.isPlaying && !speaker.loop && nextClip )
        {
            currentClip = nextClip;
            nextClip = null;
            speaker.loop = true;
            speaker.clip = currentClip;
            speaker.Play();
        }
    }

    void PlayWithIntro( AudioClip intro, AudioClip loop )
    {
        if(!intro || !loop) return;
        speaker.Stop();
        speaker.loop = false;
        currentClip = intro;
        nextClip = loop;
        speaker.clip = currentClip;
        speaker.Play();
    }

    public void PlayBattleTheme()
    {
        PlayWithIntro( battleIntro, battleLoop );
    }

    public void PlayChoosingTheme()
    {
        PlayWithIntro( placementIntro, placementLoop );
    }

    public void PlayMenuTheme()
    {
        PlayWithIntro( menuIntro, menuLoop );
    }

    public void StopAll()
    {
        speaker.Stop();
        currentClip = null;
        nextClip = null;
    }
    
    public void PlayVictory()
    {
        speaker.PlayOneShot(victory);
    }

    public void PlayPlacement()
    {
        speaker.PlayOneShot(placement);
    }

    public void PlayBlip()
    {
        speaker.PlayOneShot(blip);
    }
}
