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
        //TODO 实现Alignment、LinsSpacing以及Overflow的情况
        //TODO 看看Text是什么时候更新的Texture

        toFill.Clear();

        float currentLineTotalWidth = 0f;
        int lineCount = 1;
        Vector3 startPos = Vector3.zero;
        startPos = new Vector3(rectTransform.rect.xMin, rectTransform.rect.yMax - fontSize * lineCount, 0);
        //定义字间距向量
        Vector3 characterSpacingVector = new Vector3(m_characterSpacing, 0, 0);
        Vector3 spacing = Vector3.zero;

        Font mfont = Font.CreateDynamicFontFromOSFont("Arail", fontSize);
        mfont.RequestCharactersInTexture(text, fontSize);
        
        CharacterInfo ch_firstChar;
        mfont.GetCharacterInfo(text[0], out ch_firstChar);
        currentLineTotalWidth = ch_firstChar.advance;
        
        for (int i = 0; i < text.Length; i++)
        {
            //TODO 研究动态字体和静态字体的区别

            CharacterInfo ch;
            mfont.GetCharacterInfo(text[i], out ch);

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

            if(i + 1 < text.Length)
            {
                CharacterInfo next_ch;
                mfont.GetCharacterInfo(text[i + 1], out next_ch);

                if ((currentLineTotalWidth + next_ch.advance + m_characterSpacing) > rectTransform.sizeDelta.x)
                {
                    lineCount++;
                    currentLineTotalWidth = next_ch.advance;
                    spacing = Vector3.zero;
                    startPos = new Vector3(rectTransform.rect.xMin, rectTransform.rect.yMax - fontSize * lineCount, 0) + spacing;
                }
                else
                {
                    spacing += characterSpacingVector;
                    startPos += new Vector3(ch.advance, 0, 0) + characterSpacingVector;
                    currentLineTotalWidth += next_ch.advance + m_characterSpacing;
                }
            }

            toFill.AddUIVertexQuad(vertices);
        }

        m_DisableFontTextureRebuiltCallback = false;
    }
}

