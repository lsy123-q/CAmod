using Terraria;
using Terraria.ModLoader;
using CAmod.Projectiles;
namespace CAmod.Players
{
    public class GlassCannonPlayer : ModPlayer
    {
        public bool glassCannonEquipped;

        public override void ResetEffects()
        {
            glassCannonEquipped = false;
            // 매 틱마다 초기화한다
        }
        

        public override void UpdateEquips()
        {
            if (!glassCannonEquipped)
                return;
           
            // ===== 최대 체력 25% 감소  =====
            Player.statLifeMax2 -= (int)(Player.statLifeMax * 0.25f);
            // 최종 최대 체력을 직접 깎는다
            Player.statDefense = (Player.statDefense * 0.75f);
            // 방어력을 25% 감소시킨다

            Player.GetCritChance(DamageClass.Magic) += 12.5f;
            Player.GetArmorPenetration(DamageClass.Magic) += 25f;
            Player.GetDamage(DamageClass.Magic) += 0.25f;
            Player.moveSpeed += 0.25f;
            Player.maxRunSpeed += 0.25f;

            Player.jumpSpeedBoost += 0.25f;


        }
        public override void GetHealLife(Item item, bool quickHeal, ref int healValue)
        {
            if (!glassCannonEquipped)
                return;

            if (item.healLife <= 0)
                return;
            // 체력 회복 포션이 아닐 경우 무시한다

            healValue = (int)(healValue * 0.75f);
            // 체력 회복량을 25% 감소시킨다
        }
        

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!glassCannonEquipped)
                return;

            if (hit.DamageType != DamageClass.Magic)
                return;
            // 마법 피해가 아니면 즉시 차단한다
            if (!Main.rand.NextBool(4))
                return;

            int bonusDamage = damageDone;
            // 최종 피해 기준 25%를 산출한다

            if (bonusDamage <= 0)
                return;

            Projectile.NewProjectile(
                Player.GetSource_OnHit(target),
                target.Center,
                Microsoft.Xna.Framework.Vector2.Zero,
                ModContent.ProjectileType<GlassCannonBonusHit>(),
                bonusDamage,
                0f,
                Player.whoAmI
            );
            // 추가 고정 피해를 투사체로 전송한다
        }

        public override void UpdateLifeRegen()
        {
            if (!glassCannonEquipped)
                return;

            Player.lifeRegen = (int)(Player.lifeRegen * 0.75f);
            // 체력 재생 최종값을 25% 감소시킨다
        }

    }
}
