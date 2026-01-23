using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using System.Collections.Generic;
using Terraria.Audio;
using System;

namespace CAmod.Projectiles
{
    public class FlameEcho_Controller : ModProjectile
    {
        private const int SearchInterval = 6;   // 6프레임마다 탐색한다
        private const int MaxChainCount = 15;   // 최대 전이 횟수다
        private const float SearchRange = 1250f; // 탐색 반경이다
        private int lockedTarget = -1;
        private int frameTimer = 0;
        private int chainCount = 0;
        private const float AltSearchRange = 125f;
        private Vector2 previousPos;
        // 이전 전이 위치를 저장한다

        private HashSet<int> hitCache = new HashSet<int>();
        // 전이 대상으로 이미 사용된 NPC를 저장한다

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
            // 탐색 주기가 되었으므로 타이머를 초기화한다

            if (chainCount >= MaxChainCount)
            {
                Projectile.Kill(); // 최대 전이 횟수에 도달하면 종료한다
                return;
            }
            Vector2 dir = Vector2.Zero;

          
            int nextTarget = FindNextTarget();

            if (nextTarget == -1)
            {
                Projectile.Kill(); // 더 이상 타겟이 없으면 종료한다
                return;
            }

            NPC npc = Main.npc[nextTarget];

            previousPos = Projectile.Center;
            // 현재 위치를 이전 위치로 저장한다

            hitCache.Add(nextTarget);
            chainCount++;

            Projectile.Center = npc.Center;
            // 컨트롤러 위치를 타겟 NPC 위치로 이동시킨다

            DrawDustLine(previousPos, npc.Center);
            // 전이 경로를 Dust 선분으로 그린다
           Vector2 chainDir = npc.Center - previousPos;
if (chainDir.LengthSquared() > 100f)
    chainDir.Normalize();
else
    chainDir = Vector2.Zero;
            SoundEngine.PlaySound(SoundID.Item14, npc.Center);


            for (int i2 = 0; i2 < 75; i2++)
            {
                float offsetAngle = MathHelper.TwoPi * i2 / 75f;
                int dustType = Main.rand.Next(2);
                if (dustType == 0)
                {
                    dustType = 244;
                }
                else
                {
                    dustType = 246;
                }
                // asteroid(둥근 4엽) 파라메트릭 곡선이다
                float unitOffsetX = (float)Math.Pow(Math.Cos(offsetAngle), 3d);
                float unitOffsetY = (float)Math.Pow(Math.Sin(offsetAngle), 3d);
                float dir2 = chainDir.ToRotation();
                Vector2 puffDustVelocity =
                    new Vector2(unitOffsetX, unitOffsetY)
                    .RotatedBy(dir2) * 5f;
                // 벡터 자체를 커서 방향으로 회전시킨다

                Dust gold = Dust.NewDustPerfect(
                    npc.Center,
                    dustType,
                    puffDustVelocity
                );

                gold.scale = 1.6f;
                gold.fadeIn = 1.2f;
                gold.noGravity = true;
                gold.velocity *= 0.65f;
                gold.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                // 골드 코인 더스트로 회전된 아스테로이드 연출을 만든다
            }


            SpawnExplosion(npc, chainDir);
            // 전이 대상에게 폭발 투사체를 소환한다

            SpawnExplosionAlongLine(previousPos, npc.Center, chainDir);
            // 전이 선분에 닿은 모든 NPC에게 폭발 투사체를 소환한다
        }

        private int FindNextTarget()
        {
            int result = -1;

            bool altMode = Projectile.ai[0] == 1f;
            bool isFirstChain = chainCount == 0;

            // ===== 기준점 =====
            Vector2 origin = isFirstChain
                ? Main.MouseWorld
                : Projectile.Center;
            // 첫 전이는 좌/우 동일하게 마우스 기준이다

            // ===== 전이 반경 =====
            float range;
            if (isFirstChain || !altMode)
                range = SearchRange;
            else
                range = AltSearchRange;
            // 우클릭 2번째 전이부터만 반경이 줄어든다

            // ===== 거리 기준 =====
            float bestDist;
            if (isFirstChain || altMode)
                bestDist = float.MaxValue; // 최근접이다
            else
                bestDist = 0f;             // 좌클릭 2번째부터 최원이다

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];

                if (!n.active)
                    continue;

                if (n.friendly || n.dontTakeDamage)
                    continue;

                // 첫 전이는 좌/우 동일하게 중복 방지한다
                // 이후 전이는 좌클릭만 중복 방지한다
                if ((!altMode || isFirstChain) && hitCache.Contains(i))
                    continue;

                float dist = Vector2.Distance(origin, n.Center);

                if (dist > range)
                    continue;

                if (!Collision.CanHitLine(origin, 1, 1, n.Center, 1, 1))
                    continue;

                if (isFirstChain || altMode)
                {
                    // 최근접을 고른다
                    if (dist >= bestDist)
                        continue;

                    bestDist = dist;
                }
                else
                {
                    // 좌클릭 2번째 전이부터 최원을 고른다
                    if (dist <= bestDist)
                        continue;

                    bestDist = dist;
                }

                result = i;
            }

            return result;
        }









        private void SpawnExplosion(NPC npc, Vector2 dir)
{
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
    // ai[0], ai[1]에 전이 방향을 전달한다
}

        private void SpawnExplosionAlongLine(Vector2 start, Vector2 end, Vector2 dir)
        {
            float lineRadius = 16f;
            // 선분 판정 반경이다

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];

                if (!n.active)
                    continue;

                if (n.friendly || n.dontTakeDamage)
                    continue;

                if (hitCache.Contains(i))
                    continue;
                // 전이 대상은 이미 처리했으므로 제외한다

                float dist = DistancePointToLine(n.Center, start, end);

                if (dist > lineRadius)
                    continue;

                SpawnExplosion(n, dir);
                // 선분에 닿은 NPC 위치에 폭발 투사체를 소환한다
            }
        }

        private float DistancePointToLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
        {
            Vector2 line = lineEnd - lineStart;
            float length = line.Length();

            if (length <= 0f)
                return Vector2.Distance(point, lineStart);

            float t = Vector2.Dot(point - lineStart, line) / (length * length);
            t = MathHelper.Clamp(t, 0f, 1f);

            Vector2 projection = lineStart + line * t;
            return Vector2.Distance(point, projection);
            // 점과 선분 사이의 최소 거리를 계산한다
        }

        private void DrawDustLine(Vector2 start, Vector2 end)
        {
            Vector2 dir = end - start;
            float length = dir.Length();

            if (length <= 0f)
                return;

            dir.Normalize();

            float step = 6f;
            // Dust 간격이다
            
            for (float i = 0; i < length; i += step)
            {
                Vector2 pos = start + dir * i;
                int dustType = Main.rand.Next(2);
                if (dustType == 0)
                {
                    dustType = 244;
                }
                else
                {
                    dustType = 246;
                }
                int heatGold = Dust.NewDust(pos, 1, 1, dustType, 0f, 0f, 0, default, 1f);
                Main.dust[heatGold].scale = Main.rand.Next(70, 110) * 0.013f;
                Main.dust[heatGold].velocity *= 0.2f;
                Main.dust[heatGold].noGravity = true;
                Main.dust[heatGold].fadeIn = 1.5f;
                Main.dust[heatGold].noLight = false;
                Main.dust[heatGold].rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                // 골드 코인 더스트로 금빛 전이 선분을 만든다
            }
        }
    }
}
