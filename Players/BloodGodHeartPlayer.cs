using System;
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
        public override void OnRespawn()
        {
            bloodHeartCooldown = 0; // 부활 시 혈심장 쿨타임을 0으로 초기화한다
           
        }
        // ─────────────────────────────
        // 성배 "다음 틱" 실제 감소량 계산
        // ─────────────────────────────
        private void CheckChaliceNextTick()
        {
            if (!bloodHeartEquipped || bloodHeartCooldown > 0)
                return;

            var calPlayer = Player.GetModPlayer<global::CalamityMod.CalPlayer.CalamityPlayer>();
            double buffer = calPlayer.chaliceBleedoutBuffer;
            if (buffer <= 0)
                return;

            double total = calPlayer.chaliceDamagePointPartialProgress + buffer * BleedDecay;
            if ((int)total >= Player.statLife)
                TriggerRevive();
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

            reviveInvulnTimer = 120; // 1초 하드 무적
            Player.immune = true; // 플레이어를 무적 상태로 만든다
            Player.immuneTime = 120; // 60틱 = 1초 동안 무적을 부여한다
            ClearChaliceBuffer();
        }

        // ─────────────────────────────
        // 성배 버퍼 완전 초기화
        // ─────────────────────────────
        private void ClearChaliceBuffer()
        {
            var calPlayer = Player.GetModPlayer<global::CalamityMod.CalPlayer.CalamityPlayer>();
            calPlayer.chaliceBleedoutBuffer = 0D;
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
