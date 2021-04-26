using System;

namespace Daihenka.ShaderStripper
{
    [AttributeUsage(AttributeTargets.Class)]
    internal class MutuallyExclusiveShaderProcessorAttribute : Attribute
    {
        readonly Type[] types;

        public MutuallyExclusiveShaderProcessorAttribute(params Type[] types)
        {
            this.types = types;
        }

        public Type[] Types => types;
    }
}