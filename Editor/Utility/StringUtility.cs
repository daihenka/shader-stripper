using System;
using System.Text.RegularExpressions;
using UnityEditor;

namespace Daihenka.ShaderStripper
{
    internal static class StringUtility
    {
        public static bool IsNullOrWhiteSpace(this string s)
        {
            return s == null || s.Trim() == string.Empty;
        }

        public static string GetFriendlyName(this Type type)
        {
            return ObjectNames.NicifyVariableName(type.Name);
        }

        public static string GetFriendlyName(this string name)
        {
            return ObjectNames.NicifyVariableName(name);
        }

        public static bool IsMatch(this string str, StringMatchType matchType, string matchPattern, bool ignoreCase = false)
        {
            if (str == null)
            {
                return false;
            }

            var comparisonType = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.CurrentCulture;

            switch (matchType)
            {
                case StringMatchType.Equals:
                    return string.Equals(str, matchPattern, comparisonType);
                case StringMatchType.Contains:
                    return str.Contains(matchPattern, comparisonType);
                case StringMatchType.StartsWith:
                    return str.StartsWith(matchPattern, comparisonType);
                case StringMatchType.EndsWith:
                    return str.EndsWith(matchPattern, comparisonType);
                case StringMatchType.Wildcard:
                    return Regex.Match(str, matchPattern.WildcardToRegex(), ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None).Success;
                case StringMatchType.Regex:
                    return Regex.Match(str, matchPattern, ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None).Success;
            }

            return false;
        }

        public static bool Contains(this string source, string value, StringComparison comparisonType)
        {
            return source?.IndexOf(value, comparisonType) >= 0;
        }

        public static string WildcardToRegex(this string pattern)
        {
            return "^" + Regex.Escape(pattern)
                           .Replace(@"\*", ".*")
                           .Replace(@"\?", ".")
                       + "$";
        }
    }
}