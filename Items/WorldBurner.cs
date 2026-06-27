using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CAmod.Players;
namespace CAmod.Items
{
    public class WorldBurner : ModItem
    {
        public override void SetStaticDefaults()
        {
            // 장신구 이름/설명은 hjson에서 관리한다
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;

            Item.accessory = true;

            Item.value = Item.sellPrice(gold: 10);

            Item.rare = ItemRarityID.Red;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // 세계를 불태우는 자 효과를 활성화한다
            player.GetModPlayer<WorldBurnerPlayer>().worldBurner = true;

            // 마법 데미지를 15% 증가시킨다
            player.GetDamage(DamageClass.Magic) += 0.15f;

            // 마법 치명타를 5% 증가시킨다
            player.GetCritChance(DamageClass.Magic) += 5f;
        }
    }
}