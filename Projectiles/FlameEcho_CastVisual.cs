using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;

namespace CAmod.Projectiles
{
    public class FlameEcho_CastVisual : ModProjectile
    {
        private float aimRotation;
        private bool rotationInitialized = false;

        public override void SetDefaults()
        {
            Projectile.width = 130;
            Projectile.height = 130;
            Projectile.timeLeft = 20;
            // 시전 순간만 보여주는 투사체다

            Projectile.penetrate = -1;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.DamageType = DamageClass.Magic;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Vector2 aimDir = Main.MouseWorld - player.MountedCenter;

            if (!rotationInitialized && aimDir != Vector2.Zero)
            {
                aimRotation = aimDir.ToRotation();
                rotationInitialized = true;
                // 최초 1회만 조준 각도를 고정한다
            }

            Projectile.Center = player.itemLocation;
            // 아이템을 들고 있는 위치에 고정한다

            Projectile.rotation = aimRotation;
            Projectile.direction = aimRotation.ToRotationVector2().X > 0f ? 1 : -1;
            Projectile.spriteDirection = Projectile.direction;

            if (player.dashDelay < 0)
            {
                if (Math.Sign(player.velocity.X) != 0)
                {
                    Projectile.spriteDirection = Math.Sign(player.velocity.X);
                    // 대시 중에는 무조건 실제 이동 방향을 시각 기준으로 삼는다
                }
            }

        }

        public override void DrawBehind(
            int index,
            List<int> behindNPCsAndTiles,
            List<int> behindNPCs,
            List<int> behindProjectiles,
            List<int> overPlayers,
            List<int> overWiresUI)
        {
            overPlayers.Add(index);
            // 글로우 투사체를 플레이어보다 앞에 그리게 한다
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D baseTex = ModContent.Request<Texture2D>(
                "CAmod/Items/Weapons/FlameEcho").Value;

            Texture2D glowTex = ModContent.Request<Texture2D>(
                "CAmod/Items/Weapons/FlameEcho_Glow").Value;

            Vector2 pos = Projectile.Center - Main.screenPosition;

            Vector2 origin = new Vector2(0f, baseTex.Height);
            // 손잡이 기준 원점이다

            float rotation = aimRotation - MathHelper.ToRadians(135f) + MathHelper.Pi;
            // 기본 회전 각도 계산이다

            SpriteEffects fx = SpriteEffects.None;

            if (Projectile.direction == -1)
            {
                fx = SpriteEffects.FlipVertically;
                // 추가 회전 보정 제거
            }

            Main.EntitySpriteDraw(
                glowTex,
                pos,
                null,
                Color.White,
                rotation,
                origin,
                Projectile.scale,
                fx,
                0
            );

            return false;
            // 기본 드로우를 완전히 대체한다
        }
    }
}
