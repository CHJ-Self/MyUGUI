using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Effects/TextGradient")]
public class TextGradient : BaseMeshEffect
{
    [SerializeField]
    private ColorFilterType m_VertexColorFilter = ColorFilterType.normal;
    [SerializeField]
    private Color m_VertexTopLeft = Color.white;
    [SerializeField]
    private Color m_VertexTopRight = Color.white;
    [SerializeField]
    private Color m_VertexBottomLeft = Color.white;
    [SerializeField]
    private Color m_VertexBottomRight = Color.white;
    [SerializeField]
    private Vector2 m_VertexColorOffset = Vector2.zero;

    private RectTransform rectTransform;

    public enum ColorFilterType
    {
        Additive,  //叠加
        reduce, //减去
        normal, //正常
    }

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive() || vh.currentVertCount == 0)
        {
            return;
        }

        var text = GetComponent<Text>();
        rectTransform = GetComponent<RectTransform>();

        UIVertex vt;
        var vertexs = new List<UIVertex>();
        vh.GetUIVertexStream(vertexs);

        Vector2 min = rectTransform.pivot;
        min.Scale(-rectTransform.rect.size);
        Vector2 max = rectTransform.rect.size + min;

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

        for (var i = 0; i < lines.Length; i++)
        {
            for (var j = lines[i].StartVertexIndex; j <= lines[i].EndVertexIndex; j++)
            {
                if (j < 0 || j >= vertexs.Count)
                {
                    continue;
                }

                vt = vertexs[j];

                vt.color = ColorGradient(min, max, text.color, vt.position);

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

    }

    private Color ColorGradient(Vector2 min, Vector2 max, Color color, Vector2 pos)
    {
        float x01 = max.x == min.x ? 0f : Mathf.Clamp01((pos.x - min.x) / (max.x - min.x));
        float y01 = max.y == min.y ? 0f : Mathf.Clamp01((pos.y - min.y) / (max.y - min.y));
        x01 -= m_VertexColorOffset.x * (m_VertexColorOffset.x > 0f ? x01 : (1f - x01));
        y01 -= m_VertexColorOffset.y * (m_VertexColorOffset.y > 0f ? y01 : (1f - y01));
        Color newColor = Color.Lerp(
            Color.Lerp(m_VertexBottomLeft, m_VertexBottomRight, x01),
            Color.Lerp(m_VertexTopLeft, m_VertexTopRight, x01),
            y01
        );
        switch (m_VertexColorFilter)
        {
            default:
            case ColorFilterType.Additive:
                return color + newColor;
            case ColorFilterType.reduce:
                newColor = color - newColor;
                if(newColor.a == 0)
                {
                    newColor.a = 1;
                }
                return newColor;
            case ColorFilterType.normal:
                float a = Mathf.Max(newColor.a, color.a);
                newColor = Color.Lerp(color, newColor, newColor.a);
                newColor.a = a;
                return newColor;
        }
    }
}
