using System.Linq;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEditorInternal;
using UnityEngine;

namespace Daihenka.ShaderStripper
{
    internal class StripPlatforms : ShaderProcessor
    {
        [SerializeField] ShaderCompilerPlatform[] platforms;

        public override bool ShouldStripVariant(Shader shader, ShaderSnippetData snippet, ShaderCompilerData data)
        {
            return platforms.Contains(data.shaderCompilerPlatform);
        }
    }

    [CustomEditor(typeof(StripPlatforms), true)]
    internal class StripPlatformsInspector : ShaderProcessorInspector
    {
        ReorderableList m_List;

        void OnEnable()
        {
            m_List = new ReorderableList(serializedObject, serializedObject.FindProperty("platforms"), true, true, true, true);
            m_List.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Platforms", EditorStyles.boldLabel);
            m_List.drawElementCallback = (rect, index, active, focused) => EditorGUI.PropertyField(rect, m_List.GetArrayElement(index), GUIContent.none);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();
            DrawHelpBox("This processor will strip shader variants that match the platforms specified.");
            m_List.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }
}