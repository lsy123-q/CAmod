using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using CAmod.Systems;
using CAmod.NPCs;

namespace CAmod.Players
{
    public class HarmonyCyclePlayer : ModPlayer
    {
        private int exoProjIndex = -1;

        // =========================
        // 상태 및 설정
        // =========================
        public bool harmonyEquipped = false;
        public bool structureActive = false;
        public int evaluationTimer = 0;
        public int sensorNPCIndex = -1;

        // 적응 관련 스택
        public Dictionary<int, int> adaptStack = new();
        public int debuffAdaptStack = 0;
        public int voidAdaptStack = 0;

        // 위협 기록용
        public Dictionary<int, int> hitCount = new();
        public Dictionary<int, int> totalDamage = new();
        public float dotAccumulatedDamage = 0;

        // 마지막으로 타격이 감지된 시점부터 경과한 시간
        public int noHitTimer = 0;

        // 보너스 최대 체력
        public int bonusMaxLife = 0;


        public static readonly Color[] ExoPalette = new Color[]
        {
            new Color(250, 255, 112),
            new Color(211, 235, 108),
            new Color(166, 240, 105),
            new Color(105, 240, 220),
            new Color(64, 130, 145),
            new Color(145, 96, 145),
            new Color(242, 112, 73),
            new Color(199, 62, 62),
        };

        public override void ResetEffects()
        {
            harmonyEquipped = false;

            // 구조체가 비활성화되면 보너스 최대 체력을 초기화한다
            if (!structureActive)
            {
                bonusMaxLife = 0;
            }
        }

        public override void PostUpdateBuffs()
        {
            if (harmonyEquipped && structureActive && debuffAdaptStack > 0)
            {
                // 디버프 지속시간 감소 로직
                for (int i = 0; i < Player.MaxBuffs; i++)
                {
                    int buffType = Player.buffType[i];

                    if (buffType > 0 && Main.debuff[buffType])
                    {
                        if (Main.rand.Next(100) < debuffAdaptStack * 5)
                        {
                            if (Player.buffTime[i] > 1)
                                Player.buffTime[i]--;
                        }
                    }
                }
            }
        }

        public override void PostUpdateEquips()
        {
            if (harmonyEquipped && structureActive)
            {
                // 방어력을 0으로 만들고 적응 스택에 비례하여 마법 공격력을 증가시킨다
                Player.statDefense *= 0;

                float totalStack = voidAdaptStack;
                Player.GetDamage(DamageClass.Magic) += 0.01f * totalStack;

                // 누적된 보너스 최대 체력을 적용한다
                if (bonusMaxLife > 0)
                    Player.statLifeMax2 += bonusMaxLife;
            }
        }

        public override void PostUpdate()
        {
            // 1. 장신구 미착용 시 강제 비활성화
            if (!harmonyEquipped)
            {
                if (structureActive)
                    DeactivateStructure();

                return;
            }

            // 2. 토글 버튼 처리
            if (KeySystem.HarmonyCycleToggle.JustPressed)
            {
                if (!structureActive)
                {
                    ActivateStructure();
                }
                else
                {
                    DeactivateStructure();
                }

                return;
            }

            if (!structureActive)
                return;

            // 3. 센서 NPC 관리
            ManageSensorNPC();

            // 4. DoT 데미지 누적 계산
            if (Player.lifeRegen < 0)
            {
                dotAccumulatedDamage += (-Player.lifeRegen / 2f) / 60f;
            }

            // 5. 타격이 없는 시간 측정
            noHitTimer++;

            // 30초 동안 타격이 없으면 스택만 초기화한다
            if (noHitTimer >= 1800)
            {
                ResetAdaptationStacks();
                noHitTimer = 0;
            }

            // 6. 평가 타이머 (600프레임 = 10초)
            evaluationTimer++;

            if (evaluationTimer >= 600)
            {
                evaluationTimer = 0;
                EvaluateAdaptation();
            }
        }

