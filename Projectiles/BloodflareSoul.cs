using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CAmod.Projectiles
{
    public class BloodflareSoul : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
            ProjectileID.Sets.TrailCacheLength[Type] = 5; // 잔상 길이 설정한다
            ProjectileID.Sets.TrailingMode[Type] = 0; // 잔상 모드 0번 사용한다
        }

        public override void SetDefaults()
        {
            Projectile.width = 46;
            Projectile.height = 46;
            Projectile.alpha = 100;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.extraUpdates = 1;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 60*60;
            Projectile.usesLocalNPCImmunity = true; // 개별 면역 사용한다
            Projectile.localNPCHitCooldown = -1;    // 동일 NPC 1회만 타격한다
        }
        public override void OnSpawn(IEntitySource source)
        {
            Player owner = Main.player[Projectile.owner];

            Projectile.CritChance =
                (int)owner.GetTotalCritChance(Projectile.DamageType);
            // 플레이어의 최종 마법 크리 확률을 투사체에 복사한다
        }
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            int a = 0;
            // 프레임 애니메이션 처리한다
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 6)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame > 3)
                Projectile.frame = 0;

            // 붉은 더스트 생성한다
            int redDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                DustID.RedTorch, 0f, 0f, 0, default, 1f);
            Main.dust[redDust].velocity *= 0.1f;
            Main.dust[redDust].scale = 1.3f;
            Main.dust[redDust].noGravity = true;

            // 회전 방향을 속도 방향으로 맞춘다
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(90f);

            // 빛 추가한다
            Lighting.AddLight(Projectile.Center, 0.5f, 0.2f, 0.9f);

            if (owner.active && !owner.dead)
            {
                // 플레이어와 멀어지면 복귀한다
                if (Projectile.Distance(owner.Center) > 600f)
                {
                    Vector2 moveDirection = owner.Center - Projectile.Center; // 플레이어 방향 계산한다
                    if (moveDirection == Vector2.Zero)
                        moveDirection = Vector2.UnitY; // 0벡터 방지한다
                    moveDirection.Normalize(); // 방향 벡터 정규화한다

                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, moveDirection * 10f, 0.05f);
                    return;
                }

                // 가장 가까운 NPC 추적한다
                GodKiller.HomeInOnNPC(
    Projectile,
    true,
    200f,
    11f,
    20f,
    ref a
);
            }
            else
            {
                if (Projectile.timeLeft > 30)
                    Projectile.timeLeft = 30;
            }
        }

        // 가장 가까운 적 찾는다
       

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;

            int frameHeight = texture.Height / Main.projFrames[Type];
            Rectangle sourceRect = new Rectangle(
                0,
                frameHeight * Projectile.frame,
                texture.Width,
                frameHeight
            ); // 현재 프레임 영역 계산한다

            Vector2 origin = new Vector2(texture.Width / 2f, frameHeight / 2f);

            // 잔상 그린다
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                float alpha = (Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length;

                Main.EntitySpriteDraw(
                    texture,
                    drawPos,
                    sourceRect,
                    Color.White * alpha * 0.6f,
                    Projectile.rotation,
                    origin,
                    Projectile.scale,
                    SpriteEffects.None,
                    0
                );
            }

            // 본체 그린다
            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                sourceRect,
                lightColor,
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None,
                0
            );

            return false; // 기본 그리기 막는다
        }


        public override Color? GetAlpha(Color lightColor)
        {
            if (Projectile.timeLeft < 85)
            {
                byte b2 = (byte)(Projectile.timeLeft * 3);
                byte a2 = (byte)(100f * (b2 / 255f));
                return new Color(b2, b2, b2, a2);
            }
            return new Color(255, 255, 255, 100);
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.NPCDeath39, Projectile.position);

            Projectile.position = Projectile.Center;
            Projectile.width = Projectile.height = 110;
            Projectile.position -= new Vector2(Projectile.width / 2f);

            int constant = 36;
            for (int i = 0; i < constant; i++)
            {
                Vector2 velocity = Vector2.UnitX.RotatedBy(MathHelper.TwoPi * i / constant) * 6f;
                int dust = Dust.NewDust(Projectile.Center, 0, 0, DustID.RedTorch,
                    velocity.X, velocity.Y, 100, default, 2f);
                Main.dust[dust].noGravity = true;
            }

            Projectile.Damage(); // 폭발 판정 발생시킨다
        }
    }
}
