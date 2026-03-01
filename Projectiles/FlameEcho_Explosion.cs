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
            // 폭발 판정 반경이다

            Projectile.friendly = true;
            // 엔진 기본 투사체 판정을 사용한다

            Projectile.hostile = false;

            Projectile.penetrate = 1;
            // 1회 타격 후 소멸한다 (원하면 -1로 바꾸면 다단히트 가능)

            Projectile.timeLeft = 2;
            // 즉발 판정용이다

            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            // 무기 데미지 타입을 따른다
        }

        public override void AI()
        {
            // 플레이어의 가장 높은 클래스 보정치를 따른다
            Player player = Main.player[Projectile.owner];

            DamageClass[] classes = new DamageClass[]
            {
    DamageClass.Melee,
    DamageClass.Ranged,
    DamageClass.Magic,
    DamageClass.Summon
            };

            float bestBonus = 0f;
            DamageClass bestClass = DamageClass.Default;

            foreach (var dc in classes)
            {
                float bonus = player.GetTotalDamage(dc).ApplyTo(1f);
                // 해당 클래스의 최종 배율을 계산한다

                if (bonus > bestBonus)
                {
                    bestBonus = bonus;
                    bestClass = dc;
                }
            }

            Projectile.DamageType = bestClass;
        }
        public override void ModifyHitNPC(
    NPC target,
    ref NPC.HitModifiers modifiers)
        {
            if (target.realLife != -1)
            {
                modifiers.FinalDamage *= 0.5f;
                // 지렁이 세그먼트라면 데미지를 50% 감소시킨다
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            int buffTime = 180;
            // 3초다

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
                // 방향 정보가 없으면 랜덤 방향으로 약하게 튄다
            }
            hitDir.Normalize();
            // 레이저(전이)가 날아온 방향이다

            int dustCount = 10;
            // 맞을 때 더스트 개수다

            for (int d = 0; d < dustCount; d++)
            {
                Vector2 vel = hitDir * Main.rand.NextFloat(min, max) + Main.rand.NextVector2Circular(2f, 2f);
                int dustType = Main.rand.NextBool() ? 244 : 246;

                int id = Dust.NewDust(target.Hitbox.TopLeft(), target.Hitbox.Width, target.Hitbox.Height, dustType, vel.X, vel.Y, 0, default, 1f);
                Main.dust[id].noGravity = true;
                Main.dust[id].scale = Main.rand.NextFloat(0.9f, 1.35f);
                Main.dust[id].fadeIn = 1.2f;
                Main.dust[id].rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                // 맞은 대상에게 레이저가 날아온 방향으로 얻어맞는 더스트 연출이다
            }
        }
    }
}
