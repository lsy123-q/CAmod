using CalamityMod.NPCs.StormWeaver;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using System;
using CAmod.Particles;

using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;

namespace CAmod.Projectiles
{
    public class FlameEcho_Explosion : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 2;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Magic;
        }

        public override void AI()
        {
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (target.ModNPC is not (StormWeaverHead or StormWeaverBody or StormWeaverTail))
                return;

            bool infernumActive = ModLoader.TryGetMod("InfernumMode", out Mod infernum)
                && infernum.Call("GetInfernumActive") is true;
            modifiers.FinalDamage *= infernumActive ? 0.15f : 0.5f;
        }

        public override void OnHitNPC(
            NPC target,
            NPC.HitInfo hit,
            int damageDone)
        {
            int buffTime = 180;

            // ===== 버프 처리 =====
            try
            {
                Mod calamity = ModLoader.GetMod("CalamityMod");

                if (calamity != null)
                {
                    ModBuff holyFlames =
                        calamity.Find<ModBuff>("HolyFlames");

                    if (holyFlames != null)
                        target.AddBuff(
                            holyFlames.Type,
                            buffTime
                        );
                    else
                        target.AddBuff(
                            BuffID.OnFire,
                            buffTime
                        );
                }
                else
                {
                    target.AddBuff(
                        BuffID.OnFire,
                        buffTime
                    );
                }
            }
            catch
            {
                target.AddBuff(
                    BuffID.OnFire,
                    buffTime
                );
            }

            // ===== 파티클 연출 =====

            float min = 4f;
            float max = 8f;

            Vector2 hitDir =
                new Vector2(
                    Projectile.ai[0],
                    Projectile.ai[1]
                );

            if (hitDir == Vector2.Zero)
            {
                hitDir = Main.rand.NextVector2Unit();

                min = 2f;
                max = 4f;
            }

            hitDir.Normalize();

            int particleCount = 10;

            for (int i = 0; i < particleCount; i++)
            {
                // 기존 Dust와 동일하게 타격 방향을 기준으로 속도를 생성한다
                Vector2 velocity =
                    hitDir *
                    Main.rand.NextFloat(min, max);

                // 기존 랜덤 분산을 유지한다
                velocity +=
                    Main.rand.NextVector2Circular(
                        2f,
                        2f
                    );

                // 기존 Dust 크기 범위를 유지한다
                float size =
                    Main.rand.NextFloat(
                        0.9f,
                        1.35f
                    );

                // 기존 Dust의 랜덤 회전을 유지한다
                float rotation =
                    Main.rand.NextFloat(
                        MathHelper.TwoPi
                    );

                // 기존 Dust의 위치 범위와 동일하게 타겟 히트박스 내부에서 생성한다
                Vector2 position =
    target.Hitbox.TopLeft() +
    new Vector2(
        Main.rand.NextFloat(target.Hitbox.Width),
        Main.rand.NextFloat(target.Hitbox.Height)
    );

                CosmicParticleSystem.Buffer.Create(
                    new ParticleInfo(
                        position:
                            position.ToNumerics(),

                        velocity:
                            velocity.ToNumerics(),

                        rotation:
                            rotation,

                        scale:
                            new System.Numerics.Vector2(
                                size * 10f,
                                size * 10f
                            ),

                        depth: 1f,

                        color:
                            Color.White,

                        duration: 60
                    )
                );
                
            }
        }
    }
}