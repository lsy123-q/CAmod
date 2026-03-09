using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static System.Net.Mime.MediaTypeNames;


namespace CAmod.Projectiles
{
    public class RuneBoltProj : ModProjectile
    {
        private bool turn = true;
        private int targetNPC = -1; // 최초로 맞은 NPC의 인덱스를 저장한다
        private bool go = false;

        private int Time = 0;
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;

            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;

            Projectile.penetrate = -1; // 적 1명 관통 후 사라진다
            Projectile.timeLeft = 300;

            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {

            go = true;
        }
        private NPC FindClosestNPC(float maxDetectDistance)
        {
            NPC closestNPC = null; float sqrMaxDist = maxDetectDistance * maxDetectDistance; for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i]; 
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.catchItem > 0) continue; // 공격 불가능한 npc는 제외한다

                if (npc.ModNPC != null && npc.ModNPC.Name == "RockPillar")
    continue; // RockPillar NPC면 무시한다

               float sqrDist = Vector2.DistanceSquared(Projectile.Center, npc.Center);
                if (sqrDist < sqrMaxDist)
                { sqrMaxDist = sqrDist; closestNPC = npc; } } 
                 return closestNPC; 
        }
        public override void AI()
        {
            Time++;
            Vector2 oldPos = Projectile.Center - Projectile.velocity;
            Vector2 nextPos = Projectile.Center;

            float distMove = Vector2.Distance(oldPos, nextPos);

            // 더스트 간격을 약 1.5픽셀로 보간한다
            int stepCount = Math.Max(1, (int)(distMove / 1.5f));
            float interval = distMove / stepCount;



            for (int i = 1; i <= stepCount; i++)
            {
                float t = (i * interval) / distMove;

                // 이전 위치와 현재 위치 사이를 선형보간하여 더스트 위치를 만든다
                Vector2 dustPos = Vector2.Lerp(oldPos, nextPos, t);

                int d = Dust.NewDust(
                     dustPos - new Vector2(4, 4) ,
                    0,
                    0,
                    DustID.RuneWizard, // 룬볼트 더스트
                    0f,
                    0f,
                    0,
                    default,
                    Main.rand.NextFloat(0.8f, 1.8f) // 크기 난수
                );

                Main.dust[d].velocity *= 0.5f; // 더스트가 퍼지지 않게 한다
                Main.dust[d].noGravity = true; // 중력 영향을 받지 않게 한다
            }
            NPC target = null;

            // 이미 맞춘 npc가 있다면 그 npc를 계속 추적한다
            if (targetNPC != -1)
            {
                NPC npc = Main.npc[targetNPC];
                if (npc.active && !npc.dontTakeDamage)
                {
                    target = npc; // 최초 타겟을 계속 사용한다
                }
            }

            // 아직 타겟이 없다면 기존 방식으로 탐색한다
           
                NPC closest = FindClosestNPC(500f);

                if (closest != null)
                {
                    // 기존 타겟이 없으면 바로 설정한다
                    if (targetNPC == -1)
                    {
                        targetNPC = closest.whoAmI; // 가장 가까운 npc를 타겟으로 지정한다
                    }
                    else
                    {
                        NPC current = Main.npc[targetNPC];

                        if (current.active)
                        {
                            float currentDist = Vector2.DistanceSquared(Projectile.Center, current.Center);
                            float newDist = Vector2.DistanceSquared(Projectile.Center, closest.Center);

                            // 더 가까운 npc가 있다면 타겟을 교체한다
                            if (newDist < currentDist)
                            {
                                targetNPC = closest.whoAmI;
                            }
                        }
                        else
                        {
                            // 기존 타겟이 죽었으면 교체한다
                            targetNPC = closest.whoAmI;
                        }
                    }
                }
            if (target == null) {
                Time = 0;
            }
            if (target != null)
            {
                float rd = Math.Min(target.width, target.height); // 반지름
                float rd2 = rd * 3.141592f * 2f; //원의 둘레

                float rd3 = rd / 20f; // 원의 둘레를 14등분 즉, 14속도면 @프레임만에 한바퀴 돈다

                float angle = 360f / rd3; // 필요한 선회각
                float angle4 = angle / 360f;
                float lastangle = MathHelper.ToRadians(angle/360f);

                Vector2 direction = target.Center - Projectile.Center;
                float distance = direction.Length();
                direction.Normalize();

                float speed = Projectile.velocity.Length();
                float currentRot = Projectile.velocity.ToRotation();
                float targetRot = direction.ToRotation();
                float super;
                // [핵심] AngleLerp를 위해 각도 차이를 -Pi ~ Pi 범위로 정규화
                float rotationError = MathHelper.WrapAngle(targetRot - currentRot);
                float absError = Math.Abs(rotationError); // 회전 오차 절대값
           
                if (absError > 0.0001f)
                {
                    lastangle = lastangle / absError; // 최대 선회각 기준 lerp 보정
                }
                if (lastangle > 1f)
                {
                    super = lastangle;
                    lastangle = 1f; // lerp는 1 이상 의미 없음
                }

                // 유도 강도 설정 (0.05 ~ 0.2 정도가 적당함)
                // lifeRatio에 따라 강해지게 하고 싶다면 최대치를 제한하는 것이 좋습니다.
                float lifeRatio = (float)Time / 300f;
                float lerpPower = MathHelper.Lerp(0.01f, angle4, (float)Math.Pow(lifeRatio, 0.75f));

                
                // 새로운 각도 계산
                float nextRot = currentRot + rotationError * lerpPower;

                Projectile.velocity = nextRot.ToRotationVector2() * speed;
                // 투사체 이미지 자체의 회전이 필요하다면 추가
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }

        }
    }
}