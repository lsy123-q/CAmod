using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using CAmod.Players;
using CAmod.Particles;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;

namespace CAmod.Projectiles
{
    public class BloodHealOrb : ModProjectile
    {
        private float particleDistanceRemainder;

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.timeLeft = 1000;
            Projectile.penetrate = -1;

            Projectile.friendly = false;
            Projectile.hostile = false;

            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            // 오브 자체가 선혈색 광원을 낸다
            Lighting.AddLight(
                Projectile.Center,
                0.9f,
                0.1f,
                0.1f
            );

            Projectile.extraUpdates = 3;
            // 이동 보간 밀도를 높인다

            int ownerIndex = (int)Projectile.ai[0];
            int healPotential = (int)Projectile.ai[1];

            if (ownerIndex < 0 || ownerIndex >= Main.maxPlayers)
            {
                Projectile.Kill();
                return;
            }

            Player player = Main.player[ownerIndex];

            if (player.statLifeMax2 == player.statLife)
            {
                Projectile.Kill();
            }

            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }

            healPotential = Math.Min(
                healPotential,
                player.statLifeMax2 - player.statLife
            );

            Vector2 oldPos = Projectile.Center;

            Vector2 toPlayer = player.Center - Projectile.Center;

            Vector2 targetPos = player.position + new Vector2(
                Main.rand.NextFloat(player.width),
                Main.rand.NextFloat(player.height)
            );
            // 플레이어 히트박스 내부의 랜덤 지점이다

            Vector2 toTarget = targetPos - Projectile.Center;
            float dist = toTarget.Length();

            if (dist < 25f)
            {
                int healed = Math.Min(
                    healPotential,
                    player.statLifeMax2 - player.statLife
                );

                float healRatio =
                    (float)healed / player.statLifeMax2;

                float scale = MathHelper.Lerp(
                    0.5f,
                    1.75f,
                    healRatio
                );

                // 회복량 1~1000에 따라 파티클 크기를 1~10으로 제한한다
                float burstParticleSize = MathHelper.Clamp(
                    MathHelper.Lerp(1f, 10f, healed / 1000f),
                    1f,
                    10f
                );

                // 회복량 1000에서 생존시간이 2배가 되며 그 이상은 증가하지 않는다
                float lifetimeMultiplier =
                    1f + MathHelper.Clamp(healed / 1000f, 0f, 1f);

                int burstDuration =
                    (int)(60f * lifetimeMultiplier);

                for (int i = 0; i < 35; i++)
                {
                    Vector2 vel =
                        Main.rand.NextVector2Circular(2f, 2f) * scale;

                    BloodParticleSystem.Buffer.Create(
                        new ParticleInfo(
                            position: Projectile.Center.ToNumerics(),
                            velocity: vel.ToNumerics(),
                            rotation: 0f,
                            scale: new System.Numerics.Vector2(
                                burstParticleSize,
                                burstParticleSize
                            ),
                            depth: 1f,
                            color: Color.White,
                            duration: burstDuration
                        )
                    );
                }

                var bmp = player.GetModPlayer<BloodMagePlayer>();
                bmp.allowBloodHeal = true;

                player.statLife += healed;
                bmp.lastLife = player.statLife;

                int manaToStore = healed;
                bmp.manaHealBuffer += manaToStore;

                // 체력 회복량과 동일한 양만큼 마나를 회복한다
                if (healed != 0)
                {
                    CombatText.NewText(
                        player.getRect(),
                        new Color(180, 30, 30),
                        "+" + healed
                    );
                }

                Projectile.Kill();
                return;
            }

            // 폭탄식 추적 로직
            float accel = 1.02f;
            float damp = 0.92f;
            float maxSpeed = 14f;

            if (dist > 0f)
            {
                toTarget.Normalize();
                toTarget *= accel;

                Projectile.velocity.X =
                    Projectile.velocity.X * damp + toTarget.X;

                Projectile.velocity.Y =
                    Projectile.velocity.Y * damp + toTarget.Y;

                if (Projectile.velocity.Length() > maxSpeed)
                {
                    Projectile.velocity =
                        Vector2.Normalize(Projectile.velocity) * maxSpeed;
                }
            }
            // 추적 끝

            // 혈액 파티클 연출
            Vector2 nextPos =
                Projectile.Center + Projectile.velocity;

            float distMove =
                Vector2.Distance(oldPos, nextPos);

            if (distMove > 0f)
            {
                const float particleSpacing = 2.5f;

                // 이전 프레임에서 남은 거리를 이번 이동거리에 합산한다
                float totalDistance =
                    particleDistanceRemainder + distMove;

                int stepCount =
                    (int)(totalDistance / particleSpacing);

                // 이번 프레임에서 사용하지 못한 거리를 다음 프레임으로 넘긴다
                particleDistanceRemainder =
                    totalDistance - stepCount * particleSpacing;

                if (Projectile.localAI[0] == 0f)
                {
                    Projectile.localAI[0] = 1f;
                }

                int healed = Math.Min(
                    healPotential,
                    player.statLifeMax2 - player.statLife
                );

                float particleScale =
                    0.15f + MathF.Sqrt(healed) * 0.14f;

                particleScale =
                    Math.Min(particleScale, 1.75f);

                // 회복량 1000에서 생존시간이 2배가 되며 그 이상은 증가하지 않는다
                float lifetimeMultiplier =
                    1f + MathHelper.Clamp(healed / 1000f, 0f, 1.5f);

                int trailDuration =
                    (int)(45f * lifetimeMultiplier);

                if (stepCount > 0)
                {
                    // 파티클이 실제 이동 경로를 따라 균등하게 배치되도록 한다
                    float distanceToFirst =
                        particleSpacing -
                        (particleDistanceRemainder == 0f
                            ? particleSpacing
                            : particleDistanceRemainder);

                    for (int i = 0; i < stepCount; i++)
                    {
                        float distanceFromStart =
                            distanceToFirst + i * particleSpacing;

                        if (distanceFromStart > distMove)
                            break;

                        float t =
                            distanceFromStart / distMove;

                        Vector2 particlePos =
                            Vector2.Lerp(oldPos, nextPos, t);

                        BloodParticleSystem.Buffer.Create(
                            new ParticleInfo(
                                position: particlePos.ToNumerics(),
                                velocity: Vector2.Zero.ToNumerics(),
                                rotation: 0f,
                                scale: new System.Numerics.Vector2(
                                    particleScale * 8f,
                                    particleScale * 8f
                                ),
                                depth: 1f,
                                color: Color.White,
                                duration: trailDuration
                            )
                        );
                    }
                }
            }
        }
    }
}