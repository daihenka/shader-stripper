using Daihenka.ShaderStripper.ReflectionMagic;
using UnityEditor;

namespace Daihenka.ShaderStripper
{
    internal static class UnityEditorDynamic
    {
        public static readonly dynamic ShaderUtil = typeof(ShaderUtil).AsDynamicType();
        public static readonly dynamic ProjectWindowUtil = typeof(ProjectWindowUtil).AsDynamicType();
    }
}