using System.Collections.Generic;
using CAmod.NPCs;
using Microsoft.Xna.Framework;
using Terraria;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using Terraria.UI;

namespace CAmod.UI
{
    public sealed class PhantomDragonHealthUI : ModSystem
    {
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int index = layers.FindIndex(layer => layer.Name == "Vanilla: Inventory");
            if (index >= 0)
                layers.Insert(index, new LegacyGameInterfaceLayer("CAmod: Phantom Dragon Health",
                    DrawHealth, InterfaceScaleType.UI));
        }

        private static bool DrawHealth()
        {
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.ModNPC is not LauncherPhantomDragonHead dragon || !dragon.Initialized
                    || dragon.Owner != Main.myPlayer || npc.lifeMax <= 0) continue;
                const int y = 42;
                var batch = Main.spriteBatch;
                var frame = ModContent.Request<Texture2D>("CAmod/UI/dragonHP").Value;
                var fill = ModContent.Request<Texture2D>("CAmod/UI/dragonHPbar2").Value;
                int x = (int)(Main.screenWidth / Main.UIScale / 2f) - frame.Width / 2;
                var position = new Vector2(x, y);
                batch.Draw(frame, position, Color.White);

                const int fillLeft = 10;
                const int fillRight = 122;
                float ratio = MathHelper.Clamp(npc.life / (float)npc.lifeMax, 0f, 1f);
                int visibleWidth = (int)(fill.Width * ratio);
                if (visibleWidth > 0)
                {
                    var source = new Rectangle(0, 0, visibleWidth, fill.Height);
                    var fillScale = new Vector2((fillRight - fillLeft) / (float)fill.Width, 1f);
                    batch.Draw(fill, position + new Vector2(fillLeft, 0f), source,
                        Color.White, 0f, Vector2.Zero, fillScale, SpriteEffects.None, 0f);
                }
                Vector2 mouse = Main.MouseScreen;
                if (new Rectangle(x, y, frame.Width, frame.Height).Contains(mouse.ToPoint()))
                    Utils.DrawBorderString(batch, $"팬텀 드래곤  {npc.life:N0} / {npc.lifeMax:N0}",
                        mouse + new Vector2(16f, 16f), new Color(135, 255, 230), 0.8f);
                break;
            }
            return true;
        }
    }
}
