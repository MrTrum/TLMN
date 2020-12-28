using com.nope.fishing;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class SortCard : IComparer<string>
{
    public int Compare(string x, string y)
    {
        int idCard_A = GetIDCardA(x);
        int idCard_B = GetIDCardB(y);

        if (idCard_A > idCard_B)
        {
            return 1;
        }

        else if (idCard_A == idCard_B)
        {
            return 0;
        }

        else
            return -1;
    }


    private int GetIDCardA(string name)
    {
        int idCard_A = Cards.instance.GetIdCards(name);
        return idCard_A;
    }

    private int GetIDCardB(string name)
    {
        int idCard_B = Cards.instance.GetIdCards(name);
        return idCard_B;
    }
}

public class Cards : MonoBehaviour
{
    public static Cards instance;

    public static Dictionary<string, int> dicIdCard = new Dictionary<string, int>();

    [HideInInspector]
    public List<string> selectedCards = new List<string>();

    public Dictionary<string, Image> cardsOfMine = new Dictionary<string, Image>();

    public SortedDictionary<string, Image> listObjCard;

    public Image[] cardsPrefab;

    public Image[] refabsOfGuest;

    public Transform cardsOnTable;


    public Dictionary<string, Image> objCardsGuest = new Dictionary<string, Image>();

    [SerializeField]
    private List<Sprite> listSpriteCard;
    public List<Sprite> ListSpriteCard
    {
        get { return listSpriteCard; }
    }

    public Dictionary<int, Sprite> allSpriteCard = new Dictionary<int, Sprite>();

    private float minDistanceBetweenTheCards;

    private List<Image> listImageCard;

    private int indexItemCard = 0;

    private List<TKCardInfo> tkCardInfo = new List<TKCardInfo>();

    private int cardsWereThrown;

    private int idOfUserGoFirst;

    public static int idMinCard;

    public Image bloodBar;

    private Image bloodBarOfGuest = null;

    public bool isLosers;

    private List<int> listOfPreCards = new List<int>();

    private int oldUserId;

