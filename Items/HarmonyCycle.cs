using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CAmod.Players;
namespace CAmod.Items
{
    public class HarmonyCycle : ModItem
    {
        public override void SetStaticDefaults()
        {
            // 장신구 이름/설명은 로컬라이징 쓰면 된다
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;

            Item.accessory = true;
            // 장신구다

            Item.rare = ItemRarityID.Red;
            Item.value = Item.buyPrice(0, 50, 0, 0);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<HarmonyCyclePlayer>();

            modPlayer.harmonyEquipped = true;
            // 장착 상태 활성화한다

           

            
        }
    }
}