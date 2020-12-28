using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TablePrefab : MonoBehaviour
{
    public static TablePrefab instance;

    [Header("Text")]
    public Text txtNumberTable;
    public Text txtTableOwner;
    public Text txtBetLevel;
    public Text txtLoadingBar;

    [Header("Image")]
    public Image imgLoadingBar;



    private void Awake()
    {
        instance = this;
    }

    public int GetRoomID()
    {
        int roomID = int.Parse(txtNumberTable.text);

        return roomID;
    }

    public void RequestJoinRoom()
    {
        LobbyManager.instance.RequestJoinRoom(GetRoomID());
    }
}
