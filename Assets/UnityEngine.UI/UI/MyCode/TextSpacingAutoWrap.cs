using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Effects/TextSpacing")]
public class TextSpacingAutoWrap : Text
{
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

        // We have no verts to process just return (case 1037923)
        if (vertCount <= 0)
        {
            toFill.Clear();
            return;
        }

        Vector2 roundingOffset = new Vector2(verts[0].position.x, verts[0].position.y) * unitsPerPixel;
        roundingOffset = PixelAdjustPoint(roundingOffset) - roundingOffset;
        toFill.Clear();
        if (roundingOffset != Vector2.zero)
        {
            for (int i = 0; i < vertCount; ++i)
            {
                int tempVertsIndex = i & 3;
                m_TempVerts[tempVertsIndex] = verts[i];
                m_TempVerts[tempVertsIndex].position *= unitsPerPixel;
                m_TempVerts[tempVertsIndex].position.x += roundingOffset.x;
                m_TempVerts[tempVertsIndex].position.y += roundingOffset.y;
                if (tempVertsIndex == 3)
                    toFill.AddUIVertexQuad(m_TempVerts);
            }
        }
        else
        {
            for (int i = 0; i < vertCount; ++i)
            {
                int tempVertsIndex = i & 3;
                m_TempVerts[tempVertsIndex] = verts[i];
                m_TempVerts[tempVertsIndex].position *= unitsPerPixel;
                if (tempVertsIndex == 3)
                    toFill.AddUIVertexQuad(m_TempVerts);
            }
        }
        */
        #endregion

        toFill.Clear();
        Vector3 pos = Vector3.zero;
        
        for (int i = 0; i < text.Length; i++)
        {
            // Get character rendering information from the font
            CharacterInfo ch;

            //TODO 研究动态字体和静态字体的区别
            Font mfont = Font.CreateDynamicFontFromOSFont("Arail", 22);
            mfont.RequestCharactersInTexture(text, 22);
            mfont.GetCharacterInfo(text[i], out ch);

            UIVertex[] vertices = new UIVertex[4];

            vertices[0] = UIVertex.simpleVert;
            vertices[1] = UIVertex.simpleVert;
            vertices[2] = UIVertex.simpleVert;
            vertices[3] = UIVertex.simpleVert;

            vertices[0].position = pos + new Vector3(ch.minX, ch.maxY, 0);
            vertices[1].position = pos + new Vector3(ch.maxX, ch.maxY, 0);
            vertices[2].position = pos + new Vector3(ch.maxX, ch.minY, 0);
            vertices[3].position = pos + new Vector3(ch.minX, ch.minY, 0);

            vertices[0].uv0 = ch.uvTopLeft;
            vertices[1].uv0 = ch.uvTopRight;
            vertices[2].uv0 = ch.uvBottomRight;
            vertices[3].uv0 = ch.uvBottomLeft;

            vertices[0].color = color;
            vertices[1].color = color;
            vertices[2].color = color;
            vertices[3].color = color;

            // Advance character position
            pos += new Vector3(ch.advance, 0, 0);

            toFill.AddUIVertexQuad(vertices);
        }

        m_DisableFontTextureRebuiltCallback = false;
    }

    void RebuildMesh(string str, ref VertexHelper toFill)
    {
        // Generate a mesh for the characters we want to print.
        //var vertices = new Vector3[str.Length * 4];
        var triangles = new int[str.Length * 6];
        var uv = new Vector2[str.Length * 4];
        Vector3 pos = Vector3.zero;
        for (int i = 0; i < str.Length; i++)
        {
            // Get character rendering information from the font
            CharacterInfo ch;
            font.GetCharacterInfo(str[i], out ch);

            UIVertex[] vertices = new UIVertex[4];
            /*
            vertices[4 * i + 0] = pos + new Vector3(ch.minX, ch.maxY, 0);
            vertices[4 * i + 1] = pos + new Vector3(ch.maxX, ch.maxY, 0);
            vertices[4 * i + 2] = pos + new Vector3(ch.maxX, ch.minY, 0);
            vertices[4 * i + 3] = pos + new Vector3(ch.minX, ch.minY, 0);
            
            uv[4 * i + 0] = ch.uvTopLeft;
            uv[4 * i + 1] = ch.uvTopRight;
            uv[4 * i + 2] = ch.uvBottomRight;
            uv[4 * i + 3] = ch.uvBottomLeft;

            triangles[6 * i + 0] = 4 * i + 0;
            triangles[6 * i + 1] = 4 * i + 1;
            triangles[6 * i + 2] = 4 * i + 2;

            triangles[6 * i + 3] = 4 * i + 0;
            triangles[6 * i + 4] = 4 * i + 2;
            triangles[6 * i + 5] = 4 * i + 3;
            */

            vertices[0].position = pos + new Vector3(ch.minX, ch.maxY, 0);
            vertices[1].position = pos + new Vector3(ch.maxX, ch.maxY, 0);
            vertices[2].position = pos + new Vector3(ch.maxX, ch.minY, 0);
            vertices[3].position = pos + new Vector3(ch.minX, ch.minY, 0);
            /*
            vertices[4 * i + 0].uv0 = ch.uvTopLeft;
            vertices[4 * i + 1] = ch.uvTopRight;
            vertices[4 * i + 2] = ch.uvBottomRight;
            vertices[4 * i + 3] = ch.uvBottomLeft;
            */

            // Advance character position
            pos += new Vector3(ch.advance, 0, 0);

            toFill.AddUIVertexQuad(vertices);
        }
    }
}

