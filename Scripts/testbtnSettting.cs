using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testbtnSettting : MonoBehaviour
{
    public Animator anim;
    private bool isPressSetting;

    public void PressSetting()
    {
        isPressSetting = !isPressSetting;
        anim.SetBool("isPressSetting", isPressSetting);
    }
}
