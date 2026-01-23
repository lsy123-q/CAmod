using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;
using CAmod.Players;

namespace CAmod.Projectiles
{
    public class DimGateGlobalProjectile : GlobalProjectile
    {
        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            if (projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
                return;

            Player p = Main.player[projectile.owner];
            if (!p.active)
                return;

            DimGatePlayer dp = p.GetModPlayer<DimGatePlayer>();

            projectile.localAI[0] = dp.gateGeneration;
            // 투사체 생성 당시 게이트 세대를 기록한다
        }
    }
}
