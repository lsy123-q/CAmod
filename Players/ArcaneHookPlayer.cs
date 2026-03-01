using Humanizer;
using Microsoft.Xna.Framework;

using Terraria;
using CAmod.Projectiles;
using Terraria.ModLoader;


namespace CAmod.Players
{
    public class ArcaneHookPlayer : ModPlayer
    {
        public bool hookPulling = false;          // 당기는 중인지
        public Vector2 hookTarget;        // 끌어갈 좌표
        public float hookSpeed;           // 속도

        public override void PostUpdate()
        {
            if (hookPulling) { 
            Vector2 to = hookTarget - Player.Center;
            float dist = to.Length();
            if (dist <= 5f) {
                Player.Center = hookTarget;
            }
            }
        }
        private bool HasAliveCoreProjectile()
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];

                if (p.active &&
                    p.owner == Player.whoAmI &&
                    p.type == ModContent.ProjectileType<ArcaneHookCore>())
                {
                    return true; // 살아있는 코어가 존재한다
                }
            }

            return false; // 없다
        }
        public override void PreUpdateMovement()
        {
            base.PreUpdateMovement();
            if (hookPulling && !HasAliveCoreProjectile())
            {
                hookPulling = false;        // 코어가 없으면 당김 상태 해제한다
                hookTarget = Vector2.Zero;  // 목표 좌표 초기화한다
                Player.velocity = Vector2.Zero; // 잔여 속도 제거한다
            }
            if (!hookPulling || !HasAliveCoreProjectile())
                return;
            Player.gravity = 0f;
            Vector2 to = hookTarget - Player.Center;
            float dist = to.Length();
            Player.maxFallSpeed = 60f;
            float moveLength = hookSpeed;



           
            // 남은 거리보다 속도가 크면 정확히 그 거리만큼만 이동하게 속도 잘라낸다
            if (moveLength > dist)
            {
                hookSpeed = dist; // 이번 프레임은 남은 거리만큼만 이동한다
            }

            if (dist <= 5f)
            {
                Player.Center = hookTarget; // 정확히 목표 위치로 보낸다
                Player.velocity = Vector2.Zero; // 잔여 속도 제거한다

                Player.maxFallSpeed = 0f;
         
            }

            else
            {
                to.Normalize();

                Player.gravity = 0f;

                Player.velocity = to * hookSpeed;
            }
             

                return; // 🔥 이게 핵심이다
            
        }
    }
}
