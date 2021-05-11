using System.Collections.Generic;
using System.Linq;
using Daihenka.ShaderStripper.ReflectionMagic;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEditorInternal;
using UnityEngine;

namespace Daihenka.ShaderStripper
{
    internal class IncludeShaderVariantCollection : ShaderProcessor
    {
        [SerializeField] List<ShaderVariantCollection> shaderVariantCollections = new List<ShaderVariantCollection>();
        [SerializeField] bool matchWithoutLocalKeywords;
        [SerializeField] bool onlyStripShadersInCollections;
        public List<ShaderVariantCollection> ShaderVariantCollections => shaderVariantCollections;

        List<ShaderVariant> variantCollections = new List<ShaderVariant>();

        public override void Initialize()
        {
            variantCollections = shaderVariantCollections.GetAllVariants();
        }

        public override bool ShouldIncludeVariant(Shader shader, ShaderSnippetData snippet, ShaderCompilerData data)
        {
            var keywords = data.shaderKeywordSet.ToList(shader);
            var shaderFound = false;
            foreach (var variant in variantCollections)
            {
                if (variant.shader == shader)
                {
                    shaderFound = true;
                    if (variant.keywords.HasSameElements(keywords))
                    {
                        return true;
                    }

                    if (matchWithoutLocalKeywords && variant.globalKeywords.HasSameElements(keywords))
                    {
                        return true;
                    }
                }
            }

            return onlyStripShadersInCollections && !shaderFound;
        }

        public override bool ShouldStripVariant(Shader shader, ShaderSnippetData snippet, ShaderCompilerData data)
        {
            return !ShouldIncludeVariant(shader, snippet, data);
        }
    }

    [CustomEditor(typeof(IncludeShaderVariantCollection), true)]
    internal class IncludeByShaderVariantCollectionInspector : ShaderProcessorInspector
    {
        ReorderableList m_List;
        IncludeShaderVariantCollection m_Target;
        SerializedProperty m_IncludeMatchesWithoutLocalKeywords;
        SerializedProperty m_OnlyStripShadersInCollections;
        static Styles s_Styles;

        class Styles
        {
            public static readonly GUIContent unityShaderVariantTracking = EditorGUIUtility.TrTextContent("Unity Shader Variant Tracking");
            public static readonly GUIContent shaderVariantCollections = EditorGUIUtility.TrTextContent("Shader Variant Collections");
            public static readonly GUIContent shaderPreloadSave = EditorGUIUtility.TrTextContent("Save to Shader Variant Collection", "Save currently tracked shaders into a Shader Variant Collection asset.");
            public static readonly GUIContent shaderPreloadClear = EditorGUIUtility.TrTextContent("Clear", "Clear currently tracked shader variant information.");
            public static readonly GUIContent combineVariantCollections = EditorGUIUtility.TrTextContent("Combine", "Validates shader variants and combines all listed Shader Variant Collections into a single collection.");
            public static readonly GUIContent includeMatchWithoutLocalKeywords = EditorGUIUtility.TrTextContent("Include matches without local keywords when processing variants", "When this processor executes, test variants with and without local keywords.");
            public static readonly GUIContent onlyStripShadersInCollections = EditorGUIUtility.TrTextContent("Only strip shaders that are listed in the collections", "When this processor executes, only strip shaders that are listed in the collections.  Shaders not in the collections will be skipped from this stripping processor.");
            public static readonly GUIContent optionsForStripping = EditorGUIUtility.TrTextContent("Options for stripping shader variants");
            public const string processorInfo = "This processor will include shader variants that are listed in the Shader Variant Collections.\nShader variants that are not in the Shader Variant Collections will be stripped.";
            public const string shaderVariantCollectionSave = "Save Shader Variant Collection";
            public const string unityShaderVariantTrackedFormat = "Unity has tracked {0} shaders {1} total variants from playing in the editor.";

            public readonly GUIStyle FooterButton = "RL FooterButton";
            public readonly GUIStyle FooterButtonBackground = "RL Footer";
        }

        void OnEnable()
        {
            m_Target = (IncludeShaderVariantCollection) target;
            m_IncludeMatchesWithoutLocalKeywords = serializedObject.FindProperty("matchWithoutLocalKeywords");
            m_OnlyStripShadersInCollections = serializedObject.FindProperty("onlyStripShadersInCollections");
            m_List = new ReorderableList(serializedObject, serializedObject.FindProperty("shaderVariantCollections"), true, true, true, true);
            m_List.drawHeaderCallback = rect => EditorGUI.LabelField(rect, Styles.shaderVariantCollections, EditorStyles.boldLabel);
            m_List.drawElementCallback = (rect, index, active, focused) => EditorGUI.PropertyField(rect, m_List.GetArrayElement(index), GUIContent.none);
            m_List.drawFooterCallback = rect =>
            {
                var buttonRect = new Rect(rect.x + 10, rect.y, 100, rect.height);
                if (Event.current.type == EventType.Repaint)
                    s_Styles.FooterButtonBackground.Draw(buttonRect, false, false, false, false);
                buttonRect.x += 4;
                buttonRect.width -= 8;
                buttonRect.height = 16;
                if (GUI.Button(buttonRect, Styles.combineVariantCollections, s_Styles.FooterButton))
                {
                    string assetPath = EditorUtility.SaveFilePanelInProject(Styles.shaderVariantCollectionSave, "NewShaderVariants", "shadervariants", Styles.shaderVariantCollectionSave, UnityEditorDynamic.ProjectWindowUtil.GetActiveFolderPath());
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        CombineShaderVariantCollections(assetPath);
                    }
                }
                ReorderableList.defaultBehaviours.DrawFooter(rect, m_List);
            };
        }

