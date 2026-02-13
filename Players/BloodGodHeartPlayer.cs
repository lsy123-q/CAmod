using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.ResourceSets;
using Terraria.ModLoader;
using CAmod.Buffs;
namespace CAmod.Players
{
    public class BloodGodHeartPlayer : ModPlayer
    {
        public bool bloodHeartEquipped;
        public int bloodHeartCooldown;
        public int reviveInvulnTimer;

        private const double BleedDecay = 0.0083333333333; // 성배와 동일한 수치
        public bool regen;
        public override void ResetEffects()
        {
            bloodHeartEquipped = false;
            regen = false;
        }
        public override void UpdateLifeRegen()
        {
            if (regen == true) {
                Player.lifeRegen += 4;
            }



        }
        public override void PostUpdate()
        {
            if (bloodHeartCooldown > 0)
                bloodHeartCooldown--;

            if (reviveInvulnTimer > 0)
                reviveInvulnTimer--;

            CheckChaliceNextTick(); // 다음 틱 출혈 데미지 감지한다
        }

        // ─────────────────────────────
        // 일반 치명타 차단
        // ─────────────────────────────
        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (!bloodHeartEquipped || bloodHeartCooldown > 0)
                return false;

            if (info.Damage >= Player.statLife)
            {
                TriggerRevive();
                return true;
            }

            return false;
        }

        // ─────────────────────────────
        // 성배 "다음 틱" 실제 감소량 계산
        // ─────────────────────────────
        private void CheckChaliceNextTick()
        {
            if (!bloodHeartEquipped || bloodHeartCooldown > 0)
                return;

            if (!ModLoader.HasMod("CalamityMod"))
                return;

            try
            {
                var modPlayersField =
                    typeof(Player).GetField("modPlayers", BindingFlags.Instance | BindingFlags.NonPublic);

                var modPlayersList =
                    modPlayersField?.GetValue(Player) as System.Collections.IEnumerable;

                if (modPlayersList == null)
                    return;

                foreach (var mp in modPlayersList)
                {
                    if (mp.GetType().FullName == "CalamityMod.CalPlayer.CalamityPlayer")
                    {
                        var bufferField =
                            mp.GetType().GetField("chaliceBleedoutBuffer",
                                BindingFlags.Instance | BindingFlags.Public);

                        var partialField =
                            mp.GetType().GetField("chaliceDamagePointPartialProgress",
                                BindingFlags.Instance | BindingFlags.Public);

                        if (bufferField == null || partialField == null)
                            return;

                        double buffer = (double)bufferField.GetValue(mp);
                        double partial = (double)partialField.GetValue(mp);

                        if (buffer <= 0)
                            return;

                        double amountThisFrame = buffer * BleedDecay;
                        double total = partial + amountThisFrame;
                        int nextTickDamage = (int)total;

                        if (nextTickDamage >= Player.statLife)
                        {
                            TriggerRevive();
                        }

                        break;
                    }
                }
            }
            catch
            {
                // 구조 변경 대비 무시
            }
        }

        // ─────────────────────────────
        // 부활 공통 처리
        // ─────────────────────────────
        private void TriggerRevive()
        {
            bloodHeartCooldown = 3600;
            Player.AddBuff(ModContent.BuffType<BloodflareSoul>(), 60 * 60);
            Player.statLife = 10;
            if (Player.statLife < 1)
                Player.statLife = 1;

            Player.HealEffect(10);
            
            // 혈마법사 연동
            var blood = Player.GetModPlayer<BloodMagePlayer>();
            if (blood.bloodMageEquipped)
            {
                blood.allowBloodHeal = true;
                blood.lastLife = 10;
            }

            SpawnSouls();

            SoundEngine.PlaySound(
                new SoundStyle("CAmod/Players/BloodflareRangerActivation"),
                Player.Center
            );

            reviveInvulnTimer = 60; // 1초 하드 무적

            ClearChaliceBuffer();
        }

        // ─────────────────────────────
        // 성배 버퍼 완전 초기화
        // ─────────────────────────────
        private void ClearChaliceBuffer()
        {
            try
            {
                var modPlayersField =
                    typeof(Player).GetField("modPlayers", BindingFlags.Instance | BindingFlags.NonPublic);

                var modPlayersList =
                    modPlayersField?.GetValue(Player) as System.Collections.IEnumerable;

                if (modPlayersList == null)
                    return;

                foreach (var mp in modPlayersList)
                {
                    if (mp.GetType().FullName == "CalamityMod.CalPlayer.CalamityPlayer")
                    {
                        foreach (var f in mp.GetType().GetFields(
                            BindingFlags.Instance | BindingFlags.Public))
                        {
                            if (f.Name.Contains("chaliceBleedout"))
                            {
                                if (f.FieldType == typeof(double))
                                    f.SetValue(mp, 0d);
                                else if (f.FieldType == typeof(float))
                                    f.SetValue(mp, 0f);
                                else if (f.FieldType == typeof(int))
                                    f.SetValue(mp, 0);
                            }
                        }
                        break;
                    }
                }
            }
            catch { }
        }

        // ─────────────────────────────
        // 칼라미티 하드무적 차단
        // ─────────────────────────────
        public override bool CanBeHitByNPC(NPC npc, ref int cooldownSlot)
        {
            if (reviveInvulnTimer > 0)
                return false;

            return true;
        }

        public override bool CanBeHitByProjectile(Projectile proj)
        {
            if (reviveInvulnTimer > 0)
                return false;

            return true;
        }

        // ─────────────────────────────
        // 영혼 소환
        // ─────────────────────────────
        public void SpawnSouls()
        {
            if (Player.whoAmI != Main.myPlayer)
                return;

            var source = Player.GetSource_Misc("BloodGodHeart");

            int baseDamage = 500;
            int damage = (int)Player.GetTotalDamage(DamageClass.Magic).ApplyTo(baseDamage);

            for (int i = 0; i < 16; i++)
            {
                float ai1 = Main.rand.NextFloat() + 0.5f;

                Vector2 circleVel =
                    (MathHelper.TwoPi * i / 16f).ToRotationVector2()
                    * Main.rand.NextFloat(5f, 8f);

                Projectile.NewProjectile(
                    source,
                    Player.Center,
                    circleVel,
                    ModContent.ProjectileType<Projectiles.BloodflareSoul>(),
                    damage,
                    0f,
                    Player.whoAmI,
                    0f,
                    ai1
                );
            }
        }
    }
}
