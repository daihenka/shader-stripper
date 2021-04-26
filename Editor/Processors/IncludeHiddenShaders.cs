using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

namespace Daihenka.ShaderStripper
{
    [ShaderProcessorAllowOnce]
    [MutuallyExclusiveShaderProcessor(typeof(StripHiddenShaders))]
    internal class IncludeHiddenShaders : ShaderProcessor
    {
        public override bool IsValidProcessor(Shader shader, ShaderSnippetData snippet, ShaderCompilerData data)
        {
            return shader.name.StartsWith("Hidden/");
        }

        public override bool ShouldIncludeVariant(Shader shader, ShaderSnippetData snippet, ShaderCompilerData data)
        {
            return true;
        }
    }

    [CustomEditor(typeof(IncludeHiddenShaders))]
    internal class IncludeHiddenShadersInspector : ShaderProcessorInspector
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();
            DrawHelpBox("This processor will include hidden shaders.\nThis processor can only be added once.");
            serializedObject.ApplyModifiedProperties();
        }
    }
}