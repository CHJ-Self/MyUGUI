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

        #region UGUI.Text的OnPopulateMesh部分代码
        /*
        Vector2 extents = rectTransform.rect.size;

        var settings = GetGenerationSettings(extents);
        cachedTextGenerator.PopulateWithErrors(text, settings, gameObject);

        // Apply the offset to the vertices
        IList<UIVertex> verts = cachedTextGenerator.verts;
        float unitsPerPixel = 1 / pixelsPerUnit;
        int vertCount = verts.Count;
        print(vertCount);
        // We have no verts to process just return (case 1037923)
        if (vertCount <= 0)
        {
            toFill.Clear();
            return;
        }

        Vector2 roundingOffset = new Vector2(verts[0].position.x, verts[0].position.y) * unitsPerPixel;
        roundingOffset = PixelAdjustPoint(roundingOffset) - roundingOffset;
        toFill.Clear();
        UIVertex[] m_tempVert = new UIVertex[4];
        Vector3 spacing = new Vector3(5,0,0);
        Vector3 currentSpacing = Vector3.zero;
        if (roundingOffset != Vector2.zero)
        {
            for (int i = 0; i < vertCount; ++i)
            {
                int tempVertsIndex = i & 3;
                m_tempVert[tempVertsIndex] = verts[i];
                m_tempVert[tempVertsIndex].position *= unitsPerPixel;
                m_tempVert[tempVertsIndex].position.x += roundingOffset.x;
                m_tempVert[tempVertsIndex].position.y += roundingOffset.y;

                if(i % 4 == 0)
                {
                    currentSpacing += spacing;
                }
                m_tempVert[tempVertsIndex].position += currentSpacing;

                if (tempVertsIndex == 3)
                    toFill.AddUIVertexQuad(m_tempVert);
            }
        }
        else
        {
            for (int i = 0; i < vertCount; ++i)
            {
                int tempVertsIndex = i & 3;
                m_tempVert[tempVertsIndex] = verts[i];
                m_tempVert[tempVertsIndex].position *= unitsPerPixel;

                if (i % 4 == 0)
                {
                    currentSpacing += spacing;
                }
                m_tempVert[tempVertsIndex].position += currentSpacing;

                if (tempVertsIndex == 3)
                    toFill.AddUIVertexQuad(m_tempVert);
            }
        }
        */
        #endregion


        Vector2 extents = rectTransform.rect.size;
        var settings = GetGenerationSettings(extents);
        cachedTextGenerator.PopulateWithErrors(text, settings, gameObject);

        // Apply the offset to the vertices
        IList<UIVertex> verts = cachedTextGenerator.verts;
        float unitsPerPixel = 1 / pixelsPerUnit;
        int vertCount = verts.Count;

        // We have no verts to process just return (case 1037923)
        if (vertCount <= 0)
        {
            toFill.Clear();
            return;
        }

        Vector2 roundingOffset = new Vector2(verts[0].position.x, verts[0].position.y) * unitsPerPixel;
        roundingOffset = PixelAdjustPoint(roundingOffset) - roundingOffset;
        //Debug.Log("roungingOffset: " + roundingOffset);

        /*
        foreach(var vert in verts)
        {
            Debug.Log(vert.position);
        }
        Debug.Log("---------------------------------------------");
        */
        //--------------------------------------------------------------------------------
        //TODO 对比一下UGUI的方法和自定义方法输出的vert位置
        //TODO LinsSpacing以及Overflow的情况
        //TODO 看看Text是什么时候更新的Texture
        //TODO 研究动态字体和静态字体的区别

        toFill.Clear();

        float currentLineTotalWidth = 0f;
        int lineCount = 1;
        Vector3 startPos = Vector3.zero;
        List<float> totalWidthtList = new List<float>();
        
        //定义字间距向量
        Vector3 characterSpacingVector = new Vector3(m_characterSpacing, 0, 0);
        Vector3 spacing = Vector3.zero;

        Font mfont = Font.CreateDynamicFontFromOSFont("Arail", fontSize);
        mfont.RequestCharactersInTexture(text, fontSize);
        
        CharacterInfo ch_firstChar;
        mfont.GetCharacterInfo(text[0], out ch_firstChar);
        currentLineTotalWidth = ch_firstChar.advance;

        List<CharacterInfo> characterInfoList = new List<CharacterInfo>();
        characterInfoList.Add(ch_firstChar);

        for (int i = 0;i < text.Length;i++)
        {
            if (i + 1 < text.Length)
            {
                CharacterInfo next_ch;
                mfont.GetCharacterInfo(text[i + 1], out next_ch);
                characterInfoList.Add(next_ch);

                if (text[i] == '\n')
                {
                    lineCount++;
                    totalWidthtList.Add(currentLineTotalWidth - m_characterSpacing);
                    currentLineTotalWidth = next_ch.advance;
                    continue;
                }

                if ((currentLineTotalWidth + next_ch.advance + m_characterSpacing) > rectTransform.sizeDelta.x)
                {
                    lineCount++;
                    totalWidthtList.Add(currentLineTotalWidth - m_characterSpacing);
                    currentLineTotalWidth = next_ch.advance;
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
                    totalWidthtList.Add(currentLineTotalWidth - m_characterSpacing);
                    currentLineTotalWidth = 0;
                    continue;
                }
            }
        }
        //加上最后一行的字符宽度
        totalWidthtList.Add(currentLineTotalWidth);

        foreach(var i in totalWidthtList)
        {
            print(i);
        }

        //重置部分属性
        lineCount = 1;
        currentLineTotalWidth = ch_firstChar.advance;

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

            if (i + 1 < text.Length)
            {
                CharacterInfo next_ch = characterInfoList[i + 1];
                //适应换行符，如果当前字符是换行符，则直接进行换行操作
                if (text[i] == '\n')
                {
                    lineCount++;
                    currentLineTotalWidth = next_ch.advance;
                    startPos = GetStartPosition(lineCount, totalWidthtList);
                    continue;
                }

                //换行
                if ((currentLineTotalWidth + next_ch.advance + m_characterSpacing) > rectTransform.sizeDelta.x)
                {
                    lineCount++;
                    currentLineTotalWidth = next_ch.advance;
                    startPos = GetStartPosition(lineCount, totalWidthtList);
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

            toFill.AddUIVertexQuad(vertices);
        }

        m_DisableFontTextureRebuiltCallback = false;
    }

    private Vector3 GetStartPosition(int lineCount, List<float> totalWidthtList)
    {
        switch (alignment)
        {
            case TextAnchor.UpperLeft:
                return new Vector3(rectTransform.rect.xMin, rectTransform.rect.yMax - fontSize * lineCount, 0);
            case TextAnchor.UpperCenter:
                return new Vector3(-totalWidthtList[lineCount - 1] / 2, rectTransform.rect.yMax - fontSize * lineCount, 0);
            case TextAnchor.UpperRight:
                return new Vector3(rectTransform.rect.xMin + rectTransform.sizeDelta.x - totalWidthtList[lineCount - 1], rectTransform.rect.yMax - fontSize * lineCount, 0);
            case TextAnchor.MiddleLeft:
                return new Vector3(rectTransform.rect.xMin, fontSize * totalWidthtList.Count / 2 - fontSize * lineCount, 0);
            case TextAnchor.MiddleCenter:
                return new Vector3(-totalWidthtList[lineCount - 1] / 2, fontSize * totalWidthtList.Count / 2 - fontSize * lineCount, 0);
            case TextAnchor.MiddleRight:
                return new Vector3(rectTransform.rect.xMin + rectTransform.sizeDelta.x - totalWidthtList[lineCount - 1], fontSize * totalWidthtList.Count / 2 - fontSize * lineCount, 0);
            case TextAnchor.LowerLeft:
                return new Vector3(rectTransform.rect.xMin, rectTransform.rect.yMax - fontSize * lineCount + (fontSize * totalWidthtList.Count - rectTransform.sizeDelta.y), 0);
            case TextAnchor.LowerCenter:
                return new Vector3(-totalWidthtList[lineCount - 1] / 2, rectTransform.rect.yMax - fontSize * lineCount + (fontSize * totalWidthtList.Count - rectTransform.sizeDelta.y), 0);
            case TextAnchor.LowerRight:
                return new Vector3(rectTransform.rect.xMin + rectTransform.sizeDelta.x - totalWidthtList[lineCount - 1], rectTransform.rect.yMax - fontSize * lineCount + (fontSize * totalWidthtList.Count - rectTransform.sizeDelta.y), 0);
            default:
                return Vector3.zero;
        }
    }
}

