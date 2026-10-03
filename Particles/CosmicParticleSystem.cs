using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Core.V3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using ParticleLibrary.Core;

namespace CAmod.Particles
{
    public class CosmicParticleSystem : ModSystem
    {
        public static ParticleBuffer<CosmicParticleBehavior> Buffer;

        public override void OnModLoad()
        {
            Buffer = new ParticleBuffer<CosmicParticleBehavior>(50000)
    .SetBlendState(Microsoft.Xna.Framework.Graphics.BlendState.Additive);

            ParticleManagerV3.RegisterUpdatable(Buffer);
            ParticleManagerV3.RegisterRenderable(
    Layer.BeforeDust,
    Buffer
);
        }

        public override void Unload()
        {
            Buffer = null;
        }
    }
}
