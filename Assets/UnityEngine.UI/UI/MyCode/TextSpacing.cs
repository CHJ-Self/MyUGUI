using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Effects/TextSpacing")]
public class TextSpacing : BaseMeshEffect
{
    #region Struct

    public enum HorizontalAligmentType
    {
        Left,
        Center,
        Right
    }

    public class Line
    {
        // 起点索引
        public int StartVertexIndex { get { return _startVertexIndex; } }
        private int _startVertexIndex = 0;

        // 终点索引
        public int EndVertexIndex { get { return _endVertexIndex; } }
        private int _endVertexIndex = 0;

        // 该行占的点数目
        public int VertexCount { get { return _vertexCount; } }
        private int _vertexCount = 0;

        public Line(int startVertexIndex, int length) {
            _startVertexIndex = startVertexIndex;
            _endVertexIndex = length * 6 - 1 + startVertexIndex;
            _vertexCount = length * 6;
        }
    }

    #endregion

    public float textSpacing = 1f;
    public bool isNewLines = false;
    [HideInInspector]
    public RectTransform rectTransform;
    [HideInInspector]
    public float lineSpacing = 1f;

    public override void ModifyMesh(VertexHelper vh) {
        if (!IsActive() || vh.currentVertCount == 0) {
            return;
        }

        var text = GetComponent<Text>();
        rectTransform = GetComponent<RectTransform>();
        lineSpacing = text.lineSpacing;

        if (text == null) {
            Debug.LogError("Missing Text component");
            return;
        }

        // 水平对齐方式
        HorizontalAligmentType alignment;
        if (text.alignment == TextAnchor.LowerLeft || text.alignment == TextAnchor.MiddleLeft || text.alignment == TextAnchor.UpperLeft) {
            alignment = HorizontalAligmentType.Left;
        } else if (text.alignment == TextAnchor.LowerCenter || text.alignment == TextAnchor.MiddleCenter || text.alignment == TextAnchor.UpperCenter) {
            alignment = HorizontalAligmentType.Center;
        } else {
            alignment = HorizontalAligmentType.Right;
        }

        var vertexs = new List<UIVertex>();
        vh.GetUIVertexStream(vertexs);

        var lineTexts = text.text.Split('\n');
        var lines = new Line[lineTexts.Length];

        // 根据lines数组中各个元素的长度计算每一行中第一个点的索引，每个字、字母、空母均占6个点
        for (var i = 0; i < lines.Length; i++) {
            // 除最后一行外，vertexs对于前面几行都有回车符占了6个点
            if (i == 0) {
                lines[i] = new Line(0, lineTexts[i].Length);
            }
            else if (i > 0 && i < lines.Length - 1) {
                lines[i] = new Line(lines[i - 1].EndVertexIndex + 1, lineTexts[i].Length);
            } 
            else {
                lines[i] = new Line(lines[i - 1].EndVertexIndex + 1, lineTexts[i].Length);
            }
        }

        UIVertex vt;
        int maxWordCount;
        int lastLineIndex;
        float ratio = 0;

        if (text.horizontalOverflow == HorizontalWrapMode.Wrap && !isNewLines)
        {
            if (text.fontSize == 0)
            {
                maxWordCount = int.MaxValue;
            }
            else
            {
                maxWordCount = (int)rectTransform.sizeDelta.x / text.fontSize;
            }
        }
        else
        {
            maxWordCount = int.MaxValue;
        }

        //防止maxWordCount为0（防止被除数为0）
        maxWordCount = maxWordCount == 0 ? 1 : maxWordCount;

        lastLineIndex = text.text.Length - text.text.Length % maxWordCount;

        for (var i = 0; i < lines.Length; i++) {
            for (var j = lines[i].StartVertexIndex; j <= lines[i].EndVertexIndex; j++) {
                if (j < 0 || j >= vertexs.Count) {
                    continue;
                }

                vt = vertexs[j];

                int currentVertInCharIndex = (j - lines[i].StartVertexIndex) / 6;//当前顶点对应字符在整短文本中的位置下标

                if (alignment == HorizontalAligmentType.Left)
                {
                    if(isNewLines)
                    {
                        ratio = currentVertInCharIndex % lineTexts[i].Length;
                    }
                    else
                    {
                        ratio = currentVertInCharIndex % maxWordCount;
                    }

                    vt.position += new Vector3(textSpacing * ratio, 0, 0);
                }
                else if (alignment == HorizontalAligmentType.Right)
                {
                    if (isNewLines)
                    {
                        ratio = lineTexts[i].Length - currentVertInCharIndex - 1;
                    }
                    else
                    {
                        int lineCount = currentVertInCharIndex / maxWordCount + 1;//当前warp模式下自动换行数，表示第几行
                        
                        //计算最后一行的情况
                        if (currentVertInCharIndex >= lastLineIndex)
                        {
                            if (text.text.Length <= maxWordCount)
                            {
                                ratio = text.text.Length - currentVertInCharIndex - 1;
                            }
                            else
                            {
                                ratio = (text.text.Length - 1) % currentVertInCharIndex;
                            }
                        }
                        else
                        {
                            ratio = (maxWordCount * lineCount - 1 - currentVertInCharIndex) % maxWordCount;
                        }
                        
                    }

                    vt.position -= new Vector3(textSpacing * ratio, 0, 0);
                }
                else if (alignment == HorizontalAligmentType.Center)
                {
                    if (isNewLines)
                    {
                        ratio = currentVertInCharIndex % lineTexts[i].Length - (lineTexts[i].Length / 2f - 0.5f);
                    }
                    else
                    {
                        float offset = 0;

                        //计算最后一行的情况
                        if (currentVertInCharIndex >= lastLineIndex)
                        {
                            int lastLineCount = text.text.Length - lastLineIndex;
                            if(lastLineCount % 2 == 0)
                            {
                                offset = 0.5f;
                            }
                            ratio = currentVertInCharIndex - lastLineIndex - (lastLineCount / 2 - offset);
                        }
                        else
                        {
                            if (maxWordCount % 2 == 0)
                            {
                                offset = 0.5f;
                            }
                            ratio = currentVertInCharIndex % maxWordCount - (maxWordCount / 2 - offset);
                        }
                    }
                    print(ratio);
                    vt.position += new Vector3(textSpacing * ratio, 0, 0);
                }

                vertexs[j] = vt;
                // 以下注意点与索引的对应关系
                if (j % 6 <= 2) {
                    vh.SetUIVertex(vt, (j / 6) * 4 + j % 6);
                }

                if (j % 6 == 4) {
                    vh.SetUIVertex(vt, (j / 6) * 4 + j % 6 - 1);
                }
            }
        }
    }
}

