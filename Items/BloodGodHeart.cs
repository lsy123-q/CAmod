using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CAmod.Players;
namespace CAmod.Items
{
    public class BloodGodHeart : ModItem
    {
        public override void SetStaticDefaults()
        {
            
        }

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 34;
            Item.accessory = true;
            Item.rare = ItemRarityID.Red;
            Item.value = Item.buyPrice(0, 10, 0, 0);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var mp = player.GetModPlayer<BloodGodHeartPlayer>();

            mp.bloodHeartEquipped = true; // 장착 상태 등록한다
            player.GetDamage(DamageClass.Magic) += 0.15f; // 마법 피해 15% 증가한다
            player.statLifeMax2 += (int)(player.statLifeMax * 0.1f);
        }
    }
}
