using System.Collections.Generic;
using Terraria.ModLoader;

using CAmod.Items;

namespace CAmod.Systems
{
    public class AccessoryGroups : ModSystem
    {
        public static List<int> glassGroup = new List<int>();

        public override void PostSetupContent()
        {
            glassGroup = new List<int>
            {
                ModContent.ItemType<GlassCannon>(),     // 유리대포
                ModContent.ItemType<LeafShield>(),      // 리프실드
                ModContent.ItemType<BloodMage>(),       // 블러드메이지
                ModContent.ItemType<DimensionGate>(),   // 데멘션게이트
                ModContent.ItemType<Pyromancer>()       // 파이로맨서
            };
        }
    }
}