using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PerspectiveImage : MonoBehaviour
{
    public float centerX = 0f;
    public PosY posY = PosY.middle;
    public float perspective = 0;
    public float offset = 0;
    [HideInInspector]
    public Image[] images;

    private RectTransform rectTransform;
    private float _offset;

    private void Start()
    {
        images = GetComponentsInChildren<Image>();
    }

    public void ModifyMesh(VertexHelper vh)
    {
        if (!gameObject.activeInHierarchy || images.Length <= 0)
        {
            return;
        }

        for(int i = 0;i < images.Length;i++)
        {
            Graphic graphic = images[i];
            graphic.OnPopulateMesh_Public(vh);

            rectTransform = GetComponent<RectTransform>();
            var vertexs = new List<UIVertex>();
            vh.GetUIVertexStream(vertexs);

            UIVertex vt;

            Vector2 centerPoint;
            try
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(graphic.rectTransform, Camera.main.WorldToScreenPoint(new Vector3(centerX, 0, 0)), graphic.canvas.worldCamera, out centerPoint);
            }
            catch (Exception e)
            {
                print(e);
                return;
            }

            _offset = offset;
            if (centerPoint.x <= 0)
            {
                _offset = -offset;
            }

            if (posY == PosY.down)
            {
                vt = vertexs[0];
                vt.position = new Vector3((centerPoint.x - vt.position.x) / 10f * perspective + offset, 0, 0);
                vh.SetUIVertex(vt, 0);

                vt = vertexs[1];
                vt.position += new Vector3(vt.position.x + offset, vt.position.y, vt.position.z);
                vh.SetUIVertex(vt, 1);

                vt = vertexs[2];
                vt.position += new Vector3(vt.position.x + offset, vt.position.y, vt.position.z);
                vh.SetUIVertex(vt, 2);

                vt = vertexs[4];
                vt.position = new Vector3((centerPoint.x - vt.position.x) / 10f * perspective + offset, 0, 0);
                vh.SetUIVertex(vt, 3);
            }
            else if (posY == PosY.middle)
            {
                vt = vertexs[0];
                vt.position += new Vector3((centerPoint.x - vt.position.x - vt.position.y) / 10f * perspective + offset, 0, 0);
                vh.SetUIVertex(vt, 0);

                vt = vertexs[1];
                vt.position += new Vector3((centerPoint.x + vt.position.x - vt.position.y) / 10f * perspective + offset, 0, 0);
                vh.SetUIVertex(vt, 1);

                vt = vertexs[2];
                vt.position += new Vector3((centerPoint.x + vt.position.x - vt.position.y) / 10f * perspective + offset, 0, 0);
                vh.SetUIVertex(vt, 2);

                vt = vertexs[4];
                vt.position += new Vector3((centerPoint.x - vt.position.x - vt.position.y) / 10f * perspective + offset, 0, 0);
                vh.SetUIVertex(vt, 3);
            }
            else
            {
                vt = vertexs[0];
                vt.position = new Vector3((centerPoint.y - vt.position.y) / 100 * (vt.position.x - centerPoint.x) * perspective, vt.position.y, vt.position.z);
                print(vt.position.x);
                //OUT.worldPosition.x += (-v.vertex.y - _OffsetX) / 1000 * v.vertex.x * _OffsetPerspective;
                //OUT.worldPosition.x += (_CenterY - v.vertex.y) / 1000 * (v.vertex.x - _CenterX) * _OffsetX;
                vh.SetUIVertex(vt, 0);

                vt = vertexs[1];
                vt.position += new Vector3((centerPoint.x - vt.position.x) / 10f * perspective + _offset, 0, 0);
                vh.SetUIVertex(vt, 1);

                vt = vertexs[2];
                vt.position += new Vector3((centerPoint.x - vt.position.x) / 10f * perspective + _offset, 0, 0);
                vh.SetUIVertex(vt, 2);

                vt = vertexs[4];
                vt.position = new Vector3((centerPoint.y - vt.position.y) / 100 * (vt.position.x - centerPoint.x) * perspective, vt.position.y, vt.position.z);
                print(vt.position.x);
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
        middle,
        down
    }
}
