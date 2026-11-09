using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace CAmod.Systems
{
    // Retain loaded assets for every draw; dedicated servers never request textures.
    public class UltraCosmicDivideAssets : ModSystem
    {
        internal static Asset<Texture2D> ItemTexture;
        internal static Asset<Texture2D> TelegraphTexture;
        internal static Asset<Effect> CoreShader;
        internal static Asset<Effect> GlowShader;

        public override void Load()
        {
            if (Main.dedServ)
                return;

            ItemTexture = ModContent.Request<Texture2D>("CAmod/Items/Weapons/cosmic", AssetRequestMode.ImmediateLoad);
            TelegraphTexture = ModContent.Request<Texture2D>("CAmod/Projectiles/UltraCosmicBeam", AssetRequestMode.ImmediateLoad);
            CoreShader = ModContent.Request<Effect>("CAmod/Effects/CosmicDivideCore", AssetRequestMode.ImmediateLoad);
            GlowShader = ModContent.Request<Effect>("CAmod/Effects/CosmicDivideGlow", AssetRequestMode.ImmediateLoad);
        }

        public override void Unload()
        {
            ItemTexture = null;
            TelegraphTexture = null;
            CoreShader = null;
            GlowShader = null;
        }
    }
}