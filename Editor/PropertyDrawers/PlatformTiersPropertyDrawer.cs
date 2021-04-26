using UnityEditor;
using UnityEngine;

namespace Daihenka.ShaderStripper
{
    [CustomPropertyDrawer(typeof(PlatformTiers))]
    internal class PlatformTiersPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            const float spacer = 4;
            var dropdownRect = new Rect(position.x, position.y, 160, position.height);
            var tier1Rect = new Rect(dropdownRect.xMax + spacer * 6, position.y, 16, position.height);
            var tier1LabelRect = new Rect(tier1Rect.xMax + spacer, position.y, 40, position.height);
            var tier2Rect = new Rect(tier1LabelRect.xMax + spacer * 3, position.y, 16, position.height);
            var tier2LabelRect = new Rect(tier2Rect.xMax + spacer, position.y, 40, position.height);
            var tier3Rect = new Rect(tier2LabelRect.xMax + spacer * 3, position.y, 16, position.height);
            var tier3LabelRect = new Rect(tier3Rect.xMax + spacer, position.y, 40, position.height);

            EditorGUI.PropertyField(dropdownRect, property.FindPropertyRelative("platform"), GUIContent.none);
            EditorGUI.LabelField(tier1LabelRect, "Tier 1");
            EditorGUI.PropertyField(tier1Rect, property.FindPropertyRelative("stripTier1"), GUIContent.none);
            EditorGUI.LabelField(tier2LabelRect, "Tier 2");
            EditorGUI.PropertyField(tier2Rect, property.FindPropertyRelative("stripTier2"), GUIContent.none);
            EditorGUI.LabelField(tier3LabelRect, "Tier 3");
            EditorGUI.PropertyField(tier3Rect, property.FindPropertyRelative("stripTier3"), GUIContent.none);

            EditorGUI.EndProperty();
        }
    }
}