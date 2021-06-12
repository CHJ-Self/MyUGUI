using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PerspectiveImage : MonoBehaviour
{
    /// <summary>
    /// 中心点，中心点可以设置为不是原点
    /// </summary>
    public Vector3 uiCenterPoint = Vector3.zero;

    /// <summary>
    /// 变形的是哪边，up为上，down为下
    /// </summary>
    public PosY posY = PosY.up;

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

    /// <summary>
    /// 滑动列表节点
    /// </summary>
    public RectTransform scrollRectRectTransform;

    [HideInInspector]
    private Image[] images;
    

    private List<RectTransform> childRectTransformList = new List<RectTransform>();
    private float _offset;
    private Vector3[] fourConners= new Vector3[4];
    private RectTransform rectTransform;

    private void Start()
    {
        images = GetComponentsInChildren<Image>();
        rectTransform = GetComponent<RectTransform>();
        for(int i = 0;i < transform.childCount;i++)
        {
            childRectTransformList.Add(transform.GetChild(i).GetComponent<RectTransform>());
        }
    }

    public void ModifyMesh(VertexHelper vh)
    {
        if (!gameObject.activeInHierarchy || images.Length <= 0 || childRectTransformList.Count <= 0)
        {
            return;
        }

        foreach(var item in childRectTransformList)
        {
            Vector3 distanceVector = item.localPosition - uiCenterPoint;
            
        }

    }

    public void ModifyImagesInItem(Vector3 distanceVector, float offsetX, float offsetY)
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

            if (posY == PosY.down)
            {
                vt = vertexs[0];
                vt.position += new Vector3(offsetX, offsetY, 0);
                vh.SetUIVertex(vt, 0);

                vt = vertexs[1];
                vt.position += new Vector3(offsetX, offsetY, 0);
                vh.SetUIVertex(vt, 1);

                vt = vertexs[2];
                vt.position += new Vector3(offsetX, offsetY, 0);
                vh.SetUIVertex(vt, 2);

                vt = vertexs[4];
                vt.position += new Vector3(offsetX, offsetY, 0);
                vh.SetUIVertex(vt, 3);
            }
            else
            {
                vt = vertexs[0];
                vt.position += new Vector3(offsetX, offsetY, 0);
                vh.SetUIVertex(vt, 0);

                vt = vertexs[1];
                vt.position += new Vector3(offsetX, offsetY, 0);
                vh.SetUIVertex(vt, 1);

                vt = vertexs[2];
                vt.position += new Vector3(offsetX, offsetY, 0);
                vh.SetUIVertex(vt, 2);

                vt = vertexs[4];
                vt.position += new Vector3(offsetX, offsetY, 0);
                vh.SetUIVertex(vt, 3);
            }

            Mesh mesh = new Mesh();
            vh.FillMesh(mesh);
            graphic.canvasRenderer.SetMesh(mesh);
        }
    }

    public enum PosY
    {
        up,
        down
    }

    private Vector3 WorldToScreenPoint(RectTransform parent, Vector3 worldPoint)
    {
        //把直接坐标转为屏幕坐标
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, worldPoint);
        //把屏幕坐标转为UGUI坐标
        Vector2 outVector;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPoint, Camera.main, out outVector))
        {
            return new Vector3(outVector.x, outVector.y, 0);
        }
        return new Vector3(outVector.x, outVector.y,0);
    }
}
