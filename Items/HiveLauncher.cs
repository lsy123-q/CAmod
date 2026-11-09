using CAmod.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CAmod.Items
{
    public class HiveLauncher : ModItem
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.BeeHive}";

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(gold: 5);
            Item.rare = ItemRarityID.Orange;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<CreatureLauncherPlayer>().HiveEquipped = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<RabbitLauncher>()
                .AddIngredient(ItemID.BeeWax, 10)
                .AddIngredient(ItemID.Hive, 15)
                .AddCondition(Condition.DownedQueenBee)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}