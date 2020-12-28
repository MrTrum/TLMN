using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoldPlusEffect : MonoBehaviour
{

    public Sprite plus;

    public Sprite sub;

    private Sprite prefix;

    public List<Sprite> imgWin;

    public List<Sprite> imgLose;

    [Header("Win Gold")]
    public List<Image> listTextGold;

    [Header("Fined Gold")]
    public List<Image> listTextFinedGold;

    private List<Sprite> sprites = new List<Sprite>();

    public float duration;

    private float time = 0;

    private float scale = 0;

    private static Dictionary<string, Sprite> allSprite = new Dictionary<string, Sprite>();

    private bool oneCall = false;

    public GameObject imgGold;

    public GameObject imgFinedGold;


    private void Awake()
    {
        if (!oneCall)
        {
            for (int i = 0; i < imgWin.Count; i++)
                if (!allSprite.ContainsKey(imgWin[i].name))
                    allSprite.Add(imgWin[i].name, imgWin[i]);

            for (int i = 0; i < imgLose.Count; i++)
                if (!allSprite.ContainsKey(imgLose[i].name))
                    allSprite.Add(imgLose[i].name, imgLose[i]);
        }
    }

    public void SetActiveWinGold(bool isOn, long winGold = 0)
    {
        imgGold.SetActive(isOn);
        if (winGold != 0)
            RunEffectGoldPlus(winGold, listTextGold);
    }

    public void SetActiveFinedGold(bool isOn, long winGold = 0)
    {
        imgFinedGold.SetActive(isOn);
        Debug.LogError("SetActiveFinedGold = true");
        if (winGold != 0)
            RunEffectGoldPlus(winGold, listTextFinedGold);
    }

    public void RunEffectGoldPlus(long winGold, List<Image> listImgGold)
    {
        if (winGold > 0)
            prefix = plus;
        else
            prefix = sub;


        StartCoroutine(WaitOneSec(winGold, listImgGold));
    }


    private IEnumerator WaitOneSec(long winGold, List<Image> listImgGold)
    {
        yield return new WaitForSeconds(0.5f);

        StartCoroutine(RunEffect(winGold, listImgGold));

        yield return new WaitForSeconds(3f);
        imgGold.SetActive(false);
        HideTextGold();
    }

    private IEnumerator RunEffect(long winGold, List<Image> listImgGold)
    {
        sprites.Clear();
        float time = 0;

        while (time <= duration)
        {
            time += Time.deltaTime * 2;
            scale = time / duration;

            float newGold = Mathf.Lerp(0, winGold, scale);

            sprites = GetSprites((int)newGold);

            listImgGold[0].gameObject.SetActive(true);
            listImgGold[0].sprite = prefix;

            for (int i = 0; i < sprites.Count; i++)
            {
                listImgGold[i + 1].gameObject.SetActive(true);
                listImgGold[i + 1].sprite = sprites[i];
            }

            yield return null;
        }
    }

    private void HideTextGold()
    {
        int size = listTextGold.Count;


        for (int i = 0; i < size; i++)
        {
            listTextGold[i].gameObject.SetActive(false);
            listTextFinedGold[i].gameObject.SetActive(false);
        }


    }

    private List<Sprite> GetSprites(int newGold)
    {
        List<Sprite> newListSprite = new List<Sprite>();

        while (newGold != 0)
        {
            int value = newGold % 10;

            string key = value + "";

            if (newGold < 0 && value == 0)
            {
                key = "-0";
            }

            Sprite sprite;


            allSprite.TryGetValue(key, out sprite);

            newListSprite.Add(sprite);

            newGold /= 10;
        }

        newListSprite.Reverse();

        return newListSprite;

    }
}
