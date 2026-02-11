using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CAmod.Projectiles;
using System;
using System.Reflection;
using Terraria.Audio;
namespace CAmod.Players
{
    public class BloodMagePlayer : ModPlayer
    {


        public float vampCooldownMax; // UI용 최대 쿨타임을 저장한다
        public bool bloodMageEquipped;
        public int def;
        public float cachedRegenPerSecond;
        // 장신구로 재생을 0으로 만들기 전에 "기존 초당 재생량"을 캐싱한다
        public bool allowBloodHeal;
        public float vampCooldown;
        public bool wasBloodMageEquipped;
        public int lastLife; SoundStyle[] sounds =
{
    SoundID.NPCHit9,
    SoundID.NPCDeath22,  // Drippler 사망음이다
    SoundID.NPCDeath11  // Creeper 사망음이다
};


        
        public override void ResetEffects()
        {
            wasBloodMageEquipped = bloodMageEquipped;
            bloodMageEquipped = false;
            allowBloodHeal = false;
            // 기본적으로 모든 치유를 차단한다
        }
        public override void OnRespawn()
        {
            lastLife = Player.statLifeMax2;
            // 부활 시 기준 체력을 최대 체력으로 재설정한다
        }
        public override void PostUpdateEquips()
        {
            if (!bloodMageEquipped)
                return;


         
           
           
        }
        public override void PostUpdate()
        {

            if (Player.creativeGodMode) {
                lastLife = Player.statLifeMax2;
            }


            if(bloodMageEquipped == true) { 

            cachedRegenPerSecond = (Player.lifeRegen > 0) ? (Player.lifeRegen / 2f) : 0f;


            


            }

            if (bloodMageEquipped && !wasBloodMageEquipped)
            {
                lastLife = Player.statLife;
                // 장신구 착용 시점의 체력을 기준으로 저장한다
            }


            if (!bloodMageEquipped && wasBloodMageEquipped && vampCooldown > 0f)
            {
              
                // 쿨타임 도중 장신구를 해제하면 즉사한다
                return;
            }


            int missing = Player.statLifeMax2 - Player.statLife;
            
                float missingRatio = MathHelper.Clamp(
                    missing / (float)Player.statLifeMax2,
                    0f,
                    1f
                );
                // 현재 잃은 체력 비율이다

                float accel = MathHelper.Lerp(1f, 2.0f, missingRatio);


                if (vampCooldown > 0f && bloodMageEquipped)
                {
                    vampCooldown -= accel * (5f + cachedRegenPerSecond);
               
                }
            

           if (vampCooldown <= 0f && bloodMageEquipped)
            {
                vampCooldown = 0f;

                
            }
            
            

            if (!Player.dead && bloodMageEquipped)
            {
                int missingLife = Player.statLifeMax2 - Player.statLife;
                if (missingLife > 0)
                {
                    float ratio = MathHelper.Clamp(
                        missingLife / (float)Player.statLifeMax2,
                        0f,
                        1f
                    );
                    // 잃은 체력이 최대 체력 대비 몇 %인지다

                    int dustCount = (int)MathHelper.Lerp(0, 5, ratio * ratio);
                    // 잃은 체력이 많을수록 더 많이 흘린다

                    for (int i = 0; i < dustCount; i++)
                    {
                        int d = Dust.NewDust(
                            Player.position,
                            Player.width,
                            Player.height,
                            DustID.Blood,
                            0f,
                            Main.rand.NextFloat(0.2f, 0.4f),
                            120,
                            default,
                            MathHelper.Lerp(0.6f, 0.8f, ratio)
                        );
                        Main.dust[d].fadeIn = 1.25f;

                        Main.dust[d].velocity.X = 0f;
                        Main.dust[d].noGravity = false;
                        Main.dust[d].velocity.Y *= 0.4f;
                        // 바닥으로 질척하게 떨어지게 만든다
                    }
                }
            }

            if (vampCooldown > 0f && !allowBloodHeal && !Player.creativeGodMode)
            {
                if (Player.statLife > lastLife)
                {
                    int diff = Player.statLife - lastLife;

                    Player.statLife = lastLife;
                    // 기준 체력보다 증가한 모든 회복을 차단한다

                    if(diff > 2) { 
                    CombatText.NewText(
     Player.getRect(),
     new Color(180, 30, 30),
     "-" + diff
 );
                    }
                }

                if (Player.statLife < lastLife) {

                    lastLife = Player.statLife;
                }


            }

            if (bloodMageEquipped && !allowBloodHeal && !Player.creativeGodMode)
            {
                if (Player.statLife > lastLife)
                {
                    int diff = Player.statLife - lastLife;

                    Player.statLife = lastLife;
                    // 기준 체력보다 증가한 모든 회복을 차단한다

                    if (diff > 2)
                    {
                        CombatText.NewText(
         Player.getRect(),
         new Color(180, 30, 30),
         "-" + diff
     );
                    }
                }

                if (Player.statLife < lastLife)
                {

                    lastLife = Player.statLife;
                }


            }




            if (bloodMageEquipped)
            {
                int missing2 = Player.statLifeMax2 - Player.statLife;
                float missingRatio2 = MathHelper.Clamp(
                    missing2 / (float)Player.statLifeMax2,
                    0f,
                    1f
                );
                // 잃은 체력 비율이다 (0~1)

                float bonus = MathHelper.Lerp(0f, 0.25f, missingRatio2);
                // 최대 10%까지 증가한다

                Player.GetDamage(DamageClass.Magic) += bonus;
                // 마법 피해를 잃은 체력 비례로 증가시킨다
            }
            if (Player.creativeGodMode)
            {
                lastLife = Player.statLifeMax2;
            }

        }
        public override void UpdateLifeRegen()
        {
            if (!bloodMageEquipped)
                return;
        }
    
        

