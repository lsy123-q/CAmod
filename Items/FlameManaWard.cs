using CAmod.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CAmod.Items
{
    public sealed class FlameManaWard : ModItem
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.ManaFlower}";

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.accessory = true;
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.buyPrice(gold: 5);
        }

        public override void UpdateAccessory(Player player, bool hideVisual) =>
            player.GetModPlayer<FlameManaWardPlayer>().Equipped = true;

        public override void AddRecipes() => CreateRecipe()
            .AddIngredient(ItemID.ManaFlower)
            .AddIngredient(ItemID.ObsidianRose)
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
    }
}
