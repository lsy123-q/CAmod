using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CAmod.Projectiles;
using Terraria.Audio;

namespace CAmod.NPCs
{
    public class RabbitMissileNPC : ModNPC
    {
        // 총 프레임 수
        private const int FrameCount = 7;

        public override void SetStaticDefaults()
        {
            // 프레임 개수 지정
            Main.npcFrameCount[Type] = FrameCount;

            NPCID.Sets.ActsLikeTownNPC[Type] = false;

            // 잔상 캐시
            NPCID.Sets.TrailCacheLength[Type] = 5;
            NPCID.Sets.TrailingMode[Type] = 0;
        }
        public override bool CanBeHitByNPC(
    NPC attacker
)
        {
            // NPC 몸박은 전부 무효화한다
            return false;
        }
        public override void SetDefaults()
        {
            NPC.width = 20;
            NPC.height = 20;

            NPC.lifeMax = 5;
            NPC.damage = 0;
            NPC.defense = 0;

            NPC.friendly = true;

            NPC.noGravity = true;
            NPC.noTileCollide = true;

            NPC.knockBackResist = 0f;

            NPC.aiStyle = -1;

            NPC.scale = 0.225f;
        }

        public override void FindFrame(int frameHeight)
        {
            // 이동중이면 프레임 재생
            if (NPC.velocity.Length() > 0.1f)
            {
                NPC.frameCounter += 0.25;

                if (NPC.frameCounter >= 1)
                {
                    NPC.frameCounter = 0;

                    NPC.frame.Y += frameHeight;

                    // 7프레임 루프
                    if (NPC.frame.Y >= frameHeight * FrameCount)
                    {
                        NPC.frame.Y = 0;
                    }
                }
            }

            // 이동 방향 기준 반전
            NPC.spriteDirection =
                NPC.velocity.X > 0 ? 1 : -1;
        }

        public override void AI()
        {
            int targetIndex = (int)NPC.ai[0];

            // 타겟 인덱스 이상하면 제거
            if (targetIndex < 0 || targetIndex >= Main.maxNPCs)
            {
                NPC.active = false;
                return;
            }
            Lighting.AddLight(
    NPC.Center,
    1.12f,
    1.12f,
    1.12f
);
            NPC target = Main.npc[targetIndex];

            // 타겟 죽었으면 제거
            if (!target.active || target.life <= 0)
            {
                NPC.active = false;
                return;
            }

            // 유도 방향 계산
            Vector2 dir =
                (target.Center - NPC.Center)
                .SafeNormalize(Vector2.UnitX);

            // 속도 적용
            NPC.velocity = dir * 12f;

            // 스프라이트만 랜덤하게 회전시킨다
            // ai[1]이 회전 방향이다
            // 1이면 시계
            // -1이면 반시계
            if (NPC.ai[1] == 0)
            {
                NPC.ai[1] =
                    Main.rand.NextBool()
                    ? 1
                    : -1;
            }

            // 1초에 2바퀴 회전한다
            NPC.rotation +=
                MathHelper.Pi
                * 2f
                / 60f
                * NPC.ai[1];

            // 충돌 시 폭발
            if (NPC.Hitbox.Intersects(target.Hitbox))
            {
                Explode(target);
            }
        }

