using System.Linq;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEditorInternal;
using UnityEngine;

namespace Daihenka.ShaderStripper
{
    internal class StripHardwareTier : ShaderProcessor
    {
        [SerializeField] PlatformTiers[] platformTiers;

        public override bool ShouldStripVariant(Shader shader, ShaderSnippetData snippet, ShaderCompilerData data)
        {
            var platformTier = platformTiers.FirstOrDefault(x => x.platform == data.shaderCompilerPlatform);
            return platformTier != null && platformTier.ShouldStrip(data.graphicsTier);
        }
    }

    [CustomEditor(typeof(StripHardwareTier), true)]
    internal class StripHardwareTierInspector : ShaderProcessorInspector
    {
        ReorderableList m_List;

        void OnEnable()
        {
            m_List = new ReorderableList(serializedObject, serializedObject.FindProperty("platformTiers"), true, true, true, true);
            m_List.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Platform Tiers", EditorStyles.boldLabel);
            m_List.drawElementCallback = (rect, index, active, focused) => EditorGUI.PropertyField(rect, m_List.GetArrayElement(index), GUIContent.none);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();
            DrawHelpBox("This processor will strip shader variants that match the platforms tiers specified.");
            m_List.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }
}