using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Daihenka.ShaderStripper
{
    internal struct ShaderVariant
    {
        /// <summary>
        ///   <para>Shader to use in this variant.</para>
        /// </summary>
        public Shader shader;

        /// <summary>
        ///   <para>Pass type to use in this variant.</para>
        /// </summary>
        public PassType passType;

        /// <summary>
        ///   <para>Array of shader keywords to use in this variant.</para>
        /// </summary>
        public string[] keywords;

        /// <summary>
        ///   <para>Array of shader global keywords to use in this variant.</para>
        /// </summary>
        public string[] globalKeywords;

        /// <summary>
        ///   <para>Creates a ShaderVariant structure.</para>
        /// </summary>
        /// <param name="shader"></param>
        /// <param name="passType"></param>
        /// <param name="keywords"></param>
        public ShaderVariant(Shader shader, PassType passType, params string[] keywords)
        {
            this.shader = shader;
            this.passType = passType;
            this.keywords = keywords;
            globalKeywords = keywords.Where(x => !x.IsLocalKeyword(shader)).ToArray();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ShaderVariant) obj);
        }

        public bool Equals(ShaderVariant other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return shader.Equals(other.shader) && passType == other.passType && keywords.HasSameElements(other.keywords);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (shader.GetHashCode() * 13) + ((int) passType * 29);
                foreach (var obj in keywords)
                {
                    hashCode = hashCode * 31 + obj.GetHashCode();
                }
                return hashCode;
            }
        }
    }
}