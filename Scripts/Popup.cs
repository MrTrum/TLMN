using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Popup : MonoBehaviour
{
    void Start()
    {
        GameManager.instance.ExitGame();
#if UNITY_EDITOR
        Debug.LogError("Mat ket noi");
#endif
    }

}
