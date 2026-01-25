using Microsoft.Xna.Framework; // CombatText 색상용
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CAmod.Buffs
{
    public class LeafShieldCooltime : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.debuff[Type] = false; // 쿨타임 버프는 회색 테두리로
            Main.buffNoTimeDisplay[Type] = false; // 남은 시간 표시
        }

        public override void Update(Player player, ref int buffIndex)
        {
           
        }
    }
}