        private void ActivateStructure()
        {
            structureActive = true;
            evaluationTimer = 0;
            noHitTimer = 0;

            SoundEngine.PlaySound(SoundID.Item4);

            if (Main.myPlayer == Player.whoAmI)
            {
                int type = ModContent.NPCType<AdaptationSensorNPC>();

                int idx = NPC.NewNPC(
                    Player.GetSource_FromThis(),
                    (int)Player.Center.X,
                    (int)Player.Center.Y,
                    type,
                    ai0: Player.whoAmI
                );

                sensorNPCIndex = idx;
            }

            if (Main.myPlayer == Player.whoAmI)
            {
                exoProjIndex = Projectile.NewProjectile(
                    Player.GetSource_FromThis(),
                    Player.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<CAMod.Projectiles.ExoProj>(),
                    0,
                    0f,
                    Player.whoAmI
                );
            }
        }

        private void ManageSensorNPC()
        {
            if (Main.myPlayer != Player.whoAmI)
                return;

            int sensorType = ModContent.NPCType<AdaptationSensorNPC>();

            bool npcInvalid =
                sensorNPCIndex == -1 ||
                !Main.npc[sensorNPCIndex].active ||
                Main.npc[sensorNPCIndex].type != sensorType;

            if (npcInvalid)
            {
                sensorNPCIndex = NPC.NewNPC(
                    Player.GetSource_FromThis(),
                    (int)Player.Center.X,
                    (int)Player.Center.Y,
                    sensorType,
                    ai0: Player.whoAmI
                );
            }
        }

        private void EvaluateAdaptation()
        {
            PerformHealing();

            bool isAnyNewTargetRegistered = false;

            // 기존 대상 성장 및 피격 가속
            var keys = adaptStack.Keys.ToList();

            foreach (var key in keys)
            {
                adaptStack[key]++;

                if (hitCount.TryGetValue(key, out int count) && count > 0)
                {
                    float bonusChance = 1f - (float)Math.Pow(0.5, count);

                    if (Main.rand.NextFloat() < bonusChance)
                        adaptStack[key]++;
                }
            }

            // 신규 디버프 위협 등록
            if (dotAccumulatedDamage > 0 && debuffAdaptStack == 0)
            {
                debuffAdaptStack = 1;
                isAnyNewTargetRegistered = true;
            }

            if (debuffAdaptStack > 0)
                debuffAdaptStack++;

            // 신규 위협 등록
            foreach (var entry in totalDamage)
            {
                if (entry.Value > 0 && !adaptStack.ContainsKey(entry.Key))
                {
                    adaptStack[entry.Key] = 1;
                    isAnyNewTargetRegistered = true;
                }
            }

            // 공허 적응
            if (!isAnyNewTargetRegistered)
                voidAdaptStack++;

            ClearHitRecords();
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            if (!harmonyEquipped || !structureActive)
                return;

            int sourceID = GetSourceID(info.DamageSource);

            if (sourceID == -1)
                return;

            if (!hitCount.ContainsKey(sourceID))
                hitCount[sourceID] = 0;

            if (!totalDamage.ContainsKey(sourceID))
                totalDamage[sourceID] = 0;

            hitCount[sourceID]++;
            totalDamage[sourceID] += info.Damage;

            // 타격이 감지되었으므로 무피격 타이머를 초기화한다
            noHitTimer = 0;
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (!harmonyEquipped || !structureActive)
                return;

            int sourceID = GetSourceID(modifiers.DamageSource);

            if (sourceID != -1 && adaptStack.TryGetValue(sourceID, out int stack))
            {
                float originalDamage = modifiers.FinalDamage.Flat;

                float reductionRate =
                    1f - (float)Math.Pow(0.9f, stack);

                float reductionAmount =
                    originalDamage * reductionRate;

                modifiers.FinalDamage *= 1f - reductionRate;

                float bufferSubtrahend =
                    reductionAmount - 5f;

                if (bufferSubtrahend > 0)
                {
                    var calPlayer = Player.GetModPlayer<global::CalamityMod.CalPlayer.CalamityPlayer>();
                    calPlayer.chaliceBleedoutBuffer = Math.Max(
                        0D, calPlayer.chaliceBleedoutBuffer - bufferSubtrahend);
                }
            }
        }

