using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using static CAmod.Items.Pyromancer;
using System;
using System.Collections.Generic;
using ParticleLibrary.Core.V3.Particles;
using CAmod.Particles;

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

            // =========================
            // 이름을 소문자로 통일한다
            // =========================
            string npcClassName = "";

            if (target.ModNPC != null)
            {
                npcClassName = target.ModNPC.Name.ToLower();
            }

            bool isExoMech =
                npcClassName.Contains("artemis") ||
                npcClassName.Contains("apollo") ||
                npcClassName.Contains("ares");

            // =========================
            // 일반 세그먼트 여부 검사한다
            // =========================
            bool isNormalSegment =
                target.realLife != -1 &&
                !isExoMech;

            // =========================
            // 3초 이후 감쇠 시작
            // =========================
            if (Projectile.timeLeft <= 1 && now > 0)
            {
                // =========================
                // 일반 세그먼트는 즉시 제거한다
                // =========================
                if (isNormalSegment)
                {
                    Projectile.Kill();
                    return;
                }

                Projectile.timeLeft = 2;

                float ratio = 0f;

                if (max > 0)
                    ratio = MathHelper.Clamp(now / max, 0f, 1f);

                float totalFadeFrames = 900f * ratio;

                if (totalFadeFrames <= 1f)
                {
                    Projectile.Kill();
                    return;
                }

                float decreasePerFrame =
                    now / totalFadeFrames;

                fadeBuffer += decreasePerFrame;

                int actualDecrease =
                    (int)fadeBuffer;

                if (actualDecrease > 0)
                {
                    now -= actualDecrease;
                    fadeBuffer -= actualDecrease;
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
            // Fire 파티클 연출
            // ---------------------------------
            Player owner1 = Main.player[Projectile.owner];

            float magicAdd1 =
                owner1.GetDamage(DamageClass.Magic).Additive - 1f;

            float genericAdd1 =
                owner1.GetDamage(DamageClass.Generic).Additive - 1f;

            float TotalMagicCrit1 =
                owner1.GetCritChance(DamageClass.Magic) / 100f + 1f;

            // 두 증가분을 더한 뒤 1000 배율로 정수화한다
            int magicPowerValue1 =
                (int)Math.Round(
                    (magicAdd1 + genericAdd1) *
                    TotalMagicCrit1 *
                    1000f
                );

            float intensity =
                MathHelper.Clamp(
                    damagePerTick / (float)magicPowerValue1,
                    0f,
                    1f
                );

            // 피해량 비례 강도 계산
            int fireCount =
                1 + (int)((target.width + target.height) / 10 * intensity / 2);

            if (fireCount > 50)
            {
                fireCount = 50;
            }

            // 스택이 낮을수록 중앙에 집중하고 최대 스택에서 기존 범위가 된다
            float stackRatio = max > 0f
                ? MathHelper.Clamp(
                    damagePerTick / max,
                    0f,
                    1f)
                : 0f;

            // 대상 주변에 계속 불꽃을 생성한다
            for (int i = 0; i < fireCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = MathF.Sqrt(Main.rand.NextFloat());

                float radiusX = target.width * 0.35f * stackRatio;
                float radiusY = target.height * 0.35f * stackRatio;

                Vector2 offset = new Vector2(
                    MathF.Cos(angle) * radius * radiusX,
                    MathF.Sin(angle) * radius * radiusY
                );

                Vector2 spawnPos = target.Center + offset;

                Vector2 velocity = new Vector2(
                    Main.rand.NextFloat(-0.6f, 0.6f),
                    Main.rand.NextFloat(-5.0f, -0.5f)
                );

                float hitboxAverage = (target.width + target.height) * 0.5f;

                float scaleValue = MathF.Min(
                    Main.rand.NextFloat(0.7f, 1.2f) *
                    (0.7f + intensity * 0.6f) *
                    0.3f *
                    hitboxAverage,
                    40f
                );

                ParticleInfo info = new ParticleInfo(
                    new System.Numerics.Vector2(
                        spawnPos.X,
                        spawnPos.Y
                    ),
                    new System.Numerics.Vector2(
                        velocity.X,
                        velocity.Y
                    ),
                    0f,
                    new System.Numerics.Vector2(
                        scaleValue,
                        scaleValue
                    ),
                    Color.White,
                    Main.rand.Next(40, 80)
                );

                // Fire1~Fire3 중 하나를 랜덤으로 선택한다
                switch (Main.rand.Next(3))
                {
                    case 0:
                        FireParticleSystem.Buffer1.Create(info);
                        break;

                    case 1:
                        FireParticleSystem.Buffer2.Create(info);
                        break;

                    case 2:
                        FireParticleSystem.Buffer3.Create(info);
                        break;
                }
            }

            // ---------------------------------
            // 화산 분출 연출
            // ---------------------------------

            float intensity2 = intensity * 2;

            if (intensity2 > 1)
            {
                intensity = 1;
            }

            float burstCount =
                intensity * ((target.width + target.height) / 10) + 1f;

            // 피해량이 높을수록 더 많이 분출한다

            // ---------------------------------
            // 도트 데미지 틱 (0.5초마다)
            // ---------------------------------

            tickTimer++;

            if (tickTimer >= 20) // 1초에 3번
            {
                // 무적 상태 체크
                if (target.immortal ||
                    target.dontTakeDamage ||
                    target.immune[Projectile.owner] > 0)
                    return;

                tickTimer = 0;

                int damageToDeal = damagePerTick;

                Player owner = Main.player[Projectile.owner];

                float magicAdd =
                    owner.GetDamage(DamageClass.Magic).Additive - 1f;

                float genericAdd =
                    owner.GetDamage(DamageClass.Generic).Additive - 1f;

                float TotalMagicCrit =
                    (owner.GetCritChance(DamageClass.Magic) +
                     owner.GetCritChance(DamageClass.Generic)) / 100f + 1f;

                // 두 증가분을 더한 뒤 1000 배율로 정수화한다
                float magicPowerValue =
                    (magicAdd + genericAdd) *
                    TotalMagicCrit;

                magicPowerValue =
                    (int)Math.Round(1000 * magicPowerValue);

                max = magicPowerValue;

                if (damageToDeal > magicPowerValue)
                {
                    damageToDeal = (int)magicPowerValue;
                }

                if (damageToDeal <= 0)
                {
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

                if (owner != null && owner.active)
                {
                    owner.addDPS(damageToDeal);
                }
            }
        }

        public override bool? CanHitNPC(NPC target) => true;

        // 타격 시 외부에서 불러줄 메서드
        public void IncreaseDotDamage(int Tick)
        {
            damagePerTick += Tick;

            if (!DEV_MODE &&
                damagePerTick > BALANCED_TICK_DAMAGE_CAP)
            {
                damagePerTick = BALANCED_TICK_DAMAGE_CAP;
            }
        }
    }
}