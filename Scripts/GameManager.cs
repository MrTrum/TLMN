using com.nope.fishing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public bool isDealCard = false;

    public List<UserInfo> users;

    private List<TKUserInfo> tkUsersInfo;
    public List<TKUserInfo> TKUsersInfo
    {
        get { return tkUsersInfo; }
    }
    public GameObject btnStartGame;

    private bool isMasterClient = false;
    private bool isCountdown = false;

    public static bool isNewGame = true;
    public static bool isPlayingGame = false;
    public static bool isGameOver = false;

    private List<Image> listFlipCard = new List<Image>();

    private bool isOutOfGold = false;

    private void Awake()
    {
        instance = this;
    }

    private void OnEnable()
    {
        RegisterCallback();
    }

    private void OnDisable()
    {
        UnregisterCallback();
    }

    private void RegisterCallback()
    {
        GameManagerServer.Instance.RegisterCallback(MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRespond.ThriteenKillerResponse.LEAVE_ROOM, LeaveRoom);
        GameManagerServer.Instance.RegisterCallback(MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRespond.ThriteenKillerResponse.UPDATE_USERS_JOIN_ROOM, UpdateUsersJoinRoom);
        GameManagerServer.Instance.RegisterCallback(MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRespond.ThriteenKillerResponse.UPDATE_USERS_LEAVE_ROOM, UserLeaveRoom);
        GameManagerServer.Instance.RegisterCallback(MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRespond.ThriteenKillerResponse.COUNT_DOWN, ResponseCountdown);
        GameManagerServer.Instance.RegisterCallback(MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRespond.ThriteenKillerResponse.COUNT_DOWN_AGAIN, ResponseCountdownAgain);
        GameManagerServer.Instance.RegisterCallback(MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRespond.ThriteenKillerResponse.WIN_MONEY, UpdateWinMoney);
        GameManagerServer.Instance.RegisterCallback(MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRespond.ThriteenKillerResponse.THEFINED, UpdateTheFined);
        GameManagerServer.Instance.RegisterCallback(MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRespond.ThriteenKillerResponse.FLIP_CARDS, ResFlipCard);
    }


    private void UnregisterCallback()
    {
        GameManagerServer.Instance.UnregisterCallback(MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRequest.ThriteenKillerRequest.LEAVE_ROOM, LeaveRoom);
        GameManagerServer.Instance.UnregisterCallback(MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRespond.ThriteenKillerResponse.UPDATE_USERS_JOIN_ROOM, UpdateUsersJoinRoom);
        GameManagerServer.Instance.UnregisterCallback(MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRespond.ThriteenKillerResponse.UPDATE_USERS_LEAVE_ROOM, UserLeaveRoom);
        GameManagerServer.Instance.UnregisterCallback(MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRequest.ThriteenKillerRequest.COUNT_DOWN, ResponseCountdown);
        GameManagerServer.Instance.UnregisterCallback(MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRequest.ThriteenKillerRequest.COUNT_DOWN_AGAIN, ResponseCountdownAgain);
        GameManagerServer.Instance.UnregisterCallback(MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRespond.ThriteenKillerResponse.WIN_MONEY, UpdateWinMoney);
        GameManagerServer.Instance.UnregisterCallback(MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRespond.ThriteenKillerResponse.THEFINED, UpdateTheFined);
        GameManagerServer.Instance.UnregisterCallback(MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRespond.ThriteenKillerResponse.FLIP_CARDS, ResFlipCard);
    }


    private bool UpdateWinMoney(int command, int code, byte[] data)
    {
        TKRoomInfo roomInfo = MessageHelper.ParseMessage<TKRoomInfo>(data);

        tkUsersInfo = new List<TKUserInfo>(roomInfo.users);

        SortPosForUsers();

        bool isLostFour = false;

        for (int i = 0; i < tkUsersInfo.Count; i++)
        {
            if (tkUsersInfo[i].nickName != "")
            {
                users[i].effect.SetActiveWinGold(true, tkUsersInfo[i].winGold);
                users[i].txtGold.text = UIHelper.FormatMoneyDot(tkUsersInfo[i].money);

                if (tkUsersInfo[i].isLostFour && tkUsersInfo[i].userId == UserProfile.Instance.UserId)
                    isLostFour = true;

                if (tkUsersInfo[i].money <= 0)
                {
                    isOutOfGold = true;
                }
            }
        }

        if (isLostFour)
        {
            ReFlipCard();
        }

        return true;
    }

    private void ReFlipCard()
    {
        List<TKCardInfo> idCards = new List<TKCardInfo>();
        foreach (var card in Cards.instance.cardsOfMine.Values)
        {
            TKCardInfo cardInfo = new TKCardInfo();
            string key = card.sprite.name;
            cardInfo.cardId = Cards.instance.GetIdCards(key);
            idCards.Add(cardInfo);
        }

        TKCards cards = new TKCards();

        cards.roomId = int.Parse(ImagesInGame.instance.txtNumberTable.text);

        cards.listCard.AddRange(idCards);

        cards.userId = UserProfile.Instance.UserId;

        GameManagerServer.Instance.Send(cards, MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRequest.ThriteenKillerRequest.FLIP_CARDS);
    }

    private bool ResFlipCard(int command, int code, byte[] data)
    {
        TKCards cards = MessageHelper.ParseMessage<TKCards>(data);

        // Nếu là bản thân mình thì k cần chạy dòng code bên dưới
        if (UserProfile.Instance.UserId == cards.userId)
        {
            StartCoroutine(DeleteFlipCard(cards.userId));
            return true;
        }


        // Hiển thị những lá bài còn lại của user thua bét
        List<int> cardListOfFines = cards.cardListOfFines;
        for (int i = 0; i < users.Count; i++)
        {
            if (users[i].userId == cards.userId)
            {
                foreach (var card in cards.listCard)
                {
                    Sprite sprite;
                    Cards.instance.allSpriteCard.TryGetValue(card.cardId, out sprite);

                    Image item = Cards.instance.GetCardPrefabOfGuest();

                    DrawBlack(item, cardListOfFines, card);

                    item.sprite = sprite;

                    listFlipCard.Add(Instantiate(item, users[i].flipCardTranform));

                }
            }
        }

        StartCoroutine(DeleteFlipCard(cards.userId));

        return true;
    }

    private void DrawBlack(Image item, List<int> cardListOfFines, TKCardInfo card)
    {
        if (cardListOfFines != null)
        {
            bool check = false;

            foreach (var id in cardListOfFines)
            {
                if (card.cardId == id)
                {
                    check = true;
                    break;          
                }
            }

            if (!check)
                item.color = Color.gray;
            else
                item.color = Color.white;
        }
    }

    private bool UpdateTheFined(int command, int code, byte[] data)
    {
        TKRoomInfo roomInfo = MessageHelper.ParseMessage<TKRoomInfo>(data);

        tkUsersInfo = new List<TKUserInfo>(roomInfo.users);

        SortPosForUsers();

        for (int i = 0; i < tkUsersInfo.Count; i++)
        {
            if (tkUsersInfo[i].nickName != "")
            {
                users[i].effect.SetActiveFinedGold(true, tkUsersInfo[i].winGold);
                Debug.LogError("user name = " + users[i].txtName);
                users[i].txtGold.text = UIHelper.FormatMoneyDot(tkUsersInfo[i].money);

                if (tkUsersInfo[i].money <= 0)
                {
                    isOutOfGold = true;
                }
            }
        }

        return true;
    }

    private bool ResponseCountdownAgain(int command, int code, byte[] data)
    {
        isPlayingGame = false;

        isGameOver = true;

        // bắt đầu ván mới reset lại hết tất cả các tham số
        ResetAllParameters();

        // Ẩn hết các lá bài trên bàn
        HideAllTheCardsOnTable();

        // Ẩn thanh máu
        Cards.instance.HideBloodBar();

        // Ẩn nút đánh và pass
        ButtonsInGame.instance.HideBtnThrowCardAndPass();

        TKCountdown countdouwn = MessageHelper.ParseMessage<TKCountdown>(data);

        int idMasterClient = countdouwn.userId;

        if (idMasterClient == UserProfile.Instance.UserId)
        {
            isMasterClient = true;

            RequestCountdown(countdouwn.userId, countdouwn.roomId);

            btnStartGame.SetActive(true);
        }

        return true;
    }

    private void HideAllTheCardsOnTable()
    {
        Cards.instance.objCardsGuest.Clear();

        Image[] cards = Cards.instance.cardsOnTable.GetComponentsInChildren<Image>();

        if (cards != null)
            for (int i = 0; i < cards.Length; i++)
                Destroy(cards[i].gameObject);

    }

    public void RequestLeaveRoom()
    {
        TKRoomInfo roomInfo = new TKRoomInfo();
        int roomId = int.Parse(ImagesInGame.instance.txtNumberTable.text);
        roomInfo.roomId = roomId;
        GameManagerServer.Instance.Send(roomInfo, MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRequest.ThriteenKillerRequest.LEAVE_ROOM);
    }

    private bool LeaveRoom(int command, int code, byte[] data)
    {
        try
        {
            TKRoomInfo tKRoomInfo = MessageHelper.ParseMessage<TKRoomInfo>(data);
            if (tKRoomInfo.errorCode == ErrorsTK.PLAYER_NOT_FOUND_IN_GAME)
            {
                DialogSystem.Instance.ShowDialog("Error Leaving The Room", "No players found in the game").SetEvent(null);
            }
        }
        catch (Exception)
        {
            DialogSystem.Instance.ShowDialog("Error Leaving The Room", "Unknown error, please try again").SetEvent(null);
        }

        SceneManagement.Instance.LoadScene(SceneTK.Lobby);

        return true;
    }

    private bool UpdateUsersJoinRoom(int command, int code, byte[] data)
    {
        TKRoomInfo tKRoomInfo = MessageHelper.ParseMessage<TKRoomInfo>(data);

        ImagesInGame.instance.ShowRoomInfo(tKRoomInfo.roomId, tKRoomInfo.betLevel);

        tkUsersInfo = new List<TKUserInfo>(tKRoomInfo.users);

        SortPosForUsers();

        ShowUsersOnTable();

        SoundGame.instance.gameSound.clip = SoundGame.instance.join;
        SoundGame.instance.gameSound.Play();

        return true;
    }


    // Sort lại danh sách cho mình lúc nào cũng nằm gốc dưới màn hình
    private void SortPosForUsers()
    {
        int indexMine = 0;
        for (int i = 0; i < tkUsersInfo.Count; i++)
        {
            if (tkUsersInfo[i].userId == UserProfile.Instance.UserId)
            {
                indexMine = i;
                break;
            }
        }


        if (indexMine > 0 && indexMine < 3)
        {
            // 2 3 4 1
            // 3 4 1 2
            int j = 0;
            for (int i = indexMine; i < tkUsersInfo.Count; i++)
            {
                var temp = tkUsersInfo[j];
                tkUsersInfo[j] = tkUsersInfo[indexMine];
                tkUsersInfo[indexMine] = temp;
                indexMine++;
                j++;
            }
        }

        else if (indexMine == 3)
        {
            // 4 1 2 3
            var temp = tkUsersInfo[indexMine];
            tkUsersInfo.Insert(0, temp);
            tkUsersInfo.RemoveAt(indexMine + 1);
        }
    }

    private void ShowUsersOnTable()
    {
        int numberPlayerInTheTable = 0;
        int posOfMasterClient = 0;

        for (int i = 0; i < tkUsersInfo.Count; i++)
        {

            if (tkUsersInfo[i].nickName == "") // Không có người chơi hoặc đã thoát ra
            {
                users[i].gameObject.SetActive(false);
            }

            else
            {
                users[i].gameObject.SetActive(true);

                numberPlayerInTheTable++;

                if (tkUsersInfo[i].isMasterClient)
                {
                    posOfMasterClient = i;

                    if (UserProfile.Instance.UserId == tkUsersInfo[posOfMasterClient].userId)
                    {
                        isMasterClient = true;
                    }
                }
                users[i].txtName.text = tkUsersInfo[i].nickName;
                users[i].txtGold.text = UIHelper.FormatMoneyDot(tkUsersInfo[i].money);
                users[i].userId = tkUsersInfo[i].userId;

                int idAvata = 0;
                if (tkUsersInfo[i].idAvatar == 0)
                {
                    idAvata = Random.RandomRange(0, 19);
                }

                users[i].avatar.sprite = ImagesInGame.instance.GetAvatar(idAvata);
            }
        }

        if (numberPlayerInTheTable == 1)
            isCountdown = false;


        // Không lấy tkUserInfo.Count vì lúc nào count nó cũng = 4... vì có những index là trống (# vs null)
        if (numberPlayerInTheTable >= 2)
        {
            if (isMasterClient && !isCountdown)
            {
                isCountdown = true; // Phòng lỡ đang count down mà có người mới vào thì k count down lại

                int roomId = int.Parse(ImagesInGame.instance.txtNumberTable.text);

                RequestCountdown(UserProfile.Instance.UserId, roomId);

                btnStartGame.SetActive(true);
            }
        }

        else
        {
            btnStartGame.SetActive(false);
            ImagesInGame.instance.objCountdown.SetActive(false);
        }



    }

    private void RequestCountdown(int userId, int roomId)
    {
        TKCountdown countdown = new TKCountdown();
        countdown.roomId = roomId;
        countdown.userId = userId;

        GameManagerServer.Instance.Send(countdown, MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRequest.ThriteenKillerRequest.COUNT_DOWN);
    }

    private bool ResponseCountdown(int command, int code, byte[] data)
    {
        TKCountdown countdown = MessageHelper.ParseMessage<TKCountdown>(data);
        ImagesInGame.instance.txtCountdown.text = countdown.time + "s";

        if (countdown.time == 14)
        {
            ImagesInGame.instance.objCountdown.SetActive(true);
            ButtonsInGame.instance.ShowBtnBack(true);
        }

        if (countdown.time == 12)
        {
            for (int i = 0; i < users.Count; i++)
            {
                users[i].effect.SetActiveWinGold(false);
                users[i].effect.SetActiveFinedGold(false);
            }
        }

        if (countdown.time <= 0)
        {
            if (!isDealCard && isMasterClient)
            {
                btnStartGame.SetActive(false);
                Cards.instance.ReDealCards();
            }
            ImagesInGame.instance.objCountdown.SetActive(false);
        }

        return true;
    }


    private bool UserLeaveRoom(int command, int code, byte[] data)
    {
        TKRoomInfo tKRoomInfo = MessageHelper.ParseMessage<TKRoomInfo>(data);
        tkUsersInfo = new List<TKUserInfo>(tKRoomInfo.users);

        SortPosForUsers();

        ShowUsersOnTable();

        return true;
    }


    public void ClickStartGame()
    {
        btnStartGame.SetActive(false);

        for (int i = 0; i < users.Count; i++)
        {
            users[i].effect.SetActiveWinGold(false);
            users[i].effect.SetActiveFinedGold(false);
        }

        if (!isDealCard)
            Cards.instance.ReDealCards();
    }

    private IEnumerator DeleteFlipCard(int userId)
    {
        yield return new WaitForSeconds(3f);

        // Xoá những lá bài còn trên tay
        foreach (var card in Cards.instance.cardsOfMine.Values)
        {
            Destroy(card.gameObject);
        }
        Cards.instance.cardsOfMine.Clear();

        // Tắt image Cóng
        foreach (var user in GameManager.instance.users)
        {
            user.imgPenalty.SetActive(false);
        }

        // Nếu hết tiền thì tự out ra khỏi bàn 
        if (isOutOfGold)
        {
            RequestLeaveRoom();
        }

        if (UserProfile.Instance.UserId != userId)
        {
            // Xoá những lá bài lật lên
            foreach (var card in listFlipCard)
            {
                card.color = Color.white;
                Destroy(card.gameObject);
            }
            listFlipCard.Clear();
        }

    }

    private void ResetAllParameters()
    {
        // reset lại master client để phòng khi master client hiện tại out phòng
        // nhằm set lại master mới
        isMasterClient = false;

        // reset lại để có thể thực hiện chia bài
        isDealCard = false;
    }

    private void OnApplicationQuit()
    {
        ExitGame();
    }

    public void ExitGame()
    {
        RequestLeaveRoom();
        Cards.instance.ReNewTurn(true);
    }
}
