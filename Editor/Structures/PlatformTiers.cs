using System;
using UnityEditor.Rendering;
using UnityEngine.Rendering;

namespace Daihenka.ShaderStripper
{
    [Serializable]
    internal class PlatformTiers
    {
        public ShaderCompilerPlatform platform;
        public bool stripTier1;
        public bool stripTier2;
        public bool stripTier3;

        public bool ShouldStrip(GraphicsTier tier)
        {
            switch (tier)
            {
                case GraphicsTier.Tier1:
                    return stripTier1;
                case GraphicsTier.Tier2:
                    return stripTier2;
                case GraphicsTier.Tier3:
                    return stripTier3;
            }

            return false;
        }
    }
}