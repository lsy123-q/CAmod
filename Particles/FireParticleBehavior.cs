using System;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Microsoft.Xna.Framework;

namespace CAmod.Particles
{
    public class FireParticleBehavior : Behavior<ParticleInfo>
    {
        private static readonly Color[] Palette =
        {
            new Color(255, 255, 182),
            new Color(255, 242, 106),
            new Color(255, 171, 93),
            new Color(220, 220, 220),
            new Color(55, 55, 55)
        };

        public override string Texture => "CAmod/Particles/Fire1";

        public override void Initialize(ref ParticleInfo info)
        {
            // Initialize is called only after the library successfully reserves a buffer slot.
            if (info.Time > 0)
                FireParticleSystem.ParticleCreated();
        }

        public override void Update(ref ParticleInfo info)
        {
            if (ParticleCulling.Cull(ref info))
            {
                FireParticleSystem.ParticleExpired();
                return;
            }

            FireParticleSystem.PrepareUpdate();
            float acceleration = FireParticleSystem.TimeAcceleration;
            info.Position += info.Velocity * acceleration;
            info.Velocity *= MathF.Pow(0.94f, acceleration);

            float progress = info.Time / (float)info.Duration;

            Color fireColor = GetFireColor(progress);

            float alpha = progress * progress;

            // NonPremultiplied applies opacity to RGB during blending; fade alpha only.
            info.Color = new Color(fireColor.R, fireColor.G, fireColor.B,
                (byte)(255f * MathHelper.Clamp(alpha, 0f, 1f)));

            info.Time -= FireParticleSystem.LifetimeSteps;
            if (info.Time <= 0)
                FireParticleSystem.ParticleExpired();
        }
        protected virtual Color GetFireColor(float progress)
        {
            // Alpha fades as remainingLife^2, so accumulated visibility is 1 - remainingLife^3.
            // Equal color weights in visibility space bring the darker colors forward and hold them longer.
            float remainingLife = MathHelper.Clamp(progress, 0f, 1f);
            float visibleProgress = 1f - remainingLife * remainingLife * remainingLife;
            float palettePosition = MathHelper.Clamp(
                visibleProgress * Palette.Length - 0.5f, 0f, Palette.Length - 1);
            int colorIndex = Math.Min((int)palettePosition, Palette.Length - 2);
            return Color.Lerp(Palette[colorIndex], Palette[colorIndex + 1],
                palettePosition - colorIndex);

        }
    }
}