        public override bool PreDraw(
            SpriteBatch spriteBatch,
            Vector2 screenPos,
            Color drawColor)
        {
            Texture2D texture =
                Terraria.GameContent.TextureAssets.Npc[Type].Value;

            Rectangle frame =
                NPC.frame;

            Vector2 origin =
                frame.Size() / 2f;

            SpriteEffects effects =
                NPC.spriteDirection == 1
                ? SpriteEffects.None
                : SpriteEffects.FlipHorizontally;

            // 잔상 그리기
            for (int i = 0; i < NPC.oldPos.Length; i++)
            {
                Vector2 oldPos =
                    NPC.oldPos[i] + NPC.Size / 2f;

                float alpha =
                    (NPC.oldPos.Length - i)
                    / (float)NPC.oldPos.Length;

                Color trailColor =
                    Color.White * alpha * 0.4f;

                spriteBatch.Draw(
                    texture,
                    oldPos - screenPos,
                    frame,
                    trailColor,
                    NPC.rotation,
                    origin,
                    NPC.scale,
                    effects,
                    0f
                );
            }

            // 본체 그리기
            spriteBatch.Draw(
                texture,
                NPC.Center - screenPos,
                frame,
                drawColor,
                NPC.rotation,
                origin,
                NPC.scale,
                effects,
                0f
            );

            return false;
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            // 죽을때만 실행한다
            if (NPC.life <= 0)
            {
                // 날아가던 방향 기반 벡터
                Vector2 goreVelocity =
                    NPC.velocity.SafeNormalize(Vector2.UnitX) * 6f;

                // ±15도 랜덤 회전
                Vector2 goreVel1 =
                    goreVelocity.RotatedBy(
                        MathHelper.ToRadians(
                            Main.rand.NextFloat(-15f, 15f)
                        )
                    );

                Vector2 goreVel2 =
                    goreVelocity.RotatedBy(
                        MathHelper.ToRadians(
                            Main.rand.NextFloat(-15f, 15f)
                        )
                    );
                for (int i = 0; i < 25; i++)
                {
                    Dust d = Dust.NewDustDirect(
                        NPC.position,
                        NPC.width,
                        NPC.height,
                        DustID.Blood
                    );

                    // 피를 사방으로 튀긴다
                    d.velocity =
                        NPC.velocity.RotatedByRandom(
                            MathHelper.ToRadians(40f)
                        ) *Main.rand.NextFloat(0.3f, 0.6f);

                    d.scale =
                        Main.rand.NextFloat(0.8f, 1.4f);

                    d.noGravity = false;
                }
                // 토끼 고어 생성
                Gore.NewGore(
                    NPC.GetSource_Death(),
                    NPC.Center,
                    goreVel1,
                    76
                );

                Gore.NewGore(
                    NPC.GetSource_Death(),
                    NPC.Center,
                    goreVel2,
                    77
                );
                SoundEngine.PlaySound(
    SoundID.NPCDeath1,
    NPC.Center
);
            }
        }
        private void Explode(NPC target)
        {
            // Fixed base damage; no target-health scaling.
            const int damage = 150;

            // 실제 타격용 투사체 생성
            int p =
 Projectile.NewProjectile(
     NPC.GetSource_FromAI(),
     NPC.Center,
     Vector2.Zero,
     ModContent.ProjectileType<RabbitDamageProjectile>(),
     damage,
     30f,
     Main.myPlayer
 );

            // 타겟 넘긴다
            Main.projectile[p].ai[0] =
                target.whoAmI;
            for (int i = 0; i < 25; i++)
            {
                Dust d = Dust.NewDustDirect(
                    NPC.position,
                    NPC.width,
                    NPC.height,
                    DustID.Blood
                );

                // 피를 사방으로 튀긴다
                d.velocity =
                    NPC.velocity.RotatedByRandom(
                        MathHelper.ToRadians(40f)
                    ) * Main.rand.NextFloat(0.3f, 0.6f);

                d.scale =
                    Main.rand.NextFloat(0.8f, 1.4f);

                d.noGravity = false;
            }
            // 토끼 고어 생성
            // 날아가던 방향 기반 벡터
            Vector2 goreVelocity =
                NPC.velocity.SafeNormalize(Vector2.UnitX) * 6f;

            // ±15도 랜덤 회전
            Vector2 goreVel1 =
                goreVelocity.RotatedBy(
                    MathHelper.ToRadians(
                        Main.rand.NextFloat(-15f, 15f)
                    )
                );

            Vector2 goreVel2 =
                goreVelocity.RotatedBy(
                    MathHelper.ToRadians(
                        Main.rand.NextFloat(-15f, 15f)
                    )
                );

            // 토끼 고어 생성
            Gore.NewGore(
                NPC.GetSource_Death(),
                NPC.Center,
                goreVel1,
                76
            );

            Gore.NewGore(
                NPC.GetSource_Death(),
                NPC.Center,
                goreVel2,
                77
            );
            SoundEngine.PlaySound(
    SoundID.NPCDeath1,
    NPC.Center
);
            // 즉시 제거
            NPC.active = false;
        }

        // 자동 디스폰 방지
        public override bool CheckActive()
        {
            return false;
        }
    }
}