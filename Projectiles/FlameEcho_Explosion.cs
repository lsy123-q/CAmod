using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using System;

namespace CAmod.Projectiles
{
    public class FlameEcho_Explosion : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 2;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Magic;
        }

        public override void AI()
        {
        }

        

        

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            int buffTime = 180;

            // ===== 버프 처리 =====
            try
            {
                Mod calamity = ModLoader.GetMod("CalamityMod");
                if (calamity != null)
                {
                    ModBuff holyFlames = calamity.Find<ModBuff>("HolyFlames");
                    if (holyFlames != null)
                        target.AddBuff(holyFlames.Type, buffTime);
                    else
                        target.AddBuff(BuffID.OnFire, buffTime);
                }
                else
                {
                    target.AddBuff(BuffID.OnFire, buffTime);
                }
            }
            catch
            {
                target.AddBuff(BuffID.OnFire, buffTime);
            }

            // ===== 더스트 연출 =====
            float min = 4f;
            float max = 8f;

            Vector2 hitDir = new Vector2(Projectile.ai[0], Projectile.ai[1]);
            if (hitDir == Vector2.Zero)
            {
                hitDir = Main.rand.NextVector2Unit();
                min = 2f;
                max = 4f;
            }
            hitDir.Normalize();

            int dustCount = 10;
            for (int d = 0; d < dustCount; d++)
            {
                Vector2 vel = hitDir * Main.rand.NextFloat(min, max) + Main.rand.NextVector2Circular(2f, 2f);
                int dustType = Main.rand.NextBool() ? 244 : 246;

                int id = Dust.NewDust(target.Hitbox.TopLeft(), target.Hitbox.Width, target.Hitbox.Height, dustType, vel.X, vel.Y, 0, default, 1f);
                Main.dust[id].noGravity = true;
                Main.dust[id].scale = Main.rand.NextFloat(0.9f, 1.35f);
                Main.dust[id].fadeIn = 1.2f;
                Main.dust[id].rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            }
        }
    }
}