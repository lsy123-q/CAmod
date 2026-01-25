using Terraria;
using Terraria.ModLoader;
using System;

namespace CAmod.Systems
{
    public static class MajorBoss25Filter
    {
        public static bool IsValid(NPC npc)
        {
            // 멀티파트/웜 계열에서 몸통/꼬리 중복 카운트 방지용이다
            if (npc.realLife != -1 && npc.whoAmI != npc.realLife)
                return false;

            // 보스 플래그만으로 판정한다 (바닐라/모드 보스 전부 포함된다)
            return npc.boss;
        }
    }
}
