using CAmod.Players;
using Terraria;
using Terraria.ModLoader;

namespace CAmod.Items
{
    public class RabbitLauncher : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;

            Item.accessory = true;

            Item.value = Item.buyPrice(0, 5);
            Item.rare = 4;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<RabbitLauncherPlayer>().rabbitLauncher = true;
        }
    }
}