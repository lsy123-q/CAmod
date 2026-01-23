using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using CAmod.Systems;
using Terraria.ID;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using System;
using System.Linq;
using CAmod.Items;
using CAmod.Buffs;
namespace CAmod.Players
{
    public class DimGatePlayer : ModPlayer
    {
        public float cap = 27f;
        public int gateEndGrace = 0;
        public int gateTime;        // 무적 지속 시간
        public int gateCooldown;    // 쿨타임
        public bool startflag = false;
        public bool endflag = false;
        public int progress = 0;
        public int gateGeneration = 0;
        private Vector2 oldpos;
        
        public int dustprogress = 0;

        public bool dimGateEquipped;
        public int endprogress = 0;
        public override void ResetEffects()
        {
            dimGateEquipped = false;
        }

        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
            float alpha;
            float asd = 1f;

            if (gateTime>0) {
                asd = 1 - (dustprogress / 30f);
            }

            else if (endprogress >= 0 && endflag == true)
            {
                asd = (endprogress / 10f);

            }



                alpha = MathHelper.Lerp(0.05f, 1f, asd);

        

            drawInfo.colorArmorBody *= alpha;
                drawInfo.colorArmorLegs *= alpha;
                drawInfo.colorArmorHead *= alpha;
                drawInfo.colorHair *= alpha;
                drawInfo.colorEyeWhites *= alpha;
                drawInfo.colorEyes *= alpha;
                drawInfo.colorBodySkin *= alpha;
                drawInfo.colorUnderShirt *= alpha;
                drawInfo.colorShirt *= alpha;
                drawInfo.colorPants *= alpha;
                drawInfo.colorShoes *= alpha;
                drawInfo.colorHead *= alpha;
                drawInfo.colorLegs *= alpha;
                drawInfo.cWings *= (int)alpha;


        }

