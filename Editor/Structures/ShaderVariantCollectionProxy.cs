using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Daihenka.ShaderStripper
{
    internal class ShaderVariantCollectionProxy
    {
        readonly ShaderVariantCollection m_ShaderVariantCollection;
        Dictionary<Shader, List<ShaderVariant>> m_ShaderVariants;

        public ICollection<Shader> Shaders => m_ShaderVariants.Keys.ToList();
        public ICollection<ShaderVariant> AllVariants => m_ShaderVariants.Values.SelectMany(variantList => variantList).ToList();
        public int ShaderCount => m_ShaderVariants.Keys.Count;
        public bool HasShader(Shader shader) => m_ShaderVariants.ContainsKey(shader);
        public ICollection<ShaderVariant> GetVariants(Shader shader) => m_ShaderVariants.ContainsKey(shader) ? m_ShaderVariants[shader] : new List<ShaderVariant>();
        public ICollection<ShaderVariant> GetVariants(Shader shader, PassType passType) => m_ShaderVariants.ContainsKey(shader) ? m_ShaderVariants[shader].Where(x => x.passType == passType).ToList() : new List<ShaderVariant>();

        public ShaderVariantCollectionProxy(ShaderVariantCollection shaderVariantCollection)
        {
            m_ShaderVariantCollection = shaderVariantCollection;
            GetShaderVariants();
        }

        void GetShaderVariants()
        {
            m_ShaderVariants = new Dictionary<Shader, List<ShaderVariant>>();
            var shadersProp = new SerializedObject(m_ShaderVariantCollection).FindProperty("m_Shaders");
            for (var i = 0; i < shadersProp.arraySize; i++)
            {
                var entryProp = shadersProp.GetArrayElementAtIndex(i);
                var shader = (Shader) entryProp.FindPropertyRelative("first").objectReferenceValue;
                if (!m_ShaderVariants.ContainsKey(shader))
                {
                    m_ShaderVariants.Add(shader, new List<ShaderVariant>());
                }

                var variantsProp = entryProp.FindPropertyRelative("second.variants");
                for (var j = 0; j < variantsProp.arraySize; j++)
                {
                    var variantProp = variantsProp.GetArrayElementAtIndex(j);
                    m_ShaderVariants[shader].Add(new ShaderVariant(shader, (PassType) variantProp.FindPropertyRelative("passType").intValue, variantProp.FindPropertyRelative("keywords").stringValue?.Split(' ')));
                }
            }
        }
    }
}