        public override void OnInspectorGUI()
        {
            if (s_Styles == null)
            {
                s_Styles = new Styles();
            }
            serializedObject.Update();
            base.OnInspectorGUI();
            DrawHelpBox(Styles.processorInfo);
            DrawGraphicsSettingsBlock();
            GUILayout.Label(Styles.optionsForStripping, EditorStyles.boldLabel);
            DrawIncludeMatchesWithoutLocalKeywordsBlock();
            DrawOnlyStripShadersInCollectionBlock();
            EditorGUILayout.Space(12);
            m_List.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }

        void DrawOnlyStripShadersInCollectionBlock()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(m_OnlyStripShadersInCollections, GUIContent.none, GUILayout.Width(16));
            GUILayout.Label(Styles.onlyStripShadersInCollections);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
        void DrawIncludeMatchesWithoutLocalKeywordsBlock()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(m_IncludeMatchesWithoutLocalKeywords, GUIContent.none, GUILayout.Width(16));
            GUILayout.Label(Styles.includeMatchWithoutLocalKeywords);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        void DrawGraphicsSettingsBlock()
        {
            EditorGUILayout.Space();
            GUILayout.Label(Styles.unityShaderVariantTracking, EditorStyles.boldLabel);
            GUILayout.Label(string.Format(Styles.unityShaderVariantTrackedFormat, UnityEditorDynamic.ShaderUtil.GetCurrentShaderVariantCollectionShaderCount(), UnityEditorDynamic.ShaderUtil.GetCurrentShaderVariantCollectionVariantCount()));
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(Styles.shaderPreloadSave, EditorStyles.miniButton))
            {
                string assetPath = EditorUtility.SaveFilePanelInProject(Styles.shaderVariantCollectionSave, "NewShaderVariants", "shadervariants", Styles.shaderVariantCollectionSave, UnityEditorDynamic.ProjectWindowUtil.GetActiveFolderPath());
                if (!string.IsNullOrEmpty(assetPath))
                {
                    SaveUnityTrackedShaderVariantCollection(assetPath);
                }

                GUIUtility.ExitGUI();
            }

            if (GUILayout.Button(Styles.shaderPreloadClear, EditorStyles.miniButton))
            {
                UnityEditorDynamic.ShaderUtil.ClearCurrentShaderVariantCollection();
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(12);
        }

        void SaveUnityTrackedShaderVariantCollection(string assetPath)
        {
            UnityEditorDynamic.ShaderUtil.SaveCurrentShaderVariantCollection(assetPath);
            var asset = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>(assetPath);
            if (asset != null && !m_Target.ShaderVariantCollections.Contains(asset))
            {
                serializedObject.ApplyModifiedProperties();
                m_Target.ShaderVariantCollections.Add(asset);
                for (var i = m_Target.ShaderVariantCollections.Count - 1; i >= 0; i--)
                {
                    var svc = m_Target.ShaderVariantCollections[i];
                    if (svc == null)
                    {
                        m_Target.ShaderVariantCollections.RemoveAt(i);
                    }
                }

                serializedObject.Update();
            }
        }

        void CombineShaderVariantCollections(string assetPath)
        {
            var shaderVariantType = typeof(ShaderVariantCollection.ShaderVariant).AsDynamicType();
            var validSvc = new ShaderVariantCollection();
            foreach (var svc in m_Target.ShaderVariantCollections)
            {
                if (svc == null) continue;

                var proxy = svc.GetProxy();
                var shaders = proxy.Shaders;
                foreach (var shader in shaders)
                {
                    var variants = proxy.GetVariants(shader);
                    foreach (var variant in variants)
                    {
                        string message = shaderVariantType.CheckShaderVariant(shader, variant.passType, variant.keywords);
                        if (message.IsNullOrWhiteSpace())
                        {
                            validSvc.Add(new ShaderVariantCollection.ShaderVariant(shader, variant.passType, variant.keywords));
                        }
                        else
                        {
                            var validKeywords = variant.keywords.Where(x => ((string) shaderVariantType.CheckShaderVariant(shader, variant.passType, new[] {x})).IsNullOrWhiteSpace()).ToArray();
                            validSvc.Add(new ShaderVariantCollection.ShaderVariant(shader, variant.passType, validKeywords));
                        }
                    }
                }
            }

            AssetDatabase.CreateAsset(validSvc, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            m_Target.ShaderVariantCollections.Clear();
            m_Target.ShaderVariantCollections.Add(validSvc);
            serializedObject.Update();
            GUIUtility.ExitGUI();
        }
    }
}