    public static bool haveSmallestCard = false;



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
        GameManagerServer.Instance.RegisterCallback(MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRespond.ThriteenKillerResponse.DEAL_CARDS, ResDealCards);
        GameManagerServer.Instance.RegisterCallback(MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRespond.ThriteenKillerResponse.SMALL_CARD_GO_FIRST, GetUserGoFirst);
        GameManagerServer.Instance.RegisterCallback(MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRespond.ThriteenKillerResponse.THROW_CARDS, ResThrowCardOfPrePlayer);
        GameManagerServer.Instance.RegisterCallback(MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRespond.ThriteenKillerResponse.TIME_TO_THROW_CARDS, ResTimeToThrowCards);
        GameManagerServer.Instance.RegisterCallback(MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRespond.ThriteenKillerResponse.OPEN_TURN, OpenTurn);
    }


    private void UnregisterCallback()
    {
        GameManagerServer.Instance.UnregisterCallback(MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRespond.ThriteenKillerResponse.DEAL_CARDS, ResDealCards);
        GameManagerServer.Instance.UnregisterCallback(MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRespond.ThriteenKillerResponse.SMALL_CARD_GO_FIRST, GetUserGoFirst);
        GameManagerServer.Instance.UnregisterCallback(MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRespond.ThriteenKillerResponse.THROW_CARDS, ResThrowCardOfPrePlayer);
        GameManagerServer.Instance.UnregisterCallback(MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRespond.ThriteenKillerResponse.TIME_TO_THROW_CARDS, ResTimeToThrowCards);
        GameManagerServer.Instance.UnregisterCallback(MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRespond.ThriteenKillerResponse.OPEN_TURN, OpenTurn);
    }

    #region Netword

    private bool OpenTurn(int command, int code, byte[] data)
    {
        TKUserPass userPass = MessageHelper.ParseMessage<TKUserPass>(data);

        ButtonsInGame.instance.isPass = userPass.isPass;

        return true;
    }

    private bool GetUserGoFirst(int command, int code, byte[] data)
    {
        TKUserGoFirst userGoFirst = MessageHelper.ParseMessage<TKUserGoFirst>(data);

        idOfUserGoFirst = userGoFirst.userId;

        idMinCard = userGoFirst.cardId;

        haveSmallestCard = CheckSmallest(idMinCard);

        GameManager.isNewGame = userGoFirst.isNewGame;

        return true;
    }

    private bool CheckSmallest(int idMinCard)
    {
        foreach (var card in cardsOfMine.Values)
        {
            int idCard;
            dicIdCard.TryGetValue(card.name, out idCard);
            if (idCard == idMinCard)
            {
                return true;
            }
        }

        return false;
    }


    #region DealCards
    public void ReDealCards()
    {
        TKRoomInfo roomInfo = new TKRoomInfo();

        roomInfo.roomId = int.Parse(ImagesInGame.instance.txtNumberTable.text);

        GameManagerServer.Instance.Send(roomInfo, MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRequest.ThriteenKillerRequest.DEAL_CARDS);
    }

    private bool ResDealCards(int command, int code, byte[] data)
    {
        GameManager.instance.isDealCard = true;

        // Tắt điếm ngược
        ImagesInGame.instance.objCountdown.SetActive(false);

        // Reset những lá bài được đánh ra bằng 0, vì đã bắt đầu ván bài mới
        cardsWereThrown = 0;

        TKCards tkCards = MessageHelper.ParseMessage<TKCards>(data);

        var cardsInfo = tkCards.listCard;

        listObjCard = new SortedDictionary<string, Image>(new SortCard());

        for (int i = 0; i < cardsInfo.Count; i++)
        {
            Sprite newSpriteCard;

            allSpriteCard.TryGetValue(cardsInfo[i].cardId, out newSpriteCard);

            cardsPrefab[i].sprite = newSpriteCard;

            if (!listObjCard.ContainsKey(newSpriteCard.name))
            {
                listObjCard.Add(newSpriteCard.name, cardsPrefab[i]);
            }
        }

        ButtonsInGame.instance.ShowBtnBack(false);

        ShowCard();

        return true;
    }

    #endregion

    #region ThrowCard
    private void ReThrowCard()
    {
        TKCards cards = new TKCards();

        cards.roomId = int.Parse(ImagesInGame.instance.txtNumberTable.text);

        cards.listCard.AddRange(tkCardInfo);

        cards.userId = UserProfile.Instance.UserId;

        GameManagerServer.Instance.Send(cards, MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRequest.ThriteenKillerRequest.THROW_CARDS);

        ReNewTurn(false);
    }

    private bool ResThrowCardOfPrePlayer(int command, int code, byte[] data)
    {
        TKCards cards = MessageHelper.ParseMessage<TKCards>(data);

        var cardsFrServer = cards.listCard;
        GetCardIdFrServer(cardsFrServer);

        // Thông báo nếu user trc đánh lủng
        if (cards.errorCode == ErrorsTK.PENALTY)
        {
            if (UserProfile.Instance.UserId == cards.userId)
            {
                Penalty();
                return true;
            }

            var users = GameManager.instance.users;
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].userId == cards.userId)
                    users[i].imgPenalty.SetActive(true);
            }
        }


        // Hiển thị những lá bài của người trước vừa đánh ra
        for (int i = 0; i < cardsFrServer.Count; i++)
        {
            int id = cardsFrServer[i].cardId;

            float posX = cardsFrServer[i].anchoredPosition.x;

            float posY = cardsFrServer[i].anchoredPosition.y;

            Sprite spriteCard;

            allSpriteCard.TryGetValue(id, out spriteCard);

            string name = spriteCard.name;

            Image item = GetCardPrefabOfGuest();

            if (item != null)
            {
                item.sprite = spriteCard;

                // Set vị trí những lá bài trên table
                item.rectTransform.anchoredPosition = new Vector2(posX, posY);

                #region Get_Anchor
                float anchorMax_X = cardsFrServer[i].anchorMax.x;
                float anchorMax_Y = cardsFrServer[i].anchorMax.y;
                float anchorMin_X = cardsFrServer[i].anchorMin.x;
                float anchorMin_Y = cardsFrServer[i].anchorMin.y;
                #endregion
                item.rectTransform.anchorMax = new Vector2(anchorMax_X, anchorMax_Y);
                item.rectTransform.anchorMin = new Vector2(anchorMin_X, anchorMin_Y);

                item.color = Color.white;

                objCardsGuest.Add(name, Instantiate(item, cardsOnTable));
            }
        }

        // Và nếu tới lượt mình thì bật nút đánh bài
        if (UserProfile.Instance.UserId == cards.userId)
            ButtonsInGame.instance.ShowBtnThrowCardAndPass();

        return true;
    }

    private void GetCardIdFrServer(List<TKCardInfo> cardsFrServer)
    {
        listOfPreCards = new List<int>();

        for (int i = 0; i < cardsFrServer.Count; i++)
        {
            listOfPreCards.Add(cardsFrServer[i].cardId);
        }
    }


    #endregion

    private void ReTimeToThrowCards()
    {
        TKCountdown tKCountdown = new TKCountdown();
        tKCountdown.roomId = int.Parse(ImagesInGame.instance.txtNumberTable.text);
        tKCountdown.userId = UserProfile.Instance.UserId;
        GameManagerServer.Instance.Send(tKCountdown, MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRequest.ThriteenKillerRequest.TIME_TO_THROW_CARDS);
    }

    private bool ResTimeToThrowCards(int command, int code, byte[] data)
    {
        TKCountdown tKCountdown = MessageHelper.ParseMessage<TKCountdown>(data);

        // Nếu k phải lượt của mình thì hiển thị thanh máu để biết user đó còn bao nhiêu thời gian
        if (tKCountdown.userId != UserProfile.Instance.UserId)
        {
            if (oldUserId != tKCountdown.userId)
            {
                if (bloodBarOfGuest != null) bloodBarOfGuest.gameObject.SetActive(false);
                bloodBarOfGuest = GetBloodBar(GameManager.instance.users, tKCountdown.userId);

                oldUserId = tKCountdown.userId;
            }


            if (bloodBarOfGuest != null && !bloodBarOfGuest.gameObject.activeInHierarchy && GameManager.isPlayingGame)
            {
                bloodBar.gameObject.SetActive(false);
                bloodBarOfGuest.gameObject.SetActive(true);
                ButtonsInGame.instance.HideBtnThrowCardAndPass();

            }
        }

        // Là lượt của mình thì bật thanh máu và nút đánh lên
        else
        {
            if (!bloodBar.gameObject.activeInHierarchy && GameManager.isPlayingGame)
            {
                if (bloodBarOfGuest != null) bloodBarOfGuest.gameObject.SetActive(false);

                bloodBar.gameObject.SetActive(true);
                ButtonsInGame.instance.ShowBtnThrowCardAndPass();
            }
        }

        // Thanh máu giảm dần
        BloodBarsTaperedOff(tKCountdown.time);

        // Hết time sẽ tắt thanh máu và button đánh bài
        if (tKCountdown.time > 10f)
        {
            HideBloodBar();

            ButtonsInGame.instance.HideBtnThrowCardAndPass();

            // Yêu cầu chuyển lượt cho người kế tiếp 
            if (tKCountdown.userId == UserProfile.Instance.UserId)
                ReNewTurn(true);

        }

        return true;
    }


    public void ReNewTurn(bool isPass)
    {
        HideBloodBar();

        ButtonsInGame.instance.HideBtnThrowCardAndPass();

        TKUserPass data = new TKUserPass();

        data.userId = UserProfile.Instance.UserId;

        data.roomId = int.Parse(ImagesInGame.instance.txtNumberTable.text);

        data.isPass = ButtonsInGame.instance.isPass = isPass;

        GameManagerServer.Instance.Send(data, MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRequest.ThriteenKillerRequest.NEW_TURN);
    }


    private void ReGoFirst()
    {
        TKUserPenalty user = new TKUserPenalty();

        user.roomId = int.Parse(ImagesInGame.instance.txtNumberTable.text);

        GameManagerServer.Instance.Send(user, MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRequest.ThriteenKillerRequest.GO_FIRST);
    }

    private void RePenalty()
    {
        TKUserPenalty user = new TKUserPenalty();

        user.roomId = int.Parse(ImagesInGame.instance.txtNumberTable.text);

        user.isLoser = true;

        GameManagerServer.Instance.Send(user, MessageCommand.THIRTEEN_KILLER_COMMAND, MessageRequest.ThriteenKillerRequest.GO_FIRST);
    }

    #endregion


    #region Unity

    private void BloodBarsTaperedOff(float time)
    {
        float currentHealt = 10f - time;

        // Thanh máu của mình giảm dần
        if (bloodBar.gameObject.activeInHierarchy)
            bloodBar.fillAmount = currentHealt / 10f;

        // Thanh máu của người chơi khác giảm dần
        else if (bloodBarOfGuest != null && bloodBarOfGuest.gameObject.activeInHierarchy)
            bloodBarOfGuest.fillAmount = currentHealt / 10f;

        Effects.duration = time;

    }

    public void HideBloodBar()
    {
        bloodBar.gameObject.SetActive(false);
        bloodBarOfGuest?.gameObject.SetActive(false);
    }

    private Image GetBloodBar(List<UserInfo> usersInRoom, int userId)
    {
        // Tìm thanh máu của người đang tới lượt đánh
        for (int i = 0; i < usersInRoom.Count; i++)
        {
            if (usersInRoom[i].userId == userId)
            {
                return usersInRoom[i].bloodBar;
            }
        }

        return null;
    }
    public Image GetCardPrefabOfGuest()
    {
        for (int i = 0; i < refabsOfGuest.Length; i++)
        {
            if (!refabsOfGuest[i].gameObject.activeInHierarchy)
            {
                return refabsOfGuest[i];
            }
        }

        return null;
    }

    #region ShowCard
    private void ShowCard()
    {
        indexItemCard = 0;

        listImageCard = new List<Image>(listObjCard.Values);

        InvokeRepeating("ShowCardAfterASecond", 0f, .2f);
    }

    private void ShowCardAfterASecond()
    {
        Image item = listImageCard[indexItemCard];

        cardsOfMine.Add(item.mainTexture.name, Instantiate(item, transform));

        indexItemCard++;

        if (indexItemCard >= listImageCard.Count)
        {
            CancelInvoke("ShowCardAfterASecond");

            // user sẽ dc đánh trước
            if (UserProfile.Instance.UserId == idOfUserGoFirst)
            {
                ButtonsInGame.instance.ShowBtnThrowCardAndPass();
                ReTimeToThrowCards();
            }
            GameManager.isPlayingGame = true;
        }
    }
    #endregion
    private void Start()
    {
        SetIdCards();
    }

    private void SetIdCards()
    {
        int id;
        int index = 0;

        if (dicIdCard.Count == 52 && allSpriteCard.Count == 52)
            return;


        dicIdCard = new Dictionary<string, int>();
        allSpriteCard = new Dictionary<int, Sprite>();

        // Bắt đầu từ 3 bích với Id = 30;
        // Id = 31 là 3 chuồn và cứ tăng dần lên
        for (int i = 3; i < 16; i++)
        {
            id = i * 10;
            for (int j = 0; j < 4; j++)
            {

                string name = ListSpriteCard[index].name;

                allSpriteCard.Add(id, ListSpriteCard[index]);

                dicIdCard.Add(name, id);

                id++;
                index++;
            }
        }
    }

    public int GetIdCards(string key)
    {
        int idCard = 0;

        if (dicIdCard.TryGetValue(key, out idCard))
        {
            return idCard;
        }

        return idCard;
    }

    public void ThrowCard()
    {
        selectedCards.Sort(new SortCard());

        if (selectedCards.Count <= 7)
        {
            MoveCards();
        }

        else
        {
            MoveTheCards();
        }

        // Gửi những lá bài lên server
        ReThrowCard();

        // Kiểm tra có đánh lủng không?
        List<int> newCards = new List<int>();

        // Chuyển card từ name sang ib để CheckCard nhanh hơn
        for (int i = 0; i < selectedCards.Count; i++)
        {
            int idCard = GetIdCards(selectedCards[i]);
            newCards.Add(idCard);
        }

        bool check = CheckCard(newCards);
        if (!check) Penalty();

        // Không còn new game nữa thì người tới nhất sẽ đi trước
        // Cũng như tuỳ ý đi bất kỳ con bài gì, k bị bắt buộc đi con bé nhất nữa
        GameManager.isNewGame = false;

        // Khi cardsWereThrown == 13 tức là đã hết bài. Sau đó yêu cầu server set cho ai đi trước
        if (cardsWereThrown == 13)
            ReGoFirst();

        ResetAllParameter();
    }

    private bool CheckCard(List<int> newCards)
    {
        switch (newCards.Count)
        {
            case 2:
                return TwoCards(newCards);

            case 3:
                return ThreeCards(newCards);

            case 4:
                return FourCard(newCards);

            case 5:
            case 7:
            case 9:
            case 10:
            case 11:
            case 12:
                return FiveCards(newCards);

            case 6:
            case 8:
                return SixCards(newCards);
        }

        return true;
    }

    private bool SixCards(List<int> newCards)
    {
        // Kiểm tra phải là sảnh liền k
        int count = 0;
        for (int i = newCards.Count - 1; i > 0; i--)
        {
            int idCard1 = newCards[i] / 10;
            int idCard2 = newCards[i - 1] / 10;

            int result = idCard1 - idCard2;

            if (result == 1)
                count++;
        }
        #region region_cmt
        // vd: sảnh là 5-6-7-8-9-10 thì 10-9=1(count=1); 9-8=1(count=2); 8-7=1(count=3); 7-6=1(count=4); 6-5=1(count=5)
        // count = 5 và newCards.size() - 1 = 5
        // Thì là sảnh liền và trả về true
        #endregion
        if (count == (newCards.Count - 1))
            return true;

        // Nếu k phải là sảnh thì kiểm tra có phải 3 || 4 đôi thông k
        for (int j = 0; j < newCards.Count - 1; j += 2)
        {
            int idCard1 = newCards[j] / 10;
            int idCard2 = newCards[j + 1] / 10;

            // Nếu 2 quân bài khác nhau thì k phải đôi trả về false
            if (idCard1 != idCard2)
                return false;

            #region regino_cmt
            // Nếu là đôi kiểm tra phải vừa đôi vừa sảnh k
            // Nếu card2 - card1 khác 1 thì không phải sảnh trả về false
            #endregion
            int index = j + 2;
            if (index < newCards.Count)
            {
                idCard2 = newCards[j + 2] / 10;
                if (idCard2 - idCard1 != 1)
                    return false;
            }
        }
        return true;
    }

    private bool FiveCards(List<int> newCards)
    {
        #region regino_cmt
        // Nếu là 5 cây sảnh liền thì cây 5-4= 1 và 4-3=1.... 2-1=1
        // Nếu có cây nào trừ nhau ra khác 1 là lủng
        // vd: sảnh 5-6-7-8-9- thì 9-8=1; 8-7=1; 7-6=1; 6-5=1
        #endregion
        for (int i = newCards.Count - 1; i > 0; i--)
        {
            int idCard1 = newCards[i] / 10;
            int idCard2 = newCards[i - 1] / 10;

            int result = idCard1 - idCard2;

            if (result != 1)
                return false;
        }

        return true;
    }

    private bool FourCard(List<int> newCards)
    {
        int idCard1 = newCards[0] / 10;
        int idCard2 = newCards[1] / 10;
        int idCard3 = newCards[2] / 10;
        int idCard4 = newCards[3] / 10;

        // Nếu là tứ quý
        if (idCard1 == idCard2 && idCard2 == idCard3 && idCard3 == idCard4)
            return true;

        // 4 cây sảnh liền
        else if (idCard4 - idCard3 == 1 && idCard3 - idCard2 == 1 && idCard2 - idCard1 == 1)
            return true;

        return false;
    }

    private bool ThreeCards(List<int> newCards)
    {
        int idCard1 = newCards[0] / 10;
        int idCard2 = newCards[1] / 10;
        int idCard3 = newCards[2] / 10;

        // 3 cây cùng quân bài vd: 333
        if (idCard1 == idCard2 && idCard2 == idCard3)
            return true;

        // 3 cây sảnh liền
        else if (idCard3 - idCard2 == 1 && idCard2 - idCard1 == 1)
            return true;

        return false;
    }


    private bool TwoCards(List<int> newCards)
    {
        int idCard1 = newCards[0] / 10;
        int idCard2 = newCards[1] / 10;

        // Nếu là đôi mà 2 cây là 2 quân bài khác nhau sẽ sai
        if (idCard1 != idCard2)
            return false;

        return true;
    }

    private void Penalty()
    {
        GameManager.instance.users[0].imgPenalty.SetActive(true);

        // Gửi y/c thua bét lên sv
        RePenalty();
    }



    #region MoveCard
    private void MoveCards()
    {
        var listName = selectedCards;

        float posX = listName.Count >= 4 ? Random.Range(-100f, -45f) : Random.Range(-45f, -15f);

        float posY = Random.Range(-40f, 40f);

        for (int i = 0; i < listName.Count; i++)
        {
            string nameCard = listName[i];

            Image card;

            if (cardsOfMine.TryGetValue(nameCard, out card))
            {
                cardsOfMine.Remove(nameCard);

                card.transform.SetParent(cardsOnTable);

                card.rectTransform.anchoredPosition = new Vector2(posX, posY);
                Vector2 anchor = new Vector2(0.5f, 0.5f);
                card.rectTransform.anchorMin = anchor;
                card.rectTransform.anchorMax = anchor;

                SetCardInfoSendToServer(nameCard, posX, posY, anchor, anchor);

                posX += Parameters.DISTANCE_DEFAULT;

                card.GetComponent<EventTrigger>().enabled = false;

                // Cộng 1 khi 1 lá bài được ném ra
                cardsWereThrown++;
            }
        }
    }

    private void SetCardInfoSendToServer(string nameCard, float posX, float posY, Vector2 anchorMax, Vector2 anchorMin)
    {
        TKCardInfo cardInfo = new TKCardInfo();
        cardInfo.anchoredPosition = new TKVector2();
        cardInfo.anchorMax = new TKVector2();
        cardInfo.anchorMin = new TKVector2();

        cardInfo.cardId = GetIdCards(nameCard);
        cardInfo.anchoredPosition.x = posX;
        cardInfo.anchoredPosition.y = posY;
        cardInfo.anchorMax.x = anchorMax.x;
        cardInfo.anchorMax.y = anchorMax.y;
        cardInfo.anchorMin.x = anchorMin.x;
        cardInfo.anchorMin.y = anchorMin.y;

        tkCardInfo.Add(cardInfo);
    }

    private void MoveTheCards()
    {
        var listNameCard = selectedCards;

        float posX = listNameCard.Count < 12 ? Random.Range(10f, 30f) : 0f;
        float posY = Random.Range(-40f, 40f);

        float offset = listNameCard.Count >= 10 ? 8f : 0f;


        for (int i = 0; i < listNameCard.Count; i++)
        {
            string nameCard = listNameCard[i];

            Image card;

            if (cardsOfMine.TryGetValue(nameCard, out card))
            {
                cardsOfMine.Remove(nameCard);

                card.transform.SetParent(cardsOnTable);

                card.rectTransform.anchoredPosition = new Vector2(posX, posY);

                // Sét vị trí những lá bài này cho những user khác. để đồng bộ vị trí
                SetCardInfoSendToServer(nameCard, posX, posY, new Vector2(0, 0.5f), new Vector2(0, 0.5f));

                posX += Parameters.DISTANCE_DEFAULT - offset;

                card.GetComponent<EventTrigger>().enabled = false;

                // Cộng 1 khi 1 lá bài được ném ra
                cardsWereThrown++;
            }
        }


    }
    #endregion


    private void ResetAllParameter()
    {
        // reset lại những lá bài đã chọn
        selectedCards = new List<string>();
        tkCardInfo = new List<TKCardInfo>();
    }

    #endregion

}
