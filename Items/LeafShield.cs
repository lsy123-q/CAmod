using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CAmod.Players;
using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework;
using CAmod.Systems;
using Steamworks;
namespace CAmod.Items
{
    public class LeafShield : ModItem
    {
        public override void SetStaticDefaults()
        {
            // 툴팁은 로컬라이징 방식에 맞춰 따로 구성하면 된다
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.accessory = true;
            Item.rare = ModContent.RarityType<Rarities.ArcaneGreen>();
            
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
            Item.value = Item.sellPrice( copper: priceCopper);
            
            // 최종 판매가를 설정한다
        }
        public override void UpdateAccessory(Player player2, bool hideVisual)
        {
            player2.GetModPlayer<LeafWardPlayer>().leafShieldEquipped = true;
            player2.GetModPlayer<LeafWardPlayer2>().leafShieldEquipped = true;


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
                Rarities.ArcaneGreen.Draw(Item, line);
                return false; // 기본 드로우를 막는다
            }

            return true; // 다른 줄은 기본 드로우한다
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.RemoveAll(l => l.Mod == "Terraria" && l.Name.StartsWith("Tooltip"));
            // 기존 바닐라 툴팁을 전부 제거한다

            Player player = Main.LocalPlayer;
            var ward2 = player.GetModPlayer<LeafWardPlayer2>();

            int stage = ward2.LeafShieldStage;
            // 현재 성장 단계다 (0~20)

            float bonusMagicDamage = stage * 0.5f + 10f;
            // 추가 마법 피해 퍼센트다

            float bonusMagicCrit = stage * 0.25f + 5f;
            // 추가 마법 치명타 확률이다
            int bonusdefense = (int)(stage * 0.5f) + 10;

            int bonusMana = stage * 5 + 100;
            // 추가 마나다
            float progress = (stage / 40f)*100f;
            float cooldownReductionSec = stage * 0.25f;
            // 쿨타임 감소 초 단위다 (30프레임 = 0.5초)

            float lastcool = 20f - cooldownReductionSec;

            string keyText = KeySystem.leafshield.GetAssignedKeys().Count > 0
     ? KeySystem.leafshield.GetAssignedKeys()[0]
     : "Unbound";
            // 리프 실드에 실제로 바인딩된 키를 가져온다

            tooltips.Add(new TooltipLine(Mod, "L1",
                $"Press [{keyText}] to use the Dryad NPC's Leaf Shield."));
       

            tooltips.Add(new TooltipLine(Mod, "L3",
                "Magic projectiles inflict Dryad's Bane on enemies for 3 seconds."));


            tooltips.Add(new TooltipLine(Mod, "L4",
                 "Provides light in the Abyss."));

            tooltips.Add(new TooltipLine(Mod, "L5",
                "This accessory grows with each new boss you defeat."));
            // 성장형 장신구임을 설명한다

            tooltips.Add(new TooltipLine(Mod, "L6",
                $"growth rate : {progress:F1}% "));
            // 성장으로 추가된 마법 피해다


            tooltips.Add(new TooltipLine(Mod, "L7",
                "Current Applied Bonuses:"));
            // 현재 적용 중인 능력치 헤더다

            tooltips.Add(new TooltipLine(Mod, "L8",
                $"+{bonusMagicDamage:F1}% magic damage"));
            // 성장으로 추가된 마법 피해다

            tooltips.Add(new TooltipLine(Mod, "L9",
                $"+{bonusMagicCrit:F1}% magic critical chance"));
            // 성장으로 추가된 마법 치명타 확률이다

            tooltips.Add(new TooltipLine(Mod, "L10",
                $"+{bonusMana} mana"));
            // 성장으로 추가된 마나다

            tooltips.Add(new TooltipLine(Mod, "L10",
               $"+{bonusdefense} defense"));


            tooltips.Add(new TooltipLine(Mod, "L11",
                $"Leaf Shield cooldown reduction: {lastcool:F1} seconds"));
            // 성장으로 감소된 쿨타임이다

            
        }



        public override void AddRecipes()
        {
            try
            {
                if (!ModLoader.TryGetMod("CalamityMod", out Mod calamity))
                    return;
                // 칼라미티가 없으면 레시피를 등록하지 않는다

                Recipe recipe = CreateRecipe();

                recipe.AddIngredient(
                    ModContent.ItemType<Materials.AncientEmblem>(), 1);
                // 고대의 휘장 1개다

                recipe.AddIngredient(
                    calamity.Find<ModItem>("StrangeOrb").Type, 1);
                // 스트레인지 오브 1개다

                recipe.AddIngredient(
                    calamity.Find<ModItem>("PlantyMush").Type, 15);
                // 플랜티 머쉬 15개다

                recipe.AddTile(TileID.DemonAltar);
                // 악마의 제단에서 제작한다

                recipe.Register();
            }
            catch { }

            }


    }
}
