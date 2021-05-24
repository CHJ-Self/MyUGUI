using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class TextMixSprite : BaseMeshEffect
{
    public Sprite[] textures;

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive() || vh.currentVertCount == 0)
        {
            return;
        }

        var text = GetComponent<Text>();

        UIVertex vt;
        var vertexs = new List<UIVertex>();
        vh.GetUIVertexStream(vertexs);

        string[] txts = text.text.Split(new string[] { "</color>" }, System.StringSplitOptions.None);
        //print(txts.Length);

        var lineTexts = text.text.Split('\n');
        var lines = new TextSpacing.Line[lineTexts.Length];

        // 根据lines数组中各个元素的长度计算每一行中第一个点的索引，每个字、字母、空母均占6个点
        for (var i = 0; i < lines.Length; i++)
        {
            // 除最后一行外，vertexs对于前面几行都有回车符占了6个点
            if (i == 0)
            {
                lines[i] = new TextSpacing.Line(0, lineTexts[i].Length);
            }
            else if (i > 0 && i < lines.Length - 1)
            {
                lines[i] = new TextSpacing.Line(lines[i - 1].EndVertexIndex + 1, lineTexts[i].Length);
            }
            else
            {
                lines[i] = new TextSpacing.Line(lines[i - 1].EndVertexIndex + 1, lineTexts[i].Length);
            }
        }
        /*
        for (var i = 0; i < lines.Length; i++)
        {
            for (var j = lines[i].StartVertexIndex; j <= lines[i].EndVertexIndex; j++)
            {
                if (j < 0 || j >= vertexs.Count)
                {
                    continue;
                }

                vt = vertexs[j];
                
                vertexs[j] = vt;

                // 以下注意点与索引的对应关系
                if (j % 6 <= 2)
                {
                    vh.SetUIVertex(vt, (j / 6) * 4 + j % 6);
                }

                if (j % 6 == 4)
                {
                    vh.SetUIVertex(vt, (j / 6) * 4 + j % 6 - 1);
                }
            }
        }
        */
        VertexHelper vertexHelper = new VertexHelper();

        int index = 0;
        vertexHelper.AddVert(new Vector3(-80, 15, 0), text.color, new Vector2(0, 1));
        vertexHelper.AddVert(new Vector3(-44, 15, 0), text.color, new Vector2(1, 1));
        vertexHelper.AddVert(new Vector3(-44, -21, 0), text.color, new Vector2(1, 0));
        vertexHelper.AddVert(new Vector3(-80, -21, 0), text.color, new Vector2(0, 0));

        vertexHelper.AddTriangle(index, index + 1, index + 3);
        vertexHelper.AddTriangle(index + 3, index + 1, index + 2);
        GetComponent<MeshRenderer>().sharedMaterial.mainTexture = textures[1].texture;

        Mesh mesh = new Mesh();
        vertexHelper.FillMesh(mesh);
        GetComponent<MeshFilter>().mesh = mesh;
    }
}
