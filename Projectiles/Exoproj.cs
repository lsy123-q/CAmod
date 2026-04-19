using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CAMod.Projectiles
{
    public class ExoProj : ModProjectile
    {
        private int dustTimer = 0;
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 38;

            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 1200;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public static readonly Color[] ExoPalette = new Color[]
        {
            new Color(250, 255, 112), new Color(211, 235, 108),
            new Color(166, 240, 105), new Color(105, 240, 220),
            new Color(64, 130, 145), new Color(145, 96, 145),
            new Color(242, 112, 73), new Color(199, 62, 62),
        };
        public static Color MulticolorLerp(float increment, params Color[] colors)
        {
            increment %= 0.999f;
            int idx = (int)(increment * colors.Length);
            return Color.Lerp(colors[idx], colors[(idx + 1) % colors.Length], (increment * colors.Length) % 1f);
        }
        private void SpawnAstroidDust()

        {

            Vector2 tipPosition = Projectile.Center;
            tipPosition.Y -= 0.5f; 

            SoundEngine.PlaySound(SoundID.Item158 with { Volume = 1.6f }, tipPosition);

            for (int i = 0; i < 75; i++)

            {

                float angle = MathHelper.TwoPi * i / 75f;

                float x = (float)Math.Pow(Math.Cos(angle), 3) * 5f;

                float y = (float)Math.Pow(Math.Sin(angle), 3) * 5f;

                Dust d = Dust.NewDustPerfect(tipPosition, DustID.RainbowMk2, new Vector2(x, y));

                d.scale = 1.8f;

                d.color = MulticolorLerp(i / 75f, ExoPalette);

                d.noGravity = true;

            }

        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            // 플레이어가 없으면 제거한다
            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }
            dustTimer++;
            Projectile.timeLeft = 1200;
            if (dustTimer >= 600)
            {
                SpawnAstroidDust();
                // 더스트 생성한다

                dustTimer = 0;
                // 타이머 초기화한다
            }
            // =========================
            // 어깨 위치 계산한다
            // =========================
            Vector2 destination = player.Top;
            // 머리 위 기준이다

            destination.X -= player.direction * player.width * 1.25f;
            // 바라보는 방향 반대쪽 어깨로 이동한다

            destination.Y += player.height * 0.25f;
            // 어깨 높이로 내린다

            // =========================
            // 위치 강제 고정한다
            // =========================
            Projectile.Center = Vector2.Lerp(Projectile.Center, destination, 0.35f);

            // =========================
            // 방향 동기화한다
            // =========================
            Projectile.spriteDirection = player.direction;
        }
    }
}