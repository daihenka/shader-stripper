using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

namespace Daihenka.ShaderStripper
{
    public abstract class ShaderProcessor : ScriptableObject
    {
        public bool enabled;
        public string description;

        protected bool isDevelopmentBuild => EditorUserBuildSettings.development;

        public virtual void Initialize() {}

        public virtual bool IsValidProcessor(Shader shader, ShaderSnippetData snippet, ShaderCompilerData data)
        {
            return true;
        }

        public virtual bool ShouldIncludeVariant(Shader shader, ShaderSnippetData snippet, ShaderCompilerData data)
        {
            return true;
        }

        public virtual bool ShouldStripVariant(Shader shader, ShaderSnippetData snippet, ShaderCompilerData data)
        {
            return false;
        }
    }

    [CustomEditor(typeof(ShaderProcessor), true)]
    public class ShaderProcessorInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("description"), new GUIContent("Description"));
            EditorGUILayout.Space();
        }

        protected void DrawHelpBox(string message, MessageType messageType = MessageType.Info)
        {
            EditorGUILayout.HelpBox(message, messageType);
            EditorGUILayout.Space();
        }
    }
}