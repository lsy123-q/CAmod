using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.Localization;

namespace CAmod.Players
{
    public class BossFightTimerPlayer : ModPlayer
    {
        private bool inBossFight = false;
        private int fightTimer = 0;

        private int lastBossCount = 0;

        public override void PostUpdate()
        {
            // 보스가 살아있는지 검사한다
            int bossCount = 0;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (!n.active) continue;
                if (!n.boss) continue;
                if (n.friendly) continue;
                if (n.life <= 0) continue;

                bossCount++;
            }

            if (!inBossFight)
            {
                // 보스가 새로 등장하면 전투 시작이다
                if (bossCount > 0)
                {
                    inBossFight = true;
                    fightTimer = 0;
                    lastBossCount = bossCount;
                }
            }
            else
            {
                // 전투 중이면 시간을 센다
                fightTimer++;

                // 보스가 전부 사라지면 전투 종료다
                if (bossCount <= 0)
                {
                    inBossFight = false;

                    // mm:ss로 포맷한다
                    int totalSeconds = fightTimer / 60;
                    int minutes = totalSeconds / 60;
                    int seconds = totalSeconds % 60;

                    string timeText = $"{minutes:00}:{seconds:00}";

                    if (Main.netMode != 2) // 서버에서는 로컬 채팅 출력 안 한다
                    {
                        Main.NewText($"보스 교전시간: {timeText}", 255, 200, 80); // 교전시간을 출력한다
                    }

                    fightTimer = 0;
                    lastBossCount = 0;
                }
                else
                {
                    lastBossCount = bossCount;
                }
            }
        }
    }
}