using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongPressedButtonTest : MonoBehaviour
{
    public void OnLongPressBegin()
    {
        print("开始长按");
    }

    public void OnLongPress()
    {
        print("正在长按");
    }

    public void OnLongPressEnd()
    {
        print("长按结束");
    }

    public void OnShortClick()
    {
        print("点击");
    }
}
