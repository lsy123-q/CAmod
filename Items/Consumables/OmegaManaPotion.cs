using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CAmod.Items.Consumables
{
    public class OmegaManaPotion : ModItem
    {
        public override void SetStaticDefaults()
        {
            // 아이템 이름과 툴팁은 로컬라이징에서 처리하는 구조라 가정한다
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 18;
            Item.useTurn = true;
            Item.maxStack = 9999;
            Item.healMana = 600;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.UseSound = SoundID.Item3;
            Item.consumable = true;
            if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
            {
                Item.rare = calamity.Find<ModRarity>("Turquoise").Type;

            }
            else
            {
                Item.rare = ItemRarityID.Red;

            }
            Item.value = Item.buyPrice(0, 7, 0, 0);
        }
    }
}
