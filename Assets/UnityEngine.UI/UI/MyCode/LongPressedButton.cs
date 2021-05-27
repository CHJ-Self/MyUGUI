using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LongPressedButton : Selectable
{
    public int frames = 60;
    public UnityEvent onClick = new UnityEvent();
    public UnityEvent onLongPressBegin = new UnityEvent();
    public UnityEvent onLongPress = new UnityEvent();
    public UnityEvent onLongPressEnd = new UnityEvent();

    private int timer = 0;
    private bool isDown = false;
    private bool isLongPress = false;
    private bool hasLongPressStart = false;

    public override void OnPointerDown(PointerEventData eventData)
    {
        if(!isDown && !isLongPress)
        {
            isDown = true;
        }
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if(!isLongPress)
        {
            EventSystem.current.SetSelectedGameObject(this.gameObject);
            onClick.Invoke();
        }
        else
        {
            onLongPressEnd.Invoke();
        }
        isDown = false;
        isLongPress = false;
        hasLongPressStart = false;
        timer = 0;
    }

    protected override void Start()
    {
        timer = 0;
        isDown = false;
        isLongPress = false;
        hasLongPressStart = false;
    }

    void Update()
    {
        if(isDown)
        {
            timer++;
            if(timer >= frames)
            {
                isLongPress = true;
                if(!hasLongPressStart)
                {
                    hasLongPressStart = true;
                    onLongPressBegin.Invoke();
                }
                onLongPress.Invoke();
            }
        }
    }
}
