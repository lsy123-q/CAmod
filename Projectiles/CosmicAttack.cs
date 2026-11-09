using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.Utilities;
using XPT.Core.Audio.MP3Sharp.Decoding.Decoders.LayerIII;
using static System.Net.Mime.MediaTypeNames;
using CAmod.Players;
using Steamworks;
using Terraria.ID;
namespace CAmod.Projectiles
{
    public class CosmicAttack : ModProjectile
    {
        private bool go = false;
        private const float MaximumHitboxWidth = 500f;
        private const float CoreThickness = 0.085f;
        private const float BeamRenderWidth = MaximumHitboxWidth / CoreThickness;
        

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            // 히트박스는 작게 유지한다
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;

            Projectile.penetrate = -1;
            Projectile.timeLeft = 75;
            // 지속시간이다
            Projectile.usesLocalNPCImmunity = true;
            // 이 투사체만의 독립적인 무적프레임을 사용한다

            Projectile.localNPCHitCooldown = 75;
            // 한 번 맞으면 75프레임 동안 이 투사체에만 면역이 된다
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.extraUpdates = 1;
            // 부드럽게 만든다
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // ai[2] is zero until the first hit, then stores that NPC's index + 1.
            // Never choose another enemy, even if the first one dies.
            if (Projectile.ai[2] == 0f)
            {
                Projectile.ai[2] = target.whoAmI + 1;
                Projectile.Center = target.Center;
                Projectile.netUpdate = true;
            }
            


            target.AddBuff(ModContent.BuffType<global::CalamityMod.Buffs.DamageOverTime.GodSlayerInferno>(), 600);
        }
        public override void AI()
        {

            if (Projectile.localAI[0] == 0f && Projectile.localAI[1] == 0f)
                return;


            Vector2 start = new Vector2(Projectile.ai[0], Projectile.ai[1]);
            // 시작점 = 마우스

            Vector2 target = new Vector2(Projectile.localAI[0], Projectile.localAI[1]);
            // 목표점 = 천장 랜덤 위치

            Vector2 dir = target - start;

            if (dir.LengthSquared() < 0.001f)
                return;
            // 방향이 거의 없으면 계산 중단

            dir.Normalize();
            // 방향 계산한다

            if (Projectile.ai[2] == 0f)
            {
                Projectile.Center = start;
            }
            else
            {
                int firstTarget = (int)Projectile.ai[2] - 1;
                if (firstTarget >= 0 && firstTarget < Main.maxNPCs && Main.npc[firstTarget].active)
                    Projectile.Center = Main.npc[firstTarget].Center;
                // If it dies, keep the last center rather than acquiring a new target.
            }
            Projectile.velocity = Vector2.Zero;
            // 위치 고정한다

            Projectile.rotation = dir.ToRotation() + MathHelper.PiOver2;
            // 스프라이트가 세로 기준이라 +90도 보정한다




        }
        public override void OnSpawn(IEntitySource source)
        {
            // Hyperdeath Rift Scepter uses 3f, decaying by 0.185 per frame.
            // 7.5f uses Calamity's standard screenshake decay.
            if (!Main.dedServ && Main.LocalPlayer.Distance(Projectile.Center) < 1600f)
                global::CalamityMod.CalamityUtils.SetScreenshake(Main.LocalPlayer, 7.5f);

            SoundEngine.PlaySound(new SoundStyle("CAmod/Sounds/DoGLaserWallBigAttack2"), Projectile.Center);
            // 투사체 생성 순간 원하는 사운드 재생한다

            
            // 플레이어를 Y축으로 1픽셀 위로 띄운다

            Projectile.localAI[0] = Projectile.Center.X;
            Projectile.localAI[1] = Projectile.Center.Y;
            // 초기값이라도 강제로 넣는다
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // The shader is fully hidden during the final 10 updates.
            if (Projectile.timeLeft <= 10)
                return false;

            Vector2 start = new Vector2(Projectile.ai[0], Projectile.ai[1]);
            // 레이저 기준점이다

            Vector2 target = new Vector2(Projectile.localAI[0], Projectile.localAI[1]);
            // 조준 기준점이다

            Vector2 dir = target - start;
            // 방향 벡터를 구한다

            if (dir == Vector2.Zero)
                return false;
            // 방향이 없으면 판정도 만들 수 없다

            dir.Normalize();
            // 방향을 정규화한다

            start = Projectile.Center;
            Vector2 end = start + dir * 5000f;
            // 커서 위쪽으로 5000px 뻗는다

            start -= dir * 5000f;
            // 커서 아래쪽으로도 5000px 당겨서 총 길이 10000px로 만든다

            float collisionPoint = 0f;
            // 충돌 위치 저장용 변수다

            float fade = Projectile.timeLeft / 75f;
            // 코어가 줄어드는 시간 비율이다

            float scaleX = MathHelper.Lerp(0f, 1f, fade * fade);
            // 코어 X축에 쓰는 축소 곡선을 그대로 가져온다

            float collisionWidth = MaximumHitboxWidth * scaleX;
            // 물리 판정 두께도 같은 속도로 줄어들게 한다

            return Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(),
                targetHitbox.Size(),
                start,
                end,
                collisionWidth,
                ref collisionPoint
            );
            // 코어 X축 축소 속도와 동일하게 판정 폭을 검사한다
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // The visual remains at the original cast position; only collision tracks the first target.
            Vector2 start = new Vector2(Projectile.ai[0], Projectile.ai[1]);
            Vector2 target = new Vector2(Projectile.localAI[0], Projectile.localAI[1]);
            Vector2 direction = target - start;
            if (direction.LengthSquared() < 0.001f)
                return false;

