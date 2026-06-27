using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CAmod.Projectiles
{
    public class RabbitDamageProjectile : ModProjectile
    {
        public override string Texture =>
            "Terraria/Images/Projectile_1";

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;

            Projectile.friendly = true;
            Projectile.hostile = false;

            Projectile.penetrate = 1;
            Projectile.knockBack = 30f;
            Projectile.timeLeft = 2;

            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.DamageType = DamageClass.Magic;

            Projectile.hide = true;

            // NPC 무적프레임 공유 막는다
            Projectile.usesLocalNPCImmunity = true;

            // 개별 무적 10틱
            Projectile.localNPCHitCooldown = 10;
        }
        public override void OnHitNPC
(
    NPC target,
    NPC.HitInfo hit,
    int damageDone
)
        {
            // 맞는 즉시 제거해서 중복타격 원천 차단한다
            Projectile.Kill();
        }
        public override void AI()
        {
            // 완전 투명 유지한다
            Projectile.alpha = 255;

            int targetIndex =
                (int)Projectile.ai[0];

            // 인덱스 이상하면 제거
            if (targetIndex < 0 ||
                targetIndex >= Main.maxNPCs)
            {
                Projectile.Kill();
                return;
            }

            NPC target =
                Main.npc[targetIndex];

            // 타겟 없으면 제거
            if (!target.active ||
                target.life <= 0)
            {
                Projectile.Kill();
                return;
            }

            // 타겟 중앙에 고정한다
            Projectile.Center =
                target.Center;

            // 최대 60프레임 유지
            if (Projectile.timeLeft < 60)
            {
                Projectile.timeLeft = 60;
            }
        }
    }
}