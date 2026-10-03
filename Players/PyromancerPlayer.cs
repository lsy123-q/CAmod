using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using CAmod.Projectiles;
using System.Collections.Generic;

namespace CAmod.Players
{
    public class PyromancerPlayer : ModPlayer
    {
        public bool HasConduitEquipped = false;

        public int TargetDebuffsApplied = 0;

        public float CurrentBuffCount = 0;

        public int decayDelayTimer = 0;

        public int decayTimer = 0;

        public float dotRemainderStock = 0f;

        public float bonusd = 1f;

        public int totalDustCounter = 0;
        // 1초 동안 생성된 전체 더스트 수를 저장한다

        public bool dustBlocked = false;

        private int dustTimer = 0;

        public bool visualEnabled = true;

        public int dustThisFrame = 0;

        public override void ResetEffects()
        {
            HasConduitEquipped = false;
        }

        public override void PostUpdate()
        {
            if (!HasConduitEquipped)
                return;

            dustTimer++;

            if (dustTimer >= 60)
            {
                dustTimer = 0;

                totalDustCounter = 0;
            }

            // 매 프레임 초기화한다
            dustThisFrame = 0;
        }

        public override void ModifyHitNPCWithProj(
            Projectile proj,
            NPC target,
            ref NPC.HitModifiers modifiers
        )
        {
            // 장신구 미착용시 종료한다
            if (!HasConduitEquipped)
                return;

            if (
                proj.DamageType == DamageClass.Magic &&
                proj.owner == Player.whoAmI
            )
            {
                modifiers.FinalDamage *= 1f + bonusd;
                // 마법 투사체 최종 데미지를 증가시킨다
            }
        }

        public override void OnHitNPC(
            NPC target,
            NPC.HitInfo hit,
            int damageDone
        )
        {
            if (!HasConduitEquipped)
                return;

            if (!target.active || target.life <= 0)
                return;

            // 마법 공격만 적용한다
            if (hit.DamageType != DamageClass.Magic)
                return;

            // 최소 강제 데미지는 무시한다
            if (
                (!hit.Crit && damageDone == 1) ||
                (hit.Crit && damageDone == 2)
            )
                return;

            bool existing =
                Main.projectile.Any(
                    p =>
                        p.active &&
                        p.owner == Player.whoAmI &&
                        p.type ==
                        ModContent.ProjectileType<HolyFlame>() &&
                        (int)p.ai[0] == target.whoAmI
                );

            // -------------------------
            // 최초 생성
            // -------------------------

            if (!existing)
            {
                int projIndex =
                    Projectile.NewProjectile(
                        Player.GetSource_OnHit(target),
                        target.Center,
                        Vector2.Zero,
                        ModContent.ProjectileType<HolyFlame>(),
                        0,
                        0f,
                        Player.whoAmI,
                        ai0: target.whoAmI
                    );

                // 생성된 투사체를 가져온다
                Projectile proj =
                    Main.projectile[projIndex];

                if (proj.ModProjectile is HolyFlame flame)
                {
                    int stack =
                        CalculateDotStack(damageDone);

                    // 생성 직후 즉시 스택을 부여한다
                    flame.IncreaseDotDamage(stack);
                }
            }

            // -------------------------
            // 이미 존재할 경우
            // -------------------------

            else
            {
                foreach (
                    var p in Main.projectile.Where(
                        p =>
                            p.active &&
                            p.type ==
                            ModContent.ProjectileType<HolyFlame>() &&
                            (int)p.ai[0] == target.whoAmI
                    )
                )
                {
                    // 지속시간을 갱신한다
                    p.timeLeft = 180;

                    if (p.ModProjectile is HolyFlame flame)
                    {
                        int stack =
                            CalculateDotStack(damageDone);

                        // 스택을 증가시킨다
                        flame.IncreaseDotDamage(stack);
                    }
                }
            }
        }

        // 화상 스택 증가량 계산 함수
        public int CalculateDotStack(int damageDone)
        {
            float magicAdd =
                Player.GetDamage(DamageClass.Magic)
                    .Additive - 1f;

            float genericAdd =
                Player.GetDamage(DamageClass.Generic)
                    .Additive - 1f;

            float TotalMagicCrit =
                (
                    Player.GetCritChance(DamageClass.Magic) +
                    Player.GetCritChance(DamageClass.Generic)
                ) / 100f + 1f;

            float totalAdd =
                (magicAdd + genericAdd) *
                TotalMagicCrit;

            // 배율값 계산한다
            float scaledValue = totalAdd;

            // 정수 부분 추출한다
            int integerPart =
                (int)scaledValue;

            // 소수점 부분 추출한다
            float fractionalPart =
                scaledValue - integerPart;

            // 소수점을 스톡한다
            dotRemainderStock += fractionalPart;

            // 스톡이 1 이상이면 정수로 변환한다
            if (dotRemainderStock >= 1f)
            {
                int extra =
                    (int)dotRemainderStock;

                integerPart += extra;

                dotRemainderStock -= extra;
            }

            // 실제 데미지 비례 스택 추가한다
            int damageStack =
                System.Math.Max(
                    1,
                    damageDone / 300
                );

            // 최종 스택 계산한다
            integerPart += damageStack;

            return integerPart;
        }
    }
}