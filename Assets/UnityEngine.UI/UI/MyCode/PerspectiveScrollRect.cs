using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PerspectiveScrollRect : MonoBehaviour
{
    /// <summary>
    /// 中心点，中心点可以设置为不是原点
    /// </summary>
    public Vector3 uiCenterPoint = Vector3.zero;

    /// <summary>
    /// 图片左边的回拉像素，每距离中心点100个像素往回拉的距离
    /// </summary>
    [Range(0,100)]
    public float perspective_left = 0;

    /// <summary>
    /// 图片右边的回拉像素，每距离中心点100个像素往回拉的距离
    /// </summary>
    [Range(0, 100)]
    public float perspective_right = 0;

    public ScrollRect scrollRect;

    /// <summary>
    /// 滑动列表节点
    /// </summary>
    private RectTransform scrollRectRectTransform;
    private List<RectTransform> childRectTransformList = new List<RectTransform>();
    private RectTransform rectTransform;

    private void Start()
    {
        scrollRectRectTransform = scrollRect.GetComponent<RectTransform>();
        rectTransform = GetComponent<RectTransform>();
        for(int i = 0;i < transform.childCount;i++)
        {
            if(transform.GetChild(i).gameObject.activeInHierarchy)
            {
                childRectTransformList.Add(transform.GetChild(i).GetComponent<RectTransform>());
            }
        }
        scrollRect.onValueChanged.AddListener(UpdataChilds);
        UpdataChilds(Vector2.zero);
    }

    void UpdataChilds(Vector2 vector2)
    {
        ModifyMesh(new VertexHelper());
    }

    public void ModifyMesh(VertexHelper vh)
    {
        if (!gameObject.activeInHierarchy || childRectTransformList.Count <= 0)
        {
            return;
        }

        foreach(var item in childRectTransformList)
        {
            float offset_left;
            float offset_right;
            Vector3 distanceVector = new Vector3(item.localPosition.x - scrollRectRectTransform.sizeDelta.x / 2 + rectTransform.anchoredPosition.x, item.localPosition.y, 0) - uiCenterPoint;
            //distanceVector.x小于0则证明当前节点在中心点右边，设置的透视左右值需要反过来
            if (distanceVector.x < 0)
            {
                offset_left = -perspective_left * distanceVector.x / 100f;
                offset_right = -perspective_right * distanceVector.x / 100f;
            }
            else
            {
                offset_left = -perspective_right * distanceVector.x / 100f;
                offset_right = -perspective_left * distanceVector.x / 100f;
            }
            Image[] images = item.GetComponentsInChildren<Image>();
            ModifyImagesInItem(offset_left, offset_right, images, item.sizeDelta.y);
        }

    }

    public void ModifyImagesInItem(float offset_left, float offset_right, Image[] images, float itemHeight)
    {
        VertexHelper vh = new VertexHelper();
        for (int i = 0; i < images.Length; i++)
        {
            Graphic graphic = images[i];
            vh.Clear();
            graphic.OnPopulateMesh_Public(vh);

            var vertexs = new List<UIVertex>();
            vh.GetUIVertexStream(vertexs);

            UIVertex vt;
            float ratio0;
            float ratio1;
            float ratio2;
            float ratio3;
            float graphicPosY = Mathf.Abs(graphic.rectTransform.localPosition.y);

            vt = vertexs[0];
            ratio0 = (Mathf.Abs(vt.position.y) + graphicPosY) / itemHeight;
            vt.position += new Vector3(offset_left * ratio0, 0, 0);
            vh.SetUIVertex(vt, 0);

            vt = vertexs[1];
            ratio1 = (Mathf.Abs(vt.position.y) + graphicPosY) / itemHeight;
            vt.position += new Vector3(offset_left * ratio1, 0, 0);
            vh.SetUIVertex(vt, 1);

            vt = vertexs[2];
            ratio2 = (Mathf.Abs(vt.position.y) + graphicPosY) / itemHeight;
            vt.position += new Vector3(offset_right * ratio2, 0, 0);
            vh.SetUIVertex(vt, 2);

            vt = vertexs[4];
            ratio3 = (Mathf.Abs(vt.position.y) + graphicPosY) / itemHeight;
            vt.position += new Vector3(offset_right * ratio3, 0, 0);
            vh.SetUIVertex(vt, 3);

            Mesh mesh = new Mesh();
            vh.FillMesh(mesh);
            graphic.canvasRenderer.SetMesh(mesh);
            MeshCollider meshCollider = graphic.GetComponent<MeshCollider>();
            if(meshCollider != null)
            {
                meshCollider.sharedMesh = mesh;
            }
        }
    }
}
