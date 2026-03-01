using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace CAmod.Globals
{
    public class ArcaneSuppressionGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        // NPC마다 개별 상태를 저장한다

        public bool Suppressed = false; // 제압 상태이다
        private int savedDamage = -1;   // 원래 접촉 데미지를 저장한다

        public override void ResetEffects(NPC npc)
        {
            if (Suppressed)
            {
                npc.damage = 0; // 제압 상태면 충돌 데미지를 0으로 만든다
            }
        }
        public override void AI(NPC npc)
        {
            if (!Suppressed)
                return;

            
        }
        public void SetSuppressed(NPC npc, bool on)
        {
            if (on)
            {
                if (!Suppressed)
                {
                    savedDamage = npc.damage; // 원래 값을 저장한다
                    Suppressed = true;
                }
            }
            else
            {
                if (Suppressed)
                {
                    npc.damage = savedDamage < 0 ? npc.damage : savedDamage;
                    savedDamage = -1;
                    Suppressed = false;
                }
            }
        }
    }
}