using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.Audio; // 사운드 시스템 사용한다
namespace CAmod.Projectiles
{
    public class GlassCannonBonusHit : ModProjectile
    {
        private bool struck;
        // 중복 타격 방지 플래그다
        private static readonly SoundStyle BonusHitSound = new SoundStyle("CAmod/Sounds/bonushit")
        {
            Variants = new int[] { 1, 2, 3 }, // bonushit1,2,3 랜덤 재생
            MaxInstances = 12,
            Volume = 0.4f,
            Pitch = 0.4f
        };
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
            Projectile.DamageType = DamageClass.Magic;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            if (struck)
                return;



            struck = true;
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                Projectile.Kill();
                return;
            }

            int targetIndex = (int)Projectile.ai[0];
            // ai[0]에 저장된 타겟 id를 가져온다

            if (targetIndex < 0 || targetIndex >= Main.maxNPCs)
            {
                Projectile.Kill();
                return;
            }

            NPC target = Main.npc[targetIndex];

            if (!target.active)
            {
                Projectile.Kill();
                return;
            }
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

            Vector2 spawnPos = target.position + new Vector2(
                    Main.rand.NextFloat(target.width),
                    Main.rand.NextFloat(target.height)
                );

            // 고정 피해임을 강조하기 위해 흰색으로 출력한다
            for (int k = 0; k < 25; k++)
                {
                    Dust dust2 = Dust.NewDustPerfect(spawnPos, DustID.Electric, new Vector2(5f, 5f).RotatedByRandom(100) * Main.rand.NextFloat(0.05f, 0.8f));
                    dust2.scale = Main.rand.NextFloat(0.45f, 0.75f);
                dust2.noGravity = true;
                }
               
           
            

            Projectile.Kill();
        }

        
    }
}
