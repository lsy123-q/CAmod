using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
namespace CAmod.Items.Materials
{
    public class AncientEmblem : ModItem
    {
        public override void SetStaticDefaults()
        {
            // 희생(저니) 등록 개수다
            Item.ResearchUnlockCount = 25;
            Item.rare = ModContent.RarityType<Rarities.AncientRarity>();


        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;

            Item.maxStack = 9999;
            Item.value = Item.sellPrice(silver: 50);
           Item.rare = ModContent.RarityType<Rarities.AncientRarity>();
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            float t = (float)(System.Math.Sin(Main.GlobalTimeWrappedHourly * 2.5f) * 0.5f + 0.5f);
            // 시간에 따라 0~1 사이를 왕복한다

            Color purple = new Color(160, 80, 200);
            Color red = new Color(220, 60, 60);

            Color animated = Color.Lerp(purple, red, t);
            // 보라색과 빨간색을 부드럽게 섞는다

            foreach (TooltipLine line in tooltips)
            {
                if (line.Mod == "Terraria" && line.Name == "ItemName")
                {
                    line.OverrideColor = animated;
                    // 아이템 이름 글자색만 애니메이션으로 덮어쓴다
                }
            }
        }

    }
}
