using Terraria;
using Terraria.ModLoader;
using CAmod.Systems;

namespace CAmod.Global
{
    public class CAmodGlobalItem : GlobalItem
    {
        public override bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem, Player player)
        {
            if (AccessoryGroups.glassGroup.Contains(equippedItem.type) &&
                AccessoryGroups.glassGroup.Contains(incomingItem.type))
            {
                return false; // 같은 그룹 장신구면 장착을 막는다
            }

            return base.CanAccessoryBeEquippedWith(equippedItem, incomingItem, player);
        }
    }
}