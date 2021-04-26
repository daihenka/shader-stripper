using System;
using System.Linq;
using System.Reflection;

namespace Daihenka.ShaderStripper
{
    internal static class ReflectionUtility
    {
        public static bool HasOverriddenMethod(this Type type, string methodName, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public)
        {
            return type.GetMethods(bindingFlags).Any(x => x.Name == methodName && x.DeclaringType == type);
        }
    }
}