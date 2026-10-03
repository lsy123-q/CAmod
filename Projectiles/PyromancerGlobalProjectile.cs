using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CAmod.Players;
using CAmod.Particles;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;

namespace CAmod.Globals
{
    public class PyromancerGlobalProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => false;
        // 개별 인스턴스 저장 안하므로 false로 둔다

        private static int dustSpawnCounter = 0;
        // 생성된 파티클 수를 저장한다

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

            if (Main.rand.NextBool(cnt))
            {
                if (!projectile.active)
                    return;

                if (projectile.owner != Main.myPlayer)
                    return;
                // 내 소유가 아니면 종료한다 (멀티플레이 안전)

                if (!player.active)
                    return;

                if (projectile.alpha >= 225)
                    return;

                if (!mp.HasConduitEquipped)
                    return;
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

                if (!projectile.friendly)
                    return;

                if (projectile.damage <= 0)
                    return;

                if (projectile.minion)
                    return;

                if (projectile.DamageType != DamageClass.Magic)
                    return;
                // 마법 투사체가 아니면 종료한다

                // 투사체 중심에서 이동 방향 반대쪽으로 약간 뒤에 생성한다
                Vector2 trailPos =
                    projectile.Center - projectile.velocity * 0.5f;

                // 밝은 노랑과 주황색 중 하나를 선택한다
                Color color = Main.rand.NextBool()
                    ? new Color(255, 230, 80)
                    : new Color(255, 140, 40);

                // 투사체 중심 주변에 약간의 랜덤 위치를 추가한다
                Vector2 spawnPos =
                    trailPos +
                    Main.rand.NextVector2Circular(
                        projectile.width * 0.5f,
                        projectile.height * 0.5f
                    );

                // 투사체 속도를 어느 정도 따라가게 한다
                Vector2 velocity =
                    projectile.velocity * 0.5f +
                    Main.rand.NextVector2Circular(0.5f, 0.5f);

                // 파티클 크기를 약간 랜덤하게 만든다
                float size = Main.rand.NextFloat(7f, 10f);

                // V3 파티클을 생성한다
                CosmicParticleSystem.Buffer.Create(
                    new ParticleInfo(
                        position: spawnPos.ToNumerics(),
                        velocity: velocity.ToNumerics(),
                        rotation: projectile.velocity.ToRotation(),
                        scale: new System.Numerics.Vector2(
                            size,
                            size
                        ),
                        depth: 1f,
                        color: color,
                        duration: Main.rand.Next(32, 54)
                    )
                );

                dustSpawnCounter++;
                // 생성된 파티클 수를 증가시킨다

                mp.totalDustCounter++;
                mp.dustThisFrame++;

                dustTimer++;

                Player player2 = Main.player[projectile.owner];
                var mp2 = player2.GetModPlayer<PyromancerPlayer>();

                mp2.totalDustCounter++;
                // 생성될 때마다 플레이어 누적 카운트 증가
            }
        }
    }
}