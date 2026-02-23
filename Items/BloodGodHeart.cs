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
            if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
            {
                Item.rare = calamity.Find<ModRarity>("BurnishedAuric").Type;

            }
            else
            {
                Item.rare = ItemRarityID.Red;

            }
            Item.value = Item.buyPrice(0, 50, 52, 63);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var mp = player.GetModPlayer<BloodGodHeartPlayer>();

            mp.bloodHeartEquipped = true; // 장착 상태 등록한다
            player.GetDamage(DamageClass.Generic) += 0.10f; // 마법 피해 15% 증가한다
            player.statLifeMax2 += (int)(player.statLifeMax * 0.1f);
        }
    }
}
