using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CAmod.Accessories;
using CAmod.Players;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using CAmod.Systems;
using System;

namespace CAmod.Items
{
    public class DimensionGate : ModItem
    {
        public int chaosReduced = 0;
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 25);
            Item.rare = ModContent.RarityType<Rarities.ArcaneCosmic>();


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

        public override void UpdateAccessory(Player player2, bool hideVisual)
        {
            player2.GetModPlayer<DimGatePlayer>().dimGateEquipped = true;
            // 장신구를 착용했을 때만 차원 게이트 효과를 허용한다

            player2.GetCritChance(DamageClass.Magic) += 5f;
            player2.GetDamage(DamageClass.Magic) += 0.15f;
            // 마법 데미지를 15% 증가시킨다

            player2.moveSpeed += 0.15f;
            player2.maxRunSpeed *= 1.15f;


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
        public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
        {
            if (line.Mod == "Terraria" && line.Name == "ItemName")
            {
                Rarities.ArcaneCosmic.Draw(Item, line);
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

                    recipe.AddIngredient(ModContent.ItemType<Materials.AncientEmblem>(), 1);
                    // 고대의 휘장(Ancient Emblem) 1개를 요구한다
                    recipe.AddIngredient(
                        calamity.Find<ModItem>("CosmiliteBar").Type, 10);
                    // 칼라미티가 있으면 코스밀라이트 주괴 10개를 요구한다

                    recipe.AddIngredient(
                        calamity.Find<ModItem>("DarkPlasma").Type, 1);
                    // 다크 플라즈마 1개를 요구한다

                    recipe.AddTile(
                        calamity.Find<ModTile>("CosmicAnvil").Type);
                    // 코스믹 모루에서 제작 가능하게 한다

                    recipe.Register();
                }

            }
            catch
            {  
                
                    CreateRecipe()
                        .AddIngredient(ItemID.LunarBar, 10)
                        // 루미나이트 주괴 10개를 요구한다

                        .AddTile(TileID.LunarCraftingStation)
                        // 고대 조작대에서 제작 가능하게 한다

                        .Register();
                
            }
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            

            

            // ===== 기존 [KEY] 치환 로직 유지 =====
            foreach (var line in tooltips)
{
    if (line.Text.Contains("[KEY]"))
    {
        string keyText = "[NONE]";

        if (KeySystem.DimGate != null)
        {
            var keys = KeySystem.DimGate.GetAssignedKeys();
            if (keys.Count > 0)
                keyText = keys[0];
        }

        line.Text = line.Text.Replace("[KEY]", keyText);
        // [KEY]를 실제 설정된 키로 치환한다
    }
}
            int prefixIndex = tooltips.FindIndex(t => t.Name == "Prefix");

            if (prefixIndex != -1)
            {
                TooltipLine prefixLine = tooltips[prefixIndex];
                tooltips.RemoveAt(prefixIndex);
                tooltips.Add(prefixLine);
            }
        }
















    }

}
