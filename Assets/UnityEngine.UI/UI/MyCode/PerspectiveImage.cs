using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class PerspectiveImage : BaseMeshEffect
{
    public float centerX = 0f;
    public PosY posY = PosY.middle;
    public float perspective = 0;
    public float offset = 0;
    [HideInInspector]
    public Image image;

    private RectTransform rectTransform;

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive()/* || vh.currentVertCount == 0*/)
        {
            return;
        }

        Graphic graphic = GetComponent<Graphic>();
        if (graphic == null)
        {
            return;
        }

        graphic.OnPopulateMesh_Public(vh);

        rectTransform = GetComponent<RectTransform>();
        var vertexs = new List<UIVertex>();
        vh.GetUIVertexStream(vertexs);

        UIVertex vt;

        Vector2 centerPoint;
        try
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(graphic.rectTransform, Camera.main.WorldToScreenPoint(new Vector3(centerX, 0, 0)), graphic.canvas.worldCamera, out centerPoint);
        }catch(Exception e)
        {
            print(e);
            return;
        }
        
        //print(centerPoint.x - vertexs[0].position.x);
        //print(centerPoint.x - vertexs[1].position.x);
        //print(centerPoint.x - vertexs[2].position.x);
        //print(centerPoint.x - vertexs[3].position.x);
        //print(centerPoint.x - vertexs[4].position.x);
        //print(centerPoint.x - vertexs[5].position.x);
        if(posY == PosY.down)
        {
            vt = vertexs[0];
            vt.position += new Vector3((centerPoint.x - vt.position.x) / 10f * perspective + offset, 0, 0);
            vh.SetUIVertex(vt, 0);
            
            vt = vertexs[4];
            vt.position += new Vector3((centerPoint.x - vt.position.x) / 10f * perspective + offset, 0, 0);
            vh.SetUIVertex(vt, 3);
        }
        else if(posY == PosY.middle)
        {
            vt = vertexs[0];
            vt.position += new Vector3((centerPoint.x - vt.position.x - vt.position.y) / 10f * perspective + offset, 0, 0);
            vh.SetUIVertex(vt, 0);
            print(vt.position.y);
            vt = vertexs[1];
            vt.position += new Vector3((centerPoint.x + vt.position.x - vt.position.y) / 10f * perspective + offset, 0, 0);
            vh.SetUIVertex(vt, 1);
            print(vt.position.y);
            vt = vertexs[2];
            vt.position += new Vector3((centerPoint.x + vt.position.x - vt.position.y) / 10f * perspective + offset, 0, 0);
            vh.SetUIVertex(vt, 2);
            
            vt = vertexs[4];
            vt.position += new Vector3((centerPoint.x - vt.position.x - vt.position.y) / 10f * perspective + offset, 0, 0);
            vh.SetUIVertex(vt, 3);
        }
        else
        {
            vt = vertexs[1];
            vt.position += new Vector3((centerPoint.x - vt.position.x) / 10f * perspective + offset, 0, 0);
            vh.SetUIVertex(vt, 1);

            vt = vertexs[2];
            vt.position += new Vector3((centerPoint.x - vt.position.x) / 10f * perspective + offset, 0, 0);
            vh.SetUIVertex(vt, 2);
        }

        
    }

    public enum PosY
    {
        up,
        middle,
        down
    }
}
