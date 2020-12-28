using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Effects : MonoBehaviour
{
    public static Effects effectPar;

    public float centerX;
    public float centerY;
    public float speed;
    public static float duration;
    public Image par;

    private void Awake()
    {
        effectPar = this;
    }

    private void OnDisable()
    {
        par.rectTransform.anchoredPosition = Vector2.zero;
        duration = 0;
    }

    private void Update()
    {
        float x = Mathf.Sin(duration * speed) * centerX;
        float y = Mathf.Cos(duration * speed) * centerY;

        par.rectTransform.anchoredPosition = new Vector2(x, y);
    }

}
