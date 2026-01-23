using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace CAmod.Projectiles
{
    public class GlassCannonBonusHit : ModProjectile
    {
        private bool struck;
        // 중복 타격 방지 플래그다

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;

            Projectile.friendly = false;
            Projectile.hostile = false;
            // 엔진 기본 타격을 완전히 끈다

            Projectile.penetrate = -1;
            Projectile.timeLeft = 2;
            // 즉발 처리용 투사체다

            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            if (struck)
                return;

            struck = true;

            NPC target = FindTarget();
            if (target == null)
            {
                Projectile.Kill();
                return;
            }

            int damage = Projectile.damage;
            if (damage <= 0)
            {
                Projectile.Kill();
                return;
            }
            NPC.HitInfo hitInfo = new NPC.HitInfo
            {
                Damage = damage,
                Knockback = 0f,
                HitDirection = 0,
                Crit = false,
               HideCombatText = true,
               
            };
            // 이미 계산된 확정 피해를 담는다

            target.StrikeNPC(hitInfo);
            // 엔진 정식 타격 루트를 사용한다
            Main.player[Projectile.owner].addDPS(damage);
            CombatText.NewText(
                target.getRect(),
                Color.White,
                damage
               
            );
            // 고정 피해임을 강조하기 위해 흰색으로 출력한다

            Projectile.Kill();
        }

        private NPC FindTarget()
        {
            NPC closest = null;
            float minDist = 8f;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly)
                    continue;

                float dist = Vector2.Distance(npc.Center, Projectile.Center);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = npc;
                }
            }

            return closest;
        }
    }
}
