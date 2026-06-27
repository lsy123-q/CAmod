using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CAmod.Systems;
using CAmod.Projectiles;
using Terraria.ID;
using System;
using System.Reflection;

namespace CAmod.Players
{
    public class LeafWardPlayer : ModPlayer
    {
        public bool leafShieldEquipped;
        public int leafShieldTimer;
        public bool leafShieldActive;
        public int leafShieldCooldown;
        public int leafShieldCooldownmax;
        public bool leafShieldVisible;

        private int masterProjectileIndex = -1;

        public override void ResetEffects()
        {
            leafShieldEquipped = false;
            leafShieldVisible = false;
        }

        public override void OnRespawn()
        {
            leafShieldActive = false;
            leafShieldTimer = 0;
            leafShieldCooldown = 0;
            leafShieldCooldownmax = 0;
            masterProjectileIndex = -1;
        }

        private bool IsMaxLevel()
        {
            return LeafWardSystem.GetStage() >= 40;
        }

        private int GetMaxCooldownTicks()
        {
            if (IsMaxLevel())
            {
                return 60; // 만렙 시 쿨타임 1초 (60틱) 고정
            }

            int stage = LeafWardSystem.GetStage();
            return 1200 - (stage * 15); // 20초 ~ 10.25초 사이 대역 부드럽게 감소
        }

        public override void ProcessTriggers(Terraria.GameInput.TriggersSet triggersSet)
        {
            if (!leafShieldEquipped) return;

            int maxCooldownTicks = GetMaxCooldownTicks();
            bool isMaxLevel = IsMaxLevel();

            if (KeySystem.leafshield.JustPressed)
            {
                if (isMaxLevel)
                {
                    if (leafShieldActive)
                    {
                        DeactivateInfiniteShield(maxCooldownTicks);
                    }
                    else if (leafShieldCooldown <= 0)
                    {
                        ActivateShield(maxCooldownTicks, true);
                    }
                }
                else
                {
                    if (leafShieldCooldown <= 0 && !leafShieldActive && !IsShieldProjectileValid())
                    {
                        ActivateShield(maxCooldownTicks, false);
                    }
                }
            }
        }

        private void ActivateShield(int maxCooldown, bool isMaxLevel)
        {
            leafShieldActive = true;
            leafShieldTimer = 0;

            int duration = 600;
            SpawnMasterProjectile(duration, isMaxLevel);
        }

        private void DeactivateInfiniteShield(int maxCooldown)
        {
            leafShieldActive = false;
            leafShieldTimer = 0;

            leafShieldCooldown = maxCooldown;
            leafShieldCooldownmax = maxCooldown;

            if (masterProjectileIndex != -1 && Main.projectile[masterProjectileIndex].active && Main.projectile[masterProjectileIndex].type == ModContent.ProjectileType<LeafBlowerOrbit>())
            {
                Main.projectile[masterProjectileIndex].ai[1] = 0f;
                Main.projectile[masterProjectileIndex].timeLeft = 60;
            }
            masterProjectileIndex = -1;
        }

        public override void PostUpdate()
        {
            if (leafShieldEquipped)
            {
                ApplyAbyssLayerLight_Reflection(Player);
            }

            int lostLife = Player.statLifeMax2 - Player.statLife;
            lostLife += GetCalamityBleedoutBuffer();

            float regenPerSecond = Player.lifeRegen / 2f;
            float regenDuringShield = regenPerSecond * 11f * 0.6f;

            int maxCooldownTicks = GetMaxCooldownTicks();
            bool isMaxLevel = IsMaxLevel();

            // 자동 발동 메커니즘
            if (leafShieldEquipped && lostLife >= (regenDuringShield + 33f) && leafShieldCooldown <= 0 && !leafShieldActive && leafShieldVisible)
            {
                if (!IsShieldProjectileValid())
                {
                    ActivateShield(maxCooldownTicks, isMaxLevel);
                }
            }

            // 쉴드가 활성화된 상태의 오라 및 로직 제어
            if (leafShieldActive)
            {
                leafShieldTimer++;

                // [버그 수정] 투사체 유효성 체크 방식을 변경합니다.
                // 일반 레벨에서는 투사체가 자연사하든, 강제로 지워지든 leafShieldTimer가 600을 넘어가거나 
                // 투사체가 사라진 시점을 감지해 안전하게 쿨타임을 먼저 먹이도록 구조를 개편했습니다.
                bool isProjValid = masterProjectileIndex != -1 && Main.projectile[masterProjectileIndex].active && Main.projectile[masterProjectileIndex].type == ModContent.ProjectileType<LeafBlowerOrbit>();

                if (isProjValid)
                {
                    Main.projectile[masterProjectileIndex].ai[0] = leafShieldTimer;
                }

                ApplyLeafShieldAura();
                ApplyLeafShieldLight();

                // 만렙이 아닐 때: 10초(600틱)가 지났거나, 투사체가 먼저 사라졌다면 만료 처리 진입
                if (!isMaxLevel && (leafShieldTimer > 600 || !isProjValid))
                {
                    leafShieldActive = false;
                    leafShieldTimer = 0;
                    leafShieldCooldown = maxCooldownTicks; // 드디어 정상적으로 계산된 쿨타임이 박힙니다!
                    leafShieldCooldownmax = maxCooldownTicks;

                    if (isProjValid)
                    {
                        Main.projectile[masterProjectileIndex].Kill();
                    }
                    masterProjectileIndex = -1;
                }
                // 만렙인데 의도치 않게 투사체가 터진 경우 예외 처리
                else if (isMaxLevel && !isProjValid && leafShieldTimer > 5)
                {
                    leafShieldActive = false;
                    leafShieldTimer = 0;
                    masterProjectileIndex = -1;
                }
            }
            else
            {
                if (IsShieldProjectileValid())
                {
                    ApplyLeafShieldAura();
                    ApplyLeafShieldLight();
                }

                // 쉴드가 꺼졌으니 쿨타임이 프레임당 1씩 정상적으로 감소합니다.
                if (leafShieldCooldown > 0)
                {
                    leafShieldCooldown--;
                }
            }
        }

