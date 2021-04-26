using System;

namespace Daihenka.ShaderStripper
{
    [Serializable]
    internal class StringFilter
    {
        public StringMatchType matchType;
        public string pattern;
        public bool ignoreCase;

        public StringFilter() : this(StringMatchType.Wildcard, "*"){}
        public StringFilter(StringMatchType matchType, string pattern, bool ignoreCase = false)
        {
            this.matchType = matchType;
            this.pattern = pattern;
            this.ignoreCase = ignoreCase;
        }

        public bool IsMatch(string input)
        {
            return input.IsMatch(matchType, pattern, ignoreCase);
        }
    }
}