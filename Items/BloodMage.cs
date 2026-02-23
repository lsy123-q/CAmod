using Terraria;
using Terraria.ModLoader;
using CAmod.Players;
using CAmod.Players;
using System.Reflection;
using Terraria.ID;
using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework;
namespace CAmod.Items
{
    public class BloodMage : ModItem
    {
        public override void SetDefaults()
        {
            Item.accessory = true;
            Item.width = 28;
            Item.height = 28;
            Item.value = Item.buyPrice(gold: 10);
            Item.rare = ModContent.RarityType<Rarities.ArcaneRed>();
            
        }
        public override void UpdateInventory(Player players)
        {


            Player player = Main.LocalPlayer;
            var ward2 = player.GetModPlayer<LeafWardPlayer2>();

            int stage = ward2.LeafShieldStage;


            float normalized = stage / 40f;
            // 성장 단계를 0~1로 정규화한다

            float priceGold = normalized * normalized * 40f;
            // 정규화값을 제곱 후 40골드를 곱한다

            int priceCopper = (int)(priceGold * 10000f);


            int copper = priceCopper;
            Item.value = Item.sellPrice(copper: priceCopper);

            // 최종 판매가를 설정한다
        }
        public override void UpdateVanity(Player player2)
        {
            Player player = Main.LocalPlayer;
            var ward2 = player.GetModPlayer<LeafWardPlayer2>();

            int stage = ward2.LeafShieldStage;


            float normalized = stage / 40f;
            // 성장 단계를 0~1로 정규화한다

            float priceGold = normalized * normalized * 40f;
            // 정규화값을 제곱 후 40골드를 곱한다

            int priceCopper = (int)(priceGold * 10000f);


            int copper = priceCopper;
            Item.value = Item.sellPrice(copper: priceCopper);
        }
        public override void UpdateAccessory(Player player2, bool hideVisual)
        {
            player2.GetModPlayer<BloodMagePlayer>().bloodMageEquipped = true;

           



            player2.statLifeMax2 += (int)(player2.statLifeMax * 0.25);

            Player player = Main.LocalPlayer;
            var ward2 = player.GetModPlayer<LeafWardPlayer2>();

            int stage = ward2.LeafShieldStage;


            float normalized = stage / 40f;
            // 성장 단계를 0~1로 정규화한다

            float priceGold = normalized * normalized * 40f;
            // 정규화값을 제곱 후 40골드를 곱한다

            int priceCopper = (int)(priceGold * 10000f);


            int copper = priceCopper;
            Item.value = Item.sellPrice(copper: priceCopper);
            // Blood Mage 장신구 착용 플래그를 켠다
        }

        public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
        {
            if (line.Mod == "Terraria" && line.Name == "ItemName")
            {
                Rarities.ArcaneRed.Draw(Item, line);
                return false; // 기본 드로우를 막는다
            }

            return true; // 다른 줄은 기본 드로우한다
        }

        public override void AddRecipes()
        {
            try
            {
                if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
                {
                    Recipe recipe = CreateRecipe();

                    recipe.AddIngredient(
                        calamity.Find<ModItem>("BloodPact").Type, 1);
                    recipe.AddIngredient(ModContent.ItemType<Materials.AncientEmblem>(), 1);

                  

                    recipe.AddIngredient(
                        calamity.Find<ModItem>("BloodOrb").Type, 10);
                    // 블러드 오브 10개를 요구한다
                    // 



                    recipe.AddTile(
                     TileID.DemonAltar);
                    // 하드모드 모루에서 제작 가능하게 한다

                    recipe.Register();
                }
            }
            catch
            {
                // 칼라미티가 없거나 내부명이 바뀐 경우를 대비한 폴백이다
                CreateRecipe()
                    .AddIngredient(ItemID.ManaCrystal, 1)
                    // 최소한의 바닐라 재료만 요구한다

                    .AddTile(TileID.MythrilAnvil)
                    // 동일하게 하드모드 모루를 사용한다

                    .Register();
            }
        }


        

    }
}
