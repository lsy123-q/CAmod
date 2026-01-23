using Terraria;
using Terraria.ModLoader;

namespace CAmod.Buffs
{
    public class ToAnotherWorld : ModBuff
    {
        public override void SetStaticDefaults()
        {
            // 버프 속성
            Main.buffNoSave[Type] = true;
            Main.debuff[Type] = false;
            Main.buffNoTimeDisplay[Type] = false; // 남은 시간 표시
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.buffTime[buffIndex] = 2; // 지속 유지 (Player 쪽에서 직접 갱신)
        }
    }
}