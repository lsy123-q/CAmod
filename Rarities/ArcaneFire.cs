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
    public class ArcaneFire : ModRarity
    {
        public override Color RarityColor => TextClr * 2f;

        public static float MaxRadius = 6f;
        public static Color TextClr = new Color(255, 140, 40, 255);
        // 기본 텍스트 색상은 주황 계열로 설정한다

        public static void Draw(Item item, SpriteBatch spriteBatch, string text, int X, int Y,
            Color textColor, Color lightColor, float rotation,
            Vector2 origin, Vector2 baseScale, float time,
            bool renderTextSparkles, DynamicSpriteFont font)
        {
            Texture2D crystalGlow = ModContent.Request<Texture2D>("CAmod/Assets/CrystalTextGlow").Value;
            Texture2D sparkle = ModContent.Request<Texture2D>("CAmod/Assets/smoke").Value;

            Vector2 fontSize = font.MeasureString(text);
            Vector2 center = fontSize / 2f;

            // 🔥 주황 ↔ 노랑 왕복한다
            float mix = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2f) * 0.5f + 0.5f;

            Color orange = new Color(255, 120, 20);
            Color yellow = new Color(255, 230, 40);

            Color dynText = Color.Lerp(orange, yellow, mix);
            Color dynGlow = Color.Lerp(new Color(255, 160, 60), new Color(255, 255, 100), mix);

            Vector2 glowPosition = new Vector2(X + center.X, Y + center.Y / 1.5f);

            textColor = dynText;
            textColor.A = 0;

            // 텍스트 외곽 맥동 효과를 준다
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

            // 해시 함수로 고정 시드 생성한다
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

            int sparkleCount = (rand.Next((int)fontSize.X / 7, (int)fontSize.X / 5) + 1) * 2;

            Vector2 sparkleOrigin = new Vector2(15f, 15f);

            for (int i = 0; i < sparkleCount; i++)
            {
                float startX = X + rand.NextFloat(fontSize.X);
                float startY = Y + fontSize.Y * 0.4f;
                // 시작 위치는 글자 하단 부근으로 설정한다

                float seed = rand.NextFloat();
                float life = Main.GlobalTimeWrappedHourly * 0.8f + seed;
                life -= (float)Math.Floor(life);

                float distance = life * MaxRadius * 6f;
                // 시간이 지날수록 위로 상승한다

                float fadeOut = 1f - life;
                fadeOut *= fadeOut * fadeOut;
                Vector2 basePos = new Vector2(startX, startY - distance);
                // Y축 음수 방향으로만 이동시켜 상승하게 만든다

                Color sparkleColor = Color.Lerp(
                    new Color(255, 140, 40),
                    new Color(255, 255, 80),
                    mix) * fadeOut;

                spriteBatch.Draw(
                    sparkle,
                    basePos,
                    null,
                    sparkleColor,
                    0f,
                    sparkleOrigin,
                    0.4f + life * 0.3f,
                    SpriteEffects.None,
                    0f);
            }

            // 그림자 출력한다
            ChatManager.DrawColorCodedStringShadow(
                spriteBatch, font, text,
                new Vector2(X, Y),
                dynText * 2f,
                rotation, origin, baseScale);

            // 중앙 글로우 출력한다
            spriteBatch.Draw(
                crystalGlow,
                glowPosition,
                null,
                dynGlow * 0.8f,
                rotation + MathHelper.PiOver2,
                new Vector2(6f, 33f),
                new Vector2(1.6f, fontSize.X / crystalGlow.Height * 1.2f),
                SpriteEffects.None,
                0f);

            // 최종 텍스트 그린다
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
        }

        public static void Draw(Item item, DrawableTooltipLine line)
        {
            Draw(item,
                Main.spriteBatch,
                line.Text,
                line.X,
                line.Y,
                TextClr,
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