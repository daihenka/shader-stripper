using System;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

namespace Daihenka.ShaderStripper
{
    [ShaderProcessorAllowOnce]
    [MutuallyExclusiveShaderProcessor(typeof(IncludeInternalShaders))]
    internal class StripInternalShaders : ShaderProcessor
    {
        public override bool IsValidProcessor(Shader shader, ShaderSnippetData snippet, ShaderCompilerData data)
        {
            return AssetDatabase.GetAssetPath(shader).IsNullOrWhiteSpace() ||
                   shader.name.StartsWith("Hidden/Internal", StringComparison.OrdinalIgnoreCase);
        }

        public override bool ShouldStripVariant(Shader shader, ShaderSnippetData snippet, ShaderCompilerData data)
        {
            return true;
        }
    }

    [CustomEditor(typeof(StripInternalShaders))]
    internal class StripInternalShadersInspector : ShaderProcessorInspector
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();
            DrawHelpBox("This processor will strip built-in and internal shaders.\nThis processor can only be added once.");
            serializedObject.ApplyModifiedProperties();
        }
    }
}