        private void SpawnHitGoreFromSource(Vector2 sourceCenter, Player.HurtInfo hurtInfo)
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            int[] gorePool =
            {
        669, 135, 671, 877,1128,1129,1130,1221
    };
            // 위키에서 선별한 고어 ID 풀이다

            int goreType = gorePool[Main.rand.Next(gorePool.Length)];
            // 랜덤으로 하나 선택한다

            float damageRatio = MathHelper.Clamp(
                hurtInfo.Damage / (float)Player.statLifeMax2,
                0f,
                1f
            );
            // 받은 피해가 최대 체력 대비 몇 %인지다

            
            

            Vector2 dir = Player.Center - sourceCenter;
            // 공격자 위치 기준으로 반대 방향(밀려나가는 방향)을 만든다

            if (dir.LengthSquared() < 0.0001f)
                dir = new Vector2(-hurtInfo.HitDirection, -0.2f);
            // 위치가 겹치거나 정보가 없을 때만 HitDirection으로 대체한다

            dir.Normalize();
            dir.Y -= 0.25f;
            dir.Normalize();
            // 약간 위로 튀는 느낌을 준다

            Vector2 vel =
                dir * MathHelper.Lerp(2.5f, 5.0f, damageRatio) +
                Main.rand.NextVector2Circular(0.8f, 0.8f);
            // 기본 방향을 유지하면서 랜덤만 소량 섞는다

            Gore.NewGore(
                Player.GetSource_Misc("BloodMageHit"),
                Player.Center,
                vel,
                goreType,
                0.5f
            );
            // 플레이어 위치에서 고어가 반대방향으로 튄다
        }

