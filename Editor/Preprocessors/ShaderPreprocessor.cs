using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace Daihenka.ShaderStripper
{
    public class ShaderPreprocessor : IPreprocessShaders, IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        static ShaderProcessor[] s_ShaderProcessors;

        static ShaderProcessor[] ShaderProcessors => s_ShaderProcessors ?? (s_ShaderProcessors = ShaderStripperSettings.GetEnabledShaderProcessors());

        const string kShouldIncludeVariantMethodName = "ShouldIncludeVariant";
        const string kShouldStripVariantMethodName = "ShouldStripVariant";
        const string kShaderVariantCountLogFormat = "{0} [{1} variants | {2} combinations]";
        static readonly Stopwatch s_StripTimer = new Stopwatch();
        static readonly Stopwatch s_BuildTimer = new Stopwatch();
        static int s_TotalShadersIncluded;
        static int s_TotalShadersStripped;
        static bool s_IsEnabled;
        static readonly Dictionary<string, ShaderLogEntry> s_ShaderLogEntries = new Dictionary<string, ShaderLogEntry>();
        static SerializedProperty s_AlwaysIncludedShaders;
        static bool s_ProcessAlwaysIncludedShaders;
        static bool s_IsAssetBundleBuild;

        class ShaderLogEntry
        {
            public int includeCount;
            public int stripCount;
            public HashSet<string> includeVariants = new HashSet<string>();
            public HashSet<string> stripVariants = new HashSet<string>();
        }

        [MenuItem("Tools/Shader Stripper/Pre-AssetBundle Build")]
        public static void OnPreprocessAssetBundleBuild ()
        {
            PrepareShaderStripper(true);
        }

        [MenuItem("Tools/Shader Stripper/Post-AssetBundle Build")]
        public static void OnPostprocessAssetBundleBuild ()
        {
            GenerateReport();
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            PrepareShaderStripper(false);
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            GenerateReport();
        }

        /// <summary>
        /// Prepares the shader stripper processors and report generation structures
        /// This should be called manually before generating asset bundles.
        /// </summary>
        public static void PrepareShaderStripper(bool isAssetBundleBuild)
        {
            s_IsAssetBundleBuild = isAssetBundleBuild;
            var graphicsSettingsObj = AssetDatabase.LoadAssetAtPath<GraphicsSettings>("ProjectSettings/GraphicsSettings.asset");
            var serializedObject = new SerializedObject(graphicsSettingsObj);
            s_AlwaysIncludedShaders = serializedObject.FindProperty("m_AlwaysIncludedShaders");

            s_ShaderProcessors = null;
            s_ProcessAlwaysIncludedShaders = ShaderStripperSettings.GetOrCreateSettings().processAlwaysIncludedShaders;
            s_IsEnabled = ShaderProcessors.Any(x => x.enabled);
            if (s_IsEnabled)
            {
                foreach (var processor in ShaderProcessors)
                {
                    processor.Initialize();
                }

                Logger.Clear();
                s_ShaderLogEntries.Clear();
                s_StripTimer.Reset();
                s_BuildTimer.Restart();
            }
            else
            {
                Debug.Log("[Shader Stripper] No shaders will be stripped due to no processors are enabled.");
            }
        }

        /// <summary>
        /// Generates the post build stripped shaders report.
        /// This should be called manually after generating asset bundles
        /// </summary>
        public static void GenerateReport()
        {
            if (!s_IsEnabled) return;
            s_BuildTimer.Stop();
            var buildStats = $@"Build Duration: {s_BuildTimer.ElapsedMilliseconds}ms
Shader Stripping Duration: {s_StripTimer.ElapsedMilliseconds}ms
Total shaders included: {s_TotalShadersIncluded}
Total shaders stripped: {s_TotalShadersStripped}";
            Debug.Log(buildStats);

            var settings = ShaderStripperSettings.GetOrCreateSettings();
            Logger.Log(buildStats);
            Logger.Log();
            Logger.Log(new string('-', 100));
            Logger.Log();
            Logger.Log("INCLUDED SHADER VARIANTS");
            Logger.Log();
            Logger.Log(new string('-', 100));
            Logger.Log();
            foreach (var entry in s_ShaderLogEntries)
            {
                if (entry.Value.includeCount <= 0) continue;
                Logger.LogFormat(kShaderVariantCountLogFormat, entry.Key, entry.Value.includeCount, entry.Value.includeVariants.Count);
                if (settings.logIncludedVariants)
                {
                    foreach (var keywords in entry.Value.includeVariants)
                    {
                        Logger.Log($"\t {(keywords.IsNullOrWhiteSpace() ? "<no keywords>" : keywords)}");
                    }

                    Logger.Log();
                }
            }

            Logger.Log();
            Logger.Log(new string('-', 100));
            Logger.Log();
            Logger.Log("STRIPPED SHADER VARIANTS");
            Logger.Log();
            Logger.Log(new string('-', 100));
            Logger.Log();
            foreach (var entry in s_ShaderLogEntries)
            {
                if (entry.Value.stripCount <= 0) continue;
                Logger.LogFormat(kShaderVariantCountLogFormat, entry.Key, entry.Value.stripCount, entry.Value.stripVariants.Count);
                if (settings.logStrippedVariants)
                {
                    foreach (var keywords in entry.Value.stripVariants)
                    {
                        Logger.Log($"\t {(keywords.IsNullOrWhiteSpace() ? "<no keywords>" : keywords)}");
                    }

                    Logger.Log();
                }
            }

            Logger.Save(s_IsAssetBundleBuild ? "_AssetBundles" : "_Build");
            s_StripTimer.Reset();
            s_BuildTimer.Restart();
            s_TotalShadersIncluded = 0;
            s_TotalShadersStripped = 0;
        }

        public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
        {
            if (!s_IsEnabled) { return; }

            var totalVariants = data.Count;

            var shaderKey = $"{shader.name}::{snippet.passType}::{snippet.passName}";
            if (!s_ShaderLogEntries.ContainsKey(shaderKey))
            {
                s_ShaderLogEntries.Add(shaderKey, new ShaderLogEntry());
            }

            s_StripTimer.Start();
            for (var i = data.Count - 1; i >= 0; i--)
            {
                var compilerData = data[i];
                if (!s_ProcessAlwaysIncludedShaders && IsAlwaysIncludedShader(shader))
                {
                    s_ShaderLogEntries[shaderKey].includeCount += data.Count;
                    s_ShaderLogEntries[shaderKey].includeVariants.Add(string.Join(" ", compilerData.shaderKeywordSet.ToList(shader)));
                    continue;
                }

                var shouldInclude = true;
                foreach (var processor in ShaderProcessors)
                {
                    if (!processor.IsValidProcessor(shader, snippet, compilerData))
                    {
                        continue;
                    }

                    if (processor.GetType().HasOverriddenMethod(kShouldIncludeVariantMethodName))
                    {
                        shouldInclude |= processor.ShouldIncludeVariant(shader, snippet, compilerData);
                    }

                    if (processor.GetType().HasOverriddenMethod(kShouldStripVariantMethodName))
                    {
                        shouldInclude &= !processor.ShouldStripVariant(shader, snippet, compilerData);
                    }
                }

                if (shouldInclude)
                {
                    s_ShaderLogEntries[shaderKey].includeVariants.Add(string.Join(" ", compilerData.shaderKeywordSet.ToList(shader)));
                }
                else
                {
                    s_ShaderLogEntries[shaderKey].stripVariants.Add(string.Join(" ", compilerData.shaderKeywordSet.ToList(shader)));
                    data.RemoveAt(i);
                    s_TotalShadersStripped++;
                }
            }

            s_StripTimer.Stop();

            if (data.Count > 0)
            {
                s_ShaderLogEntries[shaderKey].includeCount += data.Count;
            }

            if (totalVariants - data.Count > 0)
            {
                s_ShaderLogEntries[shaderKey].stripCount += totalVariants - data.Count;
            }

            s_TotalShadersIncluded += data.Count;
        }

        static bool IsAlwaysIncludedShader(Shader shader)
        {
            for (var i = 0; i < s_AlwaysIncludedShaders.arraySize; ++i)
            {
                var arrayElem = s_AlwaysIncludedShaders.GetArrayElementAtIndex(i);
                if (shader == arrayElem.objectReferenceValue)
                {
                    return true;
                }
            }

            return false;
        }
    }
}