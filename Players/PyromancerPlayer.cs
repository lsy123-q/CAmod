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

        public float BrandDamageBonus;

        public int totalDustCounter = 0;
        // 1초 동안 생성된 전체 더스트 수를 저장한다

        public bool dustBlocked = false;

        private int dustTimer = 0;

        public bool visualEnabled = true;

        public int dustThisFrame = 0;

        public override void ResetEffects()
        {
            HasConduitEquipped = false;
            BrandDamageBonus = 0f;
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

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            ApplyBrand(target, hit, damageDone);
        }

        internal void ApplyBrand(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!HasConduitEquipped || Player.whoAmI != Main.myPlayer
                || !target.active || target.life <= 0 || target.friendly
                || hit.DamageType != DamageClass.Magic
                || (!hit.Crit && damageDone == 1) || (hit.Crit && damageDone == 2)
                || damageDone <= 0)
                return;

            target.GetGlobalNPC<global::CAmod.Globals.PyromancerBrandGlobalNPC>()
                .AddBrand(target, Player.whoAmI, CalculateDotStack(damageDone));
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