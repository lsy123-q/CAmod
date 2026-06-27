using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace CAmod.Projectiles
{
    public class LeafBlowerOrbit : ModProjectile
    {
        private float angle;
        private const int FadeInTime = 60;
        private const int ExpandTime = 180;
        private const float ExpandMultiplier = 2f;
        private const int EndFadeTime = 60; // 페이드아웃 시간 (1초)

        private static readonly float RotatePerTick = MathHelper.ToRadians(0.5f);
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Leaf;
        private const float TrackingLerpSpeed = 0.25f;

        public override void SetDefaults()
        {
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.damage = 0;
        }

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 5;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }

            Projectile.Center = Vector2.Lerp(Projectile.Center, owner.Center, TrackingLerpSpeed);

            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 6)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % 5;
            }

            // [수정] 플레이어가 ai[1]을 1로 유지하는 동안은 timeLeft를 61로 고정하여 절대 죽지 않음 (무한 유지)
            // 플레이어가 토글을 끄면 ai[1]이 0이 되므로, 그때부터 자연스럽게 timeLeft가 60틱 동안 감소함
            bool isInfiniteMode = Projectile.ai[1] == 1f;
            if (isInfiniteMode)
            {
                Projectile.timeLeft = 61;
            }

            // 회전 속도 제어 (페이드아웃 구간 60틱 이하일 때 2배속)
            float rotateSpeedMultiplier = 1f;
            if (Projectile.timeLeft <= EndFadeTime)
            {
                rotateSpeedMultiplier = 2f;
            }
            angle += RotatePerTick * rotateSpeedMultiplier;

            // 스케일 페이드인 / 페이드아웃
            int elapsed = (int)Projectile.ai[0];
            if (elapsed < FadeInTime)
            {
                Projectile.scale = elapsed / (float)FadeInTime;
            }
            else if (Projectile.timeLeft <= EndFadeTime) // [수정] 꺼지는 도중(60틱 이하)일 때 축소
            {
                Projectile.scale = Projectile.timeLeft / (float)EndFadeTime;
            }
            else
            {
                Projectile.scale = 1f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            Vector2 origin = new Vector2(texture.Width * 0.5f, frameHeight * 0.5f);
            Color glowColor = new Color(171, 220, 82, 100);

            int elapsed = (int)Projectile.ai[0];

            DrawOrbitCircle(texture, frameHeight, origin, glowColor, elapsed, 100f, 1f);
            DrawOrbitCircle(texture, frameHeight, origin, glowColor, elapsed, 300f, -1f);

            return false;
        }

        private void DrawOrbitCircle(Texture2D texture, int frameHeight, Vector2 origin, Color glowColor, int elapsed, float baseRadius, float dir)
        {
            float radius = baseRadius;
            if (elapsed >= FadeInTime && elapsed < FadeInTime + ExpandTime)
            {
                float t = (elapsed - FadeInTime) / (float)ExpandTime;
                radius = MathHelper.Lerp(baseRadius, baseRadius * ExpandMultiplier, t);
            }
            else if (elapsed >= FadeInTime + ExpandTime)
            {
                radius = baseRadius * ExpandMultiplier;
            }

            // [수정] 무한 유지 모드가 해제되어 완전히 소멸하는 단계(60틱 이하)일 때 반경 확산 연출
            if (Projectile.timeLeft <= 60)
            {
                float t = 1f - (Projectile.timeLeft / 60f);
                radius *= MathHelper.Lerp(1f, 3.5f, t * t);
            }

            for (int i = 0; i < 10; i++)
            {
                float baseAngle = (MathHelper.TwoPi / 10f) * i;
                float finalAngle = baseAngle + (angle * dir);

                Vector2 drawPos = Projectile.Center + finalAngle.ToRotationVector2() * radius - Main.screenPosition;
                float leafRotation = finalAngle + MathHelper.PiOver2;

                int currentFrame = (Projectile.frame + i) % 5;
                Rectangle frame = new Rectangle(0, frameHeight * currentFrame, texture.Width, frameHeight);

                Main.spriteBatch.Draw(texture, drawPos, frame, glowColor, leafRotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            }
        }
    }
}