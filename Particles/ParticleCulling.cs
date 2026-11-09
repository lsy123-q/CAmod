using Microsoft.Xna.Framework;
using ParticleLibrary.Core.V3.Particles;
using Terraria;

namespace CAmod.Particles
{
    internal static class ParticleCulling
    {
        private const float ScreenMargin = 500f;

        internal static bool Cull(ref ParticleInfo info)
        {
            if (Main.dedServ) return false;

            Vector2 worldPosition = new Vector2(info.Position.X, info.Position.Y);
            Vector2 screenPosition = Vector2.Transform(
                worldPosition - Main.screenPosition, Main.GameViewMatrix.TransformationMatrix);
            if (screenPosition.X > -ScreenMargin && screenPosition.X < Main.screenWidth + ScreenMargin
                && screenPosition.Y > -ScreenMargin && screenPosition.Y < Main.screenHeight + ScreenMargin)
                return false;

            info.Time = 0;
            info.Color = Color.Transparent;
            return true;
        }
    }
}
