using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace CAmod.Projectiles
{
    public class FlameEcho_ChainNode : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;

            Projectile.timeLeft = 30;
            // 30프레임 후 사라진다

            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.friendly = false;
            Projectile.hostile = false;
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs,
           List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            int myIndex = (int)Projectile.ai[0];
            // 이 투사체의 체인 번호다

            if (myIndex <= 0)
                return false;

            Projectile prev = null;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];

                if (!p.active)
                    continue;

                if (p.type != Projectile.type)
                    continue;

                if (p.owner != Projectile.owner)
                    continue;

                if (p.type == Projectile.type &&
    p.owner == Projectile.owner &&
    (int)p.ai[0] == myIndex - 1 &&
    p.ai[1] == Projectile.ai[1])
                {
                    prev = p;
                    break;
                }
            }

            if (prev == null)
                return false;

            Texture2D tex = ModContent.Request<Texture2D>("CAmod/Projectiles/ChainBeam").Value;
            // 체인 스프라이트를 로드한다

            Vector2 start = prev.Center;
            Vector2 end = Projectile.Center;

            Vector2 dir = end - start;
            float length = dir.Length();

            if (length <= 0f)
                return false;

            dir.Normalize();

            float rotation = dir.ToRotation() + MathHelper.PiOver2;

            float step = tex.Width;

            float alpha = Projectile.timeLeft / 30f;
            // 30프레임 동안 페이드아웃한다
            float t = Projectile.timeLeft / 30f;
            float alpha2 = MathHelper.Clamp(t, 0.25f, 1f);
            Vector2 origin = tex.Size() / 2f;

            for (float i = 0; i <= length; i += step)
            {
                Vector2 pos = start + dir * i;
                if (Main.rand.NextBool(20)) // 약 16% 확률
                {
                    int dustType = Main.rand.NextBool() ? 244 : 246;
                    // dust 타입을 244 또는 246 중 하나로 선택한다

                    Vector2 vel = Main.rand.NextVector2Circular(2.0f, 2.0f);
                    // 랜덤 방향 속도를 만든다

                    Dust d = Dust.NewDustPerfect(
                        pos,
                        dustType,
                        vel,
                        (int)(1f - alpha)*255
                    );

                    d.scale = Main.rand.NextFloat(0.75f, 1.0f);
                    d.noGravity = true;
                 
                }
                Vector2 scale = new Vector2(alpha, 1f);
                // X는 유지하고 Y(두께)만 페이드아웃한다

                Main.spriteBatch.Draw(
                    tex,
                    pos - Main.screenPosition,
                    null,
                    Color.White* alpha2,
                    rotation,
                    origin,
                    scale,
                    SpriteEffects.None,
                    1f
                );
            }

            return false;
        }
    }
}