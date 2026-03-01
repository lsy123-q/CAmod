using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CAmod.Projectiles;
using System;
using CAmod.Players;
using CAmod.Systems;
namespace CAmod.Items
{
    public class Pyromancer : ModItem
    {
        public override string Texture => "CAmod/Items/Pyromancer"; // 텍스처 경로 변경해야 함

        public const bool DEV_MODE = true;
        public const int BASE_TICK_DAMAGE = 1;
        public const int TICK_INCREASE_PER_SECOND = 10;
        public const int BALANCED_TICK_DAMAGE_CAP = 600;
        public const float ATTACK_SPEED_PER_DEBUFF = 0.025f;
        public const float BALANCED_MAX_ATTACK_SPEED_BONUS = 1.0f;
        public float bonusdd;
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.accessory = true;
            
           
            Item.rare = ModContent.RarityType<Rarities.ArcaneFire>();
        }
        public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
        {
            if (line.Mod == "Terraria" && line.Name == "ItemName")
            {
                Rarities.ArcaneFire.Draw(Item, line);
                return false; // 기본 드로우를 막는다
            }

            return true; // 다른 줄은 기본 드로우한다
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

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {




            // ===== 기존 [KEY] 치환 로직 유지 =====
            Player owner = Main.LocalPlayer;
            
           

            float magicAdd = owner.GetDamage(DamageClass.Magic).Additive - 1f;
            float genericAdd = owner.GetDamage(DamageClass.Generic).Additive - 1f;

            float TotalMagicCrit = (owner.GetCritChance(DamageClass.Magic) + owner.GetCritChance(DamageClass.Generic)) / 100f + 1f;


            // 두 증가분을 더한 뒤 1000 배율로 정수화한다
            float magicPowerValue = (magicAdd + genericAdd) * TotalMagicCrit;

            magicPowerValue = (int)Math.Round(1000 * magicPowerValue);
            foreach (var line in tooltips)
            {
                if (line.Text.Contains("[fire]"))
                {
                    line.Text = line.Text.Replace("[fire]", magicPowerValue.ToString());
                    // [fire]를 현재 낙인 최대 피해값으로 치환한다
                }

                if (line.Text.Contains("[damage]"))
                {

                    line.Text = line.Text.Replace("[damage]", (bonusdd * 100f).ToString("0.##"));
                    // [damage]를 최종 마법 보정 퍼센트로 치환한다
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
                         calamity.Find<ModItem>("UnholyEssence").Type, 10);

                    recipe.AddIngredient(
                        calamity.Find<ModItem>("DivineGeode").Type, 10);

                    recipe.AddTile(TileID.LunarCraftingStation);
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

        public override void UpdateAccessory(Player player, bool hideVisual)
        {

            
            

            int maxbuffCount = 0;
            var mp = player.GetModPlayer<PyromancerPlayer>();
            mp.HasConduitEquipped = true;
            mp.visualEnabled = !hideVisual;
            if (Main.GameUpdateCount % 1 == 0)
            {
                foreach (Projectile p in Main.projectile)
                {
                    if (p.active &&
                        p.owner == player.whoAmI &&
                        p.type == ModContent.ProjectileType<HolyFlame>())
                    {
                        int targetWhoAmI = (int)p.ai[0];
                        if (targetWhoAmI >= 0 && targetWhoAmI < Main.npc.Length)
                        {
                            NPC target = Main.npc[targetWhoAmI];
                            if (target.active && target.life > 0)
                            {
                                HashSet<int> seen = new HashSet<int>();
                                int count = 0;

                                for (int i = 0; i < target.buffType.Length; i++)
                                {
                                    int b = target.buffType[i];
                                    if (b > 0 && !seen.Contains(b))
                                    {
                                        string internalName = "";

                                        if (b < BuffID.Count)
                                        {
                                            internalName = BuffID.Search.GetName(b);
                                            // 바닐라 버프의 내부 ID 이름을 가져온다 (예: OnFire, CursedInferno)
                                        }
                                        else
                                        {
                                            ModBuff modBuff = BuffLoader.GetBuff(b);
                                            if (modBuff != null)
                                                internalName = modBuff.Name;
                                            // 모드 버프의 내부 클래스 이름을 가져온다
                                        }
                                        
                                        internalName = internalName.ToLower(); // 비교를 위해 소문자로 만든다
                                        if (internalName.Contains("blood"))
                                            continue;
                                        if (internalName.Contains("fire") ||
                                            internalName.Contains("flame") ||
                                            internalName.Contains("burn") ||
                                            internalName.Contains("inferno") ||
                                            internalName.Contains("infernum") ||
                                            internalName.Contains("hex") ||
                                            internalName.Contains("daybreak")) // 데이브레이크를 추가한다
                                        {
                                            seen.Add(b);
                                            count++;
                                        }
                                    }

                                }

                                if (count > maxbuffCount)
                                    maxbuffCount = count;
                            }
                        }
                    }
                }

                if (maxbuffCount > mp.CurrentBuffCount)
                {
                    mp.CurrentBuffCount = maxbuffCount;
                    mp.decayDelayTimer = 0;
                }
                else if (maxbuffCount < mp.CurrentBuffCount)
                {
                    mp.decayDelayTimer++;
                    if (mp.decayDelayTimer % 60 == 0)
                    {
                        mp.CurrentBuffCount -= 1;
                    }
                }
                else
                {
                    mp.decayDelayTimer = 0;
                }

                mp.TargetDebuffsApplied = (int)mp.CurrentBuffCount;

                int debuffCountNow = mp.TargetDebuffsApplied;
                float bonus = ATTACK_SPEED_PER_DEBUFF * debuffCountNow;

                if (!DEV_MODE && bonus > BALANCED_MAX_ATTACK_SPEED_BONUS)
                    bonus = BALANCED_MAX_ATTACK_SPEED_BONUS;


                mp.bonusd = bonus;
                bonusdd = bonus; 
             

            }

            Player player2 = Main.LocalPlayer;
            var ward2 = player2.GetModPlayer<LeafWardPlayer2>();

            int stage = ward2.LeafShieldStage;


            float normalized = stage / 40f;
            // 성장 단계를 0~1로 정규화한다

            float priceGold = normalized * normalized * 40f;
            // 정규화값을 제곱 후 40골드를 곱한다

            int priceCopper = (int)(priceGold * 10000f);


            int copper = priceCopper;
            Item.value = Item.sellPrice(copper: priceCopper);

        }
    }
}
