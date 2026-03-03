using Humanizer;
using Microsoft.Xna.Framework;
using Terraria;
using CAmod.Projectiles;
using Terraria.ModLoader;
using Terraria.ID;

namespace CAmod.Players
{
    public class ArcaneHookPlayer : ModPlayer
    {
        public bool hookPulling = false;
        public Vector2 hookTarget;
        public float hookSpeed;

        // 텔레포트 감지용 변수들
        public Vector2 lastPosition;      // 이전 프레임 위치
        public bool wasTeleporting;       // 이전 프레임 텔레포트 상태
        public int teleportCooldown = 0;  // 텔레포트 후 쿨다운

        public override void PreUpdate()
        {
            // 방법 1: 순간 이동 거리 체크 (가장 신뢰성 높음)
            Vector2 positionDelta = Player.Center - lastPosition;
            float distanceMoved = positionDelta.Length();

            // 정상적인 이동 속도를 훨씬 초과하는 순간 이동 감지
            // (일반 최대 속도 ~100픽셀/프레임, 텔레포트는 수천~수만 픽셀)
            bool suddenTeleport = distanceMoved > 100f && Player.velocity.Length() < distanceMoved * 0.5f;

            // 방법 2: 공식 teleporting 플래그
            bool isTeleporting = Player.teleporting;

            // 방법 3: 텔레포트 관련 버프/디버프 체크
            bool hasTeleportBuff = Player.HasBuff(BuffID.ChaosState) ||  // 카오스 엘레멘탈 관련
                                  Player.HasBuff(BuffID.PotionSickness); // 리콜 포션 등

            // 방법 4: 특정 아이템 사용 중 체크
            bool usingTeleportItem = Player.itemAnimation > 0 &&
                (Player.HeldItem.type == ItemID.MagicMirror ||
                 Player.HeldItem.type == ItemID.IceMirror ||
                 Player.HeldItem.type == ItemID.CellPhone ||
                 Player.HeldItem.type == ItemID.RecallPotion);

            // 종합 텔레포트 감지
            if (isTeleporting || suddenTeleport || (wasTeleporting && !isTeleporting))
            {
                KillAllArcaneHooks();
                hookPulling = false;
                hookTarget = Vector2.Zero;
                teleportCooldown = 10; // 10프레임 쿨다운
            }

            wasTeleporting = isTeleporting;
            lastPosition = Player.Center;
        }

        public override void PostUpdate()
        {
            // 쿨다운 감소
            if (teleportCooldown > 0)
            {
                teleportCooldown--;
                // 쿨다운 중에는 훅 사용 불가 또는 훅 정리
                if (hookPulling)
                {
                    KillAllArcaneHooks();
                    hookPulling = false;
                    hookTarget = Vector2.Zero;
                }
            }

            if (hookPulling)
            {
                Vector2 to = hookTarget - Player.Center;
                float dist = to.Length();
                if (dist <= 5f)
                {
                    Player.Center = hookTarget;
                }
            }
        }

        public override void PreUpdateMovement()
        {
            // 텔레포트 쿨다운 중에는 훅 무브먼트 무시
            if (teleportCooldown > 0)
            {
                hookPulling = false;
                return;
            }

            if (hookPulling && !HasAliveCoreProjectile())
            {
                hookPulling = false;
                hookTarget = Vector2.Zero;
                Player.velocity = Vector2.Zero;
                return;
            }

            if (!hookPulling || !HasAliveCoreProjectile())
                return;

            // 기존 무브먼트 코드...
            Player.gravity = 0f;
            Vector2 to = hookTarget - Player.Center;
            float dist = to.Length();
            Player.maxFallSpeed = 60f;
            float moveLength = hookSpeed;

            if (moveLength > dist)
            {
                hookSpeed = dist;
            }

            if (dist <= 5f)
            {
                Player.Center = hookTarget;
                Player.velocity = Vector2.Zero;
                Player.maxFallSpeed = 0f;
            }
            else
            {
                to.Normalize();
                Player.velocity = to * hookSpeed;
            }
        }

        private void KillAllArcaneHooks()
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (!proj.active || proj.owner != Player.whoAmI)
                    continue;

                if (proj.type == ModContent.ProjectileType<ArcaneHookCore>())
                {
                    proj.Kill();
                }
            }
        }

        private bool HasAliveCoreProjectile()
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == Player.whoAmI &&
                    p.type == ModContent.ProjectileType<ArcaneHookCore>())
                {
                    return true;
                }
            }
            return false;
        }
    }
}