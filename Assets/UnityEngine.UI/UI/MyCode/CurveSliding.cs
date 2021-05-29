using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class CurveSliding : MonoBehaviour
{
    public Transform posListRoot;
    public float contentwidth;
    public bool isChangeHierarchy = false;

    private ScrollRect scrollRect;
    private RectTransform content;
    private List<Transform> posTransformList = new List<Transform>();
    private List<RectTransform> item_transformList = new List<RectTransform>();
    private List<float> posX_List = new List<float>();
    private List<float> distanceList = new List<float>();
    private float scrollRectWidth;
    private float scrollRectHeight;
    private float posTotalDistance;
    private float spacing = 0;

    private void Start()
    {
        scrollRect = GetComponent<ScrollRect>();
        content = scrollRect.content.transform as RectTransform;
        HorizontalLayoutGroup contentLayoutGroup = content.GetComponentsInChildren<HorizontalLayoutGroup>(true)[0];
        if(contentLayoutGroup != null && contentLayoutGroup.name == content.name)
        {
            spacing = contentLayoutGroup.spacing;
        }
        scrollRectWidth = scrollRect.GetComponent<RectTransform>().sizeDelta.x;
        scrollRectHeight = scrollRect.GetComponent<RectTransform>().sizeDelta.y;
        for (int i = 0; i < content.childCount; i++)
        {
            RectTransform item_transform = content.GetChild(i) as RectTransform;
            if (item_transform.gameObject.activeInHierarchy)
            {
                item_transformList.Add(item_transform);
                posX_List.Add(item_transform.localPosition.x);
            }
            else
            {
                continue;
            }
        }
        float totalDistance = 0;
        for(int i = 0;i < posListRoot.childCount;i++)
        {
            Transform posTransform = posListRoot.GetChild(i);
            if (i > 0)
            {
                Transform previousPosTransform = posListRoot.GetChild(i - 1);
                totalDistance += Vector3.Distance(posTransform.localPosition, previousPosTransform.localPosition);
            }
            posTransformList.Add(posTransform);
            distanceList.Add(totalDistance);
        }
        posTotalDistance = distanceList[distanceList.Count - 1];
        content.sizeDelta = new Vector2(contentwidth, content.sizeDelta.y);
        OnValueChange(Vector2.zero);
        scrollRect.onValueChanged.AddListener(OnValueChange);
    }

    public void OnValueChange(Vector2 vector)
    {
        for(int i = 0;i < item_transformList.Count;i++)
        {
            float localPosX = posX_List[i];
            float posX = localPosX + content.anchoredPosition.x;
            //如果当前节点的位置 - content的X轴偏移值 > 滑动列表的宽度，则说明当前item在可视范围外
            if (posX > posTotalDistance + 200 || posX < 0)
            {
                continue;
            }
            int index = -1;
            foreach(var totalDistance in distanceList)
            {
                if(posX < totalDistance)
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
                float ratio = (posX - distanceList[index]) / (distanceList[index + 1] - distanceList[index]);
                Vector3 newPos = posTransformList[index].localPosition - ratio * (posTransformList[index].localPosition - posTransformList[index + 1].localPosition);
                item_transformList[i].localPosition = new Vector3(newPos.x + scrollRectWidth/2 - content.anchoredPosition.x, -scrollRectHeight / 2 + newPos.y, 0);

            }
            else if(index <= -1)
            {
                item_transformList[i].localPosition = new Vector3(item_transformList[i].localPosition.x, -scrollRectHeight / 2 + posListRoot.GetChild(0).localPosition.y, 0);
            }
            else if(index >= posListRoot.childCount - 1)
            {
                if (i < 1)
                {
                    continue;
                }
                RectTransform previousItem_RectTransform = item_transformList[i - 1];
                item_transformList[i].localPosition = new Vector3(previousItem_RectTransform.localPosition.x + spacing + previousItem_RectTransform.sizeDelta.x/2 + item_transformList[i].sizeDelta.x/2, -scrollRectHeight / 2 + posListRoot.GetChild(posListRoot.childCount - 1).localPosition.y, 0);
            }
            if (isChangeHierarchy)
            {
                item_transformList[i].SetAsFirstSibling();
            }
        }
    }
}
