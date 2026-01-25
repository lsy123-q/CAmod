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
        

    }
}
