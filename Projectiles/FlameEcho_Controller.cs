using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.Audio;
using Terraria.DataStructures;
using System;
using System.Collections.Generic;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using CAmod.Particles;

namespace CAmod.Projectiles
{
    public class FlameEcho_Controller : ModProjectile
    {
        private const int SearchInterval = 6;
        private const int MaxChainCount = 15;
        private const float SearchRange = 1250f;

        private int lockedTarget = -1;
        private int frameTimer = 0;
        private int chainCount = 0;

        private const float AltSearchRange = 125f;

        private Vector2 previousPos;

        private int chainIndex = 0;

        private HashSet<int> hitCache = new HashSet<int>();

        private int chainGroup;

        private int totalHitCount = 0;

        public override void OnSpawn(IEntitySource source)
        {
            chainGroup = Main.rand.Next();

            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                Vector2.Zero,
                ModContent.ProjectileType<FlameEcho_ChainNode>(),
                0,
                0f,
                Projectile.owner,
                chainIndex,
                chainGroup
            );

            chainIndex++;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;

            Projectile.timeLeft = 300;
            Projectile.penetrate = -1;

            Projectile.friendly = false;
            Projectile.hostile = false;

            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            frameTimer++;

            if (frameTimer < SearchInterval)
                return;

            frameTimer = 0;

            if (totalHitCount >= 20)
            {
                Projectile.Kill();
                return;
            }

            if (chainCount >= MaxChainCount)
            {
                Projectile.Kill();
                return;
            }

            Vector2 dir = Vector2.Zero;

            int nextTarget = FindNextTarget();

            if (nextTarget == -1)
            {
                Projectile.Kill();
                return;
            }

            NPC npc = Main.npc[nextTarget];

            previousPos = Projectile.Center;

            hitCache.Add(nextTarget);

            chainCount++;

            Projectile.damage =
                (int)(Projectile.damage * 0.975f);

            Projectile.Center = npc.Center;

            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                npc.Center,
                Vector2.Zero,
                ModContent.ProjectileType<FlameEcho_ChainNode>(),
                0,
                0f,
                Projectile.owner,
                chainIndex,
                chainGroup
            );

            chainIndex++;

            Vector2 chainDir = npc.Center - previousPos;

            if (chainDir.LengthSquared() > 100f)
                chainDir.Normalize();
            else
                chainDir = Vector2.Zero;

            SoundEngine.PlaySound(
                SoundID.Item14,
                npc.Center
            );

            // =========================================
            // V3 파티클 아스테로이드 폭발
            // =========================================

            for (int i2 = 0; i2 < 75; i2++)
            {
                float offsetAngle =
                    MathHelper.TwoPi * i2 / 75f;

                int dustType = Main.rand.Next(2);

                // 기존 Dust 타입 선택은 더 이상 필요 없지만
                // 색상 선택용 랜덤성을 유지한다.
                Color color =
                    dustType == 0
                    ? new Color(255, 210, 80)
                    : new Color(255, 230, 130);

                // 아스테로이드(4엽) 형태
                float unitOffsetX =
                    (float)Math.Pow(
                        Math.Cos(offsetAngle),
                        3d
                    );

                float unitOffsetY =
                    (float)Math.Pow(
                        Math.Sin(offsetAngle),
                        3d
                    );

                float dir2 = chainDir.ToRotation();

                Vector2 puffVelocity =
                    new Vector2(
                        unitOffsetX,
                        unitOffsetY
                    ).RotatedBy(dir2) * 5f;

                puffVelocity *= 0.65f;

                float particleRotation =
                    Main.rand.NextFloat(
                        MathHelper.TwoPi
                    );

                CosmicParticleSystem.Buffer.Create(
                    new ParticleInfo(
                        position: npc.Center.ToNumerics(),
                        velocity: puffVelocity.ToNumerics(),
                        rotation: particleRotation,
                        scale: new System.Numerics.Vector2(
                            1.6f * 10f,
                            1.6f * 10f
                        ),
                        depth: 1f,
                        color: color,
                        duration: 60
                    )
                );
                
            }

            SpawnExplosion(npc, chainDir);

            SpawnExplosionAlongLine(
                previousPos,
                npc.Center,
                chainDir
            );
        }

        private int FindNextTarget()
        {
            int result = -1;

            bool altMode =
                Projectile.ai[0] == 1f;

            bool isFirstChain =
                chainCount == 0;

            Vector2 origin =
                isFirstChain
                ? Main.MouseWorld
                : Projectile.Center;

            float range;

            if (isFirstChain || !altMode)
                range = SearchRange;
            else
                range = AltSearchRange;

            float bestDist;

            if (isFirstChain || altMode)
                bestDist = float.MaxValue;
            else
                bestDist = 0f;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];

                if (!n.active)
                    continue;

                if (n.catchItem > 0)
                    continue;

                if (n.friendly || n.dontTakeDamage)
                    continue;

                if ((!altMode || isFirstChain)
                    && hitCache.Contains(i))
                    continue;

                Player player =
                    Main.player[Projectile.owner];

                if (Vector2.Distance(
                    player.Center,
                    n.Center
                ) > 1000f)
                    continue;

                float dist =
                    Vector2.Distance(
                        origin,
                        n.Center
                    );

                if (dist > range)
                    continue;

                if (!Collision.CanHitLine(
                    origin,
                    1,
                    1,
                    n.Center,
                    1,
                    1
                ))
                    continue;

                if (isFirstChain || altMode)
                {
                    // 최근접
                    if (dist >= bestDist)
                        continue;

                    bestDist = dist;
                }
                else
                {
                    // 최원
                    if (dist <= bestDist)
                        continue;

                    bestDist = dist;
                }

                result = i;
            }

            return result;
        }

        private void SpawnExplosion(
            NPC npc,
            Vector2 dir
        )
        {
            totalHitCount++;

            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                npc.Center,
                Vector2.Zero,
                ModContent.ProjectileType<FlameEcho_Explosion>(),
                Projectile.damage,
                0f,
                Projectile.owner,
                dir.X,
                dir.Y
            );
        }

        private void SpawnExplosionAlongLine(
            Vector2 start,
            Vector2 end,
            Vector2 dir
        )
        {
            float lineRadius = 16f;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];

                if (!n.active)
                    continue;

                if (n.friendly || n.dontTakeDamage)
                    continue;

                if (hitCache.Contains(i))
                    continue;

                float dist =
                    DistancePointToLine(
                        n.Center,
                        start,
                        end
                    );

                if (dist > lineRadius)
                    continue;

                SpawnExplosion(n, dir);
            }
        }

        private float DistancePointToLine(
            Vector2 point,
            Vector2 lineStart,
            Vector2 lineEnd
        )
        {
            Vector2 line =
                lineEnd - lineStart;

            float length =
                line.Length();

            if (length <= 0f)
            {
                return Vector2.Distance(
                    point,
                    lineStart
                );
            }

            float t =
                Vector2.Dot(
                    point - lineStart,
                    line
                ) / (length * length);

            t = MathHelper.Clamp(
                t,
                0f,
                1f
            );

            Vector2 projection =
                lineStart + line * t;

            return Vector2.Distance(
                point,
                projection
            );
        }
    }
}