            float fade = MathHelper.Clamp(Projectile.timeLeft / 75f, 0f, 1f);
            float lateFade = Projectile.timeLeft > 20 ? 1f
                : MathHelper.Clamp((Projectile.timeLeft - 10f) / 10f, 0f, 1f);
            if (lateFade <= 0f)
                return false;

            // These quads provide UV coordinates only. Both beam shapes are computed by the shaders.
            Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
            // CoreThickness * render width exactly matches the collision width.
            const float fullWidth = BeamRenderWidth;
            const float beamLength = 10000f;
            float width = fullWidth * fade * fade;
            Vector2 scale = new Vector2(width / pixel.Width, beamLength / pixel.Height);
            Vector2 origin = new Vector2(pixel.Width * 0.5f, pixel.Height * 0.5f);
            Vector2 drawPosition = start - Main.screenPosition;
            float rotation = direction.ToRotation() + MathHelper.PiOver2;

            Effect glow = global::CAmod.Systems.UltraCosmicDivideAssets.GlowShader.Value;
            Effect core = global::CAmod.Systems.UltraCosmicDivideAssets.CoreShader.Value;
            glow.Parameters["LineSpacing"].SetValue(0.11f);
            glow.Parameters["InnerRange"].SetValue(0.015f);
            glow.Parameters["OuterRange"].SetValue(0.075f);
            glow.Parameters["Intensity"].SetValue(1.6f);
            glow.Parameters["Opacity"].SetValue((float)Math.Sqrt(fade) * lateFade);
            // Follow the supplied Unity beam's sky -> purple -> pink lifetime palette.
            float colorProgress = (float)Math.Pow(1f - fade, 0.75f);
            Vector3 sky = new Vector3(0.5f, 0.9f, 1f);
            Vector3 purple = new Vector3(0.6f, 0.2f, 1f);
            Vector3 pink = new Vector3(1f, 0.3f, 0.8f);
            Vector3 beamColor = colorProgress < 0.5f
                ? Vector3.Lerp(sky, purple, colorProgress * 2f)
                : Vector3.Lerp(purple, pink, (colorProgress - 0.5f) * 2f);
            glow.Parameters["BeamColor"].SetValue(beamColor);
            core.Parameters["Thickness"].SetValue(CoreThickness);
            core.Parameters["Blur"].SetValue(0.007f);
            core.Parameters["BlurPower"].SetValue(1f);
            // Blend 25% of the original delayed fade with 75% of the early-fast fade.
            float coreFadeProgress = MathHelper.Clamp((75f - Projectile.timeLeft) / 65f, 0f, 1f);
            float earlyCoreFade = 1f - (float)Math.Sqrt(coreFadeProgress);
            float originalCoreFade = MathHelper.SmoothStep(0f, 1f,
                MathHelper.Clamp(fade / 0.15f, 0f, 1f)) * lateFade;
            core.Parameters["Opacity"].SetValue(MathHelper.Lerp(originalCoreFade, earlyCoreFade, 0.75f));

            glow.CurrentTechnique = glow.Techniques["CosmicDivideGlow"];
            Main.spriteBatch.End();
            try
            {
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
                    SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                    glow, Main.GameViewMatrix.TransformationMatrix);
                Main.spriteBatch.Draw(pixel, drawPosition, null, Color.White, rotation,
                    origin, scale, SpriteEffects.None, 0f);
                Main.spriteBatch.End();

                // Two additive glow layers with identical geometry, palette and opacity.
                glow.CurrentTechnique = glow.Techniques["CosmicDivideGlowAdditive"];
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                    SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                    glow, Main.GameViewMatrix.TransformationMatrix);
                Main.spriteBatch.Draw(pixel, drawPosition, null, Color.White, rotation,
                    origin, scale, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(pixel, drawPosition, null, Color.White, rotation,
                    origin, scale, SpriteEffects.None, 0f);
                Main.spriteBatch.End();

                // Two black core layers alpha blended over both additive glow layers.
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
                    SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                    core, Main.GameViewMatrix.TransformationMatrix);
                Main.spriteBatch.Draw(pixel, drawPosition, null, Color.White, rotation,
                    origin, scale, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(pixel, drawPosition, null, Color.White, rotation,
                    origin, scale, SpriteEffects.None, 0f);
                Main.spriteBatch.End();
            }
            finally
            {
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer,
                    null, Main.GameViewMatrix.TransformationMatrix);
            }
            return false;
        }
    }
}
