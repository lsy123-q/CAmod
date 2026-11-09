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

            bool fadingOut = Projectile.ai[2] > 0f;
            bool isInfiniteMode = Projectile.ai[1] == 1f && !fadingOut;
            if (isInfiniteMode)
                Projectile.timeLeft = EndFadeTime + 1;

            // End fade takes precedence even when the original fade-in has not finished.
            if (!isInfiniteMode && Projectile.timeLeft <= EndFadeTime && !fadingOut)
            {
                BeginFadeOut();
                fadingOut = true;
            }

            angle += RotatePerTick * (fadingOut ? 2f : 1f);
            if (fadingOut)
            {
                Projectile.timeLeft = Math.Min(Projectile.timeLeft, EndFadeTime);
                float remaining = MathHelper.Clamp(Projectile.timeLeft / (float)EndFadeTime, 0f, 1f);
                Projectile.scale = (Projectile.ai[2] - 1f) * MathHelper.SmoothStep(0f, 1f, remaining);
            }
            else
            {
                int elapsed = (int)Projectile.ai[0];
                Projectile.scale = MathHelper.Clamp(elapsed / (float)FadeInTime, 0f, 1f);
            }
        }

        public void BeginFadeOut()
        {
            if (Projectile.ai[2] > 0f)
                return;
            // Encode the current scale plus one, so zero remains the non-fading state.
            // ai[2] is synchronized, keeping early-cancel scale consistent on other clients.
            Projectile.ai[2] = 1f + MathHelper.Clamp(Projectile.scale, 0f, 1f);
            Projectile.ai[1] = 0f;
            Projectile.timeLeft = Math.Min(Projectile.timeLeft, EndFadeTime);
            Projectile.netUpdate = true;
        }

        public static float GetRadiusMultiplier(int elapsed, int timeLeft)
        {
            float expansion = MathHelper.Clamp((elapsed - FadeInTime) / (float)ExpandTime, 0f, 1f);
            float multiplier = MathHelper.SmoothStep(1f, ExpandMultiplier, expansion);
            if (timeLeft <= EndFadeTime)
            {
                float endProgress = MathHelper.Clamp(1f - timeLeft / (float)EndFadeTime, 0f, 1f);
                multiplier *= MathHelper.SmoothStep(1f, 3.5f, endProgress);
            }
            return multiplier;
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
            float radius = baseRadius * GetRadiusMultiplier(elapsed, Projectile.timeLeft);

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