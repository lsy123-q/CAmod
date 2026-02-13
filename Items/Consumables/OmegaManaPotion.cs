using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
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
            Item.healMana = 500;
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


        public override void AddRecipes()
        {
            try
            {
                if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
                {
                    Recipe recipe = CreateRecipe(20);
                    // 오메가 마나 포션 20개를 결과로 만든다

                    recipe.AddIngredient(
                        calamity.Find<ModItem>("SupremeManaPotion").Type, 20);
                    // 슈프림 마나 포션 20개다

                    recipe.AddIngredient(
                        calamity.Find<ModItem>("AscendantSpiritEssence").Type, 1);
                    // 고대 영혼의 정수 1개다

                    recipe.AddTile(TileID.Bottles);
                    // 유리병에서 제작 가능하게 한다

                    recipe.AddTile(TileID.AlchemyTable);
                    // 연금술 탁자에서도 제작 가능하게 한다

                    recipe.Register();
                }
            }
            catch
            {
                // 칼라미티 미설치 또는 내부 이름 변경 시 무시한다
            }
        }

    }
}
