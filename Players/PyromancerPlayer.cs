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
            if (!HasConduitEquipped) return;

            dustTimer++;

            if (dustTimer >= 60)
            {
                dustTimer = 0;

                

                totalDustCounter = 0;
            }

            // 매 프레임 초기화한다
            dustThisFrame = 0;
        }

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!HasConduitEquipped) return; // 장신구 미착용시 종료한다


         

            if (proj.DamageType == DamageClass.Magic && proj.owner == Player.whoAmI)
            {
                modifiers.FinalDamage *= 1f + bonusd;
                // 마법 투사체 최종 데미지를 5% 증가시킨다
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!HasConduitEquipped) return;
            if (!target.active || target.life <= 0) return;

            if (hit.DamageType != DamageClass.Magic)
                return;

            if ((!hit.Crit && damageDone == 1) || (hit.Crit && damageDone == 2))
                return;

            bool existing = Main.projectile.Any(p =>
                p.active &&
                p.owner == Player.whoAmI &&
                p.type == ModContent.ProjectileType<HolyFlame>() &&
                (int)p.ai[0] == target.whoAmI
            );

            if (!existing)
            {
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
            }
            else
            {
                foreach (var p in Main.projectile.Where(p => p.active &&
                                                             p.type == ModContent.ProjectileType<HolyFlame>() &&
                                                             (int)p.ai[0] == target.whoAmI))
                {
                    p.timeLeft = 180;
                    if (p.ModProjectile is HolyFlame flame) {


                        float magicAdd = Player.GetDamage(DamageClass.Magic).Additive - 1f;
                        float genericAdd = Player.GetDamage(DamageClass.Generic).Additive - 1f;
                        float TotalMagicCrit = (Player.GetCritChance(DamageClass.Magic) + Player.GetCritChance(DamageClass.Generic)) / 100f + 1f;

                        float totalAdd = (magicAdd + genericAdd) * TotalMagicCrit;
                        

                        // 기존 100% → 100 이었는데 10분의 1로 낮춘다
                        float scaledValue = totalAdd ;
                        // 예: 85% → 8.5

                        int integerPart = (int)scaledValue; // 정수 부분
                        float fractionalPart = scaledValue - integerPart; // 소수점 부분

                        // 소수점을 스톡한다
                        dotRemainderStock += fractionalPart;

                        // 스톡이 1 이상이면 그만큼 추가 정수로 변환한다
                        if (dotRemainderStock >= 1f)
                        {
                            int extra = (int)dotRemainderStock;
                            integerPart += extra;
                            dotRemainderStock -= extra;
                        }

                        

                        flame.IncreaseDotDamage(integerPart);


                        
                }
                }
            }
        }

       
    }
}
