using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using UnityObject = UnityEngine.Object;

namespace Daihenka.ShaderStripper
{
    internal class ShaderStripperSettingsProvider : SettingsProvider
    {
        SerializedObject m_Settings;
        ShaderStripperSettings m_Target;
        ReorderableList m_ProcessorList;
        Editor m_CachedProcessorEditor;
        string m_LogPath;

        ShaderStripperSettingsProvider(string path, SettingsScope scope = SettingsScope.User) : base(path, scope)
        {
        }

        static bool IsSettingsAvailable()
        {
            return AssetDatabase.FindAssets("t:ShaderStripperSettings").Length > 0;
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            m_Settings = ShaderStripperSettings.GetSerializedSettings();
            m_Target = (ShaderStripperSettings) m_Settings.targetObject;

            m_ProcessorList = new ReorderableList(m_Settings, m_Settings.FindProperty("m_Processors"), true, true, true, true);
            m_ProcessorList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Processors are executed from top to bottom per shader variant", new GUIStyle(EditorStyles.miniLabel) {alignment = TextAnchor.MiddleCenter});
                EditorGUI.LabelField(rect, "Shader Processors", EditorStyles.boldLabel);
            };
            m_ProcessorList.drawElementCallback = OnDrawProcessorListItem;
            m_ProcessorList.onAddCallback = OnAddProcessorButtonClicked;
            m_ProcessorList.onRemoveCallback = OnRemoveProcessorButtonClicked;
            m_ProcessorList.onSelectCallback = list => DestroyCachedEditor();

            m_LogPath = ShaderStripperSettings.LogPath;
        }

        public override void OnDeactivate()
        {
            DestroyCachedEditor();
        }

        public override void OnGUI(string searchContext)
        {
            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 200;
            m_Settings.Update();
            GUILayout.BeginVertical(new GUIStyle {padding = new RectOffset(18, 18, 14, 4)});
            DrawLogBlock();
            EditorGUILayout.PropertyField(m_Settings.FindProperty("m_ProcessAlwaysIncludedShaders"));
            EditorGUILayout.Space(12);
            m_ProcessorList.DoLayoutList();
            DrawProcessorInspector();
            GUILayout.EndVertical();
            m_Settings.ApplyModifiedProperties();
            EditorGUIUtility.labelWidth = labelWidth;
        }

        void DrawLogBlock()
        {
            GUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            m_LogPath = EditorGUILayout.TextField("Log Folder Path", m_LogPath);
            if (GUILayout.Button("…", GUILayout.Width(25)))
            {
                var path = EditorUtility.OpenFolderPanel("Select Log Folder", m_LogPath, "");
                if (!path.IsNullOrWhiteSpace())
                {
                    m_LogPath = path;
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                ShaderStripperSettings.LogPath = m_LogPath;
                Repaint();
            }

            GUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(m_Settings.FindProperty("m_LogIncludedVariants"));
            EditorGUILayout.PropertyField(m_Settings.FindProperty("m_LogStrippedVariants"));
            EditorGUILayout.Space(12);
        }

        void DrawProcessorInspector()
        {
            if (!m_ProcessorList.HasSelection()) return;
            var processor = (ShaderProcessor) m_ProcessorList.GetArrayElement(m_ProcessorList.index)?.objectReferenceValue;
            if (processor != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(processor.GetType().GetFriendlyName() + " Processor Settings", EditorStyles.boldLabel);
                Editor.CreateCachedEditor(processor, null, ref m_CachedProcessorEditor);
                m_CachedProcessorEditor.OnInspectorGUI();
            }

            EditorGUILayout.Space();
        }

        void OnDrawProcessorListItem(Rect rect, int index, bool active, bool focused)
        {
            var element = m_ProcessorList.GetArrayElement(index);
            var value = element.objectReferenceValue as ShaderProcessor;
            if (value == null)
            {
                EditorGUI.LabelField(rect, "<null>");
                return;
            }

            EditorGUI.BeginChangeCheck();
            value.enabled = EditorGUI.Toggle(new Rect(rect.x, rect.y, 16, rect.height), GUIContent.none, value.enabled);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_Target);
                m_Settings.Update();
            }

            EditorGUI.LabelField(new Rect(rect.x + 20, rect.y, 250, rect.height), element.objectReferenceValue.GetType().GetFriendlyName());
            EditorGUI.LabelField(new Rect(rect.x + 274, rect.y, rect.width - (rect.x + 274), rect.height), value.description, EditorStyles.miniLabel);
        }

        void OnAddProcessorButtonClicked(ReorderableList list)
        {
            var menu = new GenericMenu();
            var processorClasses = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes().Where(t => t.IsSubclassOf(typeof(ShaderProcessor)))).OrderBy(x => x.Name);
            var regex = new Regex(Regex.Escape(" "));
            foreach (var processorClass in processorClasses)
            {
                var mutualExclusiveTypes = processorClass.GetCustomAttribute<MutuallyExclusiveShaderProcessorAttribute>()?.Types;
                if (mutualExclusiveTypes != null)
                {
                    if (m_Target.processors.Any(x => mutualExclusiveTypes.Contains(x.GetType())))
                    {
                        continue;
                    }
                }

                var allowOnce = processorClass.GetCustomAttribute<ShaderProcessorAllowOnceAttribute>();
                if (allowOnce != null)
                {
                    if (m_Target.processors.Any(x => processorClass == x.GetType()))
                    {
                        continue;
                    }
                }

                var contentText = processorClass.GetFriendlyName();
                if (contentText.StartsWith("Include ") || contentText.StartsWith("Strip "))
                {
                    contentText = regex.Replace(contentText, "/", 1);
                }

                menu.AddItem(new GUIContent(contentText), false, OnAddProcessor, processorClass);
            }

            menu.ShowAsContext();
        }

        void OnAddProcessor(object userdata)
        {
            var type = (Type) userdata;
            var instance = ScriptableObject.CreateInstance(type);
            instance.name = type.Name;
            var index = m_ProcessorList.serializedProperty.arraySize;
            m_ProcessorList.serializedProperty.arraySize++;
            m_ProcessorList.index = index;
            var element = m_ProcessorList.GetArrayElement(index);
            instance.AddObjectToUnityAsset(m_Target);
            element.objectReferenceValue = instance;
            m_Settings.ApplyModifiedProperties();
        }

        void OnRemoveProcessorButtonClicked(ReorderableList list)
        {
            var element = list.GetArrayElement(list.index);
            var obj = element.objectReferenceValue;
            obj.RemoveNestedObjectsFromUnityAsset(AssetDatabase.GetAssetPath(m_Target));
            ReorderableList.defaultBehaviours.DoRemoveButton(list); // remove the SerializedProperty from the element
            ReorderableList.defaultBehaviours.DoRemoveButton(list); // remove the empty element from the collection
            m_Settings.ApplyModifiedProperties();
            EditorUtility.SetDirty(m_Target);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        void DestroyCachedEditor()
        {
            if (m_CachedProcessorEditor != null)
            {
                UnityObject.DestroyImmediate(m_CachedProcessorEditor);
            }
        }

        [SettingsProvider]
        public static SettingsProvider CreateProjectSettingsProvider()
        {
            if (!IsSettingsAvailable())
            {
                ShaderStripperSettings.GetOrCreateSettings();
            }

            return new ShaderStripperSettingsProvider("Project/Daihenka/Shader Stripping", SettingsScope.Project)
            {
                // TODO Setup keywords
                keywords = new string[0]
            };
        }
    }
}