using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class CurveSliding : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IScrollHandler
{
    public Transform posListRoot;
    public bool isControlChildWidth = false;
    public bool isControlChildHeight = false;

    private ScrollRect scrollRect;
    private Transform content;
    private List<RectTransform> transformList = new List<RectTransform>();
    private List<float> posX_List = new List<float>();
    private float scrollRectWidth;
    private float contentPosX;

    private void Start()
    {
        scrollRect = GetComponent<ScrollRect>();
        content = scrollRect.content.transform;
        scrollRectWidth = scrollRect.GetComponent<RectTransform>().sizeDelta.x;
        for (int i = 0; i < content.childCount; i++)
        {
            RectTransform item_transform = content.GetChild(i) as RectTransform;
            if (item_transform.gameObject.activeInHierarchy)
            {
                transformList.Add(item_transform);
            }
            else
            {
                continue;
            }
            /*
            HorizontalLayoutGroup horizontalLayoutGroup = content.GetComponent<HorizontalLayoutGroup>();
            VerticalLayoutGroup verticalLayoutGroup = content.GetComponent<VerticalLayoutGroup>();
            ContentSizeFitter contentSizeFitter = content.GetComponent<ContentSizeFitter>();
            if (contentSizeFitter != null && contentSizeFitter.enabled)
            {
                contentSizeFitter.enabled = false;
            }
            if (horizontalLayoutGroup != null && horizontalLayoutGroup.enabled)
            {
                horizontalLayoutGroup.enabled = false;
            }
            if (verticalLayoutGroup != null && verticalLayoutGroup.enabled)
            {
                verticalLayoutGroup.enabled = false;
            }
            */
        }
        for(int i = 0;i < posListRoot.childCount;i++)
        {
            posX_List.Add(posListRoot.GetChild(i).transform.localPosition.x);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        print(transformList.Count);
    }

    public void OnDrag(PointerEventData eventData)
    {
        for(int i = 0;i < transformList.Count;i++)
        {
            float posX = transformList[i].anchoredPosition.x;
            //如果当前节点的位置 - content的X轴偏移值 > 滑动列表的宽度，则说明当前item在可视范围外
            if(posX + content.localPosition.x > scrollRectWidth || posX + content.localPosition.x < 0)
            {
                break;
            }
            int index = -1;
            foreach(var posX_inlist in posX_List)
            {
                if(posX < posX_inlist)
                {
                    break;
                }
                else
                {
                    index++;
                }
            }
            print(index);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        print("end drag");
    }

    public void OnScroll(PointerEventData eventData)
    {
        print("scroll");
    }
}
