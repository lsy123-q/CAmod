using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using System.Collections.Generic;
using System;

namespace CAmod.Players
{
    public class LeafWardPlayer2 : ModPlayer
    {
        public bool leafShieldEquipped;
        // 리프 실드 착용 여부다

        public HashSet<int> defeatedBossTypes = new();
        // 이 캐릭터가 최초 처치한 보스 타입 목록이다

        public int LeafShieldStage => Math.Min(defeatedBossTypes.Count, 40);
        // 성장 단계다 (최대 25)

        public override void ResetEffects()
        {
            leafShieldEquipped = false;
            // 착용 여부를 매 틱 초기화한다
        }

        public override void UpdateEquips()
        {
            if (!leafShieldEquipped)
                return;

            int count = LeafShieldStage;

            Player.GetDamage(DamageClass.Magic) += count * 0.005f;
            // 보스 1종당 마법 피해 0.5% 증가다
            
            Player.GetCritChance(DamageClass.Magic) += count * 0.25f;
            // 보스 1종당 마법 치명타 0.25% 증가다

            Player.statManaMax2 += count * 10;

            Player.statDefense += (int)(count*0.5f);
            // 보스 1종당 최대 마나 10 증가다
        }

        public override void SaveData(TagCompound tag)
        {
            tag["defeatedBossTypes"] = new List<int>(defeatedBossTypes);
            // 보스 처치 기록을 저장한다
        }

        public override void LoadData(TagCompound tag)
        {
            defeatedBossTypes = new HashSet<int>(
                tag.GetList<int>("defeatedBossTypes")
            );
            // 보스 처치 기록을 복원한다
        }
    }
}
