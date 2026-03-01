using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CAmod.Players;

namespace CAmod.Globals
{
    public class PyromancerGlobalProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => false;
        // 개별 인스턴스 저장 안하므로 false로 둔다
        private static int dustSpawnCounter = 0;
        // 1초 동안 생성된 더스트 수를 저장한다

        private static int dustTimer = 0;
        // 1초 타이머이다
        public override void AI(Projectile projectile)
        {



            Player player = Main.player[projectile.owner];
            var mp = player.GetModPlayer<PyromancerPlayer>();
            int cnt = mp.dustThisFrame / 50;
            if (cnt <= 2)
                cnt = 2;
           
                if (!mp.visualEnabled)
                return;
            if (Main.rand.NextBool(cnt)) // 과도한 생성을 방지한다
            {

                if (!projectile.active) return;
            if (projectile.owner != Main.myPlayer) return;
            // 내 소유가 아니면 종료한다 (멀티플레이 안전)


            if (!player.active) return;
            if (projectile.alpha >= 225)
                return;

            if (!mp.HasConduitEquipped) return;
            // 장신구 미착용시 종료한다
            string projName = Lang.GetProjectileName(projectile.type).Value;

            string internalName = ProjectileID.Search.GetName(projectile.type);

            // LastPrism 포함 시 제외한다
            if (internalName.Contains("LastPrism"))
                return;
            if (projectile.ModProjectile != null)
            {
                string modName = projectile.ModProjectile.Name;


            }


            // 표시 이름이 정확히 "Crystal"이면 제외한다
            if (projName == "Crystal")
                return;
            if (!projectile.friendly) return;
            if (projectile.damage <= 0) return;
            if (projectile.minion) return;

            if (projectile.DamageType != DamageClass.Magic) return;

            // 마법 투사체가 아니면 종료한다
            // 소환수는 제외하고 싶으면 유지한다

            // 마법만 하고싶으면 이 줄 추가
            // if (projectile.DamageType != DamageClass.Magic) return;

          
            // 현재 투사체의 속도를 구한다

            

           
                



                    Vector2 trailPos = projectile.position - projectile.velocity * 0.5f;

                   

                    // 이동 경로 약간 뒤에 생성한다

                    int dustType = Main.rand.Next(3) switch
                    {
                        0 => DustID.CopperCoin,   // 구리 코인 더스트
                        1 => DustID.GoldCoin,     // 금 코인 더스트
                        _ => DustID.IchorTorch    // 이코르 횃불 더스트
                    };

                    int dustIndex = Dust.NewDust(
                        trailPos,
                        projectile.width,
                        projectile.height,
                        dustType,
                        0f,
                        0f,
                        120,
                        default,
                        0.9f
                    );

                    Dust d = Main.dust[dustIndex];
                    d.noGravity = true; // 중력을 제거한다
                    d.velocity *= 0.5f;

                    dustSpawnCounter++;
                    // 중력을 제거한다
                    mp.totalDustCounter++;
                    mp.dustThisFrame++;

                    // 투사체 속도를 일부 따라가게 한다
                
                dustTimer++;

                Player player2 = Main.player[projectile.owner];
                var mp2 = player2.GetModPlayer<PyromancerPlayer>();

                mp2.totalDustCounter++;
                // 생성될 때마다 플레이어 누적 카운트 증가
            }
        }
    }
}