        public override void PostUpdate()
        {
            

           




            Vector2 currentCenter = Player.Center;
            // 무적 타이머 감소
            if (gateTime > 0)
                gateTime--;

            // 쿨타임 감소
            if (gateCooldown > 0)
                gateCooldown--;

            // 단축키 입력 처리
            if (KeySystem.DimGate.JustPressed && dimGateEquipped && Player.statManaMax2 >= 400)
            {
                if (gateTime > 0 && gateTime < 270)
                {
                    // 무적 중 재시전 시 즉시 종료한다
                    gateTime = 0;
                    endflag = true;
                    gateCooldown = 60 * 30;
                    Player.dashDelay = 0;


                    SoundEngine.PlaySound(
                     new SoundStyle("CAmod/Sounds/close")
                     {
                         Volume = 1.0f,
                         Pitch = 0.0f,
                         MaxInstances = 1
                     },
                     Player.Center
                 );
                    dustprogress = 0;
                }
                else if (gateCooldown <= 0 && gateTime ==0 && Player.CheckMana(400, true))
                {
                    // 새로 발동한다
                    
                    gateTime = 60 * 5;      // 5초 무적
                     // 15초 쿨타임
                    gateGeneration++;
                    startflag = true;
                    

                    SoundEngine.PlaySound(
                    new SoundStyle("CAmod/Sounds/open")
                    {
                        Volume = 1.0f,
                        Pitch = 0.0f,
                        MaxInstances = 1
                    },
                    Player.Center
                );

                }
            }

            // 자연 종료 시 종료 연출을 발생시킨다
            if (gateTime == 1) {
                gateCooldown = 60 * 30;
                Player.dashDelay = 0;
               endflag = true;
                SoundEngine.PlaySound(
                    new SoundStyle("CAmod/Sounds/close")
                    {
                        Volume = 1.0f,
                        Pitch = 0.0f,
                        MaxInstances = 1
                    },
                    Player.Center
                );
                dustprogress = 0; 

            }

            if (gateEndGrace > 0)
                gateEndGrace--;
            if (gateTime == 0)
            {
                gateEndGrace = 30;
            }

            if (gateTime > 0 && Player.dashDelay < 0)
            {
                // 면역 종료 직후 유예 구간이 아닐 때만 차단
                Player.dash = -1;
                Player.dashDelay = gateTime;
                Player.velocity.X *= 0.6f;
            }
            if (gateTime > 0) {
                Player.dash = -1;
                Player.dashDelay = gateTime;

                    if (Math.Abs(Player.velocity.X) > cap)
                        Player.velocity.X *= 0.3f;
                    // X축 속도를 ±28.5로 제한한다

                    if (Math.Abs(Player.velocity.Y) > cap)
                        Player.velocity.Y *= 0.3f;
                
            }
            
            if (Player.dashDelay < 0 && gateTime > 0 &&  gateTime < 270)
            {
                gateCooldown = 60 * 30;
                gateTime = 0;
                endflag = true;
                SoundEngine.PlaySound(
                    new SoundStyle("CAmod/Sounds/close")
                    {
                        Volume = 1.0f,
                        Pitch = 0.0f,
                        MaxInstances = 1
                    },
                    Player.Center
                );
                dustprogress = 0;

            }
           

            if (startflag == true)
            {
                progress++;
                dustprogress++;
                if (progress < 30)
                {

                    SpawnGateStartDust(progress,Player.Center);

                    if ((int)(Vector2.Distance(oldpos, Player.Center) / 5f) >= 1)
                    {
                        float dist = Vector2.Distance(oldpos, Player.Center); // 이전 좌표와 현재 좌표 거리다
                        int count = (int)(dist / 5f); // 2px 기준으로 개수만 산출한다

                        Vector2 dir = Player.Center - oldpos; // 이동 방향이다
                        dir.Normalize(); // 방향 벡터를 정규화한다

                        float step = dist / count; // 전체 거리를 균등 분할한 실제 간격이다

                        for (int i = 1; i <= count; i++)
                        {
                            Vector2 interpPos = oldpos + dir * (step * i); // 균등 간격으로 보정된 좌표다


                            SpawnGateStartDust(progress, interpPos); // 중간 지점에서 더스트를 생성한다

                        }
                    }

                    else {
                        SpawnGateStartDust(progress, Player.Center);
                    }

                }

                if (progress >= 30) { 
                    startflag = false;
                    progress = 0;
                }
            }

            if (endflag == true) {
                endprogress++;

                if (endprogress < 10) { 

                    SpawnGateEndDust(Player.Center, endprogress);

                if ((int)(Vector2.Distance(oldpos, Player.Center) / 5f) >= 1)
                {
                    float dist = Vector2.Distance(oldpos, Player.Center); // 이전 좌표와 현재 좌표 거리다
                    int count = (int)(dist / 5f); // 2px 기준으로 개수만 산출한다

                    Vector2 dir = Player.Center - oldpos; // 이동 방향이다
                    dir.Normalize(); // 방향 벡터를 정규화한다

                    float step = dist / count; // 전체 거리를 균등 분할한 실제 간격이다

                    for (int i = 1; i <= count; i++)
                    {
                        Vector2 interpPos = oldpos + dir * (step * i); // 균등 간격으로 보정된 좌표다


                        SpawnGateEndDust(interpPos, endprogress); // 중간 지점에서 더스트를 생성한다

                    }
                }

                else
                {
                    SpawnGateEndDust(Player.Center, endprogress);
                }

                }
                if (endprogress >= 10)
                {
                    endflag = false;
                    endprogress = 0;
                }

            }


            if (gateTime > 0 && gateTime < 270)
            {
                
                Player.aggro = -9999;
                Player.GetDamage(DamageClass.Generic) *= 0f;
                Player.GetDamage(DamageClass.Melee) *= 0f;
                Player.GetDamage(DamageClass.Magic) *= 0f;
                Player.GetDamage(DamageClass.Ranged) *= 0f;
                Player.GetDamage(DamageClass.Summon) *= 0f;
            }

            oldpos = currentCenter;

            if (gateTime > 0)
            {
                int buffType = ModContent.BuffType<ToAnotherWorld>();
                if (!Player.HasBuff(buffType))
                    Player.AddBuff(buffType, gateTime);
                else
                {
                    int idx = Player.FindBuffIndex(buffType);
                    if (idx >= 0)
                        Player.buffTime[idx] = gateTime;
                }
            }
            else
            {
                if (gateCooldown > 0)
                {
                    
                    Player.ClearBuff(ModContent.BuffType<ToAnotherWorld>());
                    int buffType = ModContent.BuffType<DimensionGateCooltime>();
                    Player.AddBuff(buffType, gateCooldown);
                }
                else
                {
                    Player.ClearBuff(ModContent.BuffType<DimensionGateCooltime>());
                }
            }
        }
        public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
        {
            if (gateTime > 0 && gateTime < 270)
            {
                damage *= 0f; // 무적 도중 플레이어의 모든 최종 공격력을 0으로 만든다
                Player.controlUseItem = false;   // 좌클릭을 봉인한다
                Player.controlUseTile = false;   // 우클릭을 봉인한다
                Player.releaseUseItem = true;    // 강제 입력 해제 처리다
                Player.releaseUseTile = true;    // 강제 입력 해제 처리다
            }
        }
        // ================= 무적 판정 =================

