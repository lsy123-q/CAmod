using CAmod.Projectiles;
using Terraria;
using Terraria.ModLoader;

namespace CAmod.Systems
{
    /// <summary>
    /// 투사체 AI 갱신이 끝난 뒤 봉인 투사체의 위치를 소유 플레이어에 다시 맞춘다.
    /// </summary>
    public sealed class SimpleAlchemicalSealFollowSystem : ModSystem
    {
        public override void PostUpdateEverything()
        {
            int sealType = ModContent.ProjectileType<SimpleAlchemicalSealProjectile>();

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile seal = Main.projectile[i];
                if (!seal.active || seal.type != sealType)
                    continue;

                int owner = seal.owner;
                if (owner < 0 || owner >= Main.maxPlayers)
                {
                    seal.Kill();
                    continue;
                }

                Player player = Main.player[owner];
                if (!player.active || player.dead)
                {
                    seal.Kill();
                    continue;
                }

                seal.Center = player.Center;
                seal.timeLeft = 2;
            }
        }
    }
}
