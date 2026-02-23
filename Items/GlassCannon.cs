using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CAmod.Players;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;
namespace CAmod.Items
{
    public class GlassCannon : ModItem
    {
        public override void SetStaticDefaults()
        {
            // 이름, 설명은 Localization으로 처리하는 게 정석이다
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.accessory = true;
            Item.rare = ModContent.RarityType<Rarities.ArcaneGlass>();
         
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

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();

            recipe.AddIngredient(ModContent.ItemType<Materials.AncientEmblem>(), 1);
            // 고대의 휘장 1개를 요구한다

            recipe.AddIngredient(ItemID.Cannon, 1);
            // 대포 1개를 요구한다

            recipe.AddIngredient(ItemID.Glass, 50);
            // 유리 블럭 50개를 요구한다

            recipe.AddTile(TileID.DemonAltar);
            // 악마의 제단에서 제작 가능하게 한다

            recipe.Register();
        }
        public override void UpdateAccessory(Player player2, bool hideVisual)
        {
            GlassCannonPlayer gPlayer = player2.GetModPlayer<GlassCannonPlayer>();
            gPlayer.glassCannonEquipped = true;
            // 유리대포 장착 플래그를 켠다
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
                Rarities.ArcaneGlass.Draw(Item, line);
                return false; // 기본 드로우를 막는다
            }

            return true; // 다른 줄은 기본 드로우한다
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            

            float t = (float)(Math.Sin(Main.GlobalTimeWrappedHourly * 2.2f) * 0.5f + 0.5f);
            Color white = Color.White;
            Color sky = new Color(140, 200, 255);

            foreach (var line in tooltips)
            {
                if (line.Mod == "Terraria" && line.Name == "ItemName")
                {
                    line.OverrideColor = Color.Lerp(white, sky, t);
                    // 유리대포 이름을 하양~하늘색으로 왕복시킨다
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
