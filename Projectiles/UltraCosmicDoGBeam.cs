using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace CAmod.Projectiles
{
    public class UltraCosmicBeam : ModProjectile
    {
        private bool go = false;
        private int Time = 150; 
        public override void SetDefaults()
        {
            Projectile.width = 0;
            Projectile.height = 0;
            // 히트박스는 작게 유지한다
            Projectile.friendly = false;
         
            Projectile.DamageType = DamageClass.Magic;

            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            // 지속시간이다

            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.extraUpdates = 1;
            // 부드럽게 만든다
        }

        public override void AI()
        {
            if (Time > 0) {
                Time--;
            }

            Vector2 start = new Vector2(Projectile.ai[0], Projectile.ai[1]);
            // 시작점 = 마우스

            Vector2 target = new Vector2(Projectile.localAI[0], Projectile.localAI[1]);
            // 목표점 = 천장 랜덤 위치

            Vector2 dir = target - start;
            dir.Normalize();
            // 방향 계산한다

            Projectile.Center = start;
            Projectile.velocity = Vector2.Zero;
            // 위치 고정한다

            Projectile.rotation = dir.ToRotation() + MathHelper.PiOver2;
            // 스프라이트가 세로 기준이라 +90도 보정한다


            if (Projectile.timeLeft <= 120 && go == false)
            {
                // 150 → 60이면 90프레임 지난 시점이다

                int newProj = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<CosmicAttack>(),
                    Projectile.damage,
                    Projectile.knockBack,
                    Projectile.owner,
                    Projectile.ai[0],
                    Projectile.ai[1]
                );

                Main.projectile[newProj].localAI[0] = Projectile.localAI[0];
                Main.projectile[newProj].localAI[1] = Projectile.localAI[1];
                go = true;
                // 목표 지점에 CosmicAttack 생성한다
            }
            Player player = Main.player[Projectile.owner];
            Projectile.Center = player.Center;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>("CAmod/Projectiles/UltraCosmicBeam").Value;
            // 초대형 세로 텍스처다

            Vector2 start = new Vector2(Projectile.ai[0], Projectile.ai[1]);
            Vector2 target = new Vector2(Projectile.localAI[0], Projectile.localAI[1]);

            Vector2 dir = target - start;
            float length = dir.Length();
            dir.Normalize();
            // 길이 계산한다

            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f);
            // 아래쪽 기준으로 늘린다

            float scaleY = length / tex.Height;
            // 길이에 맞춰 세로 스케일 조정한다
            Color beamColor = new Color(220, 0, 255);

            // X축 페이드 (시간 기반)
            float fade = Time / 150f;
            // 처음은 두껍고 끝으로 갈수록 얇아진다

            float scaleX = (float)Math.Sqrt(MathHelper.Lerp(0f, 1f, fade));
            float scaleX2 = (float)Math.Sqrt(MathHelper.Lerp(0f, 1f, fade));
            // 프레임 기반 랜덤 노이즈다 (부드럽지 않다)
            float noise = Main.rand.NextFloat();

            // 시간 단위로 끊어서 갱신 (너무 빠른 깜빡임 방지)
            int step = (int)(Main.GlobalTimeWrappedHourly * 60f / 3f);
            // 3프레임마다 값이 바뀐다

            UnifiedRandom rand = new UnifiedRandom(step + Projectile.identity * 999);

            // 랜덤값 생성한다
            noise = rand.NextFloat();

            // 노이즈 강도 적용 (상당히 강하게)
            float noiseStrength = 0.05f;

            // scaleX에 노이즈 반영한다
            scaleX *= MathHelper.Lerp(1f - noiseStrength, 1f + noiseStrength, noise);

        
            Main.spriteBatch.End();

            // Additive 블렌딩으로 다시 시작한다
            Main.spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.Additive,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                Main.Rasterizer,
                null,
                Main.GameViewMatrix.TransformationMatrix
            );

            // 레이저 그린다
            Main.spriteBatch.Draw(
    tex,
    start - Main.screenPosition,
    null,
    beamColor * 0.7f* scaleX2,
    dir.ToRotation() + MathHelper.PiOver2,
    origin,
    new Vector2(scaleX, 1f),
    SpriteEffects.None,
    0f
);

            // 다시 원래 상태로 복구한다
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                Main.Rasterizer,
                null,
                Main.GameViewMatrix.TransformationMatrix
            );

            return false;
            // 기본 드로우 막는다
        }
    }
}