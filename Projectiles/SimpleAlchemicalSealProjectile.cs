
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using Luminance.Core.Graphics;

namespace CAmod.Projectiles
{
    public class SimpleAlchemicalSealProjectile : ModProjectile
    {
        private static ManagedShader SealShaderA;
        private static ManagedShader SealShaderB;
        private static Asset<Texture2D> SealTexture;

        private const int DrawSize = 200;
        private const float RenderScale = 1f;

        private const float Alpha = 1f;
        private const float RotationSpeed = 0f;

        // 공통 색상
        private static readonly Vector4 OuterRingColor =
            new Vector4(1f, 1f, 1f, 1f);

        private static readonly Vector4 InnerOuterRingColor =
            new Vector4(1f, 1f, 1f, 1f);

        private static readonly Vector4 StarColor =
            new Vector4(0.55f, 0.95f, 1f, 1f);

        private static readonly Vector4 NodeCircleColor =
            new Vector4(1f, 1f, 1f, 1f);

        private static readonly Vector4 MiniStarColor =
            new Vector4(0.55f, 0.95f, 1f, 1f);

        private static readonly Vector4 GlowColor =
            new Vector4(0f, 1f, 1f, 1f);

        public override string Texture =>
            "CAmod/Effects/SimpleAlchemicalTexture";

        public override void Load()
        {
            if (Main.dedServ)
                return;

            SealTexture = ModContent.Request<Texture2D>(
                "CAmod/Effects/SimpleAlchemicalTexture",
                AssetRequestMode.ImmediateLoad);
        }

        public override void Unload()
        {
            SealShaderA = null;
            SealShaderB = null;
            SealTexture = null;
        }

        public override void SetDefaults()
        {
            Projectile.width = DrawSize;
            Projectile.height = DrawSize;

            Projectile.timeLeft = 2;
            Projectile.penetrate = -1;

            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.friendly = false;
            Projectile.hostile = false;

            Projectile.netImportant = false;
            Projectile.hide = true;
        }

        public override void DrawBehind(
    int index,
    List<int> behindNPCsAndTiles,
    List<int> behindNPCs,
    List<int> behindProjectiles,
    List<int> overPlayers,
    List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }

        public override bool ShouldUpdatePosition()
        {
            return false;
        }

        public override void AI()
        {
            if (Projectile.owner < 0 ||
                Projectile.owner >= Main.maxPlayers)
            {
                Projectile.Kill();
                return;
            }

            Player player = Main.player[Projectile.owner];

            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }

            Projectile.Center = player.Center;
            Projectile.timeLeft = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ || SealTexture == null)
                return false;

            if (Projectile.owner < 0 ||
                Projectile.owner >= Main.maxPlayers)
            {
                return false;
            }

            Player player = Main.player[Projectile.owner];

            if (!player.active || player.dead)
                return false;

            if (!ShaderManager.TryGetShader(
                "CAmod.SimpleAlchemicalSeal_A",
                out ManagedShader shaderA))
            {
                Mod.Logger.Warn(
                    "[SimpleAlchemicalSeal] 셰이더 A를 찾을 수 없습니다: CAmod.SimpleAlchemicalSeal_A");

                return false;
            }

            if (!ShaderManager.TryGetShader(
                "CAmod.SimpleAlchemicalSeal_B",
                out ManagedShader shaderB))
            {
                Mod.Logger.Warn(
                    "[SimpleAlchemicalSeal] 셰이더 B를 찾을 수 없습니다: CAmod.SimpleAlchemicalSeal_B");

                return false;
            }

            SealShaderA = shaderA;
            SealShaderB = shaderB;

            Texture2D texture = SealTexture.Value;

            Vector2 drawPosition =
                player.Center - Main.screenPosition;

            Vector2 origin = new Vector2(
                texture.Width * 0.5f,
                texture.Height * 0.5f);

            SetShaderAParameters(shaderA);
            SetShaderBParameters(shaderB);

            Main.spriteBatch.End();

            Main.spriteBatch.Begin(
                SpriteSortMode.Immediate,
                BlendState.Additive,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                Main.GameViewMatrix.TransformationMatrix);

            // A: 바깥 원 + 중앙 도형
            shaderA.Apply();

            Main.spriteBatch.Draw(
                texture,
                drawPosition,
                null,
                Color.White,
                0f,
                origin,
                RenderScale,
                SpriteEffects.None,
                0f);

            // B: 꼭짓점 원 + 미니 별 + 내부 글로우
            shaderB.Apply();

            Main.spriteBatch.Draw(
                texture,
                drawPosition,
                null,
                Color.White,
                0f,
                origin,
                RenderScale,
                SpriteEffects.None,
                0f);

            Main.spriteBatch.End();

            Main.spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        private static void SetShaderAParameters(
            ManagedShader shader)
        {
            shader.TrySetParameter(
                "_OuterRingColor",
                OuterRingColor);

            shader.TrySetParameter(
                "_InnerOuterRingColor",
                InnerOuterRingColor);

            shader.TrySetParameter(
                "_StarColor",
                StarColor);

            shader.TrySetParameter(
                "_LineWidth",
                0.0246f);

            shader.TrySetParameter(
                "_Radius",
                1f);

            shader.TrySetParameter(
                "_GlowIntensity",
                5.06f);

            shader.TrySetParameter(
                "_Alpha",
                Alpha);

            shader.TrySetParameter(
                "_RotationSpeed",
                RotationSpeed);

            shader.TrySetParameter(
                "_OuterRingGap",
                0.439f);

            shader.TrySetParameter(
                "uTime",
                (float)Main.GlobalTimeWrappedHourly);
        }

        private static void SetShaderBParameters(
            ManagedShader shader)
        {
            shader.TrySetParameter(
                "_NodeCircleColor",
                NodeCircleColor);

            shader.TrySetParameter(
                "_MiniStarColor",
                MiniStarColor);

            shader.TrySetParameter(
                "_GlowColor",
                GlowColor);

            shader.TrySetParameter(
                "_LineWidth",
                0.0246f);

            shader.TrySetParameter(
                "_Radius",
                1f);

            shader.TrySetParameter(
                "_GlowIntensity",
                5.06f);

            shader.TrySetParameter(
                "_Alpha",
                Alpha);

            shader.TrySetParameter(
                "_RotationSpeed",
                RotationSpeed);

            shader.TrySetParameter(
                "_NodeSize",
                0.132f);

            shader.TrySetParameter(
                "_InwardGlowPower",
                3.88f);

            shader.TrySetParameter(
                "_InwardGlowStrength",
                2f);

            shader.TrySetParameter(
                "uTime",
                (float)Main.GlobalTimeWrappedHourly);
        }
    }
}
