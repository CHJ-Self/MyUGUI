using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Effects/TextSpacing")]
public class TextSpacingAutoWrap : Text
{
    [SerializeField] private float m_characterSpacing = 0f;

    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        if (font == null)
            return;

        if (text.Length <= 0)
        {
            return;
        }

        // We don't care if we the font Texture changes while we are doing our Update.
        // The end result of cachedTextGenerator will be valid for this instance.
        // Otherwise we can get issues like Case 619238.
        m_DisableFontTextureRebuiltCallback = true;

        Vector2 extents = rectTransform.rect.size;
        var settings = GetGenerationSettings(extents);
        cachedTextGenerator.PopulateWithErrors(text, settings, gameObject);

        //--------------------------------------------------------------------------------
        //TODO 实现其它Text的属性
        //TODO 看看Text是什么时候更新的Texture

        toFill.Clear();

        float currentLineTotalWidth = 0f;
        float currentTotalHeight = 0f;
        int lineCount = 1;
        Vector3 startPos = Vector3.zero;
        List<float> totalWidthtList = new List<float>();
        
        //定义字间距向量
        Vector3 characterSpacingVector = new Vector3(m_characterSpacing, 0, 0);
        font.RequestCharactersInTexture(text, fontSize, fontStyle);
        
        CharacterInfo ch_firstChar;
        font.GetCharacterInfo(text[0], out ch_firstChar,fontSize, fontStyle);
        currentLineTotalWidth = ch_firstChar.advance;

        currentTotalHeight = fontSize;
        if(rectTransform.sizeDelta.y < currentTotalHeight)
        {
            return;
        }

        List<CharacterInfo> characterInfoList = new List<CharacterInfo>();
        characterInfoList.Add(ch_firstChar);

        for (int i = 0;i < text.Length;i++)
        {
            if (i + 1 < text.Length)
            {
                CharacterInfo next_ch;
                font.GetCharacterInfo(text[i + 1], out next_ch, fontSize, fontStyle);
                characterInfoList.Add(next_ch);

                if (text[i] == '\n')
                {
                    lineCount++;
                    totalWidthtList.Add(currentLineTotalWidth);
                    currentLineTotalWidth = next_ch.advance;
                    currentTotalHeight += lineSpacing + fontSize;
                    if (verticalOverflow == VerticalWrapMode.Truncate && currentTotalHeight > rectTransform.sizeDelta.y)
                    {
                        break;
                    }
                    continue;
                }
                //自动换行
                if (horizontalOverflow == HorizontalWrapMode.Wrap && (currentLineTotalWidth + next_ch.advance + m_characterSpacing) > rectTransform.sizeDelta.x)
                {
                    lineCount++;
                    totalWidthtList.Add(currentLineTotalWidth);
                    currentLineTotalWidth = next_ch.advance;
                    currentTotalHeight += lineSpacing + fontSize;
                    if (verticalOverflow == VerticalWrapMode.Truncate && currentTotalHeight > rectTransform.sizeDelta.y)
                    {
                        break;
                    }
                }
                else
                {
                    if (!(text[i + 1] == '\n'))
                    {
                        currentLineTotalWidth += next_ch.advance + m_characterSpacing;
                    }
                }
            }
            else
            {
                if (text[i] == '\n')
                {
                    lineCount++;
                    totalWidthtList.Add(currentLineTotalWidth);
                    currentLineTotalWidth = 0;
                    continue;
                }
            }
        }
        //加上最后一行的字符宽度
        totalWidthtList.Add(currentLineTotalWidth);

        //重置部分属性
        lineCount = 1;
        currentLineTotalWidth = ch_firstChar.advance;
        currentTotalHeight = fontSize;

        startPos = GetStartPosition(lineCount, totalWidthtList);

