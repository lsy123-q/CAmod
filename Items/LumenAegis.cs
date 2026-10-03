using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CAmod.Players;

namespace CAmod.Items
{
    public class LumenAegis : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;

            Item.accessory = true;

            Item.rare = ItemRarityID.Red;

            Item.value = Item.buyPrice(
                gold: 10);
        }


        public override void UpdateAccessory(
            Player player,
            bool hideVisual)
        {
            // 최대 체력을 70 증가시킨다.
            player.statLifeMax2 += 70;

            // Lumen Aegis 효과를 활성화한다.
            player.GetModPlayer<LumenAegisPlayer>()
                .lumenAegisEquipped = true;
        }


        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.CrimsonHeart)
                .AddIngredient(ItemID.ManaCrystal, 2)
                .AddTile(TileID.DemonAltar)
                .Register();
        }
    }
}