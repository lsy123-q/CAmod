using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CAmod.Projectiles
{
    public class ArcaneHookProj : ModProjectile
    {
        // Asset<Texture2D>로 변경하여 비동기 로딩 지원
        public static Asset<Texture2D> ChainTex;
        private int hookedNPC = -1; // 붙잡은 몬스터 인덱스를 저장한다
        public override void SetStaticDefaults()
        {
            // 여기서 텍스처를 요청합니다
            ChainTex = ModContent.Request<Texture2D>("CAmod/Projectiles/ArcaneHookChain");
        }
       
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.GemHookAmethyst);
            Projectile.aiStyle = 7;
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true; // 몬스터와 충돌하게 한다
            Projectile.damage = 1; // 피해는 주지 않는다
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 2;
        }

        // Load() 메서드 제거 - SetStaticDefaults()로 대체

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 0.3f, 0.0f, 0.5f);
            Projectile.Center = Main.projectile[(int)Projectile.ai[0]].Center;

            

        }

        public override void OnSpawn(IEntitySource source)
        {
            Player player = Main.player[Projectile.owner];

            int core = Projectile.NewProjectile(
                source,
                player.Center,
                Vector2.Zero,
                ModContent.ProjectileType<ArcaneHookCore>(),
                0,
                0f,
                player.whoAmI
            );

            Projectile.ai[0] = core; // Core 인덱스를 저장한다
            Projectile.netUpdate = true; // 멀티 동기화한다
        }

        public override float GrappleRange()
        {
            return 1360f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            Vector2 center = Projectile.Center;
            float angle = Projectile.AngleTo(player.MountedCenter) - MathHelper.PiOver2;

            // 텍스처 로드 확인한다
            if (ChainTex == null || !ChainTex.IsLoaded)
                return true; // 로드 안됐으면 기본 드로우 사용한다

            Texture2D chainTexture = ChainTex.Value;

            // 갈고리 염료 슬롯 shader이다
            int shader = player.cGrapple;

            // ✅ 염료 셰이더 적용하려면 Immediate로 다시 Begin 해야 한다
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(
                SpriteSortMode.Immediate,
                BlendState.AlphaBlend,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                RasterizerState.CullCounterClockwise,
                null,
                Main.GameViewMatrix.TransformationMatrix
            );

            // 현재 염료 셰이더를 적용한다
            if (shader != 0)
                GameShaders.Armor.Apply(shader, Projectile, null); // 갈고리 염료를 투사체 드로우에 적용한다

            // =========================
            // 🔗 체인 드로우
            // =========================
            while (true)
            {
                float dist = (player.MountedCenter - center).Length();

                if (dist < chainTexture.Height + 1f || float.IsNaN(dist))
                    break;

                Vector2 dir = player.MountedCenter - center;
                float lenSq = dir.LengthSquared();
                if (lenSq < 0.0001f)
                    break;

                dir *= 1f / MathF.Sqrt(lenSq);
                center += dir * chainTexture.Height;

                Color light = Lighting.GetColor((int)center.X / 16, (int)center.Y / 16);

                Main.EntitySpriteDraw(
                    chainTexture,
                    center - Main.screenPosition,
                    null,
                    light,
                    angle,
                    chainTexture.Size() / 2f,
                    1f,
                    SpriteEffects.None,
                    0
                );
            }

            // =========================
            // 🪝 코어 드로우
            // =========================
            Texture2D coreTex = TextureAssets.Projectile[Projectile.type].Value;
            Color coreLight = Lighting.GetColor((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16);

            Main.EntitySpriteDraw(
                coreTex,
                Projectile.Center - Main.screenPosition,
                null,
                coreLight,
                Projectile.rotation,
                coreTex.Size() / 2f,
                Projectile.scale,
                SpriteEffects.None,
                0
            );

            // ✅ 원래 상태로 복구한다 (다른 드로우 깨지지 않게)
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                RasterizerState.CullCounterClockwise,
                null,
                Main.GameViewMatrix.TransformationMatrix
            );

            return false; // 기본 드로우 막는다
        }

        public override void NumGrappleHooks(Player player, ref int numHooks)
        {
            numHooks = 1; // 주석과 실제 값 불일치 수정 필요 (40 vs "3개")
        }

        public override void GrappleRetreatSpeed(Player player, ref float speed)
        {
            speed = 10f;
        }

        public override void GrapplePullSpeed(Player player, ref float speed)
        {
            speed = 28f;
        }
    }
}