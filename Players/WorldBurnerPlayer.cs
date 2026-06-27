using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CAmod.Players
{
    public class WorldBurnerPlayer : ModPlayer
    {
        // 장신구 착용 여부를 저장한다
        public bool worldBurner;

        public override void ResetEffects()
        {
            // 매 프레임 초기화한다
            worldBurner = false;
        }

        public override void OnHitNPCWithItem
        (
            Item item,
            NPC target,
            NPC.HitInfo hit,
            int damageDone
        )
        {
            // 장신구를 착용하지 않았다면 종료한다
            if (!worldBurner)
            {
                return;
            }

            // 마법 무기만 적용한다
            if (item.DamageType != DamageClass.Magic)
            {
                return;
            }

            ApplyWorldBurnerDebuff(target);
        }

        public override void OnHitNPCWithProj
        (
            Projectile proj,
            NPC target,
            NPC.HitInfo hit,
            int damageDone
        )
        {
            // 장신구를 착용하지 않았다면 종료한다
            if (!worldBurner)
            {
                return;
            }

            // 마법 투사체만 적용한다
            if (proj.DamageType != DamageClass.Magic)
            {
                return;
            }

            ApplyWorldBurnerDebuff(target);
        }

        // 디버프를 적용하는 함수다
        // 디버프를 적용하는 함수다
        // 디버프를 적용하는 함수다
        private void ApplyWorldBurnerDebuff(NPC target)
        {
            int buffTime = 180;

            try
            {
                Mod calamity = ModLoader.GetMod("CalamityMod");

                if (calamity != null)
                {
                    ModBuff godSlayerInferno =
                        calamity.Find<ModBuff>("GodSlayerInferno");

                    if (godSlayerInferno != null)
                    {
                        target.AddBuff(godSlayerInferno.Type, buffTime);
                    }
                    else
                    {
                        target.AddBuff(BuffID.OnFire, buffTime);
                    }

                    ModBuff holyFlames =
                        calamity.Find<ModBuff>("HolyFlames");

                    if (holyFlames != null)
                    {
                        target.AddBuff(holyFlames.Type, buffTime);
                    }
                    else
                    {
                        target.AddBuff(BuffID.OnFire3, buffTime);
                    }

                    ModBuff dragonFire =
                        calamity.Find<ModBuff>("Dragonfire");

                    if (dragonFire != null)
                    {
                        target.AddBuff(dragonFire.Type, buffTime);
                    }
                    else
                    {
                        target.AddBuff(BuffID.CursedInferno, buffTime);
                    }

                    ModBuff brimstoneFlames =
                        calamity.Find<ModBuff>("BrimstoneFlames");

                    if (brimstoneFlames != null)
                    {
                        target.AddBuff(brimstoneFlames.Type, buffTime);
                    }
                    else
                    {
                        target.AddBuff(BuffID.ShadowFlame, buffTime);
                    }
                }
                else
                {
                    target.AddBuff(BuffID.OnFire, buffTime);
                }
            }
            catch
            {
                // 오류 발생시 기본 화염만 적용한다
                target.AddBuff(BuffID.OnFire, buffTime);
            }
        }
    }
}