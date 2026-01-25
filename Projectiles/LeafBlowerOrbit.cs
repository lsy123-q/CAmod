using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace CAmod.Projectiles
{
    public class LeafBlowerOrbit : ModProjectile
    {
        private float angle;
        private const int FadeInTime = 60 * 1;
        // 2초 페이드 인 시간이다

        private const int ExpandTime = 60 * 3;
        // 3초 동안 반경 확장 시간이다

        private const float ExpandMultiplier = 2f;
        // 5초 후 반경이 2배가 된다
        int endFadeTime = 60;
        private static readonly float RotatePerTick = MathHelper.ToRadians(0.5f);
        // 1초에 30도 회전한다

        // 바닐라 Leaf Blower 스프라이트를 그대로 사용한다
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Leaf;

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;

            Projectile.timeLeft = 60 * 10;
            // 10초 동안 지속된다

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
            // 나뭇잎 스프라이트는 총 5프레임이다
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.frame = Main.rand.Next(5);
            // 생성 시 나뭇잎 시작 프레임을 0~4 중 랜덤으로 설정한다
        }

        private void ApplyLeafShieldLight()
        {
            float intensity = 1f;
            // 기본 광원 강도다
            int leafShieldTimer = 600 - Projectile.timeLeft;
            if (leafShieldTimer <= 60)
            {
                float t = leafShieldTimer / 60f;
                // 0.0 → 1.0 으로 선형 증가한다

                intensity *= MathHelper.Clamp(t, 0f, 1f);
            }

            if (leafShieldTimer >= 540)
            {
                float t = (600f - leafShieldTimer) / 60f;
                // 1.0 → 0.0 으로 선형 감소한다

                intensity = MathHelper.Clamp(t, 0f, 1f);
            }

            Lighting.AddLight(
                Projectile.Center,
                0.15f * intensity,
                0.3f * intensity,
                0.15f * intensity
            );
            // 연두색 계열 광원을 플레이어 중심에 부여한다
        }
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            ApplyLeafShieldLight();
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 6)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;

                if (Projectile.frame >= 5)
                    Projectile.frame = 0;
            }
            float baseRadius = Projectile.ai[0];
            float dir = Projectile.ai[1];

            
            // 초당 30도로 회전한다
            float rotateSpeedMultiplier = 1f;
            if (Projectile.timeLeft < endFadeTime)
            {
                rotateSpeedMultiplier = 2f;
                // 페이드아웃 구간에서는 회전 속도를 2배로 증가시킨다
            }

            angle += RotatePerTick * dir * rotateSpeedMultiplier;
            // === 시간 진행 ===
            int elapsed = (60 * 10) - Projectile.timeLeft;
            // 생성 후 경과 시간이다

            
            float radius = baseRadius;

            // === 스케일 페이드 인 (0~2초) ===
            if (elapsed < FadeInTime)
            {
                Projectile.scale = elapsed / (float)FadeInTime;
                // 2초 동안 스케일이 0에서 1로 증가한다
            }
            else 
            {
                Projectile.scale = 1f;
            }
            if (Projectile.timeLeft < endFadeTime) {
                Projectile.scale = (float)(Projectile.timeLeft/(float)endFadeTime);
               
            }
            // === 반경 확장 (2~5초) ===
            if (elapsed >= FadeInTime && elapsed < FadeInTime + ExpandTime)
            {
                float t = (elapsed - FadeInTime) / (float)ExpandTime;
                radius = MathHelper.Lerp(baseRadius, baseRadius * ExpandMultiplier, t);
                // 3초 동안 반경이 선형으로 확장된다
            }
            else if (elapsed >= FadeInTime + ExpandTime)
            {
                radius = baseRadius * ExpandMultiplier;
                // 확장 완료 후에는 2배 반경을 유지한다
            }

            if (Projectile.timeLeft <= 60)
            {
                float t = 1f - (Projectile.timeLeft / 60f);
                // 0 → 1로 진행된다

                radius *= MathHelper.Lerp(1f, 1.5f, t);
                // 회전하는 반경 자체가 1.5배까지 커진다
            

            }

            float baseAngle = Projectile.localAI[0];
            float finalAngle = baseAngle + angle;

            Projectile.Center =
                owner.Center +
                finalAngle.ToRotationVector2() * radius;

            Projectile.rotation = finalAngle + MathHelper.PiOver2;


            

        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
           

           
            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            Rectangle frame = new Rectangle(
                0,
                frameHeight * Projectile.frame,
                texture.Width,
                frameHeight
            );

            Vector2 origin = frame.Size() * 0.5f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Color glowColor = new Color(171, 220, 82, 100);
            // 야광 연두색으로 덮어쓴다

            Main.spriteBatch.Draw(
                texture,
                drawPos,
                frame,
                glowColor,
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None,
                0f
            );

            return false;
            // 기본 Draw를 막고 우리가 직접 그린다
        }

        
    }
}
