using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using static CAmod.Items.Pyromancer;
using System;
using System.Collections.Generic;

namespace CAmod.Projectiles
{
    public class HolyFlame : ModProjectile
    {
        private int tickTimer = 0;
        private int damagePerTick = BASE_TICK_DAMAGE;
        private int lastDamageDealt = 0;
        private int burnIndex = 0; // 화상 디버프 순환 인덱스를 저장한다
        private static int brimstoneType = -1; // 유황불꽃 타입을 캐싱한다
        private int burnTickCounter = 0; // 화상 발동 카운터를 저장한다
        private int holy = -1;


        private int now = 0;
        private float max = 0;
        private float fadeBuffer = 0f; // 소수 감소량 누적 버퍼다
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.timeLeft = 180; // 기본 3초
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
            Projectile.damage = 1;

            Projectile.alpha = 255; // 완전 투명 (판정만 남김)
        }
        public override void SetStaticDefaults()
        {
            try
            {
                brimstoneType = ModContent.Find<ModBuff>("CalamityMod", "BrimstoneFlames").Type;
            }
            catch
            {
                brimstoneType = -1; // 칼라미티가 없으면 -1로 둔다
            }

            try
            {
                holy = ModContent.Find<ModBuff>("CalamityMod", "HolyFlames").Type;
                
            }
            catch {
                brimstoneType = -1;
            }

        }
        public override void AI()
        {
            int targetWhoAmI = (int)Projectile.ai[0];
            if (targetWhoAmI < 0 || targetWhoAmI >= Main.npc.Length)
            {
                Projectile.Kill();
                return;
            }

            NPC target = Main.npc[targetWhoAmI];
            if (!target.active || target.life <= 0)
            {
                Projectile.Kill();
                return;
            }





            // 3초 이후 감쇠 시작
            if (Projectile.timeLeft <= 1 && now > 0 && target.realLife == -1)
            {
                Projectile.timeLeft = 2; // 소멸을 막는다

                float ratio = 0f;
                if (max > 0)
                    ratio = MathHelper.Clamp(now / max, 0f, 1f); //0.5

                float totalFadeFrames = 900f * ratio; // 300f

                if (totalFadeFrames <= 1f)
                {
                    Projectile.Kill();
                    return;
                }

                float decreasePerFrame = now / totalFadeFrames; // 3.3

                // 소수 누적한다
                fadeBuffer += decreasePerFrame; // 4.2 저장

                int actualDecrease = (int)fadeBuffer; // 4 

                if (actualDecrease > 0)
                {
                    now -= actualDecrease; // 
                    fadeBuffer -= actualDecrease; //
                }

                if (now <= 1)
                {
                    Projectile.Kill();
                    return;
                }

                damagePerTick = now;
            }
            // 대상에 고정
            Projectile.Center = target.Center;

            // ---------------------------------
            // Dust 연출 (강도 = damagePerTick 비례)
            // ---------------------------------
            Player owner1 = Main.player[Projectile.owner];
            float magicAdd1 = owner1.GetDamage(DamageClass.Magic).Additive - 1f;
            float genericAdd1 = owner1.GetDamage(DamageClass.Generic).Additive - 1f;

            float TotalMagicCrit1 = owner1.GetCritChance(DamageClass.Magic) / 100f + 1f;


            // 두 증가분을 더한 뒤 1000 배율로 정수화한다
            int magicPowerValue1 = (int)Math.Round((magicAdd1 + genericAdd1) * TotalMagicCrit1 * 1000f);

           



            float intensity = MathHelper.Clamp(damagePerTick / (float)magicPowerValue1, 0f, 1f);
            // 피해량 비례 강도 계산

            int dustCount = 1 + (int)((target.width + target.height) / 10 * intensity);

            if (dustCount > 100) {
                dustCount = 100;
            }
            // 기본 잔불 개수

            for (int i = 0; i < dustCount; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(target.width * ((0.4f * intensity)+0.1f), target.height*((0.4f * intensity) + 0.1f));
                

                
                // 중심은 밝은 성염, 외곽은 어두운 화염
                float maxRadius = Math.Max(target.width, target.height) * ((0.4f * intensity) + 0.1f);
                // 현재 실제 생성 반경을 정확히 구한다

                float radiusRatio = offset.Length() / maxRadius;
                // 생성된 위치가 전체 반경 대비 몇 %인지 계산한다

                int dustType = (radiusRatio <= 0.35f) ? DustID.GoldCoin : DustID.CopperCoin;
                // 중심 30% 이내는 Gold, 나머지는 무조건 Copper로 한다
                Vector2 spawnPos = target.Center + offset;

                int dustIndex = Dust.NewDust(spawnPos, 0, 0, dustType,
                    Main.rand.NextFloat(-1.2f, 1.2f),
                    Main.rand.NextFloat(-2.5f, -0.3f),
                    200,
                    default,
                    1.0f + 1.0f * intensity); // 강할수록 커짐

                Dust dust = Main.dust[dustIndex];
                dust.noGravity = true;
                dust.velocity *= 0.5f + intensity;
                dust.scale *= 1f + 0.4f * intensity;
                dust.fadeIn = 1.2f;
               
            }

            // ---------------------------------
            // 2️⃣ 화산 분출 연출 (폭발성 튀김)
            // ---------------------------------

            float intensity2 = intensity * 2;
            if (intensity2 > 1) {
                intensity = 1;
            }

            float burstCount = intensity * ((target.width+target.height)/10)  + 1f;
                // 피해량이 높을수록 더 많이 분출한다

                

            // ---------------------------------
            // 도트 데미지 틱 (0.5초마다)
            // ---------------------------------
            tickTimer++;
            if (tickTimer >= 20) // 1초에 3번
            {
                // 무적 상태 체크
                if (target.immortal || target.dontTakeDamage || target.immune[Projectile.owner] > 0)
                    return;

                for (int i = 0; i < burstCount; i++)
                {
                    Vector2 offset = Main.rand.NextVector2Circular(target.width * ((0.4f * intensity) + 0.1f), target.height * ((0.4f * intensity) + 0.1f));
                    float radiusRatio = offset.Length() / (Math.Max(target.width, target.height) * 0.6f);

                    int dustType = Main.rand.Next(2);
                    if (dustType == 0)
                    {
                        dustType = 169;
                    }
                    else
                    {
                        dustType = 158;
                    }
                    // 중심은 밝은 성염, 외곽은 어두운 화염

                    Vector2 spawnPos = target.Center + offset;


                    float angle = MathHelper.ToRadians(Main.rand.NextFloat(-40f, 40f) - 90f);
                    float speed = Main.rand.NextFloat(3f, 6f);
                    Vector2 eruptDir = angle.ToRotationVector2() * speed;

                    int dustIndex = Dust.NewDust(
                        spawnPos,
                        0,
                        0,
                        dustType, // 화산 마그마 느낌
                        eruptDir.X,
                        eruptDir.Y,
                        100,
                        default,
                        0.75f + 1.0f
                    );

                    Dust burst = Main.dust[dustIndex];
                    burst.noGravity = false; // 중력을 켠다
                    burst.velocity.Y = -Main.rand.NextFloat(1,4);
                    // 위로 강하게 튀어오르게 한다

                    burst.velocity.X = Main.rand.NextFloat(-2.5f, 2.5f);
                    // 좌우 퍼짐

                   
                   
                  

                }


                tickTimer = 0;



                int damageToDeal = damagePerTick;
                

                Player owner = Main.player[Projectile.owner];

                float magicAdd = owner.GetDamage(DamageClass.Magic).Additive - 1f;
                float genericAdd = owner.GetDamage(DamageClass.Generic).Additive - 1f;

                float TotalMagicCrit = (owner.GetCritChance(DamageClass.Magic) + owner.GetCritChance(DamageClass.Generic)) / 100f + 1f;


                // 두 증가분을 더한 뒤 1000 배율로 정수화한다
                float magicPowerValue = (magicAdd + genericAdd ) * TotalMagicCrit;

                magicPowerValue = (int)Math.Round(1000 * magicPowerValue);

                max = magicPowerValue;
            

                if (damageToDeal > magicPowerValue)
                {
                    damageToDeal = (int)magicPowerValue;

                }

                if (damageToDeal <= 0) {
                    damageToDeal = 1;


                }

                NPC.HitInfo hitInfo = new NPC.HitInfo()
                {
                    Damage = damageToDeal,
                    Knockback = 0f,
                    HitDirection = 0,
                    Crit = false,
                    DamageType = DamageClass.Generic,
                    HideCombatText = true
                };

                target.StrikeNPC(hitInfo);
                CombatText.NewText(
                   target.Hitbox,
                   new Color(255, 189, 25),
                   damageToDeal,
                   dramatic: false
               );

                now = damageToDeal;


                int brimstone = -1; // 유황불꽃 버프 타입을 저장한다

                try
                {
                    brimstone = ModContent.Find<ModBuff>("CalamityMod", "BrimstoneFlames").Type;
                }
                catch
                {
                    brimstone = -1; // 칼라미티가 없으면 -1 유지
                }
                if (owner != null && owner.active)
                {
                    owner.addDPS(damageToDeal);
                }

               
                if (damageToDeal == magicPowerValue)
                {
                    burnTickCounter++; // 틱 누적한다

                    int stage = burnTickCounter / 15 + 1;
                    // 30틱 = 10초
                    // 0~29 = 1단계
                    // 30~59 = 2단계 ...
                  
                    if (stage > 8)
                        stage = 8; // 최대 7단계 제한

                    List<int> burnList = new List<int>()
                    {
                        BuffID.OnFire,        // 일반 화상
                      
                        BuffID.OnFire3,       // 지옥불 (Hellfire)
                    
  
                        BuffID.Daybreak       // 데이브레이크 (최상위)
                    };

                   

                    if (holy != -1)
                    {
                        burnList.Insert(burnList.Count, holy);
                      
                    }
                    // 단계 수만큼 순서대로 부여한다
                    for (int i = 0; i < stage && i < burnList.Count; i++)
                    {
                        target.AddBuff(burnList[i], 180);
                    }
                }


            }
        }

        public override bool? CanHitNPC(NPC target) => true;

        // 타격 시 외부에서 불러줄 메서드
        public void IncreaseDotDamage(int Tick)
        {
            damagePerTick += Tick;
            if (!DEV_MODE && damagePerTick > BALANCED_TICK_DAMAGE_CAP)
                damagePerTick = BALANCED_TICK_DAMAGE_CAP;
        }
    }
}