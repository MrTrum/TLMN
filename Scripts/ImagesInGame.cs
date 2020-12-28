using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImagesInGame : MonoBehaviour
{
    public static ImagesInGame instance;

    public Text txtBetLevel;
    public Text txtNumberTable;
    public GameObject objCountdown;
    public Text txtCountdown;

    public Sprite[] listAvar;

    private void Awake()
    {
        instance = this;
    }

    public Sprite GetAvatar(int id)
    {
        if (id >=0 && id <= 19)
        {
            return listAvar[id];
        }
        return listAvar[0];
    }

    public void ShowRoomInfo(int roomId, long money)
    {
        string betLevel;
        if (money >= 1000)
        {
            betLevel = money / 1000 + "K";
        }
        else
            betLevel = money.ToString();

        txtBetLevel.text = betLevel.ToString();
        txtNumberTable.text = roomId.ToString();
    }

}
