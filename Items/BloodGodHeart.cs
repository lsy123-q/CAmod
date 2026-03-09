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
            player.GetDamage(DamageClass.Magic) += 0.10f; // 마법 피해 15% 증가한다
            player.statLifeMax2 += (int)(player.statLifeMax * 0.10f);
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();

           
            try
            {
      
                if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
                {
                   

                    if (calamity.TryFind("NebulousCore", out ModItem NebulousCore))
                        recipe.AddIngredient(NebulousCore.Type, 1);

                    if (calamity.TryFind("BloodPact", out ModItem bloodPact))
                        recipe.AddIngredient(bloodPact.Type, 1);

                    if (calamity.TryFind("AuricBar", out ModItem auricBar))
                        recipe.AddIngredient(auricBar.Type, 5);
                    // 전우주의 모루
                    if (calamity.TryFind("CosmicAnvil", out ModTile cosmicAnvil))
                        recipe.AddTile(cosmicAnvil.Type);
                }

                recipe.Register();
            }
            catch { }
        }

    }
}
