using System;
using Microsoft.Xna.Framework;

namespace CAmod.Particles
{
    public class AdditiveFireParticleBehavior : FireParticleBehavior
    {
        protected override Color GetFireColor(float progress)
        {
            Color fireColor;

            // 검은 회색 35%
            // 회색 25%
            // 주황 20%
            // 노랑 13%
            // 밝은 노랑 7%

            if (progress > 0.93f)
            {
                float t = (progress - 0.93f) / 0.07f;
                t = 1f - MathF.Pow(1f - t, 85f);

                fireColor = Color.Lerp(
                    new Color(255, 255, 182),
                    new Color(255, 242, 106),
                    t
                );
            }
            else if (progress > 0.80f)
            {
                float t = (progress - 0.80f) / 0.13f;
                t = 1f - MathF.Pow(1f - t, 85f); 

                fireColor = Color.Lerp(
                    new Color(255, 242, 106),
                    new Color(255, 171, 93),
                    t
                );
            }
            else if (progress > 0.60f)
            {
                float t = (progress - 0.60f) / 0.20f;
                t = 1f - MathF.Pow(1f - t, 85f);

                fireColor = Color.Lerp(
                    new Color(220, 220, 220),
                    new Color(75, 75, 75),
                    t
                );
            }
            else if (progress > 0.35f)
            {
                float t = (progress - 0.35f) / 0.25f;
                t = 1f - MathF.Pow(1f - t, 85f);

                fireColor = Color.Lerp(
                    new Color(220, 220, 220),
                    new Color(55, 55, 55),
                    t
                );
            }
            else
            {
                fireColor = new Color(55, 55, 55);
            }

           

            

            return fireColor;
        }
    }

    public class AdditiveFireParticleBehavior2 : AdditiveFireParticleBehavior
    {
        public override string Texture => "CAmod/Particles/Fire2";
    }

    public class AdditiveFireParticleBehavior3 : AdditiveFireParticleBehavior
    {
        public override string Texture => "CAmod/Particles/Fire3";
    }
}
