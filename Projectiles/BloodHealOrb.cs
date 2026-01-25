using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using CAmod.Players;
using CAmod.Players;
using Terraria.ID;
using Terraria.Graphics.Shaders;
using Terraria.Audio;
namespace CAmod.Projectiles
{
    public class BloodHealOrb : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.timeLeft = 1000;
            Projectile.penetrate = -1;

            Projectile.friendly = false;
            Projectile.hostile = false;

            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {

            // 최초 소환 시점이다 (1회만 실행된다
            Lighting.AddLight(
Projectile.Center,
0.9f,  // R : 선혈 빨강
0.1f,  // G : 거의 없음
0.1f   // B : 거의 없음
);
            Projectile.extraUpdates = 3;
            // 이동 보간 밀도를 높인다

            int ownerIndex = (int)Projectile.ai[0];
            int healPotential = (int)Projectile.ai[1];
            
            if (ownerIndex < 0 || ownerIndex >= Main.maxPlayers)
            {
                Projectile.Kill();
                return;
            }

            Player player = Main.player[ownerIndex];
            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }
            healPotential = Math.Min(healPotential, player.statLifeMax2 - player.statLife);
            Vector2 oldPos = Projectile.Center;
            Vector2 toPlayer = player.Center - Projectile.Center;


            Vector2 targetPos = player.position + new Vector2(
    Main.rand.NextFloat(player.width),
    Main.rand.NextFloat(player.height)
);
            // 플레이어 히트박스 내부의 랜덤 지점이다

            Vector2 toTarget = targetPos - Projectile.Center;
            float dist = toTarget.Length();




            if (dist < 25f)
            {
                int healed = Math.Min(healPotential, player.statLifeMax2 - player.statLife);
                float healRatio = (float)healed / player.statLifeMax2;
                float scale = MathHelper.Lerp(1.0f, 2.0f, healRatio);

                for (int i = 0; i < 15; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(1f, 1f) * scale;

                    int d = Dust.NewDust(
                         Projectile.Center,
                             0,
                          0,
                        DustID.Blood,
                        vel.X,
                        vel.Y,
                        80,
                        default,
                        scale
                    );
                    // === 생존시간 연장 핵심 ===

                    Main.dust[d].noGravity = true;


                    // === 생존시간 연장 핵심 ===
                    Main.dust[d].fadeIn = 1.5f + healRatio * 2.5f;
                    // 페이드인 시간을 늘려 소멸을 최대한 늦춘다

                    Main.dust[d].scale *= 1.2f;
                }
            




                var bmp = player.GetModPlayer<BloodMagePlayer>();
                bmp.allowBloodHeal = true;

               
                player.statLife += healed;
                bmp.lastLife = player.statLife;
                // 체력 회복 후 기준 체력을 동기화한다

                int manaHealed = Math.Min(healed, player.statManaMax2 - player.statMana);
                player.statMana += manaHealed;
                // 체력 회복량과 동일한 양만큼 마나를 회복한다

                CombatText.NewText(
    player.getRect(),
    new Color(180, 30, 30),
    "+" + healed
);
                Projectile.Kill();
                return;
            }

            // === 폭탄식 추적 로직 ===
            float accel = 1.02f;
            float damp = 0.92f;
            float maxSpeed = 14f;

            if (dist > 0f)
            {
                toTarget.Normalize();
                toTarget *= accel;

                Projectile.velocity.X = Projectile.velocity.X * damp + toTarget.X;
                Projectile.velocity.Y = Projectile.velocity.Y * damp + toTarget.Y;

                if (Projectile.velocity.Length() > maxSpeed)
                    Projectile.velocity = Vector2.Normalize(Projectile.velocity) * maxSpeed;
            }
            // === 추적 끝 ===

            // === Dust 연출 ===
            Vector2 nextPos = Projectile.Center + Projectile.velocity;
            float distMove = Vector2.Distance(oldPos, nextPos);

            if (distMove > 0f)
            {
                int stepCount = Math.Max(1, (int)(distMove / 2.5f));
                float interval = distMove / stepCount;
                float healRatio = MathHelper.Clamp(
                    (float)healPotential / player.statLifeMax2,
                    0f,
                    1f
                );
    if (Projectile.localAI[0] == 0f)
    {
        Projectile.localAI[0] = 1f;
        
    }

    float dustScale = MathHelper.Lerp(1.0f, 2.0f, healRatio);
                for (int i = 1; i <= stepCount; i++)
                {
                    float t = (i * interval) / distMove;
                    Vector2 dustPos = Vector2.Lerp(oldPos, nextPos, t);

                    int d = Dust.NewDust(
    dustPos, 0, 0,
    DustID.Blood,
    0f, 0f, 25,
    default,
    dustScale
);

                    Main.dust[d].velocity = Vector2.Zero;
                    Main.dust[d].noGravity = true;
                    Main.dust[d].noGravity = true;
                    Main.dust[d].shader = GameShaders.Armor.GetSecondaryShader(1, Main.LocalPlayer);

                }
            }
        }


        
    }
}
