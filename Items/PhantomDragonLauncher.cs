using System.Collections.Generic;
using CAmod.NPCs;
using CAmod.Players;
using CAmod.Systems;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CAmod.Items
{
    public class PhantomDragonLauncher : ModItem
    {
        public override string Texture => "CAmod/Items/PhantomDragonLauncher";
        public override void SetDefaults()
        {
            Item.width = Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(gold: 15);
            Item.rare = ItemRarityID.Red;
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
            => player.GetModPlayer<CreatureLauncherPlayer>().DragonEquipped = true;
        public override void UpdateItemDye(Player player, int dye, bool hideVisual)
            => player.GetModPlayer<CreatureLauncherPlayer>().DragonDye = dye;

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var keys = KeySystem.PhantomDragonMode?.GetAssignedKeys();
            string key = keys != null && keys.Count > 0 ? string.Join(" / ", keys) : "미지정";
            int health = LauncherPhantomDragonHead.GetMaxHealth(player);
            var (head, body) = LauncherPhantomDragonHead.GetAttackDamage(player);
            foreach (var line in tooltips)
            {
                if (line.Mod != "Terraria" || !line.Name.StartsWith("Tooltip")) continue;
                line.Text = line.Text.Replace("[DRAGON_KEY]", key)
                    .Replace("[DRAGON_HP]", $"{health:N0}")
                    .Replace("[DRAGON_DAMAGE]", $"머리 {head:N0} / 몸통·꼬리 {body:N0}");
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe().AddIngredient<SharkLauncher>()
                .AddIngredient(ItemID.Ectoplasm, 10).AddIngredient(ItemID.ChlorophyteBar, 10)
                .AddCondition(Condition.DownedCultist).AddTile(TileID.MythrilAnvil).Register();
        }
    }
}
