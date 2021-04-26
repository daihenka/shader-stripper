using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;

namespace Daihenka.ShaderStripper
{
    internal class StripPasses : ShaderProcessor
    {
        [SerializeField] List<ShaderPassStripData> passTypes = new List<ShaderPassStripData>();

        public override bool ShouldStripVariant(Shader shader, ShaderSnippetData snippet, ShaderCompilerData data)
        {
            return passTypes.Any(x => x.shaderNameFilter.IsMatch(shader.name) && x.passType == snippet.passType);
        }
    }

    [Serializable]
    internal class ShaderPassStripData
    {
        public ShaderNameFilter shaderNameFilter;
        public PassType passType;
    }

    [CustomPropertyDrawer(typeof(ShaderPassStripData))]
    internal class ShaderPassStripDataPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            const float passDropdownWidth = 150;
            const float spacer = 4;
            var passDropdownRect = new Rect(position.x, position.y + 1, passDropdownWidth, position.height);
            var filterRect = new Rect(position.x + spacer + passDropdownWidth, position.y + 1, position.width - (passDropdownWidth + spacer), position.height);

            EditorGUI.PropertyField(filterRect, property.FindPropertyRelative("shaderNameFilter"), GUIContent.none);
            EditorGUI.PropertyField(passDropdownRect, property.FindPropertyRelative("passType"), GUIContent.none);

            EditorGUI.EndProperty();
        }
    }

    [CustomEditor(typeof(StripPasses), true)]
    internal class StripPassesInspector : ShaderProcessorInspector
    {
        ReorderableList m_List;

        void OnEnable()
        {
            m_List = new ReorderableList(serializedObject, serializedObject.FindProperty("passTypes"), true, true, true, true);
            m_List.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(new Rect(rect.x + 16, rect.y, 150, rect.height), "Pass", EditorStyles.boldLabel);
                EditorGUI.LabelField(new Rect(rect.x + 170, rect.y, rect.width - 154, rect.height), "Shader Name", EditorStyles.boldLabel);
                EditorGUI.LabelField(new Rect(rect.x + rect.width - 75, rect.y, 75, rect.height), "Ignore Case", EditorStyles.boldLabel);
            };
            m_List.drawElementCallback = (rect, index, active, focused) => EditorGUI.PropertyField(rect, m_List.GetArrayElement(index), GUIContent.none);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();
            DrawHelpBox("This processor will strip shader variants that match the passes specified.");
            m_List.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }
}