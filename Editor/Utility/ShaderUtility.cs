using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Daihenka.ShaderStripper
{
    internal static class ShaderUtility
    {
        public static string[] GetShaderNames()
        {
            return ShaderUtil.GetAllShaderInfo().Where(x => x.name != "Hidden/Daihenka/Editor/ColoredTexture").Select(x => x.name).ToArray();
        }

        public static bool IsLocalKeyword(this string keyword, Shader shader)
        {
            return ShaderKeyword.IsKeywordLocal(keyword.ToShaderKeyword(shader));
        }

        public static string GetKeywordName(this ShaderKeyword keyword, Shader shader)
        {
            return ShaderKeyword.IsKeywordLocal(keyword) ? ShaderKeyword.GetKeywordName(shader, keyword) : ShaderKeyword.GetGlobalKeywordName(keyword);
        }

        public static ShaderKeyword ToShaderKeyword(this string str, Shader shader)
        {
            return new ShaderKeyword(shader, str);
        }

        public static List<string> ToList(this ShaderKeywordSet set, Shader shader)
        {
            return set.GetShaderKeywords().Select(x => x.GetKeywordName(shader)).ToList();
        }

        public static ShaderVariantCollectionProxy GetProxy(this ShaderVariantCollection collection)
        {
            return new ShaderVariantCollectionProxy(collection);
        }

        public static IEnumerable<ShaderVariant> GetAllVariants(this ShaderVariantCollection collection)
        {
            if (collection == null)
            {
                return new List<ShaderVariant>();
            }

            return collection.GetProxy().AllVariants;
        }

        public static List<ShaderVariant> GetAllVariants(this IEnumerable<ShaderVariantCollection> collections)
        {
            var result = new List<ShaderVariant>();
            foreach (var collection in collections.Where(x => x != null))
            {
                result.AddRange(collection.GetAllVariants());
            }

            return result;
        }
    }
}