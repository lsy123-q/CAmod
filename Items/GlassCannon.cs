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
            Item.rare = ModContent.RarityType<Rarities.AncientRarity>();
            Item.value = Item.sellPrice(gold: 25);
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
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            GlassCannonPlayer gPlayer = player.GetModPlayer<GlassCannonPlayer>();
            gPlayer.glassCannonEquipped = true;
            // 유리대포 장착 플래그를 켠다
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.RemoveAll(l => l.Mod == "Terraria" && l.Name.StartsWith("Tooltip"));

            tooltips.Add(new TooltipLine(Mod, "L1", "Maximum life reduced by 25%"));
            tooltips.Add(new TooltipLine(Mod, "L2", "Defense reduced by 25%"));
            tooltips.Add(new TooltipLine(Mod, "L3",
                "Healing potion effectiveness reduced by 25%"));
            tooltips.Add(new TooltipLine(Mod, "L4",
                "Life regeneration effectiveness reduced by 25%"));
         
            tooltips.Add(new TooltipLine(Mod, "L5",
                "Mana potion effectiveness increased by 25%"));
            tooltips.Add(new TooltipLine(Mod, "L6",
                "Magic critical chance increased by 12.5%"));
            tooltips.Add(new TooltipLine(Mod, "L7",
                "Magic attacks ignore 25 defense"));
            tooltips.Add(new TooltipLine(Mod, "L8",
                "All magic attacks deal additional fixed damage equal to 25% of damage dealt"));

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
        }


    }
}
