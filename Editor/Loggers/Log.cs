using System;
using System.Collections.Generic;
using System.IO;

namespace Daihenka.ShaderStripper
{
    internal static class Logger
    {
        static readonly List<string> s_Log = new List<string>();
        public static string[] Entries => s_Log.ToArray();
        public static int Count => s_Log.Count;
        public static void Log() => s_Log.Add(string.Empty);
        public static void Log(string entry) => s_Log.Add(entry);
        public static void LogFormat(string format, params object[] args) => s_Log.Add(string.Format(format, args));
        public static void AddRange(IEnumerable<string> entries) => s_Log.AddRange(entries);
        public static void Clear() => s_Log.Clear();
        public static void Insert(int index, string entry) => s_Log.Insert(index, entry);
        public static bool Contains(string entry) => s_Log.Contains(entry);

        public static void Save(string fileSuffix)
        {
            var targetFolder = ShaderStripperSettings.LogPath;
            if (targetFolder.IsNullOrWhiteSpace() || Count == 0)
            {
                Clear();
                return;
            }

            targetFolder = targetFolder.FixPathSeparators();
            if (!targetFolder.EndsWith("/"))
            {
                targetFolder += "/";
            }

            var path = string.Format("{0}ShaderStripLog_{1:yyyy-MM-dd}{2}.log", targetFolder, DateTime.Now, fileSuffix);
            PathUtility.CreateDirectoryIfNeeded(targetFolder);
            File.WriteAllLines(path, Entries);
        }

    }
}