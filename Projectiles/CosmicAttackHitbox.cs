using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CAmod.Projectiles
{
    public class CosmicAttackHitbox : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 0;
            Projectile.height = 0;
            // 기본 히트박스는 의미 없게 만든다

            Projectile.friendly = true;
            Projectile.hostile = false;

            Projectile.penetrate = -1;
            Projectile.timeLeft = 1;
            // 짧게 유지한다

            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            // 레이저처럼 지속타격 가능하게 만든다
        }

        public override void AI()
        {
            // 위치 고정 (아무 의미 없음)
            Projectile.Center = new Vector2(Projectile.ai[0], Projectile.ai[1]);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 start = new Vector2(Projectile.ai[0], Projectile.ai[1]);
            Vector2 target = Main.MouseWorld;
            // 방향은 마우스 기준으로 잡는다

            Vector2 dir = target - start;
            if (dir == Vector2.Zero)
                return false;

            dir.Normalize();

            Vector2 end = start + dir * 5000f;
            start -= dir * 5000f;
            // 총 길이 10000px 만든다

            float collisionPoint = 0f;

            return Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(),
                targetHitbox.Size(),
                start,
                end,
                500f,
                ref collisionPoint
            );
            // 두께 1000px 레이저 판정이다
        }
    }
}