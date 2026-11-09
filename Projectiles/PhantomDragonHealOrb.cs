using System;
using CAmod.Particles;
using Microsoft.Xna.Framework;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CAmod.Projectiles
{
    public class PhantomDragonHealOrb : ModProjectile
    {
        private static readonly Color TrailColor = new Color(0x90, 0xB6, 0xE2);
        private float distanceRemainder;

        public override string Texture => "CAmod/Particles/Star";

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 14;
            Projectile.timeLeft = 1000;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 3;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hide = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }

            Vector2 offset = BloodHealOrb.GetHomingOffset(Projectile, player);
            if (offset.LengthSquared() < 25f * 25f)
            {
                if (Main.myPlayer == Projectile.owner)
                {
                    int missingLife = player.statLifeMax2 - player.statLife;
                    float missingRatio = MathHelper.Clamp(missingLife / (float)player.statLifeMax2, 0f, 1f);
                    int heal = Math.Min(missingLife, (int)MathF.Round(MathHelper.Lerp(1f, 10f, missingRatio)));
                    if (heal > 0)
                    {
                        player.statLife += heal;
                        if (Main.netMode == NetmodeID.MultiplayerClient)
                            NetMessage.SendData(MessageID.PlayerLifeMana, number: player.whoAmI);
                    }
                    EmitHealBurst(Projectile.Center);
                    Projectile.Kill();
                }
                return;
            }

            BloodHealOrb.SteerTowards(Projectile, offset);
            if (Main.dedServ) return;

            const float spacing = 2.5f;
            float distance = Projectile.velocity.Length();
            for (float step = spacing - distanceRemainder; step <= distance; step += spacing)
            {
                Vector2 position = Projectile.Center + Projectile.velocity * (step / distance);
                CosmicParticleSystem.Buffer.Create(new ParticleInfo(
                    position: position.ToNumerics(),
                    velocity: System.Numerics.Vector2.Zero,
                    rotation: Projectile.velocity.ToRotation(),
                    scale: new System.Numerics.Vector2(5.5f, 6.5f),
                    depth: 1f,
                    color: TrailColor,
                    duration: 60));
            }
            distanceRemainder = (distanceRemainder + distance) % spacing;
        }


        private static void EmitHealBurst(Vector2 center)
        {
            const int count = 35;
            const float radius = 7f;
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                float x = MathF.Cos(angle);
                float y = MathF.Sin(angle);
                Vector2 shape = new Vector2(x * x * x, y * y * y);
                Vector2 position = center + shape * radius;
                Vector2 velocity = shape * 1.5f;
                CosmicParticleSystem.Buffer.Create(new ParticleInfo(
                    position: position.ToNumerics(),
                    velocity: velocity.ToNumerics(),
                    rotation: velocity.ToRotation(),
                    scale: new System.Numerics.Vector2(6f, 6f),
                    depth: 1f,
                    color: TrailColor,
                    duration: 60));
            }
        }
    }
}
