using CAmod.Particles;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CAmod.Projectiles
{
    public class Asteroid : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.extraUpdates = 2;
        }

        public override void AI()
        {
            if (Projectile.position.Y > Main.player[Projectile.owner].position.Y - 300f)
            {
                Projectile.tileCollide = true;
            }
            if ((double)Projectile.position.Y < Main.worldSurface * 16.0)
            {
                Projectile.tileCollide = true;
            }
            Projectile.scale = Projectile.ai[1];
            Projectile.rotation += Projectile.velocity.X * 2f;
            EmitFlightParticles();
        }

        private void EmitFlightParticles()
        {
            if (Main.dedServ) return;

            Vector2 position = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitY) * 10f;
            Vector2 side = Projectile.velocity.RotatedBy(MathHelper.PiOver2);
            EmitStar(position + side, side * 0.33f + Projectile.velocity / 4f);
            EmitStar(position - side, -side * 0.33f + Projectile.velocity / 4f);
            Vector2 scatteredPosition = Projectile.position + new Vector2(
                Main.rand.NextFloat(Projectile.width), Main.rand.NextFloat(Projectile.height));
            EmitStar(scatteredPosition, Main.rand.NextVector2Circular(1f, 1f));
        }

        private static void EmitStar(Vector2 position, Vector2 velocity)
        {
            CosmicParticleSystem.Buffer.Create(new ParticleInfo(
                position: position.ToNumerics(),
                velocity: velocity.ToNumerics(),
                rotation: velocity.ToRotation(),
                scale: new System.Numerics.Vector2(12f, 12f),
                color: new Color(0x80, 0xFF, 0x56),
                duration: 60));
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item89, Projectile.Center);
            Projectile.position.X = Projectile.position.X + (float)(Projectile.width / 2);
            Projectile.position.Y = Projectile.position.Y + (float)(Projectile.height / 2);
            Projectile.width = (int)(128f * Projectile.scale);
            Projectile.height = (int)(128f * Projectile.scale);
            Projectile.position.X = Projectile.position.X - (float)(Projectile.width / 2);
            Projectile.position.Y = Projectile.position.Y - (float)(Projectile.height / 2);
            for (int i = 0; i < 4; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 1.5f);
            }
            for (int j = 0; j < 8; j++)
            {
                int killDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.TerraBlade, 0f, 0f, 100, default, 2.5f);
                Main.dust[killDust].noGravity = true;
                Main.dust[killDust].velocity *= 3f;
                killDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.TerraBlade, 0f, 0f, 100, default, 1.5f);
                Main.dust[killDust].velocity *= 2f;
                Main.dust[killDust].noGravity = true;
            }
            if (Main.netMode != NetmodeID.Server)
            {
                for (int k = 0; k < 2; k++)
                {
                    int gored = Gore.NewGore(Projectile.GetSource_Death(), Projectile.position + new Vector2((float)(Projectile.width * Main.rand.Next(100)) / 100f, (float)(Projectile.height * Main.rand.Next(100)) / 100f) - Vector2.One * 10f, default, Main.rand.Next(61, 64), 1f);
                    Gore gore = Main.gore[gored];
                    gore.velocity *= 0.3f;
                    gore.velocity.X += (float)Main.rand.Next(-10, 11) * 0.05f;
                    gore.velocity.Y += (float)Main.rand.Next(-10, 11) * 0.05f;
                }
            }
            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.localAI[1] = -1f;
                Projectile.maxPenetrate = 0;
                Projectile.Damage();
            }
            for (int l = 0; l < 3; l++)
            {
                int dustType = 107;
                int exploding = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dustType, 2.5f * (float)Projectile.direction, -2.5f, 0, default, 1f);
                Main.dust[exploding].alpha = 200;
                Main.dust[exploding].velocity *= 2.4f;
                Main.dust[exploding].scale += Main.rand.NextFloat();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;

            Vector2 origin = texture.Size() / 2f;

            // 잔상 직접 루프 구현한다
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;

                float progress = (float)(Projectile.oldPos.Length - i) / Projectile.oldPos.Length;
                Color color = lightColor * progress * 0.6f; // 뒤로 갈수록 점점 흐려지게 한다

                Main.spriteBatch.Draw(
                    texture,
                    drawPos,
                    null,
                    color,
                    Projectile.rotation,
                    origin,
                    Projectile.scale,
                    SpriteEffects.None,
                    0f
                );
            }

            // 본체를 마지막에 그린다
            Main.spriteBatch.Draw(
                texture,
                Projectile.Center - Main.screenPosition,
                null,
                lightColor,
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None,
                0f
            );

            return false; // 기본 드로우는 막는다
        }

    }
}
