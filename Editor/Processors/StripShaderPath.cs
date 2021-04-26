using System.Linq;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEditorInternal;
using UnityEngine;

namespace Daihenka.ShaderStripper
{
    internal class StripShaderPath : ShaderProcessor
    {
        [SerializeField] StringFilter[] paths;

        public override bool ShouldStripVariant(Shader shader, ShaderSnippetData snippet, ShaderCompilerData data)
        {
            var shaderPath = AssetDatabase.GetAssetPath(shader);
            return paths.Any(x => x.IsMatch(shaderPath));
        }
    }

    [CustomEditor(typeof(StripShaderPath), true)]
    internal class StripShaderPathInspector : ShaderProcessorInspector
    {
        ReorderableList m_List;

        void OnEnable()
        {
            m_List = new ReorderableList(serializedObject, serializedObject.FindProperty("paths"), true, true, true, true);
            m_List.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Shader Paths", EditorStyles.boldLabel);
                EditorGUI.LabelField(new Rect(rect.x + rect.width - 75, rect.y, 75, rect.height), "Ignore Case", EditorStyles.boldLabel);
            };
            m_List.drawElementCallback = (rect, index, active, focused) => EditorGUI.PropertyField(rect, m_List.GetArrayElement(index), GUIContent.none);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();
            DrawHelpBox("This processor will strip shaders where their path matches the path filters.");
            m_List.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }
}