        public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
        {

            if (bloodMageEquipped)
            {
                int lastdamage = hurtInfo.Damage;
                try
                {
                    Mod calamity = ModLoader.GetMod("CalamityMod");
                    if (calamity != null)
                    {
                        var modPlayersField = typeof(Player).GetField("modPlayers", BindingFlags.Instance | BindingFlags.NonPublic);
                        var modPlayersList = modPlayersField?.GetValue(Player) as System.Collections.IEnumerable;

                        foreach (var mp in modPlayersList)
                        {
                            if (mp.GetType().FullName == "CalamityMod.CalPlayer.CalamityPlayer")
                            {
                                var bleedField = mp.GetType().GetField("chaliceBleedoutToApplyOnHurt", BindingFlags.Instance | BindingFlags.Public);
                                if (bleedField != null)
                                {
                                    int bleedThisHit = (int)bleedField.GetValue(mp);
                                    lastdamage += bleedThisHit; // 🩸 이번 피격에서 출혈로 전환된 피해량 추가
                                }
                                break;
                            }
                        }
                    }
                }
                catch
                {
                    // Calamity가 없거나 구조 변경 시 조용히 무시
                }

                float damageRatio = MathHelper.Clamp(
        lastdamage / (float)Player.statLifeMax2,
        0f,
        1f
    );

                





                // 받은 피해가 최대 체력 대비 몇 %인지다
                int bloodcount = lastdamage / 2;
            float dustScale = MathHelper.Lerp(0.8f, 2.0f, damageRatio);
            // 데미지가 클수록 더 크게 튄다

            Vector2 hitDir = Player.Center - npc.Center;
            // 공격자가 있는 쪽 → 맞은 방향이다

            if (hitDir.LengthSquared() < 0.0001f)
                hitDir = new Vector2(hurtInfo.HitDirection, 0f);
            // 예외 상황 대비용 보정이다

            hitDir.Normalize();

            for (int i = 0; i < 25; i++)
            {
                Vector2 vel =
                    hitDir * Main.rand.NextFloat(2.5f, 5.0f) +
                    Main.rand.NextVector2Circular(0.6f, 0.6f);
                // 맞은 방향을 중심으로 퍼지게 만든다

                int d = Dust.NewDust(
                    Player.Center,
                    0,
                    0,
                    DustID.Blood,
                    vel.X,
                    vel.Y,
                    100,
                    default,
                    dustScale
                );

                Main.dust[d].noGravity = false;
            }

                int gorecount = lastdamage / 25;
                if (gorecount > 50)
                {
                    gorecount = 50;
                }
                for (int a = 0; a< gorecount; a++) { 
            SpawnHitGoreFromSource(npc.Center, hurtInfo);
            }

           }
        }

