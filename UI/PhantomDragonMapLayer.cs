using System;
using System.Collections.Generic;
using System.Reflection;
using CAmod.NPCs;
using CAmod.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.Map;
using Terraria.ModLoader;

namespace CAmod.UI
{
    public sealed class PhantomDragonMapLayer : ModMapLayer
    {
        // Read the vanilla overlay transform so rotated sprites follow the player's map zoom.
        private const BindingFlags MapFields = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly FieldInfo MapPosition = typeof(MapOverlayDrawContext).GetField("_mapPosition", MapFields);
        private static readonly FieldInfo MapOffset = typeof(MapOverlayDrawContext).GetField("_mapOffset", MapFields);
        private static readonly FieldInfo MapScale = typeof(MapOverlayDrawContext).GetField("_mapScale", MapFields);
        private static readonly FieldInfo DrawScale = typeof(MapOverlayDrawContext).GetField("_drawScale", MapFields);
        private static readonly FieldInfo ClippingRect = typeof(MapOverlayDrawContext).GetField("_clippingRect", MapFields);

        private readonly Dictionary<int, (Texture2D Source, Texture2D Mask)> outlineTextures = new();
        private readonly List<(NPC Part, Texture2D Texture, Texture2D Outline, Vector2 Position, float Opacity)> visibleParts = new();

        public override void Draw(ref MapOverlayDrawContext context, ref string text)
        {
            object map = context;
            if (MapPosition?.GetValue(map) is not Vector2 mapPosition
                || MapOffset?.GetValue(map) is not Vector2 mapOffset
                || MapScale?.GetValue(map) is not float mapScale
                || DrawScale?.GetValue(map) is not float drawScale || ClippingRect == null)
                return;

            Rectangle? clippingRect = (Rectangle?)ClippingRect.GetValue(map);
            float scale = 0.5f * drawScale;
            visibleParts.Clear();
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.ModNPC is not LauncherPhantomDragonPart part
                    || part.Head is not { Initialized: true } head || head.Owner != Main.myPlayer
                    || npc.frame.Width <= 0 || npc.frame.Height <= 0)
                    continue;

                Vector2 position = (npc.Center / 16f - mapPosition) * mapScale + mapOffset;
                if (clippingRect.HasValue && !clippingRect.Value.Contains(position.ToPoint()))
                    continue;

                Main.instance.LoadNPC(npc.type);
                Texture2D texture = TextureAssets.Npc[npc.type].Value;
                visibleParts.Add((npc, texture, GetOutlineTexture(npc.type, texture), position, part.MapOpacity));
            }

            // Draw the entire silhouette first so outlines do not cover adjacent body segments.
            foreach (var entry in visibleParts)
            {
                NPC npc = entry.Part;
                Vector2 origin = new Vector2(npc.frame.Width, npc.frame.Height) * 0.5f;
                var effects = npc.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                const int outlineLayers = 8;
                float outlineOpacity = 1f - MathF.Pow(1f - entry.Opacity, 1f / outlineLayers);
                for (int i = 0; i < outlineLayers; i++)
                {
                    Vector2 offset = (i * MathHelper.PiOver4).ToRotationVector2() * 2f * drawScale;
                    Main.spriteBatch.Draw(entry.Outline, entry.Position + offset, npc.frame, Color.White * outlineOpacity,
                        npc.rotation, origin, scale, effects, 0f);
                }
            }

            if (visibleParts.Count == 0) return;

            int dye = Main.LocalPlayer.GetModPlayer<CreatureLauncherPlayer>().DragonDye;
            SpriteBatch batch = Main.spriteBatch;
            // The corner minimap uses a default deferred batch. The other map views already use Immediate.
            bool switchBatch = dye != 0 && !Main.mapFullscreen && Main.mapStyle == 1;
            if (switchBatch)
            {
                batch.End();
                batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            }

            try
            {
                foreach (var entry in visibleParts)
                {
                    NPC npc = entry.Part;
                    Vector2 origin = new Vector2(npc.frame.Width, npc.frame.Height) * 0.5f;
                    var effects = npc.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                    var data = new DrawData(entry.Texture, entry.Position, npc.frame, Color.White * entry.Opacity,
                        npc.rotation, origin, scale, effects);
                    if (dye != 0) GameShaders.Armor.Apply(dye, npc, data);
                    data.Draw(batch);

                    Vector2 localMouse = (Main.MouseScreen - entry.Position).RotatedBy(-npc.rotation);
                    if (Math.Abs(localMouse.X) <= origin.X * scale && Math.Abs(localMouse.Y) <= origin.Y * scale)
                        text = "팬텀 드래곤";
                }
            }
            finally
            {
                if (dye != 0) GameShaders.Armor.Apply(0, null);
                if (switchBatch)
                {
                    batch.End();
                    batch.Begin();
                }
            }
        }

        private Texture2D GetOutlineTexture(int type, Texture2D source)
        {
            if (outlineTextures.TryGetValue(type, out var cached) && cached.Source == source)
                return cached.Mask;

            var pixels = new Color[source.Width * source.Height];
            source.GetData(pixels);
            for (int i = 0; i < pixels.Length; i++)
            {
                byte alpha = pixels[i].A;
                pixels[i] = new Color(alpha, alpha, alpha, alpha);
            }

            var mask = new Texture2D(source.GraphicsDevice, source.Width, source.Height);
            mask.SetData(pixels);
            cached.Mask?.Dispose();
            outlineTextures[type] = (source, mask);
            return mask;
        }

        public override void Unload()
        {
            if (Main.dedServ) return;
            Main.QueueMainThreadAction(() =>
            {
                foreach (var texture in outlineTextures.Values)
                    texture.Mask.Dispose();
                outlineTextures.Clear();
                visibleParts.Clear();
            });
        }
    }
}
