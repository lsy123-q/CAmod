using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
namespace CAmod.Projectiles
{
    public class ArcaneHookDamage : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 1; // 2틱만 존재한다
            Projectile.alpha = 255; // 완전 투명이다
            Projectile.damage = 50;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.usesLocalNPCImmunity = true; // 이 투사체만의 무적프레임을 사용한다
            Projectile.localNPCHitCooldown = -1;    // 같은 틱 중복타격을 막는다

        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];

            int hitDirection = (target.Center.X < player.Center.X) ? -1 : 1;
            // 플레이어 기준 바깥 방향으로 넉백 방향을 결정한다

            target.velocity.X = hitDirection * hit.Knockback;

            try
            {
                Mod calamity = ModLoader.GetMod("CalamityMod");
                if (calamity != null)
                {
                    ModBuff CrushDepth = calamity.Find<ModBuff>("CrushDepth");
                    ModBuff ArmorCrunch = calamity.Find<ModBuff>("ArmorCrunch");
                    if (CrushDepth != null)
                        target.AddBuff(CrushDepth.Type, 180);
                    if (ArmorCrunch != null)
                        target.AddBuff(ArmorCrunch.Type, 180);
                }
               
            }
            catch
            {
                target.AddBuff(BuffID.OnFire, 180);
            }


        }
        public override void AI()
        {
            int targetIndex = (int)Projectile.ai[0];

           

            NPC target = Main.npc[targetIndex];

            for (int i = 0; i < 3; i++)
            {
                int d = Dust.NewDust(
    target.position,          // NPC 히트박스 좌상단이다
    target.width,             // NPC 전체 가로 크기이다
    target.height,            // NPC 전체 세로 크기이다
    DustID.Blood,             // 피 더스트이다
    Main.rand.NextFloat(-2.5f, 2.5f),
    Main.rand.NextFloat(-2.5f, 2.5f),
    100,
    default,
    1.2f
);

                Main.dust[d].noGravity = false; // 피는 약간 떨어지게 둔다
            }

            Projectile.Center = target.Center; // 항상 대상 중심에 붙는다
        }
    }
}