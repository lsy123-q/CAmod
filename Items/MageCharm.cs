using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CAmod.Accessories;

namespace CAmod.Items
{
    public class MageCharm : ModItem, mageflag
    {
        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.accessory = true; // 장신구다
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(gold: 5);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.manaRegenBonus += 10; // 마나 재생을 증가시킨다
        }
    }
}
