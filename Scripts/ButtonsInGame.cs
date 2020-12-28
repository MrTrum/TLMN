using com.nope.fishing;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class ButtonsInGame : MonoBehaviour
{
    public static ButtonsInGame instance;


    public Button btnThrowCard;
    public Button btnBack;

    [SerializeField]
    private GameObject btnPass;

    [HideInInspector]
    public bool isPass;


    private void Awake()
    {
        instance = this;
    }

    public void ShowBtnThrowCardAndPass()
    {
        if (!isPass)
        {
            btnThrowCard.gameObject.SetActive(true);
            btnThrowCard.interactable = false;
            btnPass.SetActive(true);

            SoundGame.instance.gameSound.clip = SoundGame.instance.yourTurn;
            SoundGame.instance.gameSound.Play();
        }
    }

    public void HideBtnThrowCardAndPass()
    {
        btnThrowCard.gameObject.SetActive(false);
        btnPass.SetActive(false);
    }

    public void ThrowCard()
    {
        Cards.instance.ThrowCard();
        SoundGame.instance.gameSound.clip = SoundGame.instance.throwCard;
        SoundGame.instance.gameSound.Play();
    }

    public void Pass()
    {
        Cards.instance.ReNewTurn(true);
        SoundGame.instance.gameSound.clip = SoundGame.instance.pass;
        SoundGame.instance.gameSound.Play();
    }

    public void ShowBtnBack(bool isShow)
    {
        btnBack.interactable = isShow;
    }
}
