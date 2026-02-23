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
    public class ArcaneGreen : ModRarity
    {
        public override Color RarityColor => TextClr * 2f;

        public static float MaxRadius = 6f;
        public static Color TextClr = new Color(80, 220, 120, 255);

        public static void Draw(Item item, SpriteBatch spriteBatch, string text, int X, int Y,
            Color textColor, Color lightColor, float rotation,
            Vector2 origin, Vector2 baseScale, float time,
            bool renderTextSparkles, DynamicSpriteFont font)
        {
            Texture2D crystalGlow = ModContent.Request<Texture2D>("CAmod/Assets/CrystalTextGlow").Value;
            Texture2D sparkle = ModContent.Request<Texture2D>("CAmod/Assets/CrystalTextSparkle").Value;

            Vector2 fontSize = font.MeasureString(text);
            Vector2 center = fontSize / 2f;

            // 🌿 연두 ↔ 라임 왕복
            float mix = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2f) * 0.5f + 0.5f;

            Color green = new Color(120, 220, 40);
            Color lime = new Color(190, 255, 60);

            Color dynText = Color.Lerp(green, lime, mix);
            Color dynGlow = Color.Lerp(new Color(120, 255, 150), new Color(200, 255, 100), mix);

            Vector2 glowPosition = new Vector2(X + center.X, Y + center.Y / 1.5f);

            textColor = dynText;
            textColor.A = 0;

            // 맥동 50%
            
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
                // X는 문자열 전체 길이에서 랜덤 시작점
                float startX = X + rand.NextFloat(fontSize.X);
                float startY = Y + fontSize.Y * 0.4f;

                // 완전 360도 방향
                float angle = rand.NextFloat(MathHelper.TwoPi);

                float seed = rand.NextFloat(); // 파티클별 고정 시드
                float life = Main.GlobalTimeWrappedHourly * 0.8f + seed;
                life -= (float)Math.Floor(life);

                float alpha = 1f - Math.Abs(life * 2f - 1f); // 0→1→0 구조


                float distance = life * MaxRadius * 4f; // 점점 바깥으로 확산한다
                float fadeOut = 1f - life; // 멀어질수록 서서히 사라진다
                fadeOut *= fadeOut; // 부드럽게 감쇠한다
                Vector2 direction = new Vector2(
                    (float)Math.Cos(angle),
                    (float)Math.Sin(angle));

                Vector2 basePos = new Vector2(startX, startY) + direction * distance;

                

                Color sparkleColor = Color.Lerp(
    new Color(120, 255, 150),
    new Color(200, 255, 100),
    mix) * fadeOut;

                spriteBatch.Draw(
                    sparkle,
                    basePos,
                    null,
                    sparkleColor,
                    angle,
                    sparkleOrigin,
                    0.35f + life * 0.25f,
                    SpriteEffects.None,
                    0f);
            }
            // dynText 사용하도록 수정
            ChatManager.DrawColorCodedStringShadow(
                spriteBatch, font, text,
                new Vector2(X, Y),
                dynText * 2f,
                rotation, origin, baseScale);

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
