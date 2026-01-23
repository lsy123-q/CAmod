using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace CAmod.Projectiles
{
    public class bomb : ModProjectile
    {

        private const int TrailCount = 120;
        private const int ClonePerTrail = 10;
        private float dist;
        private Vector2[] oldPos = new Vector2[TrailCount];
        private bool trailInit = false;
        private float lastRotation = 0f;
        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            
            Projectile.timeLeft = 600;
            // 투사체가 10초 후 자동 소멸한다
        }

        public override void AI()
        {

            Projectile.localAI[0]++;
            if (Projectile.localAI[0] > 60f)
                Projectile.localAI[0] = 60f;
            // 60틱 동안만 선회한다

            // 선회 감쇠 비율이다 (1 → 0)
            float faderot = MathHelper.Lerp(1f, 0.1f, Projectile.localAI[0] / 60f);

            // 기본 선회각이다 (느린 투사체 기준 조금 크게)
            float baseTurnDeg = 3f;
            float turnRad = MathHelper.ToRadians(baseTurnDeg) * faderot;

            // 현재 속도를 기준으로 회전시킨다
            Projectile.velocity =
                Projectile.velocity.RotatedBy(turnRad * Projectile.ai[1]);

            // ===== 시각적 회전 정렬 =====
            if (Projectile.velocity.Length() > 0.1f)
                Projectile.rotation =
                    Projectile.velocity.ToRotation() - MathHelper.PiOver2;

            // ===== 선회 안정화 계수 계산 =====
            float speed1 = Projectile.velocity.Length();
            // 속도 기반 보정값이다
            float speedFactor =
                MathHelper.Clamp(0.7f + (speed1 / 20f) * 0.6f, 0.4f, 1.3f);

            // 회전 변화량을 계산한다
            float turnRate = Math.Abs(
                MathHelper.WrapAngle(Projectile.rotation - lastRotation));
            lastRotation = Projectile.rotation;

            // 선회가 심할수록 보정이 줄어든다
            float turnFactor =
                MathHelper.Clamp(1.0f - turnRate * 8f, 0.4f, 1.0f);

            // PreDraw에서 사용할 값으로 저장한다
            Projectile.localAI[1] = speedFactor * turnFactor;
            // 잔상 길이/밝기 보정용이다


            if (Projectile.localAI[2] == 0f)
            {
                Projectile.ai[0] = Main.rand.NextFloat();
                // 시드용 값이다

                if (Projectile.ai[1] == 0f)
                    Projectile.ai[1] = 1f;
                // 회전 방향이다

                Projectile.localAI[2] = 1f;
                // 초기화 완료 플래그다
            }

            Projectile.Center += Projectile.velocity;

            // ===== 잔상 기록 =====
            if (!trailInit)
            {
                for (int i = 0; i < TrailCount; i++)
                    oldPos[i] = Projectile.Center;
                // 초기 위치로 채운다

                trailInit = true;
            }
            else
            {
                for (int i = TrailCount - 1; i > 0; i--)
                    oldPos[i] = oldPos[i - 1];
                // 뒤로 민다

                oldPos[0] = Projectile.Center;
                // 현재 위치를 맨 앞에 기록한다
            }

            // ===== 선회 → 직선 수렴 로직 =====

            // ===== 선회 → 직선 수렴 로직 =====

            // 경과 시간 누적이다



        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(
                "CAmod/Projectiles/bomb").Value;

            Texture2D trailTex = ModContent.Request<Texture2D>(
                "CAmod/Projectiles/bomb_trail").Value;

            Vector2 originMain = tex.Size() * 0.5f;
            Vector2 originTrail = trailTex.Size() * 0.5f;

            float baseRotSpeed = MathHelper.TwoPi / 60f;
            // 1초에 한 바퀴다

            float t = 600f - Projectile.timeLeft;
            // 경과 시간이다

            // ===== 스케일 인/아웃 =====
            float scale = 1f;
            float scale2 = 1f;
            if (t < 60f) { 
            scale = MathHelper.SmoothStep(0f, 1f, t / 60f);

            scale2 = MathHelper.SmoothStep(0f, 1f, t / 60f); }

            else if (Projectile.timeLeft < 30f) {
                scale2 = MathHelper.SmoothStep(0f, 1f, Projectile.timeLeft / 30f);
            }

            float seed = Projectile.ai[0];

            // =========================
            // 본체 (분신 10개)
            // =========================
            Vector2 pos = Projectile.Center - Main.screenPosition;

            for (int i = 0; i < ClonePerTrail; i++)
            {
                float hash = MathF.Abs(MathF.Sin(seed * 100f + i * 37.123f));

                float speedFactor = MathHelper.Lerp(0.4f, 1.0f, hash);
                float phase = hash * MathHelper.TwoPi;
                float dir = (hash > 0.5f ? 1f : -1f) * -Projectile.ai[1];

                float rotation =
                    Projectile.rotation +
                    baseRotSpeed * speedFactor * t * dir +
                    phase;

                SpriteEffects effect =
                    dir > 0f ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

                Main.spriteBatch.Draw(
                    tex,
                    pos,
                    null,
                    Color.White * 0.25f* scale2,
                    rotation,
                    originMain,
                    Projectile.scale * scale2,
                    effect,
                    0f
                );
            }
            float totalTrailLen = 0f;
            Vector2 lenPrev = Projectile.Center;

            for (int i = 0; i < TrailCount; i++)
            {
                totalTrailLen += Vector2.Distance(lenPrev, oldPos[i]);
                lenPrev = oldPos[i];
            }
            // =========================
            // 잔상 (누적 거리 기반 원뿔)
            // =========================
            float accumulatedLen = 0f;
            Vector2 prev = Projectile.Center;

            const float StepSpacing = 1f;
            const float TotalLenNorm = 600f;
            // 원뿔 길이 정규화 기준이다

            for (int j = 0; j < TrailCount; j++)
            {
                Vector2 cur = oldPos[j];

                float dist = Vector2.Distance(prev, cur);
                if (dist <= 0.01f)
                {
                    prev = cur;
                    continue;
                }

                int steps = Math.Max(1, (int)(dist / StepSpacing));
                float stepLen = dist / steps;

                Vector2 dirVec = cur - prev;
                dirVec.Normalize();

                for (int k = 0; k <= steps; k++)
                {
                    float localLen = stepLen * k;
                    float globalLen = accumulatedLen + localLen;

                    float globalT = globalLen / totalTrailLen;
                    globalT = MathHelper.Clamp(globalT, 0f, 1f);
                    // 전체 길이 기준 단일 보간값이다

                    Vector2 worldPos = prev + dirVec * localLen;
                    Vector2 drawPos = worldPos - Main.screenPosition;

                    float sharpT = MathF.Sqrt(globalT);
                    float coneScale = MathHelper.Lerp(1f, 0f, sharpT);
                    float alpha = MathHelper.Lerp(0.1f, 0f, globalT);
                    Color startColor = Color.Black;
                    Color endColor = new Color(0xC4, 0x27, 0x3A); // C4273A

                    Color trailColor = Color.Lerp(startColor, endColor, globalT);

                    for (int i = 0; i < 1; i++)
                    {
                        float hash = MathF.Abs(
                            MathF.Sin(seed * 97.1f + i * 31.3f + j * 17.9f + k * 13.7f));

                        float speedFactor = MathHelper.Lerp(0.4f, 1f, hash);
                        float phase = hash * MathHelper.TwoPi;
                        float dirRot = hash > 0.5f ? 1f : -1f;

                        float rotation =
                            baseRotSpeed * speedFactor * t * dirRot + phase;

                        rotation *= coneScale;
                        // 끝으로 갈수록 회전을 감쇠한다

                        Main.spriteBatch.Draw(
                            trailTex,
                            drawPos,
                            null,
                            trailColor * alpha * scale2,
                            rotation,
                            originTrail,
                            Projectile.scale * scale2 * coneScale,
                            SpriteEffects.None,
                            0f
                        );
                    }
                }

                accumulatedLen += dist;
                prev = cur;
            }

            return false;
            // 기본 Draw를 차단한다
        }





    }
}