        public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
        {
            if (gateTime > 0 && gateTime < 270)
            {
                int projGen = (int)proj.localAI[0];

                if (projGen == gateGeneration)
                {
                    modifiers.FinalDamage *= 0f;
                    // 게이트 이후 생성된 투사체만 무효화한다
                }
            }
        }
        public override bool CanHitNPC(NPC target)
        {
            if (gateTime < 0 && gateTime < 270)
                return false;

            return true;
        }

        public override bool FreeDodge(Player.HurtInfo info)
        {
            // 차원 게이트 발동 중이면 모든 피해를 회피 처리한다
            return gateTime > 0 || base.FreeDodge(info);
        }
        public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
        {
            if (gateTime > 0 && gateTime < 270)
            {
                modifiers.FinalDamage *= 0f; // NPC 피해를 완전히 0으로 만든다
            }
        }

       
        
        // ================= 연출 =================

        private void SpawnGateStartDust(int speed, Vector2 pos)
        {
            if (speed == 0) {
                speed = 1;
            }
            int count = (int)(10f * (1f - (speed / 60f))) ;
            if (count == 0) {
                count = 1;
            }
            float radius = 120* (1f-(speed/30f));

            float size = ((speed + 100f) / 100f)*2;

            for (int i = 0; i < count; i++)
            {
                float rot = MathHelper.TwoPi * i / count;
                Vector2 offset = Main.rand.NextVector2Unit() * radius;

                int type = DustID.FireworksRGB; // RGB 폭죽을 사용한다
                Color color = Main.rand.NextBool()
                    ? new Color(170, 80, 255)   // 보라색이다
                    : new Color(80, 160, 255);  // 파란색이다
                


                Vector2 vel = offset.SafeNormalize(Vector2.Zero) * -5f * (1f - (speed / 30f)); ; // 안쪽으로 수축한다
                
                Dust d = Dust.NewDustPerfect(
                    pos + offset,
                    type,
                    vel,
                    150,
                    default,
                    1.0f * size
                );
                d.scale *= 0.7f;
                d.alpha *= 2;
                d.color = color; // 색을 강제로 적용한다
                d.velocity.Y /= 1.125f;
                d.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                d.noGravity = true;
            }
        }

       
        private void SpawnGateEndDust(Vector2 pos,int speed)
        {
            if (speed == 0) {
                speed = 1;
            }

            float asd2 = speed / 10f;

            int count = 15;

            
            float radius = 120 * (speed / 10f);
            float size = ((speed + 100f) / 100f) * 2;
            for (int i = 0; i < count; i++)
            {
                float rot = MathHelper.TwoPi * i / count;
                Vector2 offset = Main.rand.NextVector2Unit() * radius;

                int type = DustID.FireworksRGB; // RGB 폭죽을 사용한다
                Color color = Main.rand.NextBool()
                    ? new Color(170, 80, 255)   // 보라색이다
                    : new Color(80, 160, 255);  // 파란색이다

                float asd =  speed/10f;

                

                float lastspeed = MathHelper.Lerp(2.5f, 5f, asd);


                Vector2 vel = offset.SafeNormalize(Vector2.Zero) * lastspeed; // 바깥으로 팽창한다

                Dust d = Dust.NewDustPerfect(
                    pos,
                    type,
                    vel,
                    150,
                    default,
                    1f * size
                );
                d.scale *= 0.7f;
                d.alpha *= 2;
                d.color = color; // 색을 강제로 적용한다
                d.velocity.Y /= 1.125f;
                d.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                d.noGravity = true;
            }
        }

    }
}
