using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CAmod.Systems;
using CAmod.Projectiles;
using Terraria.ID;
using CAmod.Buffs;
using System;
using Terraria.Audio;
using System.Reflection;

namespace CAmod.Players
{
    public class LeafWardPlayer : ModPlayer
    {
        public bool leafShieldEquipped;
        public float leafShieldMaxRadius;
        public int leafShieldTimer;
        public int leafShieldTimer2;
        public bool leafShieldActive;
        public int leafShieldCooldown;
        public int leafShieldCooldownmax; 
        public bool leafflag = false;
        public bool leafflag2 = false;
        public bool leafShieldVisible;
        public override void ResetEffects()
        {
            leafShieldEquipped = false;
            leafShieldActive = false;
            leafShieldVisible = false;


        }
        public override void OnRespawn()
        {
            leafflag = false;
            leafflag2 = false;

            leafShieldTimer = 0;
            leafShieldTimer2 = 0;

            leafShieldCooldown = 0;
            leafShieldCooldownmax = 0;

            // 사망 시 리프실드 활성 상태와 쿨을 초기화한다
        }

        public override void ProcessTriggers(Terraria.GameInput.TriggersSet triggersSet)
        {
            if (KeySystem.leafshield.JustPressed && leafShieldEquipped && leafShieldCooldown <= 0 && leafflag == false && leafflag2 == false)
            {
                SpawnLeaves();

                int count = Player.GetModPlayer<LeafWardPlayer2>().LeafShieldStage;
                int count2 = count * 15;

              leafflag = true;
                leafflag2 = true;
            }
        }
        public override void PostUpdate()
        {
            if (leafShieldEquipped) { 
            ApplyAbyssLayerLight_Reflection(Player);
                
            }

            int lostLife = Player.statLifeMax2 - Player.statLife;


            try
            {
                var modPlayersField =
                    typeof(Player).GetField("modPlayers", BindingFlags.Instance | BindingFlags.NonPublic);

                var modPlayersList =
                    modPlayersField?.GetValue(Player) as System.Collections.IEnumerable;

                if (modPlayersList != null)
                {
                    foreach (var mp in modPlayersList)
                    {
                        if (mp.GetType().FullName == "CalamityMod.CalPlayer.CalamityPlayer")
                        {
                            var bufferField =
                                mp.GetType().GetField("chaliceBleedoutBuffer",
                                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                            if (bufferField != null)
                            {
                                double buffer = (double)bufferField.GetValue(mp);

                                if (buffer > 0)
                                    lostLife += (int)buffer; // 현재 누적 출혈량을 손실 체력에 더한다
                            }

                            break;
                        }
                    }
                }
            }
            catch
            {
                // 구조 변경 대비 무시한다
            }


            float regenPerSecond = Player.lifeRegen / 2f;
            float regenDuringShield = regenPerSecond * 11f * 0.6f;

            if (leafShieldEquipped
                && lostLife >= regenDuringShield + 33f
                && leafShieldCooldown <= 0
                && !leafflag
                && !leafflag2
                && leafShieldVisible)
            {
                SpawnLeaves(); // 리프를 생성한다

                leafflag = true;
                leafflag2 = true;
            }

            int count = Player.GetModPlayer<LeafWardPlayer2>().LeafShieldStage;
            int count2 = count * 15;
            if (leafflag == true) {
                leafShieldTimer++;
              

            }
            if (leafflag2 == true)
            {
                leafShieldTimer2++;


            }


            if (leafflag)
                ApplyLeafShieldAura();

            if (leafflag)
            {
                ApplyLeafShieldLight();
            }
            if (leafShieldTimer2 > 660) {

                leafShieldCooldown = 60 * 20 - count2;
                leafShieldCooldownmax = 60 * 20 - count2;

                leafflag2 = false;

                leafShieldTimer2 = 0;


            }
            if (leafShieldTimer > 600) {
                leafShieldTimer = 0;
                leafflag = false;
            }


            if (leafflag == false && leafShieldEquipped && leafShieldCooldown > 0) {
                leafShieldCooldown--;

               

            }
           


            /*
            if (leafShieldCooldown > 0 && leafShieldCooldown <= 1200 - count2)
            {
                Player.AddBuff(
                    ModContent.BuffType<Buffs.LeafShieldCooltime>(),
                    leafShieldCooldown
                );
            }
            else
            {
                Player.ClearBuff(ModContent.BuffType<LeafShieldCooltime>());
            }*/
          

        }


        private static object GetCalamityPlayerSafe(Player player, out Type calPlayerType)
        {
            calPlayerType = null;

            try
            {
                if (!ModLoader.TryGetMod("CalamityMod", out Mod calamity))
                    return null;

                calPlayerType = calamity.Code.GetType("CalamityMod.CalPlayer.CalamityPlayer");
                if (calPlayerType == null)
                    return null;

                MethodInfo getModPlayer = null;

                foreach (MethodInfo m in typeof(Player).GetMethods(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (m.Name != "GetModPlayer")
                        continue;

                    if (!m.IsGenericMethodDefinition)
                        continue;

                    if (m.GetParameters().Length != 0)
                        continue;

                    getModPlayer = m;
                    break;
                }

                if (getModPlayer == null)
                    return null;

                MethodInfo generic = getModPlayer.MakeGenericMethod(calPlayerType);
                return generic.Invoke(player, null);
            }
            catch
            {
                return null;
                // 칼라미티 플레이어를 가져오지 못하면 실패 처리한다
            }
        }

        private int GetAbyssLayerByReflection(Player player)
        {
            try
            {
                object calPlayer = GetCalamityPlayerSafe(player, out Type calType);
                if (calPlayer == null || calType == null)
                    return -1;

                bool HasLayer(string name)
                {
                    PropertyInfo p = calType.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
                    return p != null && (bool)p.GetValue(calPlayer);
                }

                if (HasLayer("ZoneAbyssLayer4")) return 4;
                if (HasLayer("ZoneAbyssLayer3")) return 3;
                if (HasLayer("ZoneAbyssLayer2")) return 2;
                if (HasLayer("ZoneAbyssLayer1")) return 1;

                return -1;
                // 심연이 아니면 -1이다
            }
            catch
            {
                return -1;
                // 리플렉션 실패 시 심연이 아니라고 처리한다
            }
        }



        private void SetAbyssLightLevelReflection(Player player, int level)
        {
            try
            {
                Mod calamity = ModLoader.GetMod("CalamityMod");
                if (calamity == null)
                    return;

                Type calPlayerType = calamity.Code.GetType("CalamityMod.CalPlayer.CalamityPlayer");
                if (calPlayerType == null)
                    return;

                MethodInfo getModPlayerMethod = typeof(Player).GetMethod(
                    "GetModPlayer",
                    BindingFlags.Instance | BindingFlags.Public,
                    null,
                    new Type[] { typeof(Type) },
                    null
                );

                if (getModPlayerMethod == null)
                    return;

                ModPlayer calPlayer = (ModPlayer)getModPlayerMethod.Invoke(
                    player,
                    new object[] { calPlayerType }
                );

                if (calPlayer == null)
                    return;

                FieldInfo field = calPlayerType.GetField(
                    "externalAbyssLight",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                );

                if (field == null)
                    return;

                field.SetValue(calPlayer, level);
                // 심연 기본 밝기 레벨을 설정한다
            }
            catch
            {
                // 실패 시 아무 동작도 하지 않는다
            }
        }


        public override void PostUpdateMiscEffects()
        {
            if (!leafShieldEquipped)
                return;

            SetAbyssLightLevelReflection(Player, 3);
            // 리틀 라이트와 동일한 심연 기본 밝기 3을 부여한다
        }


        private void ApplyAbyssLayerLight_Reflection(Player player)
        {
         

            int layer = GetAbyssLayerByReflection(player);


            switch (layer)
            {
                case -1:
                    Lighting.AddLight(player.Center, 0.1f, 0.2f, 0.1f);
                    break;

                case 1:
                    Lighting.AddLight(player.Center, 1.25f, 1.25f, 1.25f);
                    break;

                case 2:
                    Lighting.AddLight(player.Center, 1.5f, 1.5f, 1.5f);
                    break;

                case 3:
                    Lighting.AddLight(player.Center, 2.0f, 2.0f, 2.0f);
                    break;

                case 4:
                    Lighting.AddLight(player.Center, 2.5f, 2.5f, 2.5f);
                    break;
            }
        }
        private void ApplyLeafShieldLight()
        {
            float intensity = 1f;
            // 기본 광원 강도다

            if (leafShieldTimer <= 60)
            {
                float t = leafShieldTimer / 60f;
                // 0.0 → 1.0 으로 선형 증가한다

                intensity *= MathHelper.Clamp(t, 0f, 1f);
            }

            if (leafShieldTimer >= 540)
            {
                float t = (600f - leafShieldTimer) / 60f;
                // 1.0 → 0.0 으로 선형 감소한다

                intensity = MathHelper.Clamp(t, 0f, 1f);
            }

            Lighting.AddLight(
                Player.Center,
                0.6f * intensity,
                1.2f * intensity,
                0.6f * intensity
            );
            // 연두색 계열 광원을 플레이어 중심에 부여한다
        }

        private float GetLeafShieldRadius()
        {
            float a = 300f;
            // 초기 반경이다

            // === 1~3초 구간 (60~180틱): 2초에 걸쳐 1 → 2배 선형 증가 ===
            if (leafShieldTimer >= 60 && leafShieldTimer <= 240)
            {
                float t = (leafShieldTimer - 60f) / 180f;
                // 0.0 ~ 1.0 으로 정규화한다

                a *= MathHelper.Lerp(1f, 2f, t);
                // 2초 동안 선형으로 2배까지 증가한다
            }
            else if (leafShieldTimer > 240)
            {
                a *= 2f;
                // 해당 구간을 넘겼다면 2배 상태를 유지한다
            }

            // === 9~10초 구간 (540~600틱): 1초에 걸쳐 추가 1 → 1.5배 선형 증가 ===
            if (leafShieldTimer >= 540 && leafShieldTimer <= 600)
            {
                float t = (leafShieldTimer - 540f) / 60f;
                // 0.0 ~ 1.0 으로 정규화한다

                a *= MathHelper.Lerp(1f, 1.5f, t);
                // 1초 동안 선형으로 1.5배까지 추가 증가한다
            }
            else if (leafShieldTimer > 600)
            {
                a *= 1.5f;
                // 해당 구간을 넘겼다면 1.5배 상태를 유지한다
            }

            return a;
        }


        private void ApplyLeafShieldAura()
        {
            if (!Player.active || Player.dead)
                return;
            float radius = GetLeafShieldRadius();
            float radiusSq = radius * radius;
            Vector2 center = Player.Center;
            
            // === NPC 대상 처리 ===
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];

                if (!npc.active)
                    continue;

                if (Vector2.DistanceSquared(center, npc.Center) > radiusSq)
                    continue;

                if (npc.damage > 0 && !npc.friendly && !npc.townNPC)
                {
                    npc.AddBuff(BuffID.DryadsWardDebuff, 60);
                    // 공격 능력이 있는 적에게만 디버프를 건다
                }
                // 🟢 아군 판정
                else if (npc.friendly || npc.townNPC)
                {
                    npc.AddBuff(BuffID.DryadsWard, 60);
                    // 아군 NPC에게 버프를 건다
                }
            }

            // === 플레이어(아군) 처리 ===
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player other = Main.player[i];

                if (!other.active || other.dead)
                    continue;

                if (Vector2.DistanceSquared(center, other.Center) > radiusSq)
                    continue;

                other.AddBuff(BuffID.DryadsWard, 60);
                // 아군 플레이어에게 드라이어드의 축복을 부여한다
            }
            SpawnLeafShieldDust(GetLeafShieldRadius());
        }
        private void SpawnLeafShieldDust(float radius)
        {
            // 연출 밀도 제어용 확률이다
            if (!Player.active || Player.dead)
                return;

            for (int i =0; i<radius/100; i++) {

                if (Main.rand.NextBool(10) == false)
                    continue;
            // 매틱마다 1/5 확률로만 생성한다


            // 반경 a 지점에서 생성한다

            float scaleFade = Main.rand.NextFloat(0.8f, 1.2f);
            // 기본 랜덤 크기다
           


                if (leafShieldTimer >= 540)
            {
                float t = (600f - leafShieldTimer) / 60f;
                // 1.0 → 0.0 으로 정규화한다

                scaleFade *= MathHelper.Lerp(0f, 1f, t);
                // 종료 1초 전부터 선형으로 추가 감쇠한다
            }






            // 플레이어 방향으로 빨려들어가는 속도다


            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                // 원 둘레의 랜덤한 각도다

                Vector2 spawnPos = Player.Center + angle.ToRotationVector2() * radius;
                Vector2 toPlayer = Player.Center - spawnPos;

                Vector2 velocity = toPlayer.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(10f, 14f);
                ;


                int d = Dust.NewDust(
                spawnPos,
                0,
                0,
                 DustID.DryadsWard,
                velocity.X,
                velocity.Y,
                0,
                new Color(150, 255, 150),
               scaleFade
            );

            Main.dust[d].noGravity = true;
                // 중력 영향을 받지 않게 한다

            }

        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {

            if (!leafShieldEquipped || target.friendly || damageDone <= 0)
                return;

            if (hit.DamageType != DamageClass.Magic)
                return;



            


            target.AddBuff(BuffID.DryadsWardDebuff, 60 * 3);


        }

        private void SpawnLeaves()
        {
            if (!Player.active || Player.dead)
                return;
            int owner = Player.whoAmI;

            // === 반경 100px / 시계방향 ===
            for (int i = 0; i < 10; i++)
            {
                int p = Projectile.NewProjectile(
                    Player.GetSource_FromThis(),
                    Player.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<LeafBlowerOrbit>(),
                    0,
                    0f,
                    owner,
                    100f,
                    1f
                );

                Main.projectile[p].localAI[0] = MathHelper.TwoPi / 10f * i;
                // 360도를 10등분한 고정 시작 각도를 부여한다
            }


            // === 반경 300px / 반시계방향 ===
            for (int i = 0; i < 10; i++)
            {
                int p = Projectile.NewProjectile(
                    Player.GetSource_FromThis(),
                    Player.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<LeafBlowerOrbit>(),
                    0,
                    0f,
                    owner,
                    300f,
                    -1f
                );

                Main.projectile[p].localAI[0] = MathHelper.TwoPi / 10f * i;
                // 360도를 10등분한 고정 시작 각도를 부여한다
            }

            // 반경 300, 반시계방향으로 회전한다
        }
    }
    
}
