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
    public class GodKiller : ModProjectile
    {
        private int defExtraUpdates = -1;
        // 칼라미티 defExtraUpdates 대체 변수다

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            // 잔상 모드다
        }

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 46;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.alpha = 255;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.DamageType = DamageClass.Magic;

            // 매직 무기 판정이다
        }
        public override void OnSpawn(IEntitySource source)
        {
            Player player = Main.player[Projectile.owner];

            Projectile.CritChance =
                (int)player.GetTotalCritChance(Projectile.DamageType);
            // 플레이어의 최종 크리 확률을 투사체에 복사한다
        }
        public override void AI()
        {
            if (Projectile.alpha > 0)
                Projectile.alpha -= 5;

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (Projectile.owner == Main.myPlayer && Projectile.timeLeft <= 3)
            {
                Projectile.tileCollide = false;
                Projectile.ai[1] = 0f;
                Projectile.alpha = 255;
                Projectile.position.X = Projectile.position.X + (float)(Projectile.width / 2);
                Projectile.position.Y = Projectile.position.Y + (float)(Projectile.height / 2);
                Projectile.width = 100;
                Projectile.height = 100;
                Projectile.position.X = Projectile.position.X - (float)(Projectile.width / 2);
                Projectile.position.Y = Projectile.position.Y - (float)(Projectile.height / 2);
                Projectile.knockBack = 5f;
            }
            else
            {
                if (Math.Abs(Projectile.velocity.X) >= 2f || Math.Abs(Projectile.velocity.Y) >= 2f)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        float shortXVel = 0f;
                        float shortYVel = 0f;
                        if (i == 1)
                        {
                            shortXVel = Projectile.velocity.X * 0.5f;
                            shortYVel = Projectile.velocity.Y * 0.5f;
                        }
                        int dusting = Dust.NewDust(new Vector2(Projectile.position.X + 3f + shortXVel, Projectile.position.Y + 3f + shortYVel) - Projectile.velocity * 0.5f, Projectile.width - 8, Projectile.height - 8, DustID.ShadowbeamStaff, 0f, 0f, 100, default, 1f);
                        Main.dust[dusting].scale *= 1f + (float)Main.rand.Next(5) * 0.1f;
                        Main.dust[dusting].velocity *= 0.2f;
                        Main.dust[dusting].noGravity = true;
                        dusting = Dust.NewDust(new Vector2(Projectile.position.X + 3f + shortXVel, Projectile.position.Y + 3f + shortYVel) - Projectile.velocity * 0.5f, Projectile.width - 8, Projectile.height - 8, DustID.Butterfly, 0f, 0f, 100, default, 0.1f);
                        Main.dust[dusting].fadeIn = 1f + (float)Main.rand.Next(5) * 0.1f;
                        Main.dust[dusting].velocity *= 0.05f;
                    }
                }
            }
            HomeInOnNPC(Projectile, true, 1200f, 12f, 20f, ref defExtraUpdates);
        }

        // ─────────────────────────────
        // 칼라미티 HomeInOnNPC 재현 함수
        // ─────────────────────────────
        private static void HomeInOnNPC(
            Projectile projectile,
            bool ignoreTiles,
            float distanceRequired,
            float homingVelocity,
            float inertia,
            ref int defaultExtraUpdates
        )
        {
            if (!projectile.friendly)
                return;
            // 아군 투사체가 아니면 처리하지 않는다

            if (defaultExtraUpdates == -1)
                defaultExtraUpdates = projectile.extraUpdates;
            // 기본 extraUpdates를 저장한다

            Vector2 destination = projectile.Center;
            bool locatedTarget = false;

            float npcDistCompare = 25000f;
            int index = -1;

            foreach (NPC n in Main.ActiveNPCs)
            {
                float extraDistance = (n.width * 0.5f) + (n.height * 0.5f);

                if (!n.CanBeChasedBy(projectile, false))
                    continue;

                if (!projectile.WithinRange(n.Center, distanceRequired + extraDistance))
                    continue;

                float currentNPCDist = Vector2.Distance(n.Center, projectile.Center);

                if (currentNPCDist < npcDistCompare &&
                    (ignoreTiles || Collision.CanHit(
                        projectile.Center, 1, 1,
                        n.Center, 1, 1)))
                {
                    npcDistCompare = currentNPCDist;
                    index = n.whoAmI;
                }
            }

            if (index != -1)
            {
                destination = Main.npc[index].Center;
                locatedTarget = true;
            }

            if (locatedTarget)
            {
                projectile.extraUpdates = defaultExtraUpdates + 1;
                // 유도 중 가속을 건다

                Vector2 homeDirection =
                    (destination - projectile.Center).SafeNormalize(Vector2.UnitY);

                projectile.velocity =
                    (projectile.velocity * inertia + homeDirection * homingVelocity)
                    / (inertia + 1f);
                // 칼라미티와 동일한 관성 보간이다
            }
            else
            {
                projectile.extraUpdates = defaultExtraUpdates;
                // 타겟이 없으면 복구한다
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float fade = 1f;
            if (Projectile.timeLeft <= 3)
            {
                fade = 0f;

            }
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Color color = lightColor * ((Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length);

                Main.EntitySpriteDraw(
                    TextureAssets.Projectile[Projectile.type].Value,
                    drawPos,
                    null,
                    color*fade,
                    Projectile.rotation,
                    Projectile.Size / 2f,
                    Projectile.scale,
                    SpriteEffects.None,
                    0
                );
            }
            // 수동 잔상 렌더링이다

            return true;
        }
        public override void PostDraw(Color lightColor)
        {
            float fade = 1f;
            if (Projectile.timeLeft <= 3)
            {
                fade = 0f;

            }

            Vector2 origin = new Vector2(11f, 23f);
            Main.EntitySpriteDraw(ModContent.Request<Texture2D>("CAmod/Projectiles/GodKillerGlow").Value, 
                Projectile.Center - Main.screenPosition, null, Color.White * fade, Projectile.rotation, origin, 1f, SpriteEffects.None, 0);
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item89, Projectile.position);
            for (int j = 0; j < 5; j++)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Butterfly, 0f, 0f, 100, default, 1.5f);
                Main.dust[dust].velocity *= 3f;
                if (Main.rand.NextBool())
                {
                    Main.dust[dust].scale = 0.5f;
                    Main.dust[dust].fadeIn = 1f + (float)Main.rand.Next(10) * 0.1f;
                }
            }
            for (int k = 0; k < 10; k++)
            {
                int dust2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.ShadowbeamStaff, 0f, 0f, 100, default, 2f);
                Main.dust[dust2].noGravity = true;
                Main.dust[dust2].velocity *= 5f;
                dust2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.ShadowbeamStaff, 0f, 0f, 100, default, 1.5f);
                Main.dust[dust2].velocity *= 2f;
            }
        }


    }
}
