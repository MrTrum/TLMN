using com.nope.fishing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager instance;
    private Dictionary<int, TablePrefab> roomsInHierarchy = new Dictionary<int, TablePrefab>();

    public Transform listTable;
    public TablePrefab tablePrefab;


    private void Awake()
    {
        instance = this;
    }

    private void OnEnable()
    {
        RegisterCallBack();
    }

    private void OnDisable()
    {
        UnRegisterCallBack();
    }


    private void RegisterCallBack()
    {
        GameManagerServer.Instance.RegisterCallback(MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRespond.ThriteenKillerResponse.GAME_INFO, ShowListRoomFromServer);
        GameManagerServer.Instance.RegisterCallback(MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRespond.ThriteenKillerResponse.LOBBY_INFO, LobbyInfo);
        GameManagerServer.Instance.RegisterCallback(MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRespond.ThriteenKillerResponse.TRY_JOIN_ROOM, TryJoinRoom);
    }


    private void UnRegisterCallBack()
    {
        GameManagerServer.Instance.UnregisterCallback(MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRespond.ThriteenKillerResponse.GAME_INFO, ShowListRoomFromServer);
        GameManagerServer.Instance.UnregisterCallback(MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRespond.ThriteenKillerResponse.LOBBY_INFO, LobbyInfo);
        GameManagerServer.Instance.UnregisterCallback(MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRespond.ThriteenKillerResponse.TRY_JOIN_ROOM, TryJoinRoom);
    }

    private void Start()
    {
        GameManagerServer.Instance.Send(MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRequest.ThriteenKillerRequest.GAME_INFO);
    }

    public void CreateRoom()
    {
        long betLevel = ButtonsInLobby.instance.GetBetLevel();
        string tableOwner = ButtonsInLobby.instance.GetTableOwner();
        int maxPlayersOfRoom = ButtonsInLobby.instance.GetMaxPlayerOfRoom();
        int playersJoinedRoom = 0;

        if (UserProfile.Instance.Gold < betLevel * 5)
        {
            DialogSystem.Instance.ShowDialog("Vào phòng lỗi", "Bạn không đủ Gold vui lòng nạp thêm.").SetEvent(null);
            return;
        }

        RequestCreateRoom(betLevel, tableOwner, maxPlayersOfRoom, playersJoinedRoom);
    }

    private void RequestCreateRoom(long betLevel, string tableOwner, int maxPlayersOfRoom, int playersJoinedRoom)
    {
        TKRoomInfo newRoom = new TKRoomInfo();
        newRoom.betLevel = betLevel;
        newRoom.tableOwner = tableOwner;
        newRoom.maxPlayersOfRoom = maxPlayersOfRoom;
        newRoom.playersJoinedRoom = playersJoinedRoom;

        GameManagerServer.Instance.Send(newRoom, MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRequest.ThriteenKillerRequest.CREATE_ROOM);
        int newlyCreatedRoomID = 0;
        RequestJoinRoom(newlyCreatedRoomID);
    }


    private bool LobbyInfo(int command, int code, byte[] data)
    {
        TKGameInfo tkGameInfo = MessageHelper.ParseMessage<TKGameInfo>(data);

        ShowListRoom(tkGameInfo);

        return true;
    }

    private void ShowListRoom(TKGameInfo tkGameInfo)
    {
        var rooms = tkGameInfo.rooms;
        var idOfDeletedRooms = tkGameInfo.iDOfDeletedRooms;


        // Xoá những room đã k còn tồn tại
        for (int i = 0; i < idOfDeletedRooms.Count; i++)
        {
            TablePrefab room;

            if (roomsInHierarchy.TryGetValue(idOfDeletedRooms[i], out room))
            {
                roomsInHierarchy.Remove(idOfDeletedRooms[i]);
                Destroy(room.gameObject);
            }
        }


        // Hiển thị thông tin những room đang hoạt động

        for (int i = 0; i < rooms.Count; i++)
        {
            if (!roomsInHierarchy.ContainsKey(rooms[i].roomId))
            {
                tablePrefab.txtNumberTable.text = rooms[i].roomId.ToString();
                tablePrefab.txtTableOwner.text = rooms[i].tableOwner;
                tablePrefab.txtBetLevel.text = UIHelper.FormatMoneyDot(rooms[i].betLevel);
                tablePrefab.txtLoadingBar.text = rooms[i].playersJoinedRoom + "/" + rooms[i].maxPlayersOfRoom;
                tablePrefab.imgLoadingBar.fillAmount = rooms[i].playersJoinedRoom / (float)rooms[i].maxPlayersOfRoom;

                roomsInHierarchy.Add(rooms[i].roomId, Instantiate(tablePrefab, listTable));
            }
            else
            {
                TablePrefab roomPrefab;
                if (roomsInHierarchy.TryGetValue(rooms[i].roomId, out roomPrefab))
                {
                    roomPrefab.txtNumberTable.text = rooms[i].roomId.ToString();
                    roomPrefab.txtTableOwner.text = rooms[i].tableOwner;
                    roomPrefab.txtBetLevel.text = UIHelper.FormatMoneyDot(rooms[i].betLevel);
                    roomPrefab.txtLoadingBar.text = rooms[i].playersJoinedRoom + "/" + rooms[i].maxPlayersOfRoom;
                    roomPrefab.imgLoadingBar.fillAmount = rooms[i].playersJoinedRoom / (float)rooms[i].maxPlayersOfRoom;
                }
            }
        }
    }

    private bool ShowListRoomFromServer(int command, int code, byte[] data)
    {
        TKGameInfo tkGameInfo = MessageHelper.ParseMessage<TKGameInfo>(data);

        // Hiển thị gold trong lobby
        string gold = UIHelper.FormatMoneyDot(tkGameInfo.money);

        ButtonsInLobby.instance.ShowGold(gold);

        ButtonsInLobby.instance.UpdateAvatar(tkGameInfo.avatar);

        ShowListRoom(tkGameInfo);

        return true;
    }

    public void RequestJoinRoomRandom()
    {
        GameManagerServer.Instance.Send(MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRequest.ThriteenKillerRequest.JOIN_ROOM_RANDOM);
    }

    public void RequestJoinRoom(int roomID)
    {
        TKRoomInfo tKRoomInfoRequest = new TKRoomInfo();
        tKRoomInfoRequest.roomId = roomID;
        GameManagerServer.Instance.Send(tKRoomInfoRequest.roomId, MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRequest.ThriteenKillerRequest.TRY_JOIN_ROOM);
    }


    private bool TryJoinRoom(int command, int code, byte[] data)
    {
        try
        {
            TKRoomInfo tKRoomInfo = MessageHelper.ParseMessage<TKRoomInfo>(data);

            if (tKRoomInfo.errorCode == 0)
            {
                SceneManagement.Instance.LoadScene(SceneTK.Game);
                GameManagerServer.Instance.Send(tKRoomInfo, MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRequest.ThriteenKillerRequest.JOIN_ROOM);
            }

            else if (tKRoomInfo.errorCode == ErrorsTK.NOT_ENOUGH_GOLD)
            {
                DialogSystem.Instance.ShowDialog("Vào phòng lỗi", "Bạn không đủ Gold vui lòng nạp thêm.").SetEvent(null);
            }

            else if (tKRoomInfo.errorCode == ErrorsTK.FULL_ROOM)
            {
                DialogSystem.Instance.ShowDialog("Vào phòng lỗi", "Phòng đầy bạn vui lòng qua phòng khác hoặc tạo phòng mới.").SetEvent(null);
            }

            else
                DialogSystem.Instance.ShowDialog("Vào phòng lỗi", "Không tìm thấy phòng phù hợp.").SetEvent(null);
        }
        catch (Exception)
        {
            DialogSystem.Instance.ShowDialog("Vào phòng lỗi", "Lỗi không xác định, vui lòng thử lại").SetEvent(null);
        }

        return true;
    }


    public void BackHome()
    {
        SceneManagement.Instance.LoadScene("HomeScene");
    }



}
