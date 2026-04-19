using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.Utilities;
using XPT.Core.Audio.MP3Sharp.Decoding.Decoders.LayerIII;
using static System.Net.Mime.MediaTypeNames;
using Terraria.Graphics.CameraModifiers;
using CAmod.Players;
using Steamworks;
namespace CAmod.Projectiles
{
    public class CosmicAttack : ModProjectile
    {
        private bool go = false;
        private float width = 1f;
        private int spawnShakeTimer = 0;
        

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            // 히트박스는 작게 유지한다

            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;

            Projectile.penetrate = -1;
            Projectile.timeLeft = 75;
            // 지속시간이다
            Projectile.usesLocalNPCImmunity = true;
            // 이 투사체만의 독립적인 무적프레임을 사용한다

            Projectile.localNPCHitCooldown = 10;
            // 한 번 맞으면 75프레임 동안 이 투사체에만 면역이 된다
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.extraUpdates = 1;
            // 부드럽게 만든다
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            


            if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
            {
                try
                {

                    Type vulnType = calamity.Code.GetType("CalamityMod.Buffs.DamageOverTime.GodSlayerInferno");



                    if (vulnType != null)
                    {
                        int vulnID = calamity.Find<ModBuff>(vulnType.Name).Type;
                        target.AddBuff(vulnID, 600);
                    }
                }
                catch { }
            }
        }
        public override void AI()
        {

            if (Projectile.localAI[0] == 0f && Projectile.localAI[1] == 0f)
                return;


            if (spawnShakeTimer > 0 && Main.myPlayer == Projectile.owner)
            {
                float progress = 1f - (spawnShakeTimer / 10f);
                // 0에서 시작해서 1로 끝나는 진행도다

                float strength = MathHelper.Lerp(15f, 0.5f, progress);
                // 처음엔 조금 세고 점점 잔잔해진다

                float vibrance = MathHelper.Lerp(5f, 1f, progress);
                // 흔들림의 잔진동도 같이 줄인다

                Main.instance.CameraModifiers.Add(
                    new PunchCameraModifier(
                        Projectile.Center,
                        Main.rand.NextVector2Unit(),
                        strength,
                        vibrance,
                        1,
                        0f,
                        "CosmicAttackSpawnShake"
                    )
                );
                // 매 프레임 약한 펀치 카메라를 누적해서 점점 잔잔해지는 지진처럼 만든다

                spawnShakeTimer--;
                // 남은 지진 프레임을 줄인다
            }

            Vector2 start = new Vector2(Projectile.ai[0], Projectile.ai[1]);
            // 시작점 = 마우스

            Vector2 target = new Vector2(Projectile.localAI[0], Projectile.localAI[1]);
            // 목표점 = 천장 랜덤 위치

            Vector2 dir = target - start;

            if (dir.LengthSquared() < 0.001f)
                return;
            // 방향이 거의 없으면 계산 중단

            dir.Normalize();
            // 방향 계산한다

            Projectile.Center = start;
            Projectile.velocity = Vector2.Zero;
            // 위치 고정한다

            Projectile.rotation = dir.ToRotation() + MathHelper.PiOver2;
            // 스프라이트가 세로 기준이라 +90도 보정한다

            Vector2 screenCenter = Main.screenPosition + new Vector2(Main.screenWidth, Main.screenHeight) / 2f;

            // 레이저 시작점 기준으로 투사체 위치를 끌어온다
            Player player = Main.player[Projectile.owner];
            Projectile.Center = player.Center;


        }
        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(new SoundStyle("CAmod/Sounds/DoGLaserWallBigAttack2"), Projectile.Center);
            // 투사체 생성 순간 원하는 사운드 재생한다

            
            // 플레이어를 Y축으로 1픽셀 위로 띄운다

            Projectile.localAI[0] = Projectile.Center.X;
            Projectile.localAI[1] = Projectile.Center.Y;
            // 초기값이라도 강제로 넣는다
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 start = new Vector2(Projectile.ai[0], Projectile.ai[1]);
            // 레이저 기준점이다

            Vector2 target = new Vector2(Projectile.localAI[0], Projectile.localAI[1]);
            // 조준 기준점이다

            Vector2 dir = target - start;
            // 방향 벡터를 구한다

            if (dir == Vector2.Zero)
                return false;
            // 방향이 없으면 판정도 만들 수 없다

            dir.Normalize();
            // 방향을 정규화한다

            Vector2 end = start + dir * 5000f;
            // 커서 위쪽으로 5000px 뻗는다

            start -= dir * 5000f;
            // 커서 아래쪽으로도 5000px 당겨서 총 길이 10000px로 만든다

            float collisionPoint = 0f;
            // 충돌 위치 저장용 변수다

            float fade = Projectile.timeLeft / 75f;
            // 코어가 줄어드는 시간 비율이다

            float scaleX = MathHelper.Lerp(0f, 1f, fade * fade);
            // 코어 X축에 쓰는 축소 곡선을 그대로 가져온다

            float collisionWidth = 500f * scaleX;
            // 물리 판정 두께도 같은 속도로 줄어들게 한다

