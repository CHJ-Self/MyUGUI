using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.UI.Button;

public class LongPressedButton : Selectable
{
    public int frames = 120;
    public ButtonClickedEvent onClick = new ButtonClickedEvent();
    public ButtonClickedEvent onLongPressBegin = new ButtonClickedEvent();
    public ButtonClickedEvent onLongPress = new ButtonClickedEvent();
    public ButtonClickedEvent onLongPressEnd = new ButtonClickedEvent();

    private int timer = 0;
    private bool isDown = false;
    private bool isLongPress = false;
    private bool hasLongPressStart = false;

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (!isDown && !isLongPress)
        {
            isDown = true;
        }
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if(!isLongPress)
        {
            // Selection tracking
            if (IsInteractable() && navigation.mode != Navigation.Mode.None && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(gameObject, eventData);
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
