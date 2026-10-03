using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Core.V3;
using Terraria.ModLoader;
using ParticleLibrary.Core;

namespace CAmod.Particles
{
    public class FireParticleSystem : ModSystem
    {
        public static ParticleBuffer<FireParticleBehavior> Buffer1;
        public static ParticleBuffer<FireParticleBehavior2> Buffer2;
        public static ParticleBuffer<FireParticleBehavior3> Buffer3;

        public override void OnModLoad()
        {
            Buffer1 = new ParticleBuffer<FireParticleBehavior>(20000)
                .SetBlendState(Microsoft.Xna.Framework.Graphics.BlendState.Additive);

            Buffer2 = new ParticleBuffer<FireParticleBehavior2>(20000)
                .SetBlendState(Microsoft.Xna.Framework.Graphics.BlendState.Additive);

            Buffer3 = new ParticleBuffer<FireParticleBehavior3>(20000)
                .SetBlendState(Microsoft.Xna.Framework.Graphics.BlendState.Additive);

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
        }

        public override void Unload()
        {
            Buffer1 = null;
            Buffer2 = null;
            Buffer3 = null;
        }
    }
}