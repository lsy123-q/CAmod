using Terraria;
using Terraria.ModLoader;

namespace CAmod.Buffs
{
    public class BloodMageOn : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.debuff[Type] = false; // 쿨타임 버프는 회색 테두리로
            Main.buffNoTimeDisplay[Type] = true; // 남은 시간 표시
        }
    }
}
