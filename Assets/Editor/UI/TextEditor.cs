using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(Text), true)]
    [CanEditMultipleObjects]
    /// <summary>
    ///   Custom Editor for the Text Component.
    ///   Extend this class to write a custom editor for an Text-derived component.
    /// </summary>
    public class TextEditor : GraphicEditor
    {
        SerializedProperty m_Text;
        SerializedProperty m_FontData;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Text = serializedObject.FindProperty("m_Text");
            m_FontData = serializedObject.FindProperty("m_FontData");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Text);
            EditorGUILayout.PropertyField(m_FontData);

            AppearanceControlsGUI();
            RaycastControlsGUI();
            serializedObject.ApplyModifiedProperties();
        }

        public override bool HasPreviewGUI() { return true; }

        public override void OnPreviewGUI(Rect rect, GUIStyle background)
        {
            Text text = target as Text;
            if (text == null) return;

            Texture texture = text.mainTexture;
            if (texture == null) return;

            Sprite sprite = Sprite.Create(texture as Texture2D, new Rect(0,0,texture.width,texture.height), Vector2.zero);

            SpriteDrawUtility.DrawSprite(sprite, rect, new Color(1,1,1,1));
        }
    }
}
