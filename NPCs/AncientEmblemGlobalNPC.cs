using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;

namespace CAmod.NPCs
{
    public class AncientEmblemGlobalNPC : GlobalNPC
    {
        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            if (!npc.boss)
                return;
            // 보스가 아니면 처리하지 않는다

            npcLoot.Add(
                ItemDropRule.Common(
                    ModContent.ItemType<Items.Materials.AncientEmblem>(),
                    40
                )
            );
            // 모든 보스에게서 1% 확률로 AncientEmblem을 드랍한다
        }
    }
}