        public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
        {
            if (bloodMageEquipped) {
                int lastdamage = hurtInfo.Damage;
                try
                {
                    Mod calamity = ModLoader.GetMod("CalamityMod");
                    if (calamity != null)
                    {
                        var modPlayersField = typeof(Player).GetField("modPlayers", BindingFlags.Instance | BindingFlags.NonPublic);
                        var modPlayersList = modPlayersField?.GetValue(Player) as System.Collections.IEnumerable;

                        foreach (var mp in modPlayersList)
                        {
                            if (mp.GetType().FullName == "CalamityMod.CalPlayer.CalamityPlayer")
                            {
                                var bleedField = mp.GetType().GetField("chaliceBleedoutToApplyOnHurt", BindingFlags.Instance | BindingFlags.Public);
                                if (bleedField != null)
                                {
                                    int bleedThisHit = (int)bleedField.GetValue(mp);
                                    lastdamage += bleedThisHit; // 🩸 이번 피격에서 출혈로 전환된 피해량 추가
                                }
                                break;
                            }
                        }
                    }
                }
                catch
                {
                    // Calamity가 없거나 구조 변경 시 조용히 무시
                }
                float damageRatio = MathHelper.Clamp(
    lastdamage / (float)Player.statLifeMax2,
    0f,
    1f
);
            // 받은 피해가 최대 체력 대비 몇 %인지다

            float dustScale = MathHelper.Lerp(0.8f, 2.0f, damageRatio);
            // 데미지가 클수록 더 크게 튄다

            Vector2 hitDir = Player.Center - proj.Center;
            // 공격자가 있는 쪽 → 맞은 방향이다

            if (hitDir.LengthSquared() < 0.0001f)
                hitDir = new Vector2(hurtInfo.HitDirection, 0f);
            // 예외 상황 대비용 보정이다
            int bloodcount = lastdamage / 2;
            hitDir.Normalize();

            for (int i = 0; i < 25; i++)
            {
                Vector2 vel =
                    hitDir * Main.rand.NextFloat(2.5f, 5.0f) +
                    Main.rand.NextVector2Circular(0.6f, 0.6f);
                // 맞은 방향을 중심으로 퍼지게 만든다

                int d = Dust.NewDust(
                    Player.Center,
                    0,
                    0,
                    DustID.Blood,
                    vel.X,
                    vel.Y,
                    100,
                    default,
                    dustScale
                );

                Main.dust[d].noGravity = false;
                }


                int gorecount = lastdamage / 25;
                if (gorecount > 50) {
                    gorecount = 50;
                }

                for (int a = 0; a < gorecount; a++)
            {
                SpawnHitGoreFromSource(proj.Center, hurtInfo);
            }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!bloodMageEquipped || target.friendly || damageDone <= 0)
                return;

            if (hit.DamageType != DamageClass.Magic)
                return;

            if (vampCooldown > 0)
                return;
            // 이미 쿨타임이면 흡혈 자체를 허용하지 않는다

            int healPotential = damageDone / 10;
            if (healPotential <= 0)
                return;

            int missingLife = Player.statLifeMax2 - Player.statLife;
            if (missingLife <= 0)
                return;

            // 실질적으로 회복 가능한 양만 사용한다
            int effectiveHeal = Math.Min(healPotential, missingLife);

            // ===== 여기서 실질 회복량 기준으로 쿨타임 확정 =====
            int cooldown = effectiveHeal * 60;
            vampCooldown = cooldown;
            vampCooldownMax = cooldown;


            // 구체가 날아가기 전에 이미 쿨타임이 예약된다
            // ===== 그로테스크한 흡혈 Dust 연출 =====
            float healRatio = MathHelper.Clamp(
                (float)effectiveHeal / Player.statLifeMax2,
                0f,
                1f
            );
            // 실제 회복량이 최대 체력의 몇 %인지다

            float scale = MathHelper.Lerp(0.5f, 1.75f, healRatio);
            // 회복량이 클수록 더 크게 튄다

            Vector2 origin = new Vector2(
     Main.rand.NextFloat(target.position.X, target.position.X + target.width),
     Main.rand.NextFloat(target.position.Y, target.position.Y + target.height)
 );
           

            for (int i = 0; i < 35; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(1f, 1f) * scale;

                int d = Dust.NewDust(
                      origin,
                         0,
                      0,
                    DustID.Blood,
                    vel.X,
                    vel.Y,
                    80,
                    default,
                    scale
                );
                // === 생존시간 연장 핵심 ===

                Main.dust[d].noGravity = true;


                // === 생존시간 연장 핵심 ===
                Main.dust[d].fadeIn = 5.0f + healRatio * 2.5f;
                // 페이드인 시간을 늘려 소멸을 최대한 늦춘다

                Main.dust[d].scale *= 1.2f;
            }


            SoundStyle style = sounds[Main.rand.Next(sounds.Length)];
            style.Volume = 1f;


            SoundEngine.PlaySound(style, origin);



            Projectile.NewProjectile(
                Player.GetSource_OnHit(target),
                origin,
                Vector2.Zero,
                ModContent.ProjectileType<BloodHealOrb>(),
                0,
                0f,
                Player.whoAmI,
                Player.whoAmI,
                healPotential
            );
            // 적 위치에서 흡혈 구체를 생성한다



            

        }




        private int CalculateVampCooldown(int healPotential)
        {
            float at = 1f;
            float regen = cachedRegenPerSecond;

            float framesPerHeal = 60f / (5f + regen * at);
            int cooldown = (int)(healPotential * framesPerHeal);

            if (cooldown < 1)
                cooldown = 1;

            return cooldown;
        }
        // 예상 회복량 기준으로 쿨타임을 산출한다

    }
}
