using System;

using Terraria.Audio;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace CAmod.Projectiles
{
    public class DivineRetributionSpear : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 7;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
            // 잔상 설정이다
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Magic;
            // 마법 투사체로 설정한다

            Projectile.extraUpdates = 1;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 420;
        }

        public override void AI()
        {
            float num953 = 25f * Projectile.ai[1];
            // 관성 계수다
            int a = 1;
            float scaleFactor12 = 5f * Projectile.ai[1];
            // 유도 보정 강도다

            float num954 = 1000f;
            // 플레이어와의 최대 거리다

            if (Projectile.velocity.X < 0f)
            {
                Projectile.spriteDirection = -1;
                Projectile.rotation = (float)Math.Atan2(-Projectile.velocity.Y, -Projectile.velocity.X);
            }
            else
            {
                Projectile.spriteDirection = 1;
                Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X);
            }

            Lighting.AddLight(Projectile.Center, 0.7f, 0.3f, 0f);
            // 발광 효과다

            Player owner = Main.player[Projectile.owner];

            if (owner.active && !owner.dead)
            {
                if (Projectile.Distance(owner.Center) > num954)
                {
                    Vector2 moveDirection = owner.Center - Projectile.Center;
                    if (moveDirection.LengthSquared() < 0.001f)
                        moveDirection = Vector2.UnitY;
                    else
                        moveDirection.Normalize();
                    Projectile.velocity =
                        (Projectile.velocity * (num953 - 1f) + moveDirection * scaleFactor12) / num953;
                    // 플레이어 쪽으로 복귀시킨다
                    return;
                }

                GodKiller.HomeInOnNPC(
                    Projectile,
                    true,
                    200f,
                    9f,
                    20f,
                    ref a);
                // 적을 추적한다
            }
            else
            {
                if (Projectile.timeLeft > 30)
                    Projectile.timeLeft = 30;
                // 주인이 사망했으면 빠르게 소멸시킨다
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = tex.Size() * 0.5f;

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                float fade = (Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length;

                Color color = lightColor * fade;
                color.A = 0;

                Main.spriteBatch.Draw(
                    tex,
                    drawPos,
                    null,
                    Color.White * fade,
                    Projectile.rotation,
                    origin,
                    Projectile.scale,
                    Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
                    0f
                );
            }

            return false;
            // 기본 투사체도 함께 그린다
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            try
            {
                int crush = ModContent.Find<ModBuff>("CalamityMod", "CrushDepth").Type;
                target.AddBuff(crush, 180);
                // CrushDepth 디버프를 3초 부여한다
            }
            catch { }

            try
            {
                int holy = ModContent.Find<ModBuff>("CalamityMod", "HolyFlames").Type;
                target.AddBuff(holy, 180);
                // HolyFlames 디버프를 3초 부여한다
            }
            catch { }
        }

        public override void Kill(int timeLeft)
        {
            Projectile.position = Projectile.Center;
            Projectile.width = Projectile.height = 96;
            Projectile.position -= new Vector2(Projectile.width / 2f, Projectile.height / 2f);
            // 폭발 판정 크기를 확장한다

            Projectile.maxPenetrate = -1;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            // 다단히트 판정을 설정한다

            Projectile.Damage();
            // 폭발 데미지를 준다

            SoundEngine.PlaySound(SoundID.Item74, Projectile.Center);
            // 폭발음을 재생한다

            for (int i = 0; i < 6; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 244);
            }

            for (int i = 0; i < 10; i++)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 244);
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity *= 3f;

                d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 244);
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity *= 2f;
            }
        }



    }
}
