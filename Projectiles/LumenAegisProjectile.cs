using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using Luminance.Core.Graphics;
using CAmod.Players;

namespace CAmod.Projectiles
{
    public class LumenAegisProjectile : ModProjectile
    {
        private static ManagedShader LumenAegisShader;
        private static Asset<Texture2D> LumenAegisTexture;

        private const int DrawSize = 120;

        // 셰이더에 사용하는 기본 색상을 지정한다
        private static readonly Vector4 BaseColor =
            new Vector4(0.03f, 0.15f, 1.0f, 1.0f);

        // 셰이더에 사용하는 전기 색상을 지정한다
        private static readonly Vector4 ElectricColor =
            new Vector4(0.2f, 0.8f, 1.0f, 1.0f);

        // 원형 영역의 반지름을 지정한다
        private const float CircleRadius = 0.5f;

        // 원형 가장자리의 부드러운 정도를 지정한다
        private const float CircleSoftness = 0.0537f;

        // 노이즈가 시작되는 내부 반지름을 지정한다
        private const float NoiseInnerRadius = 0.397f;

        // 내부 노이즈 페이드 범위를 지정한다
        private const float NoiseInnerFade = 2.0f;

        // 노이즈가 끝나는 외부 반지름을 지정한다
        private const float NoiseOuterRadius = 1.0f;

        // 외부 노이즈 페이드 범위를 지정한다
        private const float NoiseOuterFade = 0.08f;

        // 전기 움직임 속도를 지정한다
        private const float ElectricSpeed = -2.59f;

        // 전기 노이즈의 크기를 지정한다
        private const float ElectricScale = 4.3f;

        // 전기 줄기의 두께를 지정한다
        private const float ElectricThickness = 0.339f;

        // 전기 줄기의 밝기를 지정한다
        private const float ElectricIntensity = 20.0f;

        // 첫 번째 나선의 빈도를 지정한다
        private const float SpiralFrequency = 7.9f;

        // 나선의 휘어지는 정도를 지정한다
        private const float SpiralStrength = 6.12f;

        // 일반 상태의 프레넬 지수를 지정한다
        private const float NormalFresnelPower = 5.0f;

        // 프레넬 방출 강도를 지정한다
        private const float FresnelStrength = 7.62f;

        // 피격 순간 사용할 프레넬 지수를 지정한다
        private const float HitFresnelPower = 0.1f;

        public override string Texture =>
            "CAmod/Effects/LumenAegisTexture";

        public override void Load()
        {
            if (Main.dedServ)
                return;

            LumenAegisTexture =
                ModContent.Request<Texture2D>(
                    "CAmod/Effects/LumenAegisTexture",
                    AssetRequestMode.ImmediateLoad);
        }

        public override void Unload()
        {
            LumenAegisShader = null;
            LumenAegisTexture = null;
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

            // 기본 투사체 그리기 순서에서 제외하고 DrawBehind로 넘긴다
            Projectile.hide = true;
        }

        // 투사체를 플레이어 및 일반 요소들보다 앞으로 레이어링한다
        public override void DrawBehind(
            int index,
            List<int> behindNPCsAndTiles,
            List<int> behindNPCs,
            List<int> behindProjectiles,
            List<int> overPlayers,
            List<int> overWiresUI)
        {
            overPlayers.Add(index);
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

            Player player =
                Main.player[Projectile.owner];

            if (!player.active ||
                player.dead)
            {
                Projectile.Kill();
                return;
            }

            LumenAegisPlayer lumenPlayer =
                player.GetModPlayer<LumenAegisPlayer>();

            // 보호막의 준비/활성/종료 상태가 아니면 투사체를 제거한다
            if (!lumenPlayer.CanKeepLumenAegisProjectile())
            {
                lumenPlayer.ClearLumenAegisProjectileIndex(
                    Projectile.whoAmI);

                Projectile.Kill();
                return;
            }

            // 투사체를 오너 플레이어의 중심에 고정한다
            Projectile.Center =
                player.Center;

            // 투사체가 계속 존재하도록 유지한다
            Projectile.timeLeft = 2;
        }

