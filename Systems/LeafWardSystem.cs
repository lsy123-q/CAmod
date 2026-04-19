using Terraria;
using Terraria.ModLoader;
using System;
using System.Reflection;

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
            if (!ModLoader.TryGetMod("CalamityMod", out Mod calamity))
                return 0;
            // 칼라미티 없으면 0 반환한다

            try
            {
                Type communityType = calamity.Code.GetType("CalamityMod.Items.Accessories.TheCommunity");
                // Community 클래스 가져온다

                if (communityType == null)
                    return 0;

                MethodInfo method = communityType.GetMethod("CalculatePower", BindingFlags.NonPublic | BindingFlags.Static);
                // 내부 CalculatePower 메서드 가져온다

                if (method == null)
                    return 0;

                object result = method.Invoke(null, new object[] { true });
                // killsOnly = true로 호출한다 (보스 처치 비율 반환)

                float ratio = (float)result;

                int totalBosses = 42;
                // 칼라미티 내부 총 보스 수다

                return (int)(ratio * totalBosses);
                // 비율을 실제 보스 수로 환산한다
            }
            catch
            {
                return 0;
                // 실패하면 안전하게 0 반환한다
            }
        }
    }
}