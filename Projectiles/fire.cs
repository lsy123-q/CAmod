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

        private float fadeout = 1f;
        private float fadecount = 255f;

        private bool nope = false;

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
            Projectile.penetrate = 1;      // 적 관통 1회다
            Projectile.tileCollide = false; // 타일 충돌 비활성화다
            Projectile.aiStyle = 0;
            Projectile.scale = 0.8f;       // 초기 크기다
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

            // 소멸 페이드 계수(0~1) timeLeft 기반이다
            if (Projectile.timeLeft <= 60)
            {
                lifeFade01 = Projectile.timeLeft / 60f;
                // timeLeft 60일 때 1, 0일 때 0이다
            }
            else
            {
                lifeFade01 = 1f;
                // 아직 소멸 구간이 아니면 1이다
            }

            float value = Projectile.ai[2];
            float detectRadius = 2500f;
            float baseSpeed = 15f;
            NPC target = FindClosestNPC(detectRadius);

            // Dust
            for (int i = 0; i < 2; i++)
            {
                int dust = Dust.NewDust(
                    Projectile.position,
                    Projectile.width,
                    Projectile.height,
                    DustID.CrimsonTorch, // 기본 Dust다
                    Projectile.velocity.X * 0.2f,
                    Projectile.velocity.Y * 0.2f,
                    100,
                    Color.White * lifeFade01,
                    1.2f * Projectile.scale * lifeFade01
                );

                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0.3f;
                Main.dust[dust].fadeIn = 0.8f*lifeFade01;
                Main.dust[dust].scale *= 0.5f;
            }

            // 프레임 애니메이션
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 4)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
                if (Projectile.frame >= Main.projFrames[Projectile.type])
                    Projectile.frame = 0;
            }

            // =========================================================
            // ✅ 스케일/알파 페이드 인을 "timeLeft 기반 절대값"으로 교체한다
            // - 누적형 Lerp 제거한다
            // - timeLeft가 멈추면 변화도 멈춘다
            // =========================================================

            float externalScale = Projectile.ai[0];
            if (externalScale <= 0f)
                externalScale = 1f;

            if (Projectile.timeLeft >= 570)
            {
                // 600 -> 570 (30틱) 동안 0~1 진행도다
                float progress = MathHelper.Clamp((600f - Projectile.timeLeft) / 30f, 0f, 1f);

                Projectile.scale = MathHelper.Lerp(externalScale * 0.1f, externalScale, progress);
                Projectile.alpha = (int)MathHelper.Lerp(255f, 0f, progress);
                Projectile.friendly = false;
            }
            else if (Projectile.timeLeft < 570 && Projectile.timeLeft > 60)
            {
                Projectile.friendly = true;
                Projectile.scale = externalScale;
                Projectile.alpha = 0;
            }
            else if ( Projectile.timeLeft <= 60)
            {
                Projectile.friendly = false;
                
            }



            // 회전 선회
            if (homing == false)
            {
                Projectile.velocity = Projectile.velocity.RotatedBy(MathHelper.ToRadians((0f + value) * faderot));
            }

            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

            // 회전 갱신
            if (Projectile.velocity.Length() > 0.1f)
                Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

            // 길이 보정 (속도 × 선회)
            float speed1 = Projectile.velocity.Length();
            float speedFactor = MathHelper.Clamp(0.7f + (speed1 / 20f) * 0.6f, 0.4f, 1.3f);

            float turnRate = Math.Abs(MathHelper.WrapAngle(Projectile.rotation - lastRotation));
            lastRotation = Projectile.rotation;
            float turnFactor = MathHelper.Clamp(1.0f - turnRate * 8f, 0.4f, 1.0f);

            Projectile.localAI[0] = speedFactor * turnFactor;

            // 호밍 로직 (0.5초 이후 활성화)
            if (Projectile.timeLeft <= 570&&Projectile.timeLeft>60)
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

                        float desiredRot = move.ToRotation();
                        float currentRot = Projectile.velocity.ToRotation();
                        float turnDiff = Math.Abs(MathHelper.WrapAngle(desiredRot - currentRot));

                        float maxTurn = 1.0f;
                        float minBoost = 1.00f;
                        float maxBoost = 1.1f;
                        float factor = MathHelper.Clamp(1f - (turnDiff / maxTurn), 0f, 1f);
                        float boost = MathHelper.Lerp(minBoost, maxBoost, factor);

                        float turnrotate = baseTurn / boost;

                    
                        Projectile.velocity = (Projectile.velocity * (turnrotate - 1f) + move) / turnrotate;
                        Projectile.velocity *= boost;
                        
                    }
                }
                else if (Projectile.timeLeft >= 60 && Projectile.timeLeft <= 540 && target == null)
                {
                    Projectile.timeLeft = 60;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // =========================================================
            // ✅ 페이드 아웃을 "timeLeft 기반 절대값"으로 교체한다
            // - 누적 감소(fadecount -=) 제거한다
            // - timeLeft가 멈추면 fadeout도 멈춘다
            // =========================================================
            if (Projectile.timeLeft <= 60)
            {
                fadeout = Projectile.timeLeft / 60f;
                fadecount = 255f * fadeout; // 외부 변수 호환용이다
            }
            else
            {
                fadeout = 1f;
                fadecount = 255f;
            }

            Texture2D bodyTex = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D headTex = ModContent.Request<Texture2D>("CAmod/Projectiles/fire_head").Value;
            Texture2D tailTex = ModContent.Request<Texture2D>("CAmod/Projectiles/fire_tail").Value;
            int frameHeight = bodyTex.Height / Main.projFrames[Projectile.type];
            Rectangle sourceRect = new Rectangle(0, frameHeight * Projectile.frame, bodyTex.Width, frameHeight);

            // 동일 anchor다
            Vector2 origin = new Vector2(bodyTex.Width / 2, frameHeight * 0.85f);
            Vector2 origin2 = new Vector2(bodyTex.Width / 2, frameHeight * 1f);
            float stretchFactor = Projectile.localAI[0];
            float growthFactor = Projectile.scale;

            Vector2 scaleBody = new Vector2(1f, stretchFactor) * growthFactor;
            Vector2 scaleHead = new Vector2(1f, 1f) * growthFactor;

            // 본체
            Main.spriteBatch.Draw(
                bodyTex,
                Projectile.Center - Main.screenPosition,
                sourceRect,
                Color.White * fadeout,
                Projectile.rotation,
                origin,
                scaleBody ,
                SpriteEffects.None,
                0.1f
            );

            // 잔상
            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition;
                float progress = (float)i / (Projectile.oldPos.Length - 1);

                // 뒤로 갈수록 투명해지는 컬러 설정
                Color c = GetGradientColor(progress) * (0.8f * (1f - progress));

                // 뒤로 갈수록 작아지는 배율 계산 (기존 스케일에 1.0 ~ 0.0 사이의 값을 곱함)
                float trailScale = 1f - progress/4f;

                Main.spriteBatch.Draw(
                    bodyTex,
                    drawPos,
                    sourceRect,
                    c * fadeout,
                    Projectile.rotation,
                    origin,
                    scaleBody * trailScale, // 스케일 적용
                    SpriteEffects.None,
                    0.1f
                );

                Main.spriteBatch.Draw(
                    headTex,
                    drawPos,
                    sourceRect,
                    c * fadeout,
                    Projectile.rotation,
                    origin,
                    scaleHead * trailScale, // 스케일 적용
                    SpriteEffects.None,
                    0.0f
                );
            }

            // 헤드
            Main.spriteBatch.Draw(
                headTex,
                Projectile.Center - Main.screenPosition,
                sourceRect,
                Color.White * fadeout,
                Projectile.rotation,
                origin,
                scaleHead ,
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

                    if (dragonType != null)
                    {
                        int dragonID = calamity.Find<ModBuff>(dragonType.Name).Type;
                        target.AddBuff(dragonID, 180);
                    }
                }
                catch { }
            }

            SpawnExplosionDust();

            float scale = Projectile.scale;
            float clampedScale = MathHelper.Clamp(scale, 0.6f, 1.0f);

            int dustCountMain = (int)(25 * clampedScale);
            int dustCountGlow = (int)(10 * clampedScale);
            float speedMult = 3f * clampedScale;
            float sizeMult = 1.2f * clampedScale;
            Vector2 moveDir = Projectile.velocity.SafeNormalize(Vector2.UnitX);

            float speedFactor = Projectile.velocity.Length() / 10f;
            speedFactor = MathHelper.Clamp(speedFactor, 0.8f, 5.5f);

            homing = false;

            if (Main.netMode != NetmodeID.Server)
            {
                Vector2 goreSource = Projectile.Center;
                int goreAmt = 5;
                Vector2 source = new Vector2(goreSource.X - 24f, goreSource.Y - 24f);

                for (int goreIndex = 0; goreIndex < goreAmt; goreIndex++)
                {
                    float velocityMult = 0.33f;
                    if (goreIndex < (goreAmt / 3))
                        velocityMult = 0.66f;
                    if (goreIndex >= (2 * goreAmt / 3))
                        velocityMult = 1f;

                    int type = Main.rand.Next(61, 64); // 스모크 고어 ID다
                    int smoke = Gore.NewGore(Projectile.GetSource_Death(), source, default, type, 1f);
                    Gore gore = Main.gore[smoke];

                    Vector2 goreDir = Main.rand.NextVector2Unit();
                    gore.velocity = goreDir * (2.5f * velocityMult);
                }
            }

            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
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

        private void SpawnExplosionDust()
        {
            for (int i = 0; i < 4; i++)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Smoke, 0f, 0f, 100, default, 1.5f);

                Main.dust[d].position =
                    Projectile.Center +
                    Vector2.UnitY.RotatedByRandom(Math.PI) *
                    Main.rand.NextFloat() *
                    Projectile.width * 0.5f;
            }

            for (int i = 0; i < 10; i++)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Torch, 0f, 0f, 200, default, 2.7f);

                Dust dust = Main.dust[d];
                dust.position =
                    Projectile.Center +
                    Vector2.UnitY.RotatedByRandom(Math.PI) *
                    Main.rand.NextFloat() *
                    Projectile.width * 0.5f;

                dust.noGravity = true;
                dust.velocity *= 3f;

                int d2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Torch, 0f, 0f, 100, default, 1.5f);

                Dust dust2 = Main.dust[d2];
                dust2.position = dust.position;
                dust2.velocity *= 2f;
                dust2.noGravity = true;
                dust2.fadeIn = 2.5f;
            }

            for (int i = 0; i < 5; i++)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Torch, 0f, 0f, 0, default, 2.7f);

                Dust dust = Main.dust[d];
                dust.position =
                    Projectile.Center +
                    Vector2.UnitX.RotatedByRandom(Math.PI)
                        .RotatedBy(Projectile.velocity.ToRotation()) *
                    Projectile.width * 0.5f;

                dust.noGravity = true;
                dust.velocity *= 3f;
            }

            for (int i = 0; i < 10; i++)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Smoke, 0f, 0f, 0, default, 1.5f);

                Dust dust = Main.dust[d];
                dust.position =
                    Projectile.Center +
                    Vector2.UnitX.RotatedByRandom(Math.PI)
                        .RotatedBy(Projectile.velocity.ToRotation()) *
                    Projectile.width * 0.5f;

                dust.noGravity = true;
                dust.velocity *= 3f;
            }
        }
    }
}
