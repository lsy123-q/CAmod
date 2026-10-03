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
    public class BloodParticleSystem : ModSystem
    {
        public static ParticleBuffer<BloodParticleBehavior> Buffer;

        public override void OnModLoad()
        {
            Buffer = new ParticleBuffer<BloodParticleBehavior>(50000)
    .SetBlendState(Microsoft.Xna.Framework.Graphics.BlendState.AlphaBlend);

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
