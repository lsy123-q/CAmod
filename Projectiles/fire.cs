using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using System;
using Terraria.Audio;

namespace CAmod.Projectiles
{
    public class fire : ModProjectile
    {

        private float lifeFade01 = 1f;
        private float lastRotation;
        // 경과 시간 (틱 단위)
        private int age2 = 0;
        private float fadeout = 1;
        private float fadecount = 255;
        private int dmgsave = 1;
        private bool homing = false;
        float count = 0f;
        private readonly Color[] TrailColors =
        {
            new Color(0xFF, 0xFE, 0xFB), // FFF EFB
    new Color(0xFF, 0xE6, 0x96), // FFE696 (보강 - 밝은 노랑)
    new Color(0xFF, 0xC9, 0x4A), // FFC94A (보강 - 중간 노랑)
    new Color(0xFC, 0x9B, 0x00), // FC9B00
    new Color(0xE4, 0x7C, 0x1A), // E47C1A (보강 - 주황톤)
    new Color(0xD8, 0x67, 0x2B), // D8672B
    new Color(0xC3, 0x32, 0x25), // C33225
    new Color(0xB0, 0x24, 0x2B), // B0242B (보강 - 붉은톤 완화)
    new Color(0xA5, 0x16, 0x2E), // A5162E
    new Color(0x8F, 0x0C, 0x34), // 8F0C34 (보강 - 어두운 자주)
    new Color(0x82, 0x00, 0x38), // 820038
    new Color(0x82, 0x00, 0x38)
        };

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
            Main.projFrames[Projectile.type] = 5;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.timeLeft = 600;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 1;           // 적 관통 무제한(원하면 변경)
            Projectile.tileCollide = false;      // ✅ 타일 충돌 자체를 비활성화          
            Projectile.aiStyle = 0;
            Projectile.scale = 0.5f; // 초기 크기
            Projectile.DamageType = DamageClass.Magic;

        }