        for (int i = 0; i < text.Length; i++)
        {
            CharacterInfo ch = characterInfoList[i];

            UIVertex[] vertices = new UIVertex[4];
            vertices[0] = UIVertex.simpleVert;
            vertices[1] = UIVertex.simpleVert;
            vertices[2] = UIVertex.simpleVert;
            vertices[3] = UIVertex.simpleVert;

            vertices[0].position = startPos + new Vector3(ch.minX, ch.maxY, 0);
            vertices[1].position = startPos + new Vector3(ch.maxX, ch.maxY, 0);
            vertices[2].position = startPos + new Vector3(ch.maxX, ch.minY, 0);
            vertices[3].position = startPos + new Vector3(ch.minX, ch.minY, 0);

            //Vector2 adjustVector = Vector2.zero;
            Vector2 adjustVector = new Vector2(0, 0.00f);

            vertices[0].uv0 = ch.uvTopLeft + adjustVector;
            vertices[1].uv0 = ch.uvTopRight + adjustVector;
            vertices[2].uv0 = ch.uvBottomRight + adjustVector;
            vertices[3].uv0 = ch.uvBottomLeft + adjustVector;
            /*
            Debug.Log(ch.uvTopLeft);
            Debug.Log(ch.uvTopRight);
            Debug.Log(ch.uvBottomRight);
            Debug.Log(ch.uvBottomLeft);
            */
            vertices[0].color = color;
            vertices[1].color = color;
            vertices[2].color = color;
            vertices[3].color = color;

            if (text[i] != '\n')
                toFill.AddUIVertexQuad(vertices);

            if (i + 1 < text.Length)
            {
                CharacterInfo next_ch = characterInfoList[i + 1];
                //适应换行符，如果当前字符是换行符，则直接进行换行操作
                if (text[i] == '\n')
                {
                    lineCount++;
                    currentLineTotalWidth = next_ch.advance;
                    startPos = GetStartPosition(lineCount, totalWidthtList);
                    currentTotalHeight += lineSpacing + fontSize;
                    if (verticalOverflow == VerticalWrapMode.Truncate && currentTotalHeight > rectTransform.sizeDelta.y)
                    {
                        break;
                    }
                    continue;
                }

                //自动换行
                if (horizontalOverflow == HorizontalWrapMode.Wrap && (currentLineTotalWidth + next_ch.advance + m_characterSpacing) > rectTransform.sizeDelta.x)
                {
                    lineCount++;
                    currentLineTotalWidth = next_ch.advance;
                    startPos = GetStartPosition(lineCount, totalWidthtList);
                    currentTotalHeight += lineSpacing + fontSize;
                    if (verticalOverflow == VerticalWrapMode.Truncate && currentTotalHeight > rectTransform.sizeDelta.y)
                    {
                        break;
                    }
                }
                else
                {
                    startPos += new Vector3(ch.advance, 0, 0) + characterSpacingVector;
                    //适应换行符，如果下一个字符是换行符，则不需要增加当前行的字符总宽度
                    if (!(text[i + 1] == '\n'))
                    {
                        currentLineTotalWidth += next_ch.advance + m_characterSpacing;
                    }
                }
            }
            else
            {
                if (text[i] == '\n')
                {
                    lineCount++;
                    currentLineTotalWidth = 0;
                    continue;
                }
            }
        }

        m_DisableFontTextureRebuiltCallback = false;
    }

    private Vector3 GetStartPosition(int lineCount, List<float> totalWidthtList)
    {
        int totalLineCount = totalWidthtList.Count;
        float leftPosInRect = rectTransform.rect.xMin;
        float upPosInRect = rectTransform.rect.yMax;
        float halfLineWidth = totalWidthtList[lineCount - 1] / 2;
        float rectTransformWidth = rectTransform.sizeDelta.x;
        float rectTransformHeight = rectTransform.sizeDelta.y;
        switch (alignment)
        {
            case TextAnchor.UpperLeft:
                return new Vector3(leftPosInRect, upPosInRect - fontSize * lineCount - lineSpacing * (lineCount - 1), 0);
            case TextAnchor.UpperCenter:
                return new Vector3(-halfLineWidth, upPosInRect - fontSize * lineCount - lineSpacing * (lineCount - 1), 0);
            case TextAnchor.UpperRight:
                return new Vector3(leftPosInRect + rectTransformWidth - totalWidthtList[lineCount - 1], upPosInRect - fontSize * lineCount - lineSpacing * (lineCount - 1), 0);
            case TextAnchor.MiddleLeft:
                return new Vector3(leftPosInRect, (fontSize * totalLineCount + lineSpacing * (totalLineCount - 1)) / 2 - fontSize * lineCount, 0);
            case TextAnchor.MiddleCenter:
                return new Vector3(-halfLineWidth, (fontSize * totalLineCount + lineSpacing * (totalLineCount - 1)) / 2 - fontSize * lineCount, 0);
            case TextAnchor.MiddleRight:
                return new Vector3(leftPosInRect + rectTransformWidth - totalWidthtList[lineCount - 1], (fontSize * totalLineCount + lineSpacing * (totalLineCount - 1)) / 2 - fontSize * lineCount, 0);
            case TextAnchor.LowerLeft:
                return new Vector3(leftPosInRect, upPosInRect - fontSize * lineCount + (fontSize * totalLineCount - rectTransformHeight) + lineSpacing * (totalLineCount - lineCount + 1), 0);
            case TextAnchor.LowerCenter:
                return new Vector3(-halfLineWidth, upPosInRect - fontSize * lineCount + (fontSize * totalLineCount - rectTransformHeight) + lineSpacing * (totalLineCount - lineCount + 1), 0);
            case TextAnchor.LowerRight:
                return new Vector3(leftPosInRect + rectTransformWidth - totalWidthtList[lineCount - 1], upPosInRect - fontSize * lineCount + (fontSize * totalLineCount - rectTransformHeight) + lineSpacing * (totalLineCount - lineCount + 1), 0);
            default:
                return Vector3.zero;
        }
    }
}

