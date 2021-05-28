using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class CurveSliding : MonoBehaviour
{
    public Transform posListRoot;
    public bool isControlChildWidth = false;
    public bool isControlChildHeight = false;

    private ScrollRect scrollRect;
    private Transform content;
    private List<RectTransform> item_transformList = new List<RectTransform>();
    private List<float> posX_List = new List<float>();
    private float scrollRectWidth;
    private float scrollRectHeight;
    private float contentPosX;

    private void Start()
    {
        scrollRect = GetComponent<ScrollRect>();
        content = scrollRect.content.transform;
        scrollRectWidth = scrollRect.GetComponent<RectTransform>().sizeDelta.x;
        scrollRectHeight = scrollRect.GetComponent<RectTransform>().sizeDelta.y;
        for (int i = 0; i < content.childCount; i++)
        {
            RectTransform item_transform = content.GetChild(i) as RectTransform;
            if (item_transform.gameObject.activeInHierarchy)
            {
                item_transformList.Add(item_transform);
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

        OnValueChange(Vector2.zero);

        scrollRect.onValueChanged.AddListener(OnValueChange);
    }

    public void OnValueChange(Vector2 vector)
    {
        for(int i = 0;i < item_transformList.Count;i++)
        {
            float posX = item_transformList[i].anchoredPosition.x + content.localPosition.x;
            //如果当前节点的位置 - content的X轴偏移值 > 滑动列表的宽度，则说明当前item在可视范围外
            if(posX > scrollRectWidth / 2 + 200 || posX < -scrollRectWidth / 2 - 200)
            {
                continue;
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
            //如果index+1小于位置列表的数量，则其在位置区间内，否则应该在位置区间外
            if (index + 1 < posListRoot.childCount && index + 1 > 0)
            {
                float previousPoint_to_currentPoint_distance = posX - posX_List[index];
                float previousPoint_to_nextPoint_distance = posX_List[index + 1] - posX_List[index];
                float ratio = previousPoint_to_currentPoint_distance / previousPoint_to_nextPoint_distance;
                Vector3 newPos = posListRoot.GetChild(index).localPosition - ratio * (posListRoot.GetChild(index).localPosition - posListRoot.GetChild(index + 1).localPosition);
                item_transformList[i].localPosition = new Vector3(item_transformList[i].localPosition.x, -scrollRectHeight / 2 + newPos.y, 0);
                
            }
            else if(index <= -1)
            {
                item_transformList[i].localPosition = new Vector3(item_transformList[i].localPosition.x, -scrollRectHeight / 2 + posListRoot.GetChild(0).localPosition.y, 0);
            }
            else if(index >= posListRoot.childCount - 1)
            {
                item_transformList[i].localPosition = new Vector3(item_transformList[i].localPosition.x, -scrollRectHeight / 2 + posListRoot.GetChild(posListRoot.childCount - 1).localPosition.y, 0);
            }
        }
    }
}
