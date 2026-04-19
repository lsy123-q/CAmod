using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using CAmod.Players;
using Terraria.ID;

namespace CAmod.NPCs
{
    public class AdaptationSensorNPC : ModNPC
    {
        public override string Texture => "Terraria/Images/NPC_0"; // 투명 텍스처

        public override void SetStaticDefaults()
        {
            NPCID.Sets.NPCBestiaryDrawModifiers value = new() { Hide = true };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
        }

        public override void SetDefaults()
        {
            NPC.width = 1920;  // 감지 영역 1000px
            NPC.height = 1080;
            NPC.damage = 0;
            NPC.defense = 0;
            NPC.lifeMax = 1;
            NPC.aiStyle = -1;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.dontTakeDamage = false; // 판정을 위해 false
            NPC.friendly = true;        // 아군 판정
            NPC.alpha = 255;            // 완전 투명
        }

        public override void AI()
        {
            // 소환한 주인(플레이어) 찾기
            Player owner = Main.player[(int)NPC.ai[0]];
            if (!owner.active || owner.dead || !owner.GetModPlayer<HarmonyCyclePlayer>().structureActive)
            {
                NPC.active = false;
                return;
            }

            // 플레이어 위치에 고정
            NPC.Center = owner.Center;

            // 무적 프레임 매 프레임 초기화 (매 프레임 모든 공격을 감지하기 위함)
            for (int i = 0; i < NPC.immune.Length; i++)
            {
                NPC.immune[i] = 0;
            }
        }

        // 투사체 감지
        public override bool? CanBeHitByProjectile(Projectile projectile)
        {
            RegisterThreat(projectile.type, isProjectile: true);
            return false; // 투사체 파괴 방지 및 데미지 무시
        }

        // NPC 접촉 감지
        public override bool CanBeHitByNPC(NPC attacker)
        {
            RegisterThreat(attacker.type, isProjectile: false);
            return false; // 데미지 무시
        }

        private void RegisterThreat(int type, bool isProjectile)
        {
            Player owner = Main.player[(int)NPC.ai[0]];
            var modPlayer = owner.GetModPlayer<HarmonyCyclePlayer>();

            int sourceID = isProjectile ? -(type + 10000) : type;

            // Player 클래스의 totalDamage나 별도의 감지 리스트에 등록
            // 여기서는 단순히 '존재함'을 알리기 위해 최소 데미지 1로 기록
            if (!modPlayer.totalDamage.ContainsKey(sourceID))
            {
                modPlayer.totalDamage[sourceID] = 1;
            }
        }
    }
}