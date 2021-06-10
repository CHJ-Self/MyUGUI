using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PerspectiveScrollRect : BaseMeshEffect
{
    public float centerX = 0f;
    [Range(0, 1)]
    public float posY = 0.5f;
    public float perspective = 0;
    public float offset = 0;
    public Camera camera;

    private RectTransform rectTransform;

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive() || vh.currentVertCount == 0)
        {
            return;
        }

        Graphic graphic = GetComponent<Graphic>();
        if (graphic == null)
        {
            return;
        }

        rectTransform = GetComponent<RectTransform>();
        var vertexs = new List<UIVertex>();
        vh.GetUIVertexStream(vertexs);

        UIVertex vt;
        //print(vh.currentVertCount);
        //print(vertexs.Count);

        //print(vertexs[0].position);
        
        //print(RectTransformUtility.s(camera, new Vector3(centerX, 0, 0)));
        Vector2 centerPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(graphic.rectTransform, Camera.main.WorldToScreenPoint(new Vector3(centerX, 0, 0)), graphic.canvas.worldCamera, out centerPoint);
        for(int i = 0;i < vh.currentVertCount;i++)
        {
            vt = vertexs[i];
            vt.position += new Vector3(centerPoint.x / 10f - perspective, 0, 0);

            vh.SetUIVertex(vt, i);
        }
    }
}
