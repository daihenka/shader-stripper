using System;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

namespace Daihenka.ShaderStripper
{
    [ShaderProcessorAllowOnce]
    [MutuallyExclusiveShaderProcessor(typeof(IncludeHiddenShaders))]
    internal class StripHiddenShaders : ShaderProcessor
    {
        public override bool IsValidProcessor(Shader shader, ShaderSnippetData snippet, ShaderCompilerData data)
        {
            // Strip hidden shaders but not internal ones.
            // Stripping internal shaders can cause rendering issue in builds
            return shader.name.StartsWith("Hidden/", StringComparison.OrdinalIgnoreCase) &&
                   !shader.name.StartsWith("Hidden/Internal", StringComparison.OrdinalIgnoreCase);
        }

        public override bool ShouldStripVariant(Shader shader, ShaderSnippetData snippet, ShaderCompilerData data)
        {
            return true;
        }
    }

    [CustomEditor(typeof(StripHiddenShaders))]
    internal class StripHiddenShadersInspector : ShaderProcessorInspector
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();
            DrawHelpBox("This processor will strip hidden shaders.\nThis processor can only be added once.");
            serializedObject.ApplyModifiedProperties();
        }
    }
}