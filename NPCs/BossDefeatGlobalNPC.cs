using Terraria;
using Terraria.ModLoader;
using CAmod.Players;
using Terraria.ID;
using CAmod.Systems;

namespace CAmod.Globals
{
    public class BossDefeatGlobalNPC : GlobalNPC
    {
        public override void OnKill(NPC npc)
        {
            if (!MajorBoss25Filter.IsValid(npc))
                return;
            

            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;
            // 서버에서만 처리한다

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (!player.active)
                    continue;

                var leaf = player.GetModPlayer<LeafWardPlayer2>();

                if (leaf.defeatedBossTypes.Contains(npc.type))
                    continue;
                // 이미 이 보스를 잡았으면 무시한다

                leaf.defeatedBossTypes.Add(npc.type);
                // 이 캐릭터의 최초 처치 보스로 기록한다
            }
        }
    }
}
