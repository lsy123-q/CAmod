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
    public class ArcaneRed : ModRarity
    {
        public override Color RarityColor => TextClr * 2f;

        public static float MaxY = 4.5f;

        public static Color BloomClr = new Color(120, 15, 25, 0);
        public static Color TextClr = new Color(190, 30, 40, 255);

        public static void Draw(Item item, SpriteBatch spriteBatch, string text, int X, int Y,
            Color textColor, Color lightColor, float rotation,
            Vector2 origin, Vector2 baseScale, float time,
            bool renderTextSparkles, DynamicSpriteFont font)
        {
            Texture2D crystalGlow = ModContent.Request<Texture2D>("CAmod/Assets/CrystalTextGlow").Value; // glow 텍스처를 불러온다
            Texture2D sparkle = ModContent.Request<Texture2D>("CAmod/Assets/CrystalTextSparkle").Value;   // 반짝이 텍스처를 불러온다

            Vector2 fontSize = font.MeasureString(text);
            Vector2 center = fontSize / 2f;

            // 🔴↔🟣 비대칭 왕복 색상 계산을 한다
            float phase = (Main.GlobalTimeWrappedHourly * 0.25f) % 1f;

            const float holdRed = 0.33333334f;
            const float transRP = 0.25f;
            const float holdPurple = 0.16666667f;
            const float transPR = 0.25f;

            float mix;

            if (phase < holdRed)
                mix = 0f; // 빨강 유지
            else if (phase < holdRed + transRP)
            {
                float u = (phase - holdRed) / transRP;
                mix = u * u * (3f - 2f * u); // 빨강→보라 전환
            }
            else if (phase < holdRed + transRP + holdPurple)
                mix = 1f; // 보라 유지
            else
            {
                float u = (phase - (holdRed + transRP + holdPurple)) / transPR;
                u = u * u * (3f - 2f * u);
                mix = 1f - u; // 보라→빨강 전환
            }

            Color redText = new Color(190, 30, 40, 255);
            Color purpleText = new Color(190, 75, 75, 255);

            Color redGlow = new Color(255, 60, 60);
            Color purpleGlow = new Color(255, 100, 100);

            Color dynText = Color.Lerp(redText, purpleText, mix); // 동적 텍스트 색
            Color dynGlow = Color.Lerp(redGlow, purpleGlow, mix); // 동적 광선 색

            Vector2 glowPosition = new Vector2(X + center.X, Y + center.Y / 1.5f);

            textColor = dynText;
            textColor.A = 0;

            float pulsing = 2.5f + (float)Math.Sin(time * 6f)*2f; // 맥동한다

            for (float f = 0f; f < MathHelper.TwoPi; f += 0.79f)
            {
                ChatManager.DrawColorCodedString(
                    spriteBatch,
                    font,
                    text,
                    new Vector2(X, Y) + new Vector2(pulsing, 0f).RotatedBy(f + time * 2.5f),
                    textColor * 0.6f,
                    rotation,
                    origin,
                    baseScale);
            }

            textColor.A = 255;

            ChatManager.DrawColorCodedStringShadow(spriteBatch, font, text,
                new Vector2(X, Y), dynText * 2.3f, rotation, origin, baseScale); // 그림자를 출력한다

            spriteBatch.Draw(
                crystalGlow,
                glowPosition,
                null,
                dynGlow * 0.8f, // 왕복 광선 색 적용
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

            if (!renderTextSparkles)
                return;

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

            int sparkleCount = rand.Next((int)fontSize.X / 7, (int)fontSize.X / 5) + 8;

            Vector2 sparkleOrigin = new Vector2(15f, 15f);

            for (int i = 0; i < sparkleCount; i++)
            {
                Vector2 v = new Vector2(
                    rand.NextFloat(fontSize.X),
                    rand.NextFloat(fontSize.Y * 0.6f) + 1f);

                float lifeTime = Main.GlobalTimeWrappedHourly * 5f + rand.NextFloat(MathHelper.TwoPi);
                lifeTime %= MathHelper.TwoPi;

                float sinValue = (float)Math.Sin(lifeTime);

                Color sparkleBase = Color.Lerp(
                    new Color(255, 180, 180, 255),
                    new Color(220, 200, 255, 255),
                    mix) * sinValue;

                Color sparkleStrong = Color.Lerp(
                    new Color(255, 40, 40),
                    new Color(160, 90, 255),
                    mix) * sinValue;

                float sparkleRotation = time * rand.NextFloat(0.9f, 1.7f);

                Vector2 basePos = new Vector2(X, Y + lifeTime * MaxY + 2f) + v; // 하강하도록 변경한다

                spriteBatch.Draw(
                    sparkle,
                    basePos,
                    null,
                    sparkleBase,
                    sparkleRotation,
                    sparkleOrigin,
                    lifeTime / MathHelper.TwoPi * 0.35f,
                    SpriteEffects.None,
                    0f);

                spriteBatch.Draw(
                    sparkle,
                    basePos,
                    null,
                    sparkleStrong,
                    sparkleRotation,
                    sparkleOrigin,
                    lifeTime / MathHelper.TwoPi * 0.6f,
                    SpriteEffects.None,
                    0f);
            }
        }

        public static void Draw(Item item, DrawableTooltipLine line)
        {
            Draw(item, Main.spriteBatch, line.Text, line.X, line.Y,
                TextClr, BloomClr,
                line.Rotation, line.Origin, line.BaseScale,
                Main.GlobalTimeWrappedHourly,
                true,
                FontAssets.MouseText.Value);
        }
    }
}
