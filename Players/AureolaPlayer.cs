using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
namespace CAmod.Players
{
    public class AureolaPlayer : ModPlayer
    {
        public bool aureola;

        public float aureolastoredMana;          // 회복해야 하는 마나를 저장한다
        public int aureolarestoreGauge;        // 1 이상이 되면 실제 마나를 회복한다
        public float aureolarestoreCooldown;     // 마나 회복 후 대기 시간을 저장한다
        public float aureolahighestObservedManaRegen;
        double Decay = 0.0166666666666;
        public double restore;
        public int aureolaDustTimer; // 스톡 Dust 생성 간격을 저장한다
        public override void ResetEffects()
        {
            aureola = false;
        }
        public override void PostUpdateEquips()
        {
            
            aureolahighestObservedManaRegen = Player.statManaMax2;
        }
        public override void PostUpdate()
        {
            if (!aureola)
            {
                aureolastoredMana = 0f;
                aureolarestoreGauge = 0;
                aureolarestoreCooldown = 0f;
                aureolahighestObservedManaRegen = 0f;
                aureolaDustTimer = 0; // 장신구를 빼면 Dust 생성 간격을 초기화한다
                return;
            }

           



            int missingMana = Player.statManaMax2 - Player.statMana;

            if (aureolarestoreCooldown > 0f)
            {
                // 회복할 마나가 없어도 마나통이 꽉차있으면 


                if (missingMana > 0 || (missingMana == 0 && aureolastoredMana < 1f))
                    aureolarestoreCooldown--;


            }
            SpawnAureolaStockDust();
            if (missingMana == 0)
                return;

            restore += aureolastoredMana * Decay;

            aureolarestoreGauge = (int)restore;

            restore -= aureolarestoreGauge;
            aureolastoredMana -= aureolarestoreGauge;


            Player.statMana += aureolarestoreGauge;
            

        }
        private void SpawnAureolaStockDust()
        {
            if (aureolastoredMana <= 1f)
            {
                aureolaDustTimer = 0; // 스톡이 없으면 Dust 간격을 초기화한다
                return;
            }
            int missingMana = Player.statManaMax2 - Player.statMana;

            if (missingMana == 0)
            {
                return;


            }
            aureolaDustTimer++;

            
            

            aureolaDustTimer = 0;

            float maxMana = Math.Max(Player.statManaMax2, 1);
            float stockRatio = MathHelper.Clamp(aureolastoredMana / maxMana, 0f, 1f);

            float dustAmount = stockRatio * 8f;

            // 1개 미만이면 확률적으로 이번 프레임 생성을 건너뛴다
            if (dustAmount < 1f && Main.rand.NextFloat() >= dustAmount)
                return;

            int dustCount = Math.Max(1, (int)dustAmount); // 최소 1개는 생성한다
            float radius = 50f * Main.rand.NextFloat(0.75f, 1.25f); // 스톡이 많을수록 원형 소환 반지름을 증가시킨다
            float dustScale = 1.2f; // 스톡이 많을수록 Dust 크기를 증가시킨다
            float moveSpeed = 2.5f * Main.rand.NextFloat(0.75f, 1.25f); // 스톡이 많을수록 플레이어 쪽 이동 속도를 증가시킨다

            for (int i = 0; i < dustCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);

                Vector2 spawnOffset = new Vector2(
                    MathF.Cos(angle),
                    MathF.Sin(angle)
                ) * radius;

                Vector2 spawnPosition = Player.Center + spawnOffset;

                Vector2 velocity = Player.Center - spawnPosition;

                if (velocity != Vector2.Zero)
                    velocity.Normalize(); // 플레이어 쪽 방향으로 정규화한다

                velocity *= moveSpeed;

                Dust dust = Dust.NewDustPerfect(
                    spawnPosition,
                    DustID.MagicMirror,
                    velocity,
                    150,
                    default,
                    dustScale + Main.rand.NextFloat(0.2f)
                );

                dust.noGravity = true; // 중력 영향을 받지 않게 한다
                dust.noLight = false; // 빛을 내게 한다
                dust.fadeIn = 0.5f + stockRatio; // 스톡이 많을수록 Dust가 더 선명하게 보이게 한다
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {


            if (!aureola)
                return;

            target.AddBuff(ModContent.BuffType<global::CalamityMod.Buffs.DamageOverTime.AuricRebuke>(), 180);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!aureola)
                return;

            if (damageDone <= 0)
                return;

            if (proj.DamageType != DamageClass.Magic)
                return; // 마법 투사체가 아니면 마나 회복을 막는다

            if (aureolarestoreCooldown > 0f)
                return;
            float missingMana = Player.statManaMax2 - Player.statMana;
            if (missingMana <= 0f)
                return;

            aureolastoredMana += damageDone * 0.10f; // 입힌 피해량의 10%만큼 마나 회복량을 저장한다
            if (aureolastoredMana > Player.statManaMax2)
                aureolastoredMana = Player.statManaMax2;

            aureolarestoreCooldown = (aureolastoredMana / (aureolahighestObservedManaRegen/10f))*60f;

            // 600 20 30
        }
    }
}