        public override void AI()
        {
            count++;
            if (count >= 60)
            {
                count = 60f;
            }
            float faderot = 1f;

            if (Projectile.timeLeft <= 60)
            {
                lifeFade01 = Projectile.timeLeft / 60f;
                // timeLeft 60일 때 1, 0일 때 0이 된다
            }
            else
            {
                lifeFade01 = 1f;
                // 아직 소멸 구간이 아니면 항상 1이다
            }
            float value = Projectile.ai[2];
            float detectRadius = 12000f;
            float baseSpeed = 15f;
            NPC target = FindClosestNPC(detectRadius);
           

                // TrailColors 기반 색상 보간

                for (int i = 0; i < 2; i++) { 
                int dust = Dust.NewDust(
                    Projectile.position,
                    Projectile.width,
                    Projectile.height,
                    DustID.CrimsonTorch, // 기본 Dust (나중에 색상 덮어씌움)
                    Projectile.velocity.X * 0.2f,
                    Projectile.velocity.Y * 0.2f,
                    100,
                    Color.White* lifeFade01,
                    1.2f * Projectile.scale* lifeFade01
                );

                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0.3f;
                Main.dust[dust].fadeIn = 0.8f;
                Main.dust[dust].scale *= 0.5f;
                }
            

            // --- 🎞️ 프레임 애니메이션 ---
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 4)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
                if (Projectile.frame >= Main.projFrames[Projectile.type])
                    Projectile.frame = 0;
            }

            // --- 초기 성장 (0~0.5초, 30틱) ---
            if (Projectile.timeLeft >= 570)
            {
                float externalScale = Projectile.ai[0];
                if (externalScale <= 0f)
                    externalScale = 1f;

                float progress = (600f - Projectile.timeLeft) / 30f;
                Projectile.scale = MathHelper.Lerp(externalScale / 10f, externalScale, progress);
                Projectile.alpha = (int)MathHelper.Lerp(255f, 0f, progress);
                Projectile.friendly = false;



            }
            else
            {
                Projectile.friendly = true;
                float externalScale = Projectile.ai[0];
                if (externalScale <= 0f) externalScale = 1f;
                Projectile.scale = externalScale;
                Projectile.alpha = 0;



            }

            if (homing == false)
            {

                Projectile.velocity = Projectile.velocity.RotatedBy(MathHelper.ToRadians((0f + value) * faderot));
            } // 현재 속도를 기준으로 5도 회전






            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;


            // --- 회전 갱신 ---
            if (Projectile.velocity.Length() > 0.1f)
                Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

            // --- 길이 보정 (속도 × 선회) ---
            float speed1 = Projectile.velocity.Length();
            float speedFactor = MathHelper.Clamp(0.7f + (speed1 / 20f) * 0.6f, 0.4f, 1.3f);

            float turnRate = Math.Abs(MathHelper.WrapAngle(Projectile.rotation - lastRotation));
            lastRotation = Projectile.rotation;
            float turnFactor = MathHelper.Clamp(1.0f - turnRate * 8f, 0.4f, 1.0f);

            Projectile.localAI[0] = speedFactor * turnFactor;

            // --- 🎯 호밍 로직 (0.5초 이후 활성화) ---
            if (Projectile.timeLeft <= 570)
            {

                float baseTurn = 10f;

                if (target != null)
                {
                    homing = true;
                    Vector2 move = target.Center - Projectile.Center;
                    float distance = move.Length();

                    if (distance > 0f)
                    {
                        move.Normalize();
                        move *= baseSpeed;

                        // ✦ 각도 차 계산
                        float desiredRot = move.ToRotation();
                        float currentRot = Projectile.velocity.ToRotation();
                        float turnDiff = Math.Abs(MathHelper.WrapAngle(desiredRot - currentRot));

                        // ✦ 각도 기반 가속
                        float maxTurn = 1.0f;
                        float minBoost = 1.00f;
                        float maxBoost = 1.1f;
                        float factor = MathHelper.Clamp(1f - (turnDiff / maxTurn), 0f, 1f);
                        float boost = MathHelper.Lerp(minBoost, maxBoost, factor);

                        // ✦ 가속과 함께 선회력도 강화
                        // boost가 높을수록 (정면일수록) turnrotate 값이 낮아져 빠르게 회전
                        float turnrotate = baseTurn / boost;

                        Projectile.velocity = (Projectile.velocity * (turnrotate - 1f) + move) / turnrotate;
                        Projectile.velocity *= boost;
                    }
                }

                else if (Projectile.timeLeft >= 60 && Projectile.timeLeft <= 540 && target == null) {

                    Projectile.timeLeft = 60;
                }

            }

            // --- 🌊 노이즈(흔들림) ---

        }

        public override bool PreDraw(ref Color lightColor)
        {

            if (Projectile.timeLeft <= 60)
            {
                fadecount -= 255f / 60f;
                fadeout = fadecount / 255f;
            }
            Texture2D bodyTex = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D headTex = ModContent.Request<Texture2D>("CAmod/Projectiles/fire_head").Value;

            int frameHeight = bodyTex.Height / Main.projFrames[Projectile.type];
            Rectangle sourceRect = new Rectangle(0, frameHeight * Projectile.frame, bodyTex.Width, frameHeight);

            // ✅ 둘 다 동일 anchor (같은 중심축)
            Vector2 origin = new Vector2(bodyTex.Width / 2, frameHeight * 0.85f);

            float stretchFactor = Projectile.localAI[0];
            float growthFactor = Projectile.scale;

            Vector2 scaleBody = new Vector2(1f, stretchFactor) * growthFactor;
            Vector2 scaleHead = new Vector2(1f, 1f) * growthFactor;

            // --- 잔상 ---
            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition;
                float progress = (float)i / (Projectile.oldPos.Length - 1);
                Color c = GetGradientColor(progress) * (0.4f * (1f - progress));

                // 본체 잔상 (찌그러짐 반영)
                Main.spriteBatch.Draw(
                    bodyTex,
                    drawPos,
                    sourceRect,
                    c * fadeout,
                    Projectile.rotation,
                    origin,
                    scaleBody* fadeout,
                    SpriteEffects.None,
                    0.1f
                );

                // 헤드 잔상 (정상 비율, 덮는 레이어)
                Main.spriteBatch.Draw(
                    headTex,
                    drawPos,
                    sourceRect,
                    c * fadeout,
                    Projectile.rotation,
                    origin,
                    scaleHead * fadeout,
                    SpriteEffects.None,
                    0.0f
                );
            }

            // --- 본체 ---
            Main.spriteBatch.Draw(
                bodyTex,
                Projectile.Center - Main.screenPosition,
                sourceRect,
                Color.White * fadeout,
                Projectile.rotation,
                origin,
                scaleBody * fadeout,
                SpriteEffects.None,
                0.1f
            );

            // --- 헤드 ---
            Main.spriteBatch.Draw(
                headTex,
                Projectile.Center - Main.screenPosition,
                sourceRect,
                Color.White * fadeout,
                Projectile.rotation,
                origin,
                scaleHead * fadeout,
                SpriteEffects.None,
                0.0f
            );

            return false;
        }

        private Color GetGradientColor(float progress)
        {
            if (TrailColors.Length == 1)
                return TrailColors[0];

            float scaled = progress * (TrailColors.Length - 1);
            int index = (int)Math.Floor(scaled);
            float t = scaled - index;

            if (index >= TrailColors.Length - 1)
                return TrailColors[^1];

            return Color.Lerp(TrailColors[index], TrailColors[index + 1], t);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {

            if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
            {
                try
                {
                    Type dragonType = calamity.Code.GetType("CalamityMod.Buffs.DamageOverTime.Dragonfire");
                    Type vulnType = calamity.Code.GetType("CalamityMod.Buffs.DamageOverTime.VulnerabilityHex");

                    if (dragonType != null)
                    {
                        int dragonID = calamity.Find<ModBuff>(dragonType.Name).Type;
                        target.AddBuff(dragonID, 180);
                    }

                    
                }
                catch { }
            }


            float scale = Projectile.scale;

            // 폭발 크기가 너무 작거나 너무 커지지 않도록 제한
            // (너무 작은 0.5 이하 or 너무 큰 2.0 이상 방지)
            float clampedScale = MathHelper.Clamp(scale, 0.6f, 1.0f);

            // 폭발 강도 관련 값들 (clampedScale 기반)
            int dustCountMain = (int)(25 * clampedScale);
            int dustCountGlow = (int)(10 * clampedScale);
            float speedMult = 3f * clampedScale;
            float sizeMult = 1.2f * clampedScale;
            Vector2 moveDir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            float speedFactor = Projectile.velocity.Length() / 10f; // 이동량을 10으로 나누고
            speedFactor = MathHelper.Clamp(speedFactor, 0.8f, 5.5f);
            // 🌟 폭발 Dust
            homing = false;
            for (int i = 0; i < dustCountMain; i++)
            {
                Vector2 vel = moveDir.RotatedByRandom(MathHelper.ToRadians(20f)) * Main.rand.NextFloat(4f, 8f) * speedFactor;
                float t = i / (float)dustCountMain;
                Color c = GetGradientColor(t);
                Vector2 speed = Main.rand.NextVector2Circular(speedMult, speedMult);

                int dust = Dust.NewDust(
                    Projectile.Center, 0, 0, DustID.FireworksRGB,
                    vel.X / 3, vel.Y / 3,
                    100, TrailColors[1] * 0.5f, sizeMult
                );
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 1.2f;
                Main.dust[dust].scale = (1.2f + Main.rand.NextFloat(0.3f)) * clampedScale;
            }
            for (int i = 0; i < dustCountMain; i++)
            {
                Vector2 vel = moveDir.RotatedByRandom(MathHelper.ToRadians(20f)) * Main.rand.NextFloat(4f, 8f) * speedFactor;
                float t = i / (float)dustCountMain;
                Color c = GetGradientColor(t);
                Vector2 speed = Main.rand.NextVector2Circular(speedMult, speedMult);

                int dust = Dust.NewDust(
                    Projectile.Center, 0, 0, DustID.CrimsonTorch,
                    vel.X / 3, vel.Y / 3,
                    100, Color.White, sizeMult
                );
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 1.2f;
                Main.dust[dust].scale = (1.2f + Main.rand.NextFloat(0.3f)) * clampedScale;
            }


            // 💫 잔광 Dust

            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact, Projectile.Center);
        }
        private NPC FindClosestNPC(float maxDetectDistance)
        {
            NPC closestNPC = null;
            float sqrMaxDetectDistance = maxDetectDistance * maxDetectDistance;

            foreach (NPC npc in Main.npc)
            {
                if (npc.CanBeChasedBy())
                {
                    float sqrDistanceToTarget = Vector2.DistanceSquared(npc.Center, Projectile.Center);
                    if (sqrDistanceToTarget < sqrMaxDetectDistance)
                    {
                        sqrMaxDetectDistance = sqrDistanceToTarget;
                        closestNPC = npc;
                    }
                }
            }
            return closestNPC;
        }
    }
}