        private int GetSourceID(PlayerDeathReason source)
        {
            if (source.SourceNPCIndex >= 0)
                return Main.npc[source.SourceNPCIndex].type;

            if (source.SourceProjectileType >= 0)
                return -(source.SourceProjectileType + 10000);

            return -1;
        }

        private void DeactivateStructure()
        {
            structureActive = false;
            bonusMaxLife = 0;

            if (sensorNPCIndex != -1 &&
                Main.npc[sensorNPCIndex].type ==
                ModContent.NPCType<AdaptationSensorNPC>())
            {
                Main.npc[sensorNPCIndex].active = false;
            }

            if (exoProjIndex != -1 &&
                Main.projectile[exoProjIndex].active)
            {
                Main.projectile[exoProjIndex].Kill();
            }

            exoProjIndex = -1;
            sensorNPCIndex = -1;
            evaluationTimer = 0;
            noHitTimer = 0;

            adaptStack.Clear();
            debuffAdaptStack = 0;
            voidAdaptStack = 0;

            ClearHitRecords();
        }

        private void ResetAdaptationStacks()
        {
            adaptStack.Clear();
            debuffAdaptStack = 0;
            voidAdaptStack = 0;
        }

        private void ClearHitRecords()
        {
            hitCount.Clear();
            totalDamage.Clear();
            dotAccumulatedDamage = 0;
        }

        private void PerformHealing()
        {
            int effectiveMaxLife = Player.statLifeMax2 + bonusMaxLife;
            int lostLife = effectiveMaxLife - Player.statLife;

            float healRatio =
                0.25f + (0.005f * voidAdaptStack);

            int heal =
                (int)(lostLife * Math.Min(healRatio, 1f));

            if (heal > 0)
            {
                // 체력을 회복한다
                Player.statLife += heal;

                // 회복량의 10%만큼 최대 체력을 증가시킨다
                int maxLifeIncrease =
                    (int)(heal * 0.1f);

                if (maxLifeIncrease > 0)
                {
                    // 보너스 최대 체력을 누적한다
                    bonusMaxLife += maxLifeIncrease;
                }

                CombatText.NewText(
                    Player.Hitbox,
                    Color.Aquamarine,
                    heal
                );
            }
        }

        private void SpawnAstroidDust()
        {
            Vector2 tipPosition =
                Player.Center + new Vector2(0f, -30f);

            SoundEngine.PlaySound(
                SoundID.Item158 with { Volume = 1.6f },
                tipPosition
            );

            for (int i = 0; i < 75; i++)
            {
                float angle =
                    MathHelper.TwoPi * i / 75f;

                float x =
                    (float)Math.Pow(Math.Cos(angle), 3) * 5f;

                float y =
                    (float)Math.Pow(Math.Sin(angle), 3) * 5f;

                Dust d =
                    Dust.NewDustPerfect(
                        tipPosition,
                        DustID.RainbowMk2,
                        new Vector2(x, y)
                    );

                d.scale = 1.8f;
                d.color =
                    MulticolorLerp(i / 75f, ExoPalette);
                d.noGravity = true;
            }
        }

        public static Color MulticolorLerp(
            float increment,
            params Color[] colors)
        {
            increment %= 0.999f;

            int idx =
                (int)(increment * colors.Length);

            return Color.Lerp(
                colors[idx],
                colors[(idx + 1) % colors.Length],
                (increment * colors.Length) % 1f
            );
        }

        public override void Kill(
            double damage,
            int hitDirection,
            bool pvp,
            PlayerDeathReason damageSource)
        {
            DeactivateStructure();
        }
    }
}