using UnityEditor;
using UnityEngine;

namespace Daihenka.ShaderStripper
{
    [CustomPropertyDrawer(typeof(StringFilter))]
    internal class StringFilterPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            const float dropdownWidth = 90;
            const float toggleWidth = 16;
            const float spacer = 4;
            var dropdownRect = new Rect(position.x, position.y + 1, dropdownWidth, position.height);
            var toggleRect = new Rect(position.x + (position.width - (spacer + toggleWidth)), position.y, toggleWidth, position.height);
            var textFieldRect = new Rect(position.x + dropdownWidth + spacer, position.y, position.width - (dropdownWidth + toggleWidth + spacer * 4), position.height);

            EditorGUI.PropertyField(dropdownRect, property.FindPropertyRelative("matchType"), GUIContent.none);
            EditorGUI.PropertyField(textFieldRect, property.FindPropertyRelative("pattern"), GUIContent.none);
            EditorGUI.PropertyField(toggleRect, property.FindPropertyRelative("ignoreCase"), GUIContent.none);

            EditorGUI.EndProperty();
        }
    }
}