using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonsInLobby : MonoBehaviour
{
    public static ButtonsInLobby instance;

    [SerializeField]
    private Text txtGold;

    [Header("Create Table")]
    public GameObject createTable;
    public Text txtTableOwner;
    public Toggle togTwoPlayer;
    public Toggle togFourPlayer;
    public Slider slider;

    public Image light10K;
    public Image light20K;
    public Image light50K;
    public Image light100K;

    private long betLevel = Parameters.Bet10K;

    public Image avatar;
    public Sprite[] listAvar;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        light10K.color = Color.white;
    }


    public void ProcessToggle()
    {
        if (slider.value > 10 && slider.value < 25)
        {
            //Bật ligh10
            BrightLights(Color.white, Color.black, Color.black, Color.black);

            slider.value = 10;
            betLevel = Parameters.Bet10K;
        }


        if (slider.value > 25 && slider.value < 55)
        {
            //Bật ligh20
            BrightLights(Color.black, Color.white, Color.black, Color.black);

            slider.value = 40;
            betLevel = Parameters.Bet20K;
        }

        if (slider.value >= 55 && slider.value < 85)
        {
            //Bật ligh50
            BrightLights(Color.black, Color.black, Color.white, Color.black);

            slider.value = 70;
            betLevel = Parameters.Bet50K;
        }

        if (slider.value >= 85)
        {
            //Bật ligh100
            BrightLights(Color.black, Color.black, Color.black, Color.white);

            slider.value = 100;
            betLevel = Parameters.Bet100K;
        }
    }

    private void BrightLights(Color light10, Color light20, Color light50, Color light100)
    {
        light10K.color = light10;
        light20K.color = light20;
        light50K.color = light50;
        light100K.color = light100;
    }

    public void ShowGold(string gold)
    {
        txtGold.text = gold;
    }

    public void ShowCreateTable()
    {
        createTable.SetActive(true);
        txtTableOwner.text = UserProfile.Instance.NickName;
    }

    public void ClickPlayNow()
    {
        LobbyManager.instance.RequestJoinRoomRandom();
    }

    public void HideCreateTable()
    {
        createTable.SetActive(false);
    }

    public long GetBetLevel()
    {
        return betLevel;
    }

    public string GetTableOwner()
    {
        return UserProfile.Instance.UserName;
    }

    public int GetMaxPlayerOfRoom()
    {
        if (togFourPlayer.isOn)
        {
            return 4;
        }
        return 2;
    }

    public void UpdateAvatar(int id)
    {
        var sprite = listAvar[id];
        avatar.sprite = sprite;
    }

}
