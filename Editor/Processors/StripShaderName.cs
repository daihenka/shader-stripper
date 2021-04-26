using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEditorInternal;
using UnityEngine;

namespace Daihenka.ShaderStripper
{
    internal class StripShaderName : ShaderProcessor
    {
        [SerializeField] List<StringFilter> shaderNames = new List<StringFilter>();

        public override bool ShouldStripVariant(Shader shader, ShaderSnippetData snippet, ShaderCompilerData data)
        {
            return shaderNames.Any(x => x.IsMatch(shader.name));
        }
    }

    [CustomEditor(typeof(StripShaderName), true)]
    internal class StripShaderNameInspector : ShaderProcessorInspector
    {
        ReorderableList m_List;

        void OnEnable()
        {
            m_List = new ReorderableList(serializedObject, serializedObject.FindProperty("shaderNames"), true, true, true, true);
            m_List.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Shader Names", EditorStyles.boldLabel);
                EditorGUI.LabelField(new Rect(rect.x + rect.width - 75, rect.y, 75, rect.height), "Ignore Case", EditorStyles.boldLabel);
            };
            m_List.drawElementCallback = (rect, index, active, focused) => EditorGUI.PropertyField(rect, m_List.GetArrayElement(index), GUIContent.none);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();
            DrawHelpBox("This processor will strip shaders that match the name filters.");
            m_List.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }
}