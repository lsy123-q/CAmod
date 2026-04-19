using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using CAmod.Systems;
using CAmod.NPCs; // Sensor NPC가 위치한 네임스페이스
using System.Reflection;

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
        private static FieldInfo _chaliceBufferField;
        // 적응 관련 스택
        public Dictionary<int, int> adaptStack = new(); // Key: NPC ID 또는 -(Proj ID + 10000)
        public int debuffAdaptStack = 0;                // 디버프 대응 스택
        public int voidAdaptStack = 0;                  // 위협 없을 때 쌓이는 기본 스택

        // 위협 기록용 (Sensor NPC와 OnHurt에서 채워짐)
        public Dictionary<int, int> hitCount = new();
        public Dictionary<int, int> totalDamage = new();
        public float dotAccumulatedDamage = 0;
        private static Type _calPlayerType;
        public static readonly Color[] ExoPalette = new Color[]
        {
            new Color(250, 255, 112), new Color(211, 235, 108),
            new Color(166, 240, 105), new Color(105, 240, 220),
            new Color(64, 130, 145), new Color(145, 96, 145),
            new Color(242, 112, 73), new Color(199, 62, 62),
        };

        public override void ResetEffects()
        {
            harmonyEquipped = false;
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
                            if (Player.buffTime[i] > 1) Player.buffTime[i]--;
                        }
                    }
                }
            }
        }

        public override void PostUpdateEquips()
        {
            if (harmonyEquipped && structureActive)
            {
                // 방어력 무력화 및 공격력 보너스
                Player.statDefense *= 0;
                float totalStack = voidAdaptStack;
                // 필요 시 adaptStack.Values.Sum() 등을 추가하여 화력을 조절하세요.
                Player.GetDamage(DamageClass.Magic) += 0.01f * totalStack;
            }
        }

        public override void PostUpdate()
        {
            // 1. 장신구 미착용 시 강제 비활성화
            if (!harmonyEquipped)
            {
                if (structureActive) DeactivateStructure();
                return;
            }

            // 2. 토글 버튼 처리
            if (KeySystem.HarmonyCycleToggle.JustPressed)
            {
                if (!structureActive)
                {
                    if (HasExoPrism())
                    {
                        ActivateStructure();
                    }
                }
                else
                {
                    DeactivateStructure();
                }
                return;
            }

            if (!structureActive) return;

            // 3. 센서 NPC 관리 (서버/싱글 동일성 유지)
            ManageSensorNPC();

            // 4. DoT 데미지 누적 계산
            if (Player.lifeRegen < 0)
            {
                dotAccumulatedDamage += (-Player.lifeRegen / 2f) / 60f;
            }

            // 5. 평가 타이머 (600프레임 = 10초)
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
            SoundEngine.PlaySound(SoundID.Item4);

            if (Main.myPlayer == Player.whoAmI)
            {
                int type = ModContent.NPCType<AdaptationSensorNPC>();
                int idx = NPC.NewNPC(Player.GetSource_FromThis(), (int)Player.Center.X, (int)Player.Center.Y, type, ai0: Player.whoAmI);
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
            if (Main.myPlayer != Player.whoAmI) return;

            int sensorType = ModContent.NPCType<AdaptationSensorNPC>();
            bool npcInvalid = sensorNPCIndex == -1 || !Main.npc[sensorNPCIndex].active || Main.npc[sensorNPCIndex].type != sensorType;

            if (npcInvalid)
            {
                sensorNPCIndex = NPC.NewNPC(Player.GetSource_FromThis(), (int)Player.Center.X, (int)Player.Center.Y, sensorType, ai0: Player.whoAmI);
            }
        }

        private void EvaluateAdaptation()
        {
            if (!ConsumeExoPrism()) { DeactivateStructure(); return; }

            
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
                    if (Main.rand.NextFloat() < bonusChance) adaptStack[key]++;
                }
            }

            // 신규 위협 등록 (Sensor NPC가 수집한 totalDamage 기반)
            if (dotAccumulatedDamage > 0 && debuffAdaptStack == 0)
            {
                debuffAdaptStack = 1;
                isAnyNewTargetRegistered = true;
            }
            if (debuffAdaptStack > 0) debuffAdaptStack++;

            foreach (var entry in totalDamage)
            {
                if (entry.Value > 0 && !adaptStack.ContainsKey(entry.Key))
                {
                    adaptStack[entry.Key] = 1;
                    isAnyNewTargetRegistered = true;
                }
            }

            // 공허 적응
            if (!isAnyNewTargetRegistered) voidAdaptStack++;

            ClearHitRecords();
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            if (!harmonyEquipped || !structureActive) return;

            int sourceID = GetSourceID(info.DamageSource);
            if (sourceID == -1) return;

            if (!hitCount.ContainsKey(sourceID)) hitCount[sourceID] = 0;
            if (!totalDamage.ContainsKey(sourceID)) totalDamage[sourceID] = 0;

            hitCount[sourceID]++;
            totalDamage[sourceID] += info.Damage;
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (!harmonyEquipped || !structureActive) return;

            int sourceID = GetSourceID(modifiers.DamageSource);
            if (sourceID != -1 && adaptStack.TryGetValue(sourceID, out int stack))
            {
                float originalDamage = modifiers.FinalDamage.Flat;
                float reductionRate = 1f - (float)Math.Pow(0.95f, stack);
                float reductionAmount = originalDamage * reductionRate;

                modifiers.FinalDamage *= (1f - reductionRate);

                float bufferSubtrahend = reductionAmount - 5f;

                if (bufferSubtrahend > 0)
                {
                    try
                    {
                        // 1. 타입을 문자열로 찾습니다. (직접 참조 피하기)
                        if (_calPlayerType == null)
                        {
                            // tModLoader 환경에서 모드 클래스 타입을 가져오는 방식
                            if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
                            {
                                _calPlayerType = calamity.Code.GetType("CalamityMod.CalPlayer.CalamityPlayer");
                            }
                        }

                        if (_calPlayerType != null)
                        {
                            // 2. 필드 정보 캐싱
                            if (_chaliceBufferField == null)
                            {
                                _chaliceBufferField = _calPlayerType.GetField("chaliceBleedoutBuffer", BindingFlags.Instance | BindingFlags.Public);
                            }

                            // 3. Player.modPlayers에서 CalamityPlayer 인스턴스 찾기
                            // GetModPlayer<T> 대신 리플렉션이나 다른 방식으로 인스턴스를 가져와야 함
                            foreach (var mp in Player.ModPlayers)
                            {
                                if (mp.GetType() == _calPlayerType)
                                {
                                    double currentBuffer = (double)_chaliceBufferField.GetValue(mp);
                                    double newBuffer = Math.Max(0, currentBuffer - (double)bufferSubtrahend);
                                    _chaliceBufferField.SetValue(mp, newBuffer);
                                    break;
                                }
                            }
                        }
                    }
                    catch
                    {
                        // 오류 발생 시 무시 (CalamityMod가 없는 경우 등)
                    }
                }
            }
        }

        private int GetSourceID(PlayerDeathReason source)
        {
            if (source.SourceNPCIndex >= 0) return Main.npc[source.SourceNPCIndex].type;
            if (source.SourceProjectileType >= 0) return -(source.SourceProjectileType + 10000);
            return -1;
        }

        private void DeactivateStructure()
        {
            structureActive = false;
            if (sensorNPCIndex != -1 && Main.npc[sensorNPCIndex].type == ModContent.NPCType<AdaptationSensorNPC>())
            {
                Main.npc[sensorNPCIndex].active = false;
            }
            if (exoProjIndex != -1 && Main.projectile[exoProjIndex].active)
            {
                Main.projectile[exoProjIndex].Kill();
            }
            exoProjIndex = -1;
            sensorNPCIndex = -1;
            evaluationTimer = 0;
            adaptStack.Clear();
            debuffAdaptStack = 0;
            voidAdaptStack = 0;
            ClearHitRecords();
        }

        private void ClearHitRecords()
        {
            hitCount.Clear();
            totalDamage.Clear();
            dotAccumulatedDamage = 0;
        }

        // --- 유틸리티 메서드 ---
        private bool HasExoPrism() => Player.inventory.Any(item => item?.ModItem?.FullName.Contains("ExoPrism") ?? false);

        private bool ConsumeExoPrism()
        {
            foreach (var item in Player.inventory)
            {
                if (item?.ModItem?.FullName.Contains("ExoPrism") ?? false)
                {
                    item.stack--;
                    if (item.stack <= 0) item.TurnToAir();
                    return true;
                }
            }
            return false;
        }

        private void PerformHealing()
        {
            int lostLife = Player.statLifeMax2 - Player.statLife;
            float healRatio = 0.25f + (0.005f * voidAdaptStack);
            int heal = (int)(lostLife * Math.Min(healRatio, 1f));

            if (heal > 0)
            {
                Player.statLife += heal;
                CombatText.NewText(Player.Hitbox, Color.Aquamarine, heal);
            }
        }

        private void SpawnAstroidDust()

        {

            Vector2 tipPosition = Player.Center + new Vector2(0f, -30f);

            SoundEngine.PlaySound(SoundID.Item158 with { Volume = 1.6f }, tipPosition);

            for (int i = 0; i < 75; i++)

            {

                float angle = MathHelper.TwoPi * i / 75f;

                float x = (float)Math.Pow(Math.Cos(angle), 3) * 5f;

                float y = (float)Math.Pow(Math.Sin(angle), 3) * 5f;

                Dust d = Dust.NewDustPerfect(tipPosition, DustID.RainbowMk2, new Vector2(x, y));

                d.scale = 1.8f;

                d.color = MulticolorLerp(i / 75f, ExoPalette);

                d.noGravity = true;

            }

        }

        public static Color MulticolorLerp(float increment, params Color[] colors)
        {
            increment %= 0.999f;
            int idx = (int)(increment * colors.Length);
            return Color.Lerp(colors[idx], colors[(idx + 1) % colors.Length], (increment * colors.Length) % 1f);
        }

        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) => DeactivateStructure();
    }
}