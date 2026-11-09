using CAmod.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CAmod.Items
{
    public class TentacleLauncher : ModItem
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.FlowerPow}";
        public override void SetDefaults()
        {
            Item.width = Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(gold: 8);
            Item.rare = ItemRarityID.Lime;
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
            => player.GetModPlayer<CreatureLauncherPlayer>().TentacleEquipped = true;
        public override void AddRecipes()
        {
            CreateRecipe().AddIngredient<HiveLauncher>()
                .AddIngredient(ItemID.ChlorophyteBar, 10).AddIngredient(ItemID.Vine, 5)
                .AddCondition(Condition.DownedPlantera).AddTile(TileID.MythrilAnvil).Register();
        }
    }

    public class SharkLauncher : ModItem
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.SharkronBalloon}";
        public override void SetDefaults()
        {
            Item.width = Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(gold: 12);
            Item.rare = ItemRarityID.Yellow;
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
            => player.GetModPlayer<CreatureLauncherPlayer>().SharkEquipped = true;
        public override void AddRecipes()
        {
            CreateRecipe().AddIngredient<TentacleLauncher>()
                .AddIngredient(ItemID.SharkFin, 10).AddIngredient(ItemID.Ectoplasm, 5)
                .AddCondition(Condition.DownedDukeFishron).AddTile(TileID.MythrilAnvil).Register();
        }
    }
}
