using System;

namespace Daihenka.ShaderStripper
{
    [AttributeUsage(AttributeTargets.Class)]
    internal class ShaderProcessorAllowOnceAttribute : Attribute
    {
    }
}