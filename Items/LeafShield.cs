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
            int stage = LeafWardSystem.GetLeafShieldStage(players);

            float normalized = stage / 40f;
            float priceGold = normalized * normalized * 40f;
            int priceCopper = (int)(priceGold * 10000f);

            Item.value = Item.sellPrice(copper: priceCopper);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<LeafWardPlayer>();

            modPlayer.leafShieldEquipped = true;

            // Calamity's current abyss system uses darkness reduction instead of integer light levels.
            player.GetModPlayer<global::CalamityMod.CalPlayer.CalamityPlayer>().abyssDarkness -= 2f;

            if (!hideVisual)
                modPlayer.leafShieldVisible = true;

            int count = LeafWardSystem.GetLeafShieldStage(player);

            // =========================
            // 스탯 적용
            // =========================

            player.GetDamage(DamageClass.Magic) += count * 0.005f + 0.05f;
            player.GetCritChance(DamageClass.Magic) += count * 0.25f + 2.5f;

            // 5% base + 0.5% per stage, using integer arithmetic to avoid float truncation.
            int bonusLife = (int)((long)player.statLifeMax * (10 + count) / 200);
            player.statLifeMax2 += bonusLife;

            

            player.statManaMax2 += count * 5 + 50;

            // =========================
            // 가격 계산
            // =========================

            float normalized = count / 40f;
            float priceGold = normalized * normalized * 40f;
            int priceCopper = (int)(priceGold * 10000f);

            Item.value = Item.sellPrice(copper: priceCopper);
        }

        public override void UpdateVanity(Player player2)
        {
            int stage = LeafWardSystem.GetLeafShieldStage(player2);

            float normalized = stage / 40f;
            float priceGold = normalized * normalized * 40f;
            int priceCopper = (int)(priceGold * 10000f);

            Item.value = Item.sellPrice(copper: priceCopper);
        }

        public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
        {
            if (line.Mod == "Terraria" && line.Name == "ItemName")
            {
                Rarities.ArcaneGreen.Draw(Item, line);
                return false;
            }

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.RemoveAll(l => l.Mod == "Terraria" && l.Name.StartsWith("Tooltip"));

            int stage = LeafWardSystem.GetLeafShieldStage(Main.LocalPlayer);

            float bonusMagicDamage = stage * 0.5f + 5f;
            float bonusMagicCrit = stage * 0.25f + 2.5f;

            float bonusLifePercent = 5f + stage * (20f / 40f);
            int bonusLifeRegen = (int)Math.Ceiling(1f + stage * (4f / 40f));
            int bonusMana = stage * 5 + 50;

            float progress = (stage / 40f) * 100f;
            float cooldownReductionSec = stage * 0.25f;
            float lastcool = stage >= 40 ? 1f : 20f - cooldownReductionSec;

            string keyText = KeySystem.leafshield.GetAssignedKeys().Count > 0
                ? KeySystem.leafshield.GetAssignedKeys()[0]
                : "Unbound";

            tooltips.Add(new TooltipLine(Mod, "L1",
                $"Press [{keyText}] to use the Dryad NPC's Leaf Shield."));

            tooltips.Add(new TooltipLine(Mod, "L3",
                "Magic projectiles inflict Dryad's Bane on enemies for 3 seconds."));

            tooltips.Add(new TooltipLine(Mod, "L4",
                "Provides a faint green light at all times and reduces Abyss darkness intensity by 2."));

            tooltips.Add(new TooltipLine(Mod, "L5",
                "This accessory grows with each new boss you defeat. Silva armor set bonus instantly grants maximum growth."));

            tooltips.Add(new TooltipLine(Mod, "L6",
                $"growth rate : {progress:F1}%"));

            tooltips.Add(new TooltipLine(Mod, "L7",
                "Current Applied Bonuses:"));

            tooltips.Add(new TooltipLine(Mod, "L8",
                $"+{bonusMagicDamage:F1}% magic damage"));

            tooltips.Add(new TooltipLine(Mod, "L9",
                $"+{bonusMagicCrit:F1}% magic critical chance"));

            tooltips.Add(new TooltipLine(Mod, "L10",
                $"+{bonusLifePercent:F1}% maximum life"));

            tooltips.Add(new TooltipLine(Mod, "L11",
                $"+{bonusLifeRegen} life regeneration"));

            tooltips.Add(new TooltipLine(Mod, "L12",
                $"+{bonusMana} mana"));

            tooltips.Add(new TooltipLine(Mod, "L13",
                $"Leaf Shield cooltime: {lastcool:F1} seconds"));
        }

        public override void AddRecipes()
        {
            try
            {
                if (!ModLoader.TryGetMod("CalamityMod", out Mod calamity))
                    return;

                Recipe recipe = CreateRecipe();

                recipe.AddIngredient(
                    ModContent.ItemType<Materials.AncientEmblem>(), 1);

                recipe.AddIngredient(
                    calamity.Find<ModItem>("StrangeOrb").Type, 1);

                recipe.AddIngredient(
                    calamity.Find<ModItem>("PlantyMush").Type, 15);

                recipe.AddTile(TileID.DemonAltar);
                recipe.Register();
            }
            catch
            {
            }
        }
    }
}