            return Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(),
                targetHitbox.Size(),
                start,
                end,
                collisionWidth,
                ref collisionPoint
            );
            // 코어 X축 축소 속도와 동일하게 판정 폭을 검사한다
        }

        public override bool PreDraw(ref Color lightColor)
        {


            Texture2D tex = ModContent.Request<Texture2D>("CAmod/Projectiles/CosmicAttack").Value;
            // 초대형 세로 텍스처다
            Texture2D tex2 = ModContent.Request<Texture2D>("CAmod/Projectiles/UltraCosmicBeam2").Value;
            Vector2 start = new Vector2(Projectile.ai[0], Projectile.ai[1]);
            Vector2 target = new Vector2(Projectile.localAI[0], Projectile.localAI[1]);

            Vector2 dir = target - start;
            float length = dir.Length();
            dir.Normalize();
            // 길이 계산한다
            float lateFade = Projectile.timeLeft > 20 ? 1f : MathHelper.Clamp((Projectile.timeLeft - 10f) / 10f, 0f, 1f);

            // 아래쪽 기준으로 늘린다

            float scaleY = length / tex.Height;
            // 길이에 맞춰 세로 스케일 조정한다
            

            // X축 페이드 (시간 기반)
            float fade = Projectile.timeLeft / 75f;
            // 처음은 두껍고 끝으로 갈수록 얇아진다

            float fade2 = (float)Math.Sqrt(fade);
            float scaleX = MathHelper.Lerp(0f, 1f, fade*fade);
           

            float scaleX2 = (float)Math.Pow(MathHelper.Lerp(0f, 1f, fade), 0.3f);
            // 프레임 기반 랜덤 노이즈다 (부드럽지 않다)
            float noise = Main.rand.NextFloat();

            // 시간 단위로 끊어서 갱신 (너무 빠른 깜빡임 방지)
            int step = (int)(Main.GlobalTimeWrappedHourly * 60f / 3f);
            // 3프레임마다 값이 바뀐다

            UnifiedRandom rand = new UnifiedRandom(step + Projectile.identity * 999);

            // 랜덤값 생성한다
            noise = rand.NextFloat();

            // 노이즈 강도 적용 (상당히 강하게)
            float noiseStrength = 0.00f;

            // scaleX에 노이즈 반영한다
            scaleX *= MathHelper.Lerp(1f - noiseStrength, 1f + noiseStrength, noise);
            Color c1 = new Color(135, 255, 255);   // 하늘색
            Color c2 = new Color(200, 150, 255);   // 보라색
            Color c3 = new Color(255, 91, 231);   // 핑크색

            Color startColor = new Color(0, 255, 255);   // 하늘색
            Color endColor = new Color(255, 0, 180);   // 핑크색

            // scaleX 기반 진행도 (0~1)
            float t = 1f - scaleX;
            // 시작은 하늘색, 끝으로 갈수록 핑크색

            Color beamColor = Color.Lerp(startColor, endColor, t);

            // =============================
            // 단일 텍스처 다중 누적 레이저
            // =============================

            

           

            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f);

            // 점점 불투명해지는 기준값
            float baseAlpha = 0.01f;
            // 매우 낮게 시작한다


            Main.spriteBatch.End();

            // Additive 블렌딩으로 다시 시작한다
            Main.spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.Additive,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                Main.Rasterizer,
                null,
                Main.GameViewMatrix.TransformationMatrix
            );
            // Additive 켠다

            Vector2 origin2 = new Vector2(tex2.Width / 2f, tex2.Height / 2f);

            // Hyperdeath식 색 변화 적용
            float t2 = (float)Math.Pow(1f - scaleX, 2);
            Color beamColor2 = Color.Lerp(Color.Magenta, Color.Cyan, t2);
            Main.spriteBatch.Draw(
                tex2,
                start - Main.screenPosition,
                null,
                beamColor * fade,
                dir.ToRotation() + MathHelper.PiOver2,
                origin2,
                new Vector2(scaleX, 1f),
                SpriteEffects.None,
                0f
            );
            // 메인 레이저
            Main.spriteBatch.Draw(
                tex2,
                start - Main.screenPosition,
                null,
                beamColor * fade,
                dir.ToRotation() + MathHelper.PiOver2,
                origin2,
                new Vector2(scaleX , 1f),
                SpriteEffects.None,
                0f
            );
            Main.spriteBatch.Draw(
                tex2,
                start - Main.screenPosition,
                null,
                beamColor * 0.75f * lateFade ,
                dir.ToRotation() + MathHelper.PiOver2,
                origin2,
                new Vector2(scaleX, 1f),
                SpriteEffects.None,
                0f
            );
            Main.spriteBatch.Draw(
                tex2,
                start - Main.screenPosition,
                null,
                beamColor * 0.5f * lateFade,
                dir.ToRotation() + MathHelper.PiOver2,
                origin2,
                new Vector2(scaleX, 1f),
                SpriteEffects.None,
                0f
            );
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                Main.Rasterizer,
                null,
                Main.GameViewMatrix.TransformationMatrix
            );






            int coreLayers =20;
            // 코어 밀도다

            for (int i = 0; i < coreLayers; i++)
            {
                float progress = (float)i / (coreLayers - 1);
                progress = (float)Math.Pow(progress, 1.5f);
                // 0 ~ 1

                // =============================
                // X 절반부터 시작
                // =============================
                float layerScaleX = scaleX * 0.5f * MathHelper.Lerp(1f, 0.2f, progress);
                // 기존보다 절반에서 시작해서 더 압축한다
                width = layerScaleX;
                // =============================
                // 알파 (더 강하게)
                // =============================
                float alpha = 0.05f * (1f + i * 0.5f);
                // 빠르게 불투명해진다

                // =============================
                // 완전 검정
                // =============================
                Color layerColor = Color.Black;
                // 무조건 검정이다

                // =============================
                // 드로우
                // =============================
                Main.spriteBatch.Draw(
                    tex,
                    start - Main.screenPosition,
                    null,
                    layerColor * alpha*scaleX2 * fade * lateFade,
                    dir.ToRotation() + MathHelper.PiOver2,
                    origin,
                    new Vector2(layerScaleX, 1f),
                    SpriteEffects.None,
                    0f
                );
            }
            // =============================
            // 복구
            // =============================
            

            return false;
            // 기본 드로우 막는다
        }
    }
}