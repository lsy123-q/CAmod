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
using System.Reflection;
using CAmod.Particles;

using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
namespace CAmod.Players
{
    public class DimGatePlayer : ModPlayer
    {
        public float cap = 27f;
        public int gateEndGrace = 0;
        public int gateTime;        // 무적 지속 시간
        public int gateCooldown;    // 쿨타임

        public int gateCooldownmax ;    // 쿨타임

        public bool startflag = false;
        public bool endflag = false;
        public int progress = 0;
        public int gateGeneration = 0;
        private Vector2 oldpos;
        
        public int dustprogress = 0;

        public bool dimGateEquipped;
        public int endprogress = 0;
        private float startDustRandomRot;
        private float endDustRandomRot;
        private float startDustDir;
        private float endDustDir;
        public override void ResetEffects()
        {
            dimGateEquipped = false;
        }
        public override void OnRespawn()
        {
            gateTime = 0;          // 무적 시간을 초기화한다
            gateCooldown = 0;      // 쿨타임을 초기화한다
            gateCooldownmax = 0;   // 최대 쿨타임을 초기화한다

            startflag = false;     // 시작 연출 플래그를 초기화한다
            endflag = false;       // 종료 연출 플래그를 초기화한다

            progress = 0;          // 시작 연출 진행도를 초기화한다
            endprogress = 0;       // 종료 연출 진행도를 초기화한다
            dustprogress = 0;      // 더스트 진행도를 초기화한다

            gateEndGrace = 0;      // 종료 유예 시간을 초기화한다

            // 사망 후 부활 시 차원 게이트의 모든 액티브 상태를 리셋한다
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!dimGateEquipped || target.friendly || damageDone <= 0)
                return;

            if (hit.DamageType != DamageClass.Magic)
                return;


            if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
            {
                try
                {
                   
                    Type vulnType = calamity.Code.GetType("CalamityMod.Buffs.DamageOverTime.GodSlayerInferno");

                    

                    if (vulnType != null)
                    {
                        int vulnID = calamity.Find<ModBuff>(vulnType.Name).Type;
                        target.AddBuff(vulnID, 180);
                    }
                }
                catch { }
            }
        }
        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
            float alpha;
            float asd = 1f;

            if (gateTime>0) {
                asd = 1 - (dustprogress / 20f);
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
            if (gateCooldown > 0&&dimGateEquipped)
                gateCooldown--;


            float costMult = Player.manaCost;  // 현재 무기 기준 마나 코스트 계산한다
            int finalCost = (int)(500 * costMult); // 500에 코스트 배율 적용한다



                // 단축키 입력 처리
                if (KeySystem.DimGate.JustPressed && dimGateEquipped && Player.statManaMax2 >= finalCost)
            {
                if (gateTime > 0 && gateTime < 270)
                {

                    

                    // 무적 중 재시전 시 즉시 종료한다
                    
                    endflag = true;
                    int baseCooldown = 60 * 25; // 기본 쿨타임 25초다
                    int maxReduce = 60 * 10; // 최대 감소량 10초다
                    endDustRandomRot = Main.rand.NextFloat(MathHelper.TwoPi);
                    endDustDir = Main.rand.NextBool() ? 1f : -1f; // 방향 고정한다
                    // 남은 지속시간 비율을 계산한다 (0~1)
                    float ratio = gateTime / 270f;

                    // 비례 감소량을 산출한다
                    int reduce = (int)(maxReduce * ratio);

                    // 최종 쿨타임을 계산한다
                    gateCooldown = baseCooldown - reduce;
                    gateCooldownmax = baseCooldown - reduce;
                    gateTime = 0;

                    Player.dashDelay = 0;


                    SoundEngine.PlaySound(
                     new SoundStyle("CAmod/Sounds/close")
                     {
                         Volume = 1.5f,
                         Pitch = 0.0f,
                         MaxInstances = 1
                     },
                     Player.Center
                 );
                    dustprogress = 0;
                }
                else if (gateCooldown <= 0 && gateTime ==0 && Player.CheckMana(500, true))
                {
                    // 새로 발동한다
                    // 무적 진입 시 기존 입력을 강제로 끊는다
                    Player.controlUseItem = false;   // 좌클릭 차단한다
                    Player.controlUseTile = false;   // 우클릭 차단한다
                    Player.releaseUseItem = true;    // 눌림 상태를 해제한다
                    Player.releaseUseTile = true;    // 눌림 상태를 해제한다
                    Player.immune = true;
                    Player.immuneTime = 1;
                    gateTime = 60 * 5;      // 5초 무적
                     // 15초 쿨타임
                    gateGeneration++;
                    startflag = true;
                    startDustRandomRot = Main.rand.NextFloat(MathHelper.TwoPi);
                    startDustDir = Main.rand.NextBool() ? 1f : -1f; // 방향 고정한다

                    SoundEngine.PlaySound(
                    new SoundStyle("CAmod/Sounds/open")
                    {
                        Volume = 1.5f,
                        Pitch = 0.0f,
                        MaxInstances = 1
                    },
                    Player.Center
                );

                }
            }

            // 자연 종료 시 종료 연출을 발생시킨다
            if (gateTime == 1) {

                gateCooldownmax = 60 * 25; // 추가
                gateCooldown = 60 * 25;
                Player.dashDelay = 0;
               endflag = true;
                endDustRandomRot = Main.rand.NextFloat(MathHelper.TwoPi);
                endDustDir = Main.rand.NextBool() ? 1f : -1f; // 방향 고정한다
                SoundEngine.PlaySound(
                    new SoundStyle("CAmod/Sounds/close")
                    {
                        Volume = 1.5f,
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

                
                gateCooldown = 60 * 25;
                gateTime = 0;
                endflag = true;
                endDustRandomRot = Main.rand.NextFloat(MathHelper.TwoPi);
                endDustDir = Main.rand.NextBool() ? 1f : -1f; // 방향 고정한다
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
                if (progress < 20)
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

                if (progress >= 20) { 
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

                    if (Player.whoAmI == Main.myPlayer)
                    {
                        int projType = ModContent.ProjectileType<Projectiles.GodKiller>();
                        int baseDamage = 750;
                        int damage = (int)Player.GetTotalDamage(DamageClass.Magic).ApplyTo((float)baseDamage);
                        float knockBack = 5f;
                        float speed = 5f;
                        int count = 8;

                        for (int i = 0; i < count; i++)
                        {
                            float angle = MathHelper.TwoPi * (i + 0.5f) / count;
                            Vector2 velocity = angle.ToRotationVector2() * speed;

                            Projectile.NewProjectile(
                                Player.GetSource_FromThis(),
                                Player.Center,
                                velocity,
                                projType,
                                damage,
                                knockBack,
                                Player.whoAmI
                            );
                        }
                    }

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

        public override bool CanBeHitByProjectile(Projectile proj)
        {
            if (gateTime > 0 && gateTime < 270)
            {
                return false; // 무적 중에는 투사체 피격 자체를 부정한다
            }

            return true;
        }

        public override bool CanBeHitByNPC(NPC npc, ref int cooldownSlot)
        {
            if (gateTime > 0 && gateTime < 270)
            {
                return false; // 무적 중에는 NPC 접촉 피격 자체를 부정한다
            }

            return true;
        }

        // ================= 연출 =================

        private void SpawnGateStartDust(int speed, Vector2 pos)
        {
            if (speed == 0)
            {
                speed = 1;
            }

            int count = (int)(20f * (1f - (speed / 60f)));

            if (count == 0)
            {
                count = 1;
            }

            // 반지름 음수 방지한다
            float t = MathHelper.Clamp(speed / 20f, 0f, 1f);
            float radius = 130f * (1f - t);

            float size = ((speed + 100f) / 100f) * 2f;

            // 20프레임 동안 180도 회전한다
            float rotation = startDustRandomRot +
                             MathHelper.TwoPi * t * startDustDir;

            for (int i = 0; i < count; i++)
            {
                float baseRot = MathHelper.TwoPi * i / count;

                // 고정된 아스테로이드 형태를 만든다
                float x = (float)Math.Pow(Math.Cos(baseRot), 3);
                float y = (float)Math.Pow(Math.Sin(baseRot), 3);

                Vector2 offset = new Vector2(x, y) * radius;

                // 생성 위치 전체를 회전시킨다
                offset = offset.RotatedBy(rotation);

                Color color = Main.rand.NextBool()
                    ? new Color(170, 80, 255)
                    : new Color(80, 160, 255);

                // 기존처럼 중심 방향으로 수축한다
                Vector2 velocity =
                    offset.SafeNormalize(Vector2.Zero) *
                    -5f *
                    (1f - t);

               
               

                // 기존 Dust의 랜덤 회전을 그대로 사용한다
                float particleRotation =
                    Main.rand.NextFloat(MathHelper.TwoPi);

                CosmicParticleSystem.Buffer.Create(
                    new ParticleInfo(
                        position: (pos + offset).ToNumerics(),
                        velocity: velocity.ToNumerics(),
                        rotation: particleRotation,
                        scale: new System.Numerics.Vector2(
                            size * 10f,
                            size * 10f
                        ),
                        depth: 1f,
                        color: color,
                        duration: 180
                    )
                );
                
            }
        }


        private void SpawnGateEndDust(Vector2 pos, int speed)
        {
            if (speed == 0)
            {
                speed = 1;
            }

            int count = 20;

            // 0~1 정규화한다
            float t = MathHelper.Clamp(speed / 10f, 0f, 1f);

            // 발산하므로 반지름을 증가시킨다
            float radius = 45f * t;

            float size = ((speed + 100f) / 100f) * 2f;

            // 시작과 동일하게 180도 회전한다
            float rotation =
                endDustRandomRot +
                MathHelper.Pi * t * endDustDir;

            float lastSpeed =
                MathHelper.Lerp(2.0f, 2.5f, t);

            for (int i = 0; i < count; i++)
            {
                float baseRot = MathHelper.TwoPi * i / count;

                // 아스테로이드 형태를 만든다
                float x = (float)Math.Pow(Math.Cos(baseRot), 3);
                float y = (float)Math.Pow(Math.Sin(baseRot), 3);

                Vector2 offset = new Vector2(x, y) * radius;

                // 전체 회전을 적용한다
                offset = offset.RotatedBy(rotation);

                Color color = Main.rand.NextBool()
                    ? new Color(170, 80, 255)
                    : new Color(80, 160, 255);

                // 바깥쪽으로 발산한다
                Vector2 velocity =
                    offset.SafeNormalize(Vector2.Zero) *
                    lastSpeed;

                

                float particleRotation =
                    Main.rand.NextFloat(MathHelper.TwoPi);

                CosmicParticleSystem.Buffer.Create(
                    new ParticleInfo(
                        position: (pos + offset).ToNumerics(),
                        velocity: velocity.ToNumerics(),
                        rotation: particleRotation,
                        scale: new System.Numerics.Vector2(
                            size * 10f,
                            size * 10f
                        ),
                        depth: 1f,
                        color: color,
                        duration: 180
                    )
                );
               
            }
        }

    }
}
