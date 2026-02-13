using Terraria;
using Terraria.ModLoader;

namespace CAmod.Common
{
    public class MagicDamageGlobal : GlobalItem
    {
        public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage)
        {
            if (item.DamageType == DamageClass.Magic &&
    (item.ModItem == null || item.ModItem.Mod.Name == "CalamityMod"))
            {
                damage *= 1.15f; // 마법 무기 데미지를 25% 증가시킨다
            }

        }
    }
}
