using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using Terraria.Utilities;
using Terraria.GameContent;

namespace CAmod.Rarities
{
    public class ArcaneCosmic : ModRarity
    {
        public override Color RarityColor => new Color(255, 120, 255);

        public static float MaxRadius = 18f;

        public static void Draw(Item item, SpriteBatch spriteBatch, string text, int X, int Y,
            Color textColor, Color lightColor, float rotation,
            Vector2 origin, Vector2 baseScale, float time,
            bool renderTextSparkles, DynamicSpriteFont font)
        {
            Texture2D crystalGlow = ModContent.Request<Texture2D>("CAmod/Assets/CrystalTextGlow").Value;
            Texture2D sparkle = ModContent.Request<Texture2D>("CAmod/Assets/CrystalTextSparkle").Value;

            Vector2 fontSize = font.MeasureString(text);
            Vector2 center = fontSize / 2f;

            // 🌌 핑크-보라-파랑 3색 왕복
            float t = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2f) * 0.5f + 0.5f;

            Color pink = new Color(255, 120, 200);
            Color purple = new Color(170, 90, 255);
            Color blue = new Color(80, 160, 255);

            Color dynText;

            if (t < 0.5f)
                dynText = Color.Lerp(pink, purple, t * 2f);
            else
                dynText = Color.Lerp(purple, blue, (t - 0.5f) * 2f);

            Vector2 glowPosition = new Vector2(X + center.X, Y + center.Y / 1.5f);

            textColor = dynText;
            textColor.A = 0;

            float pulsing = 2f + (float)Math.Sin(time * 5f);

            for (float f = 0f; f < MathHelper.TwoPi; f += 0.79f)
            {
                ChatManager.DrawColorCodedString(
                    spriteBatch,
                    font,
                    text,
                    new Vector2(X, Y) + new Vector2(pulsing, 0f).RotatedBy(f),
                    textColor * 0.6f,
                    rotation,
                    origin,
                    baseScale);
            }

            textColor.A = 255;

            // 🌌 파티클은 글자 뒤에서 그린다
            if (renderTextSparkles)
            {
                static int Hash(int x)
                {
                    x ^= x >> 16;
                    x *= unchecked((int)0x7feb352d);
                    x ^= x >> 15;
                    x *= unchecked((int)0x846ca68b);
                    x ^= x >> 16;
                    return x;
                }

                UnifiedRandom rand = new UnifiedRandom(Hash((int)(center.X + center.Y)));

                int sparkleCount = (int)(fontSize.X / 4f) + 10;

                Vector2 sparkleOrigin = new Vector2(15f, 15f);

                for (int i = 0; i < sparkleCount; i++)
                {
                    float seed = rand.NextFloat();
                    float life = Main.GlobalTimeWrappedHourly * 0.7f + seed;
                    life -= (float)Math.Floor(life);

                    // 외곽 → 중심 흡입
                    float distance = (1f - life) * MaxRadius;

                    float angle = rand.NextFloat(MathHelper.TwoPi);

                    Vector2 dir = new Vector2(
                        (float)Math.Cos(angle),
                        (float)Math.Sin(angle));

                    float startX = X + rand.NextFloat(fontSize.X);
                    float startY = Y + font.MeasureString("A").Y * 0.4f;

                    Vector2 basePos = new Vector2(startX, startY) + dir * distance;

                    // 페이드 인 + 페이드 아웃
                    float alpha = 1f - Math.Abs(life * 2f - 1f);

                    Color sparkleColor = dynText * alpha;

                    spriteBatch.Draw(
                        sparkle,
                        basePos,
                        null,
                        sparkleColor,
                        angle,
                        sparkleOrigin,
                       0.18f + alpha * 0.15f,
                        SpriteEffects.None,
                        0f);
                }
            }

            // 텍스트는 마지막에 그려서 앞에 위치
            ChatManager.DrawColorCodedStringShadow(
                spriteBatch, font, text,
                new Vector2(X, Y),
                dynText * 2f,
                rotation, origin, baseScale);

            spriteBatch.Draw(
                crystalGlow,
                glowPosition,
                null,
                dynText * 0.8f,
                rotation + MathHelper.PiOver2,
                new Vector2(6f, 33f),
                new Vector2(1.6f, fontSize.X / crystalGlow.Height * 1.2f),
                SpriteEffects.None,
                0f);

            ChatManager.DrawColorCodedString(
                spriteBatch,
                font,
                text,
                new Vector2(X, Y),
                Color.Black,
                rotation,
                origin,
                baseScale);
        }

        public static void Draw(Item item, DrawableTooltipLine line)
        {
            Draw(item,
                Main.spriteBatch,
                line.Text,
                line.X,
                line.Y,
                Color.White,
                Color.White,
                line.Rotation,
                line.Origin,
                line.BaseScale,
                Main.GlobalTimeWrappedHourly,
                true,
                FontAssets.MouseText.Value);
        }
    }
}
