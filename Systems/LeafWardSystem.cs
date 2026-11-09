using CalamityMod;
using Terraria;
using Terraria.ModLoader;
using System;

namespace CAmod.Systems
{
    public class LeafWardSystem : ModSystem
    {
        private static int cachedBossCount = 0;
        // 캐싱된 보스 수다

        private static int cacheTimer = 0;
        // 캐싱 타이머다

        public override void PostUpdateWorld()
        {
            cacheTimer++;

            if (cacheTimer > 60)
            {
                cacheTimer = 0;
                cachedBossCount = CalculateBossCount();
                // 1초마다 보스 수 갱신한다
            }
        }

        public static int GetStage()
        {
            return Math.Min(cachedBossCount, 40);
            // 최대 40으로 제한한다
        }

        public static int GetLeafShieldStage(Player player)
        {
            return player.GetModPlayer<global::CalamityMod.CalPlayer.CalamityPlayer>().silvaSet
                ? 40
                : GetStage();
        }

        private static int CalculateBossCount()
        {
            // =========================
            // 1. 칼라미티 존재하면 전부 위임한다
            // =========================
            int calamityCount = GetCalamityBossCount();

            if (calamityCount > 0)
                return calamityCount;
            // Community가 이미 바닐라 포함해서 계산한다

            // =========================
            // 2. 칼라미티 없으면 바닐라 직접 계산
            // =========================
            int count = 0;
           
            count += NPC.downedSlimeKing ? 1 : 0;
            // 킹 슬라임 처치 여부다

            count += NPC.downedBoss1 ? 1 : 0;
            // 아이 오브 크툴루 처치 여부다

            count += NPC.downedBoss2 ? 1 : 0;
            // 이터 오브 월드 / 브레인 오브 크툴루 처치 여부다

            count += NPC.downedQueenBee ? 1 : 0;
            // 퀸비 처치 여부다

            count += NPC.downedBoss3 ? 1 : 0;
            // 스켈레트론 처치 여부다

            count += NPC.downedDeerclops ? 1 : 0;
            // 디어클롭스 처치 여부다

            count += Main.hardMode ? 1 : 0;
            // 하드모드 진입 여부다

            count += NPC.downedQueenSlime ? 1 : 0;
            // 퀸 슬라임 처치 여부다

            count += NPC.downedMechBoss1 ? 1 : 0;
            // 메카 보스 1 처치 여부다

            count += NPC.downedMechBoss2 ? 1 : 0;
            // 메카 보스 2 처치 여부다

            count += NPC.downedMechBoss3 ? 1 : 0;
            // 메카 보스 3 처치 여부다

            count += NPC.downedPlantBoss ? 1 : 0;
            // 플랜테라 처치 여부다

            count += NPC.downedGolemBoss ? 1 : 0;
            // 골렘 처치 여부다

            count += NPC.downedFishron ? 1 : 0;
            // 듀크 피쉬론 처치 여부다

            count += NPC.downedEmpressOfLight ? 1 : 0;
            // 빛의 여제 처치 여부다

            count += NPC.downedAncientCultist ? 1 : 0;
            // 루나틱 컬티스트 처치 여부다

            count += NPC.downedMoonlord ? 1 : 0;
            // 문로드 처치 여부다

            return count;
        }

        private static int GetCalamityBossCount()
        {
            // Same boss flags as Calamity's TheCommunity.CalculatePower(killsOnly: true).
            int count = 0;
            count += (NPC.downedSlimeKing) ? 1 : 0;
            count += (DownedBossSystem.downedDesertScourge) ? 1 : 0;
            count += (NPC.downedBoss1) ? 1 : 0;
            count += (DownedBossSystem.downedCrabulon) ? 1 : 0;
            count += (NPC.downedBoss2) ? 1 : 0;
            count += ((DownedBossSystem.downedHiveMind || DownedBossSystem.downedPerforator)) ? 1 : 0;
            count += (NPC.downedQueenBee) ? 1 : 0;
            count += (NPC.downedBoss3) ? 1 : 0;
            count += (NPC.downedDeerclops) ? 1 : 0;
            count += (DownedBossSystem.downedSlimeGod) ? 1 : 0;
            count += (Main.hardMode) ? 1 : 0;
            count += (NPC.downedQueenSlime) ? 1 : 0;
            count += (DownedBossSystem.downedCryogen) ? 1 : 0;
            count += (NPC.downedMechBoss1) ? 1 : 0;
            count += (DownedBossSystem.downedAquaticScourge) ? 1 : 0;
            count += (NPC.downedMechBoss2) ? 1 : 0;
            count += (DownedBossSystem.downedBrimstoneElemental) ? 1 : 0;
            count += (NPC.downedMechBoss3) ? 1 : 0;
            count += (DownedBossSystem.downedCalamitasClone) ? 1 : 0;
            count += (NPC.downedPlantBoss) ? 1 : 0;
            count += (DownedBossSystem.downedLeviathan) ? 1 : 0;
            count += (DownedBossSystem.downedAstrumAureus) ? 1 : 0;
            count += (NPC.downedGolemBoss) ? 1 : 0;
            count += (DownedBossSystem.downedPlaguebringer) ? 1 : 0;
            count += (NPC.downedFishron) ? 1 : 0;
            count += (NPC.downedEmpressOfLight) ? 1 : 0;
            count += (DownedBossSystem.downedRavager) ? 1 : 0;
            count += (NPC.downedAncientCultist) ? 1 : 0;
            count += (DownedBossSystem.downedAstrumDeus) ? 1 : 0;
            count += (NPC.downedMoonlord) ? 1 : 0;
            count += (DownedBossSystem.downedGuardians) ? 1 : 0;
            count += (DownedBossSystem.downedDragonfolly) ? 1 : 0;
            count += (DownedBossSystem.downedProvidence) ? 1 : 0;
            count += (DownedBossSystem.downedCeaselessVoid) ? 1 : 0;
            count += (DownedBossSystem.downedStormWeaver) ? 1 : 0;
            count += (DownedBossSystem.downedSignus) ? 1 : 0;
            count += (DownedBossSystem.downedPolterghast) ? 1 : 0;
            count += (DownedBossSystem.downedBoomerDuke) ? 1 : 0;
            count += (DownedBossSystem.downedDoG) ? 1 : 0;
            count += (DownedBossSystem.downedYharon) ? 1 : 0;
            count += (DownedBossSystem.downedExoMechs) ? 1 : 0;
            count += (DownedBossSystem.downedCalamitas) ? 1 : 0;
            return count;
        }
    }
}