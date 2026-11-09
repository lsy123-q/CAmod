using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using System;
using Terraria;
using Microsoft.Xna.Framework;

namespace CAmod.Particles
{
    public class BloodParticleBehavior : Behavior<ParticleInfo>
    {
        public override string Texture => "CAmod/Particles/blood";

        private bool hasLight;
        private float sizeMultiplier;
        private int particleDuration;

        public override void Initialize(ref ParticleInfo info)
        {
            // 파티클 크기를 원본의 50%~150% 사이에서 무작위로 결정한다
            sizeMultiplier = Main.rand.NextFloat(0.8f, 1.33f);

            info.Scale *= sizeMultiplier;

            // 큰 파티클일수록 오래 유지되도록 수명을 크기에 비례시킨다
            particleDuration = (int)(info.Duration * sizeMultiplier);

            info.Time = particleDuration;

            // 25% 확률로 광원을 가진다
            hasLight = Main.rand.NextBool(25);
        }

        public override void Update(ref ParticleInfo info)
        {
            if (ParticleCulling.Cull(ref info)) return;

            info.Position += info.Velocity;

            info.Velocity *= 0.94f;

            float progress =
                info.Time / (float)particleDuration;

            Color lightColor =
                info.InitialColor * progress;
            
            if (hasLight)
            {
                Lighting.AddLight(
                    new Vector2(info.Position.X, info.Position.Y),
                    0.65f,
                    0.02f,
                    0.02f
                );
            }
            
            // 이동 속도에 비례해서 진행 방향으로 길게 늘린다
            float speed = info.Velocity.Length();

            float stretch =
                1f + speed * 0.25f;

            // X축을 진행 방향으로 늘리고 Y축은 원래 크기를 유지한다
            info.Scale = new System.Numerics.Vector2(
                info.InitialScale.X * stretch * progress,
                info.InitialScale.Y * progress
            );

            // 파티클의 X축이 이동 방향을 바라보게 한다
            if (speed > 0.001f)
            {
                info.Rotation =
                    info.Velocity.ToRotation();
            }

            info.Color =
                info.InitialColor * progress;

            info.Time--;
        }
    }
}