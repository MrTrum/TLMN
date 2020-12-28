using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundGame : MonoBehaviour
{
    public static SoundGame instance;

    public AudioSource gameSound;

    public AudioClip join;
    public AudioClip back;
    public AudioClip yourTurn;
    public AudioClip pass;
    public AudioClip throwCard;
    public AudioClip veryCard; //sảnh lớn
    public AudioClip threePair;
    public AudioClip fourPair;
    public AudioClip fourOfaKine;
    public AudioClip two;


    public Image btnSoundTrack;
    public Image btnSound;

    public Sprite onSoundTrack;
    public Sprite offSoundTrack;
    public Sprite onSound;
    public Sprite offSound;


    private void Awake()
    {
        instance = this;
    }


    public void ClickOnOffSound()
    {
        gameSound.volume = gameSound.volume == 1f ? 0f : 1f;

        btnSound.sprite = btnSound.sprite == onSound ? offSound : onSound;

    }

}
