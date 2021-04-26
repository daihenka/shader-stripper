using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEditorInternal;
using UnityEngine;

namespace Daihenka.ShaderStripper
{
    internal class StripKeywords : ShaderProcessor
    {
        static readonly string[] s_VRKeywords = {"UNITY_SINGLE_PASS_STEREO", "STEREO_INSTANCING_ON", "STEREO_MULTIVIEW_ON", "STEREO_CUBEMAP_RENDER_ON"};
        static readonly string[] s_InstancingKeywords = {"INSTANCING_ON"};
        static readonly string[] s_FogKeywords = {"FOG_LINEAR", "FOG_EXP", "FOG_EXP2"};
        static readonly string[] s_LightmapKeywords = {"LIGHTMAP_ON", "DIRLIGHTMAP_COMBINED", "DYNAMICLIGHTMAP_ON", "LIGHTMAP_SHADOW_MIXING", "SHADOWS_SHADOWMASK"};
        static readonly string[] s_DotsKeywords = {"UNITY_DOTS_INSTANCING_ENABLED"};

        [SerializeField] List<StringFilter> excludeDevelopmentKeywords = new List<StringFilter>();
        [SerializeField] List<StringFilter> excludeReleaseKeywords = new List<StringFilter>();
        [SerializeField] bool stripVrVariants;
        [SerializeField] bool stripLightmapVariants;
        [SerializeField] bool stripFogVariants;
        [SerializeField] bool stripInstancingVariants;
        [SerializeField] bool stripDotsVariants;

        public override bool ShouldStripVariant(Shader shader, ShaderSnippetData snippet, ShaderCompilerData data)
        {
            var keywords = data.shaderKeywordSet.ToList(shader);
            var exclusionFilters = isDevelopmentBuild ? excludeDevelopmentKeywords : excludeReleaseKeywords;

            if (exclusionFilters.Any(exclusionFilter => keywords.Any(exclusionFilter.IsMatch)))
            {
                return true;
            }

            var excludedKeywords = GetExcludedKeywords();
            return keywords.Any(x => excludedKeywords.Contains(x));
        }

        List<string> GetExcludedKeywords()
        {
            var exclusions = new List<string>();

            if (stripDotsVariants) exclusions.AddRange(s_DotsKeywords);
            if (stripVrVariants) exclusions.AddRange(s_VRKeywords);
            if (stripInstancingVariants) exclusions.AddRange(s_InstancingKeywords);
            if (stripLightmapVariants) exclusions.AddRange(s_LightmapKeywords);
            if (stripFogVariants) exclusions.AddRange(s_FogKeywords);

            return exclusions;
        }
    }

    [CustomEditor(typeof(StripKeywords), true)]
    internal class StripKeywordsInspector : ShaderProcessorInspector
    {
        ReorderableList m_DevelopmentList;
        ReorderableList m_ReleaseList;

        void OnEnable()
        {
            m_DevelopmentList = new ReorderableList(serializedObject, serializedObject.FindProperty("excludeDevelopmentKeywords"), true, true, true, true);
            m_DevelopmentList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Keywords to exclude in Development builds", EditorStyles.boldLabel);
                EditorGUI.LabelField(new Rect(rect.x + rect.width - 75, rect.y, 75, rect.height), "Ignore Case", EditorStyles.boldLabel);
            };
            m_DevelopmentList.drawElementCallback = (rect, index, active, focused) => EditorGUI.PropertyField(rect, m_DevelopmentList.GetArrayElement(index), GUIContent.none);
            m_ReleaseList = new ReorderableList(serializedObject, serializedObject.FindProperty("excludeReleaseKeywords"), true, true, true, true);
            m_ReleaseList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Keywords to exclude in Release builds", EditorStyles.boldLabel);
                EditorGUI.LabelField(new Rect(rect.x + rect.width - 75, rect.y, 75, rect.height), "Ignore Case", EditorStyles.boldLabel);
            };
            m_ReleaseList.drawElementCallback = (rect, index, active, focused) => EditorGUI.PropertyField(rect, m_ReleaseList.GetArrayElement(index), GUIContent.none);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();
            DrawHelpBox("This processor will strip shader variants where they match the specified keywords.");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("stripVrVariants"), new GUIContent("Strip VR Variants"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("stripLightmapVariants"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("stripFogVariants"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("stripInstancingVariants"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("stripDotsVariants"), new GUIContent("Strip DOTS Variants"));
            EditorGUILayout.Space(12);
            m_DevelopmentList.DoLayoutList();
            EditorGUILayout.Space(12);
            m_ReleaseList.DoLayoutList();
            EditorGUILayout.Space(12);
            serializedObject.ApplyModifiedProperties();
        }
    }
}