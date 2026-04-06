using Terraria;
using Terraria.ModLoader;
using CAmod.Systems;

namespace CAmod.Players
{
    public class AccessoryGroupPlayer : ModPlayer
    {
        public bool hasGlassGroupItem;

        public override void ResetEffects()
        {
            hasGlassGroupItem = false;
            // 매 틱 초기화한다
        }

        public override void UpdateEquips()
        {
            foreach (Item item in Player.armor)
            {
                if (item != null && AccessoryGroups.glassGroup.Contains(item.type))
                {
                    hasGlassGroupItem = true;
                    // 그룹 아이템 하나라도 있으면 true로 설정한다
                    break;
                }
            }
        }
    }
}