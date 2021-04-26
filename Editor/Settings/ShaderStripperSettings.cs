using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Daihenka.ShaderStripper
{
    public class ShaderStripperSettings : ScriptableObject
    {
        const string kSettingsPath = "Assets/Editor/ShaderStripperSettings.asset";
        public const string kLogPathKey = "DaihenkaShaderStripperLogPath";

#pragma warning disable 0414
        [SerializeField] List<ShaderProcessor> m_Processors = new List<ShaderProcessor>();
        [SerializeField] bool m_LogIncludedVariants = true;
        [SerializeField] bool m_LogStrippedVariants;
        [SerializeField] bool m_ProcessAlwaysIncludedShaders = true;
#pragma warning restore 0414

        public List<ShaderProcessor> processors => m_Processors;
        public bool logIncludedVariants => m_LogIncludedVariants;
        public bool logStrippedVariants => m_LogStrippedVariants;

        public bool processAlwaysIncludedShaders => m_ProcessAlwaysIncludedShaders;

        public static string LogPath
        {
            get => EditorPrefs.GetString(kLogPathKey, Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/Assets")) + "/Logs/");
            set => EditorPrefs.SetString(kLogPathKey, value);
        }

        internal static ShaderStripperSettings GetOrCreateSettings()
        {
            ShaderStripperSettings settings = null;
            var settingsPath = AssetDatabase.FindAssets("t:ShaderStripperSettings").Select(AssetDatabase.GUIDToAssetPath).FirstOrDefault();
            if (!string.IsNullOrEmpty(settingsPath))
            {
                settings = AssetDatabase.LoadAssetAtPath<ShaderStripperSettings>(settingsPath);
            }

            if (!settings)
            {
                settings = CreateInstance<ShaderStripperSettings>();
                var folderPath = Path.GetDirectoryName(kSettingsPath);
                PathUtility.CreateDirectoryIfNeeded(folderPath);
                AssetDatabase.CreateAsset(settings, kSettingsPath);
                AssetDatabase.SaveAssets();
            }

            return settings;
        }

        internal static ShaderProcessor[] GetEnabledShaderProcessors()
        {
            return GetOrCreateSettings().processors.Where(x => x.enabled).ToArray();
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
    }
}