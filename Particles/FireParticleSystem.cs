using System;
using Terraria;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Core.V3;
using Terraria.ModLoader;
using ParticleLibrary.Core;

namespace CAmod.Particles
{
    public class FireParticleSystem : ModSystem
    {
        private const int CapacityPerBuffer = 10000;
        private const double NormalLoad = 10000d;
        public static int ActiveParticleCount { get; private set; }
        public static float TimeAcceleration { get; private set; } = 1f;
        internal static int LifetimeSteps { get; private set; } = 1;
        private static ulong lastUpdateTick = ulong.MaxValue;
        private static double lifetimeCredit;

        internal static void ParticleCreated()
        {
            ActiveParticleCount++;
        }

        internal static void ParticleExpired()
        {
            ActiveParticleCount = Math.Max(0, ActiveParticleCount - 1);
        }

        internal static void PrepareUpdate()
        {
            if (lastUpdateTick == Main.GameUpdateCount)
                return;
            lastUpdateTick = Main.GameUpdateCount;
            // Snapshot once for all six buffers, after emission and before their updates.
            double acceleration = Math.Max(1d, ActiveParticleCount / NormalLoad);
            TimeAcceleration = (float)acceleration;
            lifetimeCredit += acceleration;
            LifetimeSteps = Math.Max(1, (int)Math.Floor(lifetimeCredit + 1e-9));
            lifetimeCredit = Math.Max(0d, lifetimeCredit - LifetimeSteps);
        }

        private static void ResetLoadTracking()
        {
            ActiveParticleCount = 0;
            TimeAcceleration = 1f;
            LifetimeSteps = 1;
            lifetimeCredit = 0d;
            lastUpdateTick = ulong.MaxValue;
        }

        public static ParticleBuffer<FireParticleBehavior> Buffer1;
        public static ParticleBuffer<FireParticleBehavior2> Buffer2;
        public static ParticleBuffer<FireParticleBehavior3> Buffer3;

        public static ParticleBuffer<AdditiveFireParticleBehavior> AdditiveBuffer1;
        public static ParticleBuffer<AdditiveFireParticleBehavior2> AdditiveBuffer2;
        public static ParticleBuffer<AdditiveFireParticleBehavior3> AdditiveBuffer3;

        public override void OnModLoad()
        {
            ResetLoadTracking();
            Buffer1 = new ParticleBuffer<FireParticleBehavior>(CapacityPerBuffer)
                .SetBlendState(Microsoft.Xna.Framework.Graphics.BlendState.NonPremultiplied);

            Buffer2 = new ParticleBuffer<FireParticleBehavior2>(CapacityPerBuffer)
                .SetBlendState(Microsoft.Xna.Framework.Graphics.BlendState.NonPremultiplied);

            Buffer3 = new ParticleBuffer<FireParticleBehavior3>(CapacityPerBuffer)
                .SetBlendState(Microsoft.Xna.Framework.Graphics.BlendState.NonPremultiplied);

            // Same textures and motion; additive variants retain the original long gray palette.
            AdditiveBuffer1 = new ParticleBuffer<AdditiveFireParticleBehavior>(CapacityPerBuffer)
                .SetBlendState(Microsoft.Xna.Framework.Graphics.BlendState.Additive);
            AdditiveBuffer2 = new ParticleBuffer<AdditiveFireParticleBehavior2>(CapacityPerBuffer)
                .SetBlendState(Microsoft.Xna.Framework.Graphics.BlendState.Additive);
            AdditiveBuffer3 = new ParticleBuffer<AdditiveFireParticleBehavior3>(CapacityPerBuffer)
                .SetBlendState(Microsoft.Xna.Framework.Graphics.BlendState.Additive);

            ParticleManagerV3.RegisterUpdatable(AdditiveBuffer1);
            ParticleManagerV3.RegisterUpdatable(AdditiveBuffer2);
            ParticleManagerV3.RegisterUpdatable(AdditiveBuffer3);

            ParticleManagerV3.RegisterUpdatable(Buffer1);
            ParticleManagerV3.RegisterUpdatable(Buffer2);
            ParticleManagerV3.RegisterUpdatable(Buffer3);

            ParticleManagerV3.RegisterRenderable(
    Layer.BeforeNPCsBehindTiles,
    Buffer1
);

            ParticleManagerV3.RegisterRenderable(
                Layer.BeforeNPCsBehindTiles,
                Buffer2
            );

            ParticleManagerV3.RegisterRenderable(
                Layer.BeforeNPCsBehindTiles,
                Buffer3
            );
            // Draw the glowing particles after the translucent particles on the same layer.
            ParticleManagerV3.RegisterRenderable(Layer.BeforeNPCsBehindTiles, AdditiveBuffer1);
            ParticleManagerV3.RegisterRenderable(Layer.BeforeNPCsBehindTiles, AdditiveBuffer2);
            ParticleManagerV3.RegisterRenderable(Layer.BeforeNPCsBehindTiles, AdditiveBuffer3);
        }

        public override void Unload()
        {
            ResetLoadTracking();
            Buffer1 = null;
            Buffer2 = null;
            Buffer3 = null;
            AdditiveBuffer1 = null;
            AdditiveBuffer2 = null;
            AdditiveBuffer3 = null;
        }
    }
}