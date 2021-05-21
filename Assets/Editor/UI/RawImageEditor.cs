using UnityEngine;
using UnityEngine.UI;
using UnityEditor.AnimatedValues;
using System.Linq;

namespace UnityEditor.UI
{
    /// <summary>
    /// Editor class used to edit UI Images.
    /// </summary>
    [CustomEditor(typeof(RawImage), true)]
    [CanEditMultipleObjects]
    /// <summary>
    ///   Custom editor for RawImage.
    ///   Extend this class to write a custom editor for a RawImage-derived component.
    /// </summary>
    public class RawImageEditor : GraphicEditor
    {
        SerializedProperty m_Texture;
        SerializedProperty m_UVRect;
        GUIContent m_UVRectContent;

        GUIContent m_SetUVRectButtonContent;
        AnimBool m_SetUVRect;

        protected override void OnDisable()
        {
            base.OnDisable();
            m_SetUVRect.valueChanged.RemoveListener(Repaint);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            // Note we have precedence for calling rectangle for just rect, even in the Inspector.
            // For example in the Camera component's Viewport Rect.
            // Hence sticking with Rect here to be consistent with corresponding property in the API.
            m_UVRectContent     = EditorGUIUtility.TrTextContent("UV Rect");

            m_Texture           = serializedObject.FindProperty("m_Texture");
            m_UVRect            = serializedObject.FindProperty("m_UVRect");

            SetShowNativeSize(true);

            //增加根据当前节点宽高设置UVRect属性
            m_SetUVRectButtonContent = EditorGUIUtility.TrTextContent("Set UVRect Value", "Sets the values to UVRect.");
            m_SetUVRect = new AnimBool(true);
            m_SetUVRect.valueChanged.AddListener(Repaint);
            SetUVRect(true);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Texture);

            AppearanceControlsGUI();
            RaycastControlsGUI();
            EditorGUILayout.PropertyField(m_UVRect, m_UVRectContent);
            SetShowNativeSize(false);
            NativeSizeButtonGUI();

            //增加设置UVRect values的按钮
            SetUVRect(false);
            if (EditorGUILayout.BeginFadeGroup(m_SetUVRect.faded))
            {
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Space(EditorGUIUtility.labelWidth);
                    if (GUILayout.Button(m_SetUVRectButtonContent, EditorStyles.miniButton))
                    {
                        foreach (RawImage rawImage in targets.Select(obj => obj as RawImage))
                        {
                            Undo.RecordObject(rawImage.rectTransform, "Set UVRect value");
                            rawImage.SetUVRectValues();
                            EditorUtility.SetDirty(rawImage);
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndFadeGroup();

            serializedObject.ApplyModifiedProperties();
        }

        void SetShowNativeSize(bool instant)
        {
            base.SetShowNativeSize(m_Texture.objectReferenceValue != null, instant);
        }

        void SetUVRect(bool instant)
        {
            if (instant)
                m_ShowNativeSize.value = m_Texture.objectReferenceValue != null;
            else
                m_ShowNativeSize.target = m_Texture.objectReferenceValue != null;
        }

        private static Rect Outer(RawImage rawImage)
        {
            Rect outer = rawImage.uvRect;
            outer.xMin *= rawImage.rectTransform.rect.width;
            outer.xMax *= rawImage.rectTransform.rect.width;
            outer.yMin *= rawImage.rectTransform.rect.height;
            outer.yMax *= rawImage.rectTransform.rect.height;
            return outer;
        }

        /// <summary>
        /// Allow the texture to be previewed.
        /// </summary>

        public override bool HasPreviewGUI()
        {
            RawImage rawImage = target as RawImage;
            if (rawImage == null)
                return false;

            var outer = Outer(rawImage);
            return outer.width > 0 && outer.height > 0;
        }

        /// <summary>
        /// Draw the Image preview.
        /// </summary>

        public override void OnPreviewGUI(Rect rect, GUIStyle background)
        {
            RawImage rawImage = target as RawImage;
            Texture tex = rawImage.mainTexture;

            if (tex == null)
                return;

            var outer = Outer(rawImage);
            SpriteDrawUtility.DrawSprite(tex, rect, outer, rawImage.uvRect, rawImage.canvasRenderer.GetColor());
        }

        /// <summary>
        /// Info String drawn at the bottom of the Preview
        /// </summary>

        public override string GetInfoString()
        {
            RawImage rawImage = target as RawImage;

            // Image size Text
            string text = string.Format("RawImage Size: {0}x{1}",
                Mathf.RoundToInt(Mathf.Abs(rawImage.rectTransform.rect.width)),
                Mathf.RoundToInt(Mathf.Abs(rawImage.rectTransform.rect.height)));

            return text;
        }
    }
}
