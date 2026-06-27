using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CAmod.NPCs;
namespace CAmod.Players
{
    public class RabbitLauncherPlayer : ModPlayer
    {
        // 장신구 착용 여부
        public bool rabbitLauncher;

        // 현재 토끼 스택 수
        public int rabbitCount;

        // 충전 타이머
        public int rabbitTimer;

        // 자동 발사 타이머
        public int rabbitShootTimer;

        // 현재 발사 대상
        public int rabbitTarget = -1;

        public override void ResetEffects()
        {
            rabbitLauncher = false;
        }

        public override void PostUpdate()
        {
            // 장신구 미착용 시 초기화한다
            if (!rabbitLauncher)
            {
                rabbitCount = 0;
                rabbitTimer = 0;
                rabbitShootTimer = 0;
                rabbitTarget = -1;
                return;
            }

            rabbitTimer++;

            // 5초마다 토끼를 충전한다
            if (rabbitTimer >= 300)
            {
                rabbitTimer = 0;

                // 최대 5개까지 저장 가능하다
                if (rabbitCount < 5)
                {
                    rabbitCount++;
                }
            }

            // =========================
            // 예약 발사 처리
            // =========================

            // 발사 대상 없으면 종료
            if (rabbitTarget == -1)
            {
                return;
            }

            // 스택 다 썼으면 종료
            if (rabbitCount <= 0)
            {
                rabbitTarget = -1;
                rabbitShootTimer = 0;
                return;
            }

            // 타겟 유효성 검사
            if
            (
                rabbitTarget < 0 ||
                rabbitTarget >= Main.maxNPCs ||
                !Main.npc[rabbitTarget].active ||
                Main.npc[rabbitTarget].life <= 0
            )
            {
                rabbitTarget = -1;
                rabbitShootTimer = 0;
                return;
            }

            rabbitShootTimer++;

            // 0.25초마다 한마리씩 발사한다
            if (rabbitShootTimer >= 15)
            {
                rabbitShootTimer = 0;

                rabbitCount--;

                NPC.NewNPC
                (
                    Player.GetSource_FromThis(),
                    (int)Player.Center.X,
                    (int)Player.Center.Y,
                    ModContent.NPCType<RabbitMissileNPC>(),
                    ai0: rabbitTarget
                );

                // 다 쐈으면 종료
                if (rabbitCount <= 0)
                {
                    rabbitTarget = -1;
                }
            }
        }

        public override void OnHitNPCWithProj
        (
            Projectile proj,
            NPC target,
            NPC.HitInfo hit,
            int damageDone
        )
        {
            // 장신구 미착용 시 무시한다
            if (!rabbitLauncher)
            {
                return;
            }

            // 마법 투사체만 허용한다
            if (proj.DamageType != DamageClass.Magic)
            {
                return;
            }

            // 적만 대상이다
            if (target.friendly || target.lifeMax <= 5)
            {
                return;
            }

            // 토끼 없으면 종료
            if (rabbitCount <= 0)
            {
                return;
            }

            // 이미 예약중이면 무시한다
            if (rabbitTarget != -1)
            {
                return;
            }

            // 발사 대상 지정한다
            rabbitTarget = target.whoAmI;

            // 즉시 1발 나가게 초기화한다
            rabbitShootTimer = 15;
        }
    }
}