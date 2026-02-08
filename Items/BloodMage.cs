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
            Item.rare = ModContent.RarityType<Rarities.AncientRarity>();
            Item.value = Item.sellPrice(gold: 25);
        }
       
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<BloodMagePlayer>().bloodMageEquipped = true;

           



            player.statLifeMax2 += (int)(player.statLifeMax * 0.25);


            // Blood Mage 장신구 착용 플래그를 켠다
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


        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.RemoveAll(l => l.Mod == "Terraria" && l.Name.StartsWith("Tooltip"));

            tooltips.Add(new TooltipLine(Mod, "L1",
                "Increases maximum life by 25%"));
            tooltips.Add(new TooltipLine(Mod, "L2",
     "Increases magic damage by up to 25% in proportion to lost health."));
            tooltips.Add(new TooltipLine(Mod, "L3",
                "Magic projectiles steal 10% of damage as life and mana"));
       
            tooltips.Add(new TooltipLine(Mod, "L4",
                "This ability has a cooldown"));
            tooltips.Add(new TooltipLine(Mod, "L5",
                "Cooldown increases with higher healing amounts"));
            tooltips.Add(new TooltipLine(Mod, "L6",
                "Cooldown is reduced by life regeneration and missing health"));
        
            tooltips.Add(new TooltipLine(Mod, "L7",
                "You cannot recover life by any means other than this ability"));
            tooltips.Add(new TooltipLine(Mod, "L8",
                "Unequipping during cooldown results in instant death"));

            float t = (float)(Math.Sin(Main.GlobalTimeWrappedHourly * 2.8f) * 0.5f + 0.5f);
            Color red = new Color(220, 60, 60);
            Color purple = new Color(150, 70, 190);

            foreach (var line in tooltips)
            {
                if (line.Mod == "Terraria" && line.Name == "ItemName")
                {
                    line.OverrideColor = Color.Lerp(red, purple, t);
                    // 블러드메이지 이름을 빨강~보라로 왕복시킨다
                }
            }
        }

    }
}
