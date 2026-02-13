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
            Item.rare = ModContent.RarityType<Rarities.AncientRarity>();
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.buyPrice(gold: 25);
            Item.value = Item.sellPrice(gold: 25);
        }
       
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<LeafWardPlayer>().leafShieldEquipped = true;
            player.GetModPlayer<LeafWardPlayer2>().leafShieldEquipped = true;
            
            // 착용 중임을 플래그로 기록한다
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.RemoveAll(l => l.Mod == "Terraria" && l.Name.StartsWith("Tooltip"));
            // 기존 바닐라 툴팁을 전부 제거한다

            Player player = Main.LocalPlayer;
            var ward2 = player.GetModPlayer<LeafWardPlayer2>();

            int stage = ward2.LeafShieldStage;
            // 현재 성장 단계다 (0~20)

            float bonusMagicDamage = stage * 0.5f ;
            // 추가 마법 피해 퍼센트다

            float bonusMagicCrit = stage * 0.25f ;
            // 추가 마법 치명타 확률이다
            int bonusdefense = (int)(stage * 0.5f) ;

            int bonusMana = stage * 5;
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

            float t = (float)(Math.Sin(Main.GlobalTimeWrappedHourly * 2.8f) * 0.5f + 0.5f);
            // 0~1 사이를 왕복하는 보간값이다

            Color lightGreen = new Color(120, 255, 120);
            Color lime = new Color(180, 255, 80);

            foreach (var line in tooltips)
            {
                if (line.Mod == "Terraria" && line.Name == "ItemName")
                {
                    line.OverrideColor = Color.Lerp(lightGreen, lime, t);
                    // 아이템 이름을 연두~라임색으로 왕복시킨다
                }
            }
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
