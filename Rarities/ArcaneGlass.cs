using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using Terraria.GameContent;

namespace CAmod.Rarities
{
    public class ArcaneGlass : ModRarity
    {
        public override Color RarityColor => new Color(120, 200, 255);

        public static void Draw(Item Item, SpriteBatch spriteBatch, string text, int X, int Y,
            Color textColor, Color lightColor, float rotation,
            Vector2 origin, Vector2 baseScale, float time,
            bool renderTextSparkles, DynamicSpriteFont font)
        {
            var fontSize = font.MeasureString(text);
            var center = fontSize / 2f;

            Color skyBlue = new Color(120, 200, 255);
            Color white = Color.White;

            // 0~1 왕복
            float mix = (float)Math.Sin(time * 2f) * 0.5f + 0.5f;

            // 하늘색 ↔ 흰색 보간
            Color outlineColor = Color.Lerp(skyBlue, white, mix);

            textColor = outlineColor;

            textColor = skyBlue;
            textColor.A = 0;

            float pulsing = 2f + (float)Math.Sin(time * 5f);

            // 🔹 외곽 오라 (맥동만 유지)
            for (float f = 0f; f < MathHelper.TwoPi; f += 0.79f)
            {
                Vector2 offset = new Vector2(pulsing, 0f)
                    .RotatedBy(f + time * 2f % MathHelper.TwoPi);

                ChatManager.DrawColorCodedString(
                    spriteBatch, font, text,
                    new Vector2(X, Y) + offset,
                    textColor * 0.5f,
                    rotation, origin, baseScale);
            }

            textColor.A = 255;

            // 🔹 테두리 그림자
            ChatManager.DrawColorCodedStringShadow(
                spriteBatch, font, text,
                new Vector2(X, Y),
                outlineColor * 2f,
                rotation, origin, baseScale);

            // 🔹 본문은 항상 검정
            ChatManager.DrawColorCodedString(
                spriteBatch, font, text,
                new Vector2(X, Y),
                Color.Black,
                rotation, origin, baseScale);

            // 🔹 한 글자씩 강조 샤인
            float shineWidth = 40f;
            float shineSpeed = 80f;

            float shineDisp = time * shineSpeed;
            float shinePos = (shineDisp % (fontSize.X + shineWidth));

            Vector2 basePos = new Vector2(X, Y);

            float charOffsetX = 0f;

            if (!renderTextSparkles)
                return;

            for (int i = 0; i < text.Length; i++)
            {
                string c = text[i].ToString();
                Vector2 charSize = font.MeasureString(c);
                Vector2 charPos = basePos + new Vector2(charOffsetX, 0f);

                float centerX = charPos.X + (charSize.X * baseScale.X) / 2f + 10.5f;
                float dist = Math.Abs(centerX - (X + shinePos - shineWidth * 0.15f));
                float intensity = 1f - MathHelper.Clamp(dist / shineWidth, 0f, 1f);

                if (intensity > 0f)
                {
                    ChatManager.DrawColorCodedString(
                        spriteBatch, font, c,
                        charPos,
                        outlineColor * intensity * 1.5f,
                        rotation, origin, baseScale);
                }

                charOffsetX += charSize.X - text.Length * 0.0085f;
            }
        }

        public static void Draw(Item item, DrawableTooltipLine line)
        {
            Draw(item, Main.spriteBatch, line.Text, line.X, line.Y,
                Color.White, Color.White,
                line.Rotation, line.Origin, line.BaseScale,
                Main.GlobalTimeWrappedHourly,
                true,
                FontAssets.MouseText.Value);
        }
    }
}
