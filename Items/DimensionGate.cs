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

            
        }
        

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<DimGatePlayer>().dimGateEquipped = true;
            // 장신구를 착용했을 때만 차원 게이트 효과를 허용한다


            player.GetDamage(DamageClass.Magic) += 0.15f;
            // 마법 데미지를 15% 증가시킨다

            player.moveSpeed += 0.15f;
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
            tooltips.RemoveAll(l => l.Mod == "Terraria" && l.Name.StartsWith("Tooltip"));
            // 기존 툴팁을 제거한다

            tooltips.Add(new TooltipLine(Mod, "L1", "15% increased magic damage"));
            tooltips.Add(new TooltipLine(Mod, "L2", "15% increased movement speed"));
          
            tooltips.Add(new TooltipLine(Mod, "L3",
                "Press [KEY] to enter another dimension for 5 seconds"));
            tooltips.Add(new TooltipLine(Mod, "L4",
                "and become immune to all attacks."));
            tooltips.Add(new TooltipLine(Mod, "L5",
                "You cannot attack or dash while in another dimension."));
            tooltips.Add(new TooltipLine(Mod, "L6",
                "Press [KEY] again to leave early."));
  
            tooltips.Add(new TooltipLine(Mod, "L7",
                "30 second cooldown, costs 400 mana."));

            float t = (float)(Math.Sin(Main.GlobalTimeWrappedHourly * 2.5f) * 0.5f + 0.5f);
            Color blue = new Color(80, 140, 255);
            Color purple = new Color(160, 90, 220);
            Color animated = Color.Lerp(blue, purple, t);

            foreach (var line in tooltips)
            {
                if (line.Mod == "Terraria" && line.Name == "ItemName")
                {
                    line.OverrideColor = animated;
                    // 디멘션 게이트 이름을 파랑~보라로 왕복시킨다
                }
            }

            // ===== 기존 [KEY] 치환 로직 유지 =====
            foreach (var line in tooltips)
            {
                if (line.Mod == Mod.Name && line.Text.Contains("[KEY]"))
                {
                    string keyText;

                    if (KeySystem.DimGate != null && KeySystem.DimGate.GetAssignedKeys().Count > 0)
                        keyText = KeySystem.DimGate.GetAssignedKeys()[0];
                    else
                        keyText = "[NONE]";

                    line.Text = line.Text.Replace("[KEY]", keyText);
                }
            }
        }
















    }

}
