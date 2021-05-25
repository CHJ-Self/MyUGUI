using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class TextMixSprite : BaseMeshEffect
{
    public Texture texture;
    //public Color _color = Color.white;

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive() || vh.currentVertCount == 0)
        {
            return;
        }
        var text = GetComponent<Text>();
        string[] txts1 = text.text.Split(new string[] { "</color>" }, StringSplitOptions.None);
        if (txts1.Length <= 1)
        {
            return;
        }
        var vertexs = new List<UIVertex>();        
        vh.GetUIVertexStream(vertexs);

        //Text组件的顶点数会有两种情况，一种是忽略富文本的顶点数，一种是计算富文本的顶点数，这里需要判断一下
        //其中，一个改变color值的富文本占23个字符
        bool isUseLessVertex = !(vertexs.Count == (text.text.Length + 1) * 6);
        GetComponent<MeshFilter>().mesh = null;
        int _index;
        int charTotalLength = 0;
        VertexHelper vertexHelper = new VertexHelper();
        UIVertex vt;
        int vert_index = 0;
        Color alpha = new Color(1, 1, 1, 0);
        int specialCharCount = 0;
        for (int i = 0;i < txts1.Length - 1;i++)
        {
            if (isUseLessVertex)
            {
                string _txt = txts1[i].Split('<')[0];
                specialCharCount += _txt.Split(new string[] { "\n"," ","\t" }, StringSplitOptions.None).Length - 1;
                _index = _txt.Length + charTotalLength - specialCharCount;
                charTotalLength = _index + specialCharCount + 1;                
            }
            else
            {
                string _txt = txts1[i].Split('<')[0];
                specialCharCount += _txt.Split(new string[] { "\n", " ", "\t" }, StringSplitOptions.None).Length - 1;
                _index = _txt.Length + 15 + charTotalLength;
                charTotalLength = _index + 9;
            }
            /*
            print("specialCharCount: " + specialCharCount);
            print("index: " + _index);
            print("charTotal: " + charTotalLength);
            */
            try
            {
                vertexHelper.AddVert(vertexs[_index * 6].position, text.color, new Vector2(0, 1));
                vertexHelper.AddVert(vertexs[_index * 6 + 1].position, text.color, new Vector2(1, 1));
                vertexHelper.AddVert(vertexs[_index * 6 + 2].position, text.color, new Vector2(1, 0));
                vertexHelper.AddVert(vertexs[_index * 6 + 4].position, text.color, new Vector2(0, 0));

                for (int j = 0; j < 4; j++)
                {
                    vt = vertexs[_index * 4 + j];
                    vt.color = alpha;
                    vh.SetUIVertex(vt, _index * 4 + j);
                }

                vertexHelper.AddTriangle(vert_index, vert_index + 1, vert_index + 3);
                vertexHelper.AddTriangle(vert_index + 3, vert_index + 1, vert_index + 2);

                vert_index += 4;
            }
            catch(ArgumentOutOfRangeException e)
            {
                Debug.LogError(e);
                return;
            }
        }

        if (texture != null)
        {
            GetComponent<MeshRenderer>().sharedMaterial.mainTexture = texture;
        }
        Mesh mesh = new Mesh();
        vertexHelper.FillMesh(mesh);
        GetComponent<MeshFilter>().mesh = mesh;
    }
}
