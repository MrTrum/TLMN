using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public RectTransform card;
    public Image image;

    public void ClickCard(int distance)
    {
        // -34 là chiều cao mặc định của lá bài
        if (card.anchoredPosition.y > -34f)
        {
            distance = -distance;

            Cards.instance.selectedCards.Remove(image.mainTexture.name);

            if (Cards.instance.selectedCards.Count == 0)
            {
                ButtonsInGame.instance.btnThrowCard.interactable = false;
            }
        }

        else
        {
            Cards.instance.selectedCards.Add(image.mainTexture.name);

            if (GameManager.isNewGame && Cards.haveSmallestCard)
            {
                int idCard = 0;

                Cards.dicIdCard.TryGetValue(image.mainTexture.name, out idCard);

                Debug.LogError("new game");

                if (idCard == Cards.idMinCard)
                    ButtonsInGame.instance.btnThrowCard.interactable = true;
            }

            else
                ButtonsInGame.instance.btnThrowCard.interactable = true;
        }

        card.anchoredPosition += new Vector2(0f, distance);
    }
}