        private bool IsShieldProjectileValid()
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.type == ModContent.ProjectileType<LeafBlowerOrbit>() && proj.owner == Player.whoAmI)
                {
                    return true;
                }
            }
            return false;
        }

        private void SpawnMasterProjectile(int duration, bool isMaxLevel)
        {
            if (!Player.active || Player.dead) return;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.type == ModContent.ProjectileType<LeafBlowerOrbit>() && proj.owner == Player.whoAmI)
                {
                    proj.Kill();
                }
            }

            masterProjectileIndex = Projectile.NewProjectile(
                Player.GetSource_FromThis(),
                Player.Center,
                Vector2.Zero,
                ModContent.ProjectileType<LeafBlowerOrbit>(),
                0,
                0f,
                Player.whoAmI,
                0f,
                isMaxLevel ? 1f : 0f
            );

            // 일반 레벨일 때 수명 10초 부여
            if (!isMaxLevel)
            {
                Main.projectile[masterProjectileIndex].timeLeft = duration;
            }
        }

        private float GetLeafShieldRadius()
        {
            float radius = 300f;
            Projectile shieldProj = null;
            if (masterProjectileIndex != -1 && Main.projectile[masterProjectileIndex].active)
                shieldProj = Main.projectile[masterProjectileIndex];
            else
            {
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    if (Main.projectile[i].active && Main.projectile[i].type == ModContent.ProjectileType<LeafBlowerOrbit>() && Main.projectile[i].owner == Player.whoAmI)
                    {
                        shieldProj = Main.projectile[i];
                        break;
                    }
                }
            }

            if (shieldProj != null)
            {
                int elapsed = (int)shieldProj.ai[0];
                if (elapsed >= 60 && elapsed <= 240)
                {
                    float t = (elapsed - 60f) / 180f;
                    radius *= MathHelper.Lerp(1f, 2f, t);
                }
                else if (elapsed > 240)
                {
                    radius *= 2f;
                }

                if (shieldProj.timeLeft <= 60)
                {
                    float t = 1f - (shieldProj.timeLeft / 60f);
                    radius *= MathHelper.Lerp(1f, 3.5f, t * t);
                }
            }
            return radius;
        }

        private void ApplyLeafShieldAura()
        {
            if (!Player.active || Player.dead) return;

            float radius = GetLeafShieldRadius();
            float radiusSq = radius * radius;
            Vector2 center = Player.Center;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || Vector2.DistanceSquared(center, npc.Center) > radiusSq) continue;

                if (npc.damage > 0 && !npc.friendly && !npc.townNPC)
                {
                    npc.AddBuff(BuffID.DryadsWardDebuff, 60);
                }
                else if (npc.friendly || npc.townNPC)
                {
                    npc.AddBuff(BuffID.DryadsWard, 60);
                }
            }

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player other = Main.player[i];
                if (!other.active || other.dead || Vector2.DistanceSquared(center, other.Center) > radiusSq) continue;

                other.AddBuff(BuffID.DryadsWard, 60);
            }

            SpawnLeafShieldDust(radius);
        }

        private void SpawnLeafShieldDust(float radius)
        {
            if (!Player.active || Player.dead) return;

            Projectile shieldProj = null;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].type == ModContent.ProjectileType<LeafBlowerOrbit>() && Main.projectile[i].owner == Player.whoAmI)
                {
                    shieldProj = Main.projectile[i];
                    break;
                }
            }

            for (int i = 0; i < radius / 100; i++)
            {
                if (!Main.rand.NextBool(10)) continue;

                float scaleFade = Main.rand.NextFloat(0.8f, 1.2f);

                if (shieldProj != null && shieldProj.timeLeft <= 60)
                {
                    float t = shieldProj.timeLeft / 60f;
                    scaleFade *= MathHelper.Lerp(0f, 1f, t);
                }

                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 spawnPos = Player.Center + angle.ToRotationVector2() * radius;
                Vector2 toPlayer = Player.Center - spawnPos;
                Vector2 velocity = toPlayer.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(10f, 14f);

                int d = Dust.NewDust(spawnPos, 0, 0, DustID.DryadsWard, velocity.X, velocity.Y, 0, new Color(150, 255, 150), scaleFade);
                Main.dust[d].noGravity = true;
            }
        }

        private void ApplyLeafShieldLight()
        {
            float intensity = 1f;
            if (leafShieldTimer <= 60)
            {
                intensity *= MathHelper.Clamp(leafShieldTimer / 60f, 0f, 1f);
            }

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.type == ModContent.ProjectileType<LeafBlowerOrbit>() && proj.owner == Player.whoAmI && proj.timeLeft <= 60)
                {
                    intensity = MathHelper.Clamp(proj.timeLeft / 60f, 0f, 1f);
                    break;
                }
            }

            Lighting.AddLight(Player.Center, 0.6f * intensity, 1.2f * intensity, 0.6f * intensity);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!leafShieldEquipped || target.friendly || damageDone <= 0 || hit.DamageType != DamageClass.Magic) return;
            target.AddBuff(BuffID.DryadsWardDebuff, 60 * 3);
        }

        private int GetCalamityBleedoutBuffer()
        {
            try
            {
                var modPlayersField = typeof(Player).GetField("modPlayers", BindingFlags.Instance | BindingFlags.NonPublic);
                var modPlayersList = modPlayersField?.GetValue(Player) as System.Collections.IEnumerable;

                if (modPlayersList != null)
                {
                    foreach (var mp in modPlayersList)
                    {
                        if (mp.GetType().FullName == "CalamityMod.CalPlayer.CalamityPlayer")
                        {
                            var bufferField = mp.GetType().GetField("chaliceBleedoutBuffer", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                            if (bufferField != null)
                            {
                                return (int)(double)bufferField.GetValue(mp);
                            }
                            break;
                        }
                    }
                }
            }
            catch { }
            return 0;
        }

        private static object GetCalamityPlayerSafe(Player player, out Type calPlayerType)
        {
            calPlayerType = null;
            try
            {
                if (!ModLoader.TryGetMod("CalamityMod", out Mod calamity)) return null;
                calPlayerType = calamity.Code.GetType("CalamityMod.CalPlayer.CalamityPlayer");
                if (calPlayerType == null) return null;

                foreach (MethodInfo m in typeof(Player).GetMethods(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (m.Name == "GetModPlayer" && m.IsGenericMethodDefinition && m.GetParameters().Length == 0)
                    {
                        return m.MakeGenericMethod(calPlayerType).Invoke(player, null);
                    }
                }
            }
            catch { }
            return null;
        }

        private int GetAbyssLayerByReflection(Player player)
        {
            try
            {
                object calPlayer = GetCalamityPlayerSafe(player, out Type calType);
                if (calPlayer == null || calType == null) return -1;

                bool HasLayer(string name)
                {
                    PropertyInfo p = calType.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
                    return p != null && (bool)p.GetValue(calPlayer);
                }

                if (HasLayer("ZoneAbyssLayer4")) return 4;
                if (HasLayer("ZoneAbyssLayer3")) return 3;
                if (HasLayer("ZoneAbyssLayer2")) return 2;
                if (HasLayer("ZoneAbyssLayer1")) return 1;
            }
            catch { }
            return -1;
        }

        private void SetAbyssLightLevelReflection(Player player, int level)
        {
            try
            {
                Mod calamity = ModLoader.GetMod("CalamityMod");
                if (calamity == null) return;

                Type calPlayerType = calamity.Code.GetType("CalamityMod.CalPlayer.CalamityPlayer");
                if (calPlayerType == null) return;

                MethodInfo getModPlayerMethod = typeof(Player).GetMethod("GetModPlayer", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(Type) }, null);
                if (getModPlayerMethod == null) return;

                ModPlayer calPlayer = (ModPlayer)getModPlayerMethod.Invoke(player, new object[] { calPlayerType });
                if (calPlayer == null) return;

                FieldInfo field = calPlayerType.GetField("externalAbyssLight", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field == null) return;

                field.SetValue(calPlayer, level);
            }
            catch { }
        }

        public override void PostUpdateMiscEffects()
        {
            if (!leafShieldEquipped) return;
            SetAbyssLightLevelReflection(Player, 3);
        }

        private void ApplyAbyssLayerLight_Reflection(Player player)
        {
            int layer = GetAbyssLayerByReflection(player);
            switch (layer)
            {
                case -1: Lighting.AddLight(player.Center, 0.1f, 0.2f, 0.1f); break;
                case 1: Lighting.AddLight(player.Center, 1.25f, 1.25f, 1.25f); break;
                case 2: Lighting.AddLight(player.Center, 1.5f, 1.5f, 1.5f); break;
                case 3: Lighting.AddLight(player.Center, 2.0f, 2.0f, 2.0f); break;
                case 4: Lighting.AddLight(player.Center, 2.5f, 2.5f, 2.5f); break;
            }
        }
    }
}