        public override bool PreDraw(
            ref Color lightColor)
        {
            if (Main.dedServ)
                return false;

            if (LumenAegisTexture == null)
                return false;

            if (Projectile.owner < 0 ||
                Projectile.owner >= Main.maxPlayers)
            {
                return false;
            }

            Player player =
                Main.player[Projectile.owner];

            if (!player.active ||
                player.dead)
            {
                return false;
            }

            LumenAegisPlayer lumenPlayer =
                player.GetModPlayer<LumenAegisPlayer>();

            if (!lumenPlayer.TryGetLumenAegisRenderData(
                out float alpha,
                out float fresnelPower,
                out float renderScale))
            {
                return false;
            }

            // Luminance가 직접 관리하는 PS 3.0 셰이더를 가져온다
            if (!ShaderManager.TryGetShader(
                "CAmod.ElectricShield",
                out ManagedShader shader))
            {
                return false;
            }

            LumenAegisShader = shader;

            Texture2D texture =
                LumenAegisTexture.Value;

            SetShaderParameters(
                shader,
                alpha,
                fresnelPower);

            Vector2 drawPosition =
                player.Center -
                Main.screenPosition;

            Vector2 origin =
                new Vector2(
                    texture.Width * 0.5f,
                    texture.Height * 0.5f);

            // 기존 SpriteBatch를 종료한다
            Main.spriteBatch.End();

            // 이지스 커스텀 렌더링 세션을 시작한다
            Main.spriteBatch.Begin(
                SpriteSortMode.Immediate,
                BlendState.Additive,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                Main.GameViewMatrix.TransformationMatrix);

            // Luminance가 관리하는 PS 3.0 Effect를 적용한다
            shader.Apply();

            Main.spriteBatch.Draw(
                texture,
                drawPosition,
                null,
                Color.White,
                0f,
                origin,
                renderScale,
                SpriteEffects.None,
                0f);

            Main.spriteBatch.End();

            // Terraria 기본 SpriteBatch 상태로 복구한다
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

        private static void SetShaderParameters(
            ManagedShader shader,
            float alpha,
            float fresnelPower)
        {
            shader.TrySetParameter(
                "BaseColor",
                BaseColor);

            shader.TrySetParameter(
                "ElectricColor",
                ElectricColor);

            shader.TrySetParameter(
                "Alpha",
                alpha);

            shader.TrySetParameter(
                "CircleRadius",
                CircleRadius);

            shader.TrySetParameter(
                "CircleSoftness",
                CircleSoftness);

            shader.TrySetParameter(
                "NoiseInnerRadius",
                NoiseInnerRadius);

            shader.TrySetParameter(
                "NoiseInnerFade",
                NoiseInnerFade);

            shader.TrySetParameter(
                "NoiseOuterRadius",
                NoiseOuterRadius);

            shader.TrySetParameter(
                "NoiseOuterFade",
                NoiseOuterFade);

            shader.TrySetParameter(
                "ElectricSpeed",
                ElectricSpeed);

            shader.TrySetParameter(
                "ElectricScale",
                ElectricScale);

            shader.TrySetParameter(
                "ElectricThickness",
                ElectricThickness);

            shader.TrySetParameter(
                "ElectricIntensity",
                ElectricIntensity);

            shader.TrySetParameter(
                "SpiralFrequency",
                SpiralFrequency);

            shader.TrySetParameter(
                "SpiralStrength",
                SpiralStrength);

            shader.TrySetParameter(
                "FresnelPower",
                fresnelPower);

            shader.TrySetParameter(
                "FresnelStrength",
                FresnelStrength);

            shader.TrySetParameter(
                "Time",
                (float)Main.GlobalTimeWrappedHourly);
        }
    }
}
