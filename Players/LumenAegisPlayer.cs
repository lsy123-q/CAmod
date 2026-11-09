using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ModLoader;
using CAmod.Projectiles;
using Terraria.ID;
using System;
using Terraria;
using CalamityMod.CalPlayer;
namespace CAmod.Players
{
    public class LumenAegisPlayer : ModPlayer
    {
        // =========================
        // 장착 및 실드 상태
        // =========================

        public bool lumenAegisEquipped;
        public bool lumenAegisActive;
        private bool lumenAegisFreeDodgePending;
        private const float NormalFresnelPower = 5f;

        public float lumenAegisShield;
        public float lumenAegisShieldMax;

        public int lumenAegisTimer;
        public int lumenAegisCooldown;
        public int lumenAegisCooldownMax;

        // 장착 후 발동 가능해질 때까지 남은 시간
        public int lumenAegisEquipTimer;

        // 실드가 공격을 막은 뒤 플레이어에게 주는 무적 시간
        public int lumenAegisImmuneTime;

        private bool lumenAegisWasEquipped;

        // =========================
        // 시각 효과 상태
        // =========================

        public int lumenAegisVisualHitTimer;
        private float lumenAegisVisualAlpha;
        private float lumenAegisVisualFresnelPower;

        // 보호막 사용 준비 페이드 진행 시간을 저장한다
        private int lumenAegisVisualReadyTimer;

        // 보호막 활성화 페이드 진행 시간을 저장한다
        private int lumenAegisVisualActivateTimer;

        // 보호막 종료 페이드 진행 시간을 저장한다
        private int lumenAegisVisualEndTimer;

        // 보호막 종료 페이드아웃 중인지 저장한다
        private bool lumenAegisVisualEnding;

        // 보호막 생성/소멸 스케일 애니메이션 진행 시간을 저장한다
        private int lumenAegisScaleTimer;

        // 보호막 생성 스케일 애니메이션 중인지 저장한다
        private bool lumenAegisScalingIn;

        // 보호막 소멸 스케일 애니메이션 중인지 저장한다
        private bool lumenAegisScalingOut;

        // 현재 보호막의 렌더링 스케일을 저장한다
        private float lumenAegisVisualScale = 1f;

        // 현재 생성된 보호막 투사체의 인덱스를 저장한다
        private int lumenAegisProjectileIndex = -1;

        // 현재 플레이어의 칼라미티 플레이어 데이터를 저장한다
        private CalamityPlayer calamityPlayer;

        // =========================
        // 상수
        // =========================

        private const int EquipDelay = 900;
        private const int ShieldDuration = 180;

        private const float ShieldDamageThreshold = 0.05f;
        private const float ShieldDamageReduction = 0.1f;

        private const float MinimumCooldown = 15f;
        private const float MaximumCooldown = 75f;

        // 준비 상태 페이드 시간 0.2초
        private const int VisualReadyTime = 12;

        // 보호막 활성화 페이드 시간 0.2초
        private const int VisualActivateTime = 12;

        // 피격 연출 시간
        private const int VisualHitTime = 18;

        // 보호막 종료 페이드 시간 0.2초
        private const int VisualEndTime = 12;

        private static readonly SoundStyle ShieldHitSound =
    new SoundStyle("CAmod/Sounds/LumenAegisShieldHit");

        private static readonly SoundStyle ShieldBreakSound =
            new SoundStyle("CAmod/Sounds/LumenAegisShieldBreak");

        public override void ResetEffects()
        {
            lumenAegisEquipped = false;
        }

        public override void Initialize()
        {
            calamityPlayer = Player.GetModPlayer<CalamityPlayer>();
        }

        public override void PostUpdate()
        {
            UpdateEquipTimer();
            UpdateShieldTimer();
            UpdateCooldown();
            UpdateImmuneTime();
            UpdateLumenAegisVisual();
        }

        // =========================
        // 성배 활성화 여부
        // =========================

        private bool IsChaliceActive()
        {
            return calamityPlayer != null &&
                   calamityPlayer.chaliceOfTheBloodGod;
        }

        // =========================
        // 성배 전체 버퍼
        // =========================

        private double GetChaliceTotalBuffer()
        {
            if (calamityPlayer == null)
                return 0d;

            return calamityPlayer.chaliceBleedoutBuffer +
                   calamityPlayer.chaliceBleedoutToApplyOnHurt;
        }

        // =========================
        // 성배 버퍼를 실드가 대신 부담한다
        // =========================


        private void ApplyChaliceBufferToShield()
        {
            if (!IsChaliceActive())
                return;

            if (lumenAegisShield <= 0f)
                return;

            double existingBuffer =
                calamityPlayer.chaliceBleedoutBuffer;

            int pendingBuffer =
                calamityPlayer.chaliceBleedoutToApplyOnHurt;

            double totalBuffer =
                existingBuffer + pendingBuffer;

            if (totalBuffer <= 0d)
                return;

            // 피해 감소율을 적용해 성배 버퍼가 보호막에 주는 피해를 계산한다
            double shieldDamageFromBuffer =
                totalBuffer * (1d - ShieldDamageReduction);

            // 현재 보호막이 감당할 수 있는 만큼만 피해를 적용한다
            shieldDamageFromBuffer =
                Math.Min(lumenAegisShield, shieldDamageFromBuffer);

            if (shieldDamageFromBuffer <= 0d)
                return;

            // 보호막에 실제로 적용된 피해량을 기준으로 성배 버퍼 소모량을 계산한다
            double bufferConsumed =
                shieldDamageFromBuffer /
                (1d - ShieldDamageReduction);

            // 이번 피격에서 새로 생긴 버퍼부터 소모한다
            int pendingDamage =
                (int)Math.Min(pendingBuffer, bufferConsumed);

            pendingBuffer -= pendingDamage;
            bufferConsumed -= pendingDamage;

            calamityPlayer.chaliceBleedoutToApplyOnHurt =
                pendingBuffer;

            // 기존에 쌓여 있던 버퍼를 소모한다
            double existingDamage =
                Math.Min(existingBuffer, bufferConsumed);

            existingBuffer -= existingDamage;

            calamityPlayer.chaliceBleedoutBuffer =
                existingBuffer;

            // 기존 성배 버퍼의 소수점 진행량도 함께 감소시킨다
            calamityPlayer.chaliceDamagePointPartialProgress -=
                existingDamage;

            if (calamityPlayer.chaliceDamagePointPartialProgress < 0d)
                calamityPlayer.chaliceDamagePointPartialProgress = 0d;

            // 성배 버퍼가 부담한 피해를 보호막에 적용한다
            lumenAegisShield -= (float)shieldDamageFromBuffer;

            if (lumenAegisShield < 0f)
                lumenAegisShield = 0f;
        }


        // =========================
        // 장착 시간
        // =========================

        private void UpdateEquipTimer()
        {
            if (lumenAegisEquipped)
            {
                if (!lumenAegisWasEquipped)
                {
                    lumenAegisEquipTimer = EquipDelay;
                    lumenAegisWasEquipped = true;
                }

                if (lumenAegisEquipTimer > 0)
                    lumenAegisEquipTimer--;
            }
            else
            {
                if (lumenAegisWasEquipped)
                {
                    // 장착 해제 시 현재 실드를 제거하지만 쿨다운은 유지한다
                    lumenAegisActive = false;
                    lumenAegisShield = 0f;
                    lumenAegisShieldMax = 0f;

                    lumenAegisImmuneTime = 0;

                    lumenAegisVisualAlpha = 0f;
                    lumenAegisVisualFresnelPower =
                        NormalFresnelPower;

                    lumenAegisVisualHitTimer = 0;

                    lumenAegisVisualReadyTimer = 0;
                    lumenAegisVisualActivateTimer = 0;
                    lumenAegisVisualEndTimer = 0;
                    lumenAegisVisualEnding = false;

                    // 스케일 애니메이션 상태를 초기화한다
                    lumenAegisScaleTimer = 0;
                    lumenAegisScalingIn = false;
                    lumenAegisScalingOut = false;
                    lumenAegisVisualScale = 1f;

                    lumenAegisProjectileIndex = -1;
                    lumenAegisWasEquipped = false;
                }
            }
        }

        // =========================
        // 실드 시간
        // =========================

        private void UpdateShieldTimer()
        {
            if (!lumenAegisActive)
                return;

            if (lumenAegisTimer > 0)
                lumenAegisTimer--;
            else
                EndShield();
        }

        private void UpdateCooldown()
        {
            if (lumenAegisCooldown > 0)
                lumenAegisCooldown--;
        }

        private void UpdateImmuneTime()
        {
            if (lumenAegisImmuneTime > 0)
                lumenAegisImmuneTime--;
        }

        // =========================
        // 자연 회복
        // =========================

        public override void UpdateLifeRegen()
        {
            if (lumenAegisCooldown <= 0)
                return;

            if (lumenAegisShield > 0f)
                return;

            if (!lumenAegisEquipped)
                return;



            int missing = Player.statLifeMax2 - Player.statLife;
            Player.lifeRegen += (int)Math.Round(
                missing / (float)Player.statLifeMax2 * 1f,
                MidpointRounding.AwayFromZero);

        }

        // =========================
        // 공격 판정
        // =========================

        public override bool CanBeHitByNPC(
            NPC npc,
            ref int cooldownSlot)
        {
            if (lumenAegisActive &&
                lumenAegisImmuneTime > 0)
            {
                return false;
            }

            return base.CanBeHitByNPC(
                npc,
                ref cooldownSlot);
        }

        public override bool CanBeHitByProjectile(
            Projectile proj)
        {
            if (lumenAegisActive &&
                lumenAegisImmuneTime > 0)
            {
                return false;
            }

            return base.CanBeHitByProjectile(proj);
        }

        // =========================
        // 피해 처리
        // =========================

        public override void ModifyHurt(
            ref Player.HurtModifiers modifiers)
        {
            if (!lumenAegisEquipped)
                return;

            if (lumenAegisActive)
            {
                modifiers.ModifyHurtInfo +=
                    ModifyLumenAegisHurt;

                return;
            }

            if (lumenAegisActive &&
                lumenAegisShield >= 1f)
            {
                return;
            }

            if (lumenAegisCooldown > 0)
                return;

            if (lumenAegisEquipTimer > 0)
                return;

            modifiers.ModifyHurtInfo +=
                ModifyLumenAegisHurt;
        }
        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (lumenAegisFreeDodgePending)
            {
                lumenAegisFreeDodgePending = false;
                return true;
            }

            return false;
        }

        private void ModifyLumenAegisHurt(
            ref Player.HurtInfo info)
        {
            // 이미 활성화된 보호막을 처리한다
            if (lumenAegisActive)
            {
                int shieldDamage =
                    (int)Math.Ceiling(
                        info.Damage * (1f - ShieldDamageReduction));

                lumenAegisShield -= shieldDamage;

                // 성배 버퍼가 적용되기 전의 값을 저장한다
                double chaliceBufferBefore =
                    GetChaliceTotalBuffer();

                // 성배 출혈 버퍼도 보호막이 대신 부담한다
                ApplyChaliceBufferToShield();

                // 실제로 보호막이 부담한 성배 버퍼 피해량을 계산한다
                double chaliceBufferAfter =
                    GetChaliceTotalBuffer();

                int chaliceShieldDamage =
                    (int)Math.Ceiling(
                        (chaliceBufferBefore - chaliceBufferAfter) *
                        (1d - ShieldDamageReduction));

                // 보호막이 부담한 총 피해량을 계산한다
                int totalShieldDamage =
                    shieldDamage + chaliceShieldDamage;

                lumenAegisVisualHitTimer = VisualHitTime;

                int dustCount = totalShieldDamage / 6;

                for (int k = 0; k < dustCount; k++)
                {
                    Vector2 direction =
                        Main.rand.NextVector2Unit();

                    Vector2 spawnPosition =
                        Player.Center + direction * 20f;

                    Vector2 particleVelocity =
                        direction * Main.rand.NextFloat(2f, 6f);

                    direction.Y *= 0.6f;

                    Dust dust2 = Dust.NewDustPerfect(
                        spawnPosition,
                        DustID.Electric,
                        particleVelocity);

                    dust2.scale = Main.rand.NextFloat(0.9f, 1.5f);
                    dust2.noGravity = true;
                }

                SoundEngine.PlaySound(
                    ShieldHitSound,
                    Player.Center);

                lumenAegisImmuneTime += 40;

                Player.immune = true;
                Player.immuneTime = 40;

                CombatText.NewText(
                    Player.Hitbox,
                    Color.Gray,
                    "-" + totalShieldDamage);

                if (lumenAegisShield > 0f)
                {
                    lumenAegisFreeDodgePending = true;
                    info.Damage = 0;
                    return;
                }

                if (lumenAegisShield == 0f)
                {
                    info.Damage = 0;
                    SoundEngine.PlaySound(
                        ShieldBreakSound,
                        Player.Center);

                    EndShield();
                    return;
                }

                // 보호막을 초과한 피해를 실제 피해량으로 환산한다
                float overflowDamage =
                    -lumenAegisShield;

                lumenAegisShield = 0f;

                SoundEngine.PlaySound(
                    ShieldBreakSound,
                    Player.Center);

                int damageToPlayer =
                    (int)Math.Ceiling(
                        overflowDamage /
                        (1f - ShieldDamageReduction));

                if (damageToPlayer < 1)
                    damageToPlayer = 1;

                info.Damage = damageToPlayer;

                EndShield();
                return;
            }

            // 보호막 최초 활성화 조건을 검사한다
            if (lumenAegisCooldown > 0)
                return;

            if (lumenAegisEquipTimer > 0)
                return;

            double effectiveDamage = info.Damage;

            if (IsChaliceActive())
            {
                // 현재 피해와 성배 출혈 버퍼를 합산한다
                effectiveDamage += GetChaliceTotalBuffer();
            }

            double threshold =
                Player.statLifeMax2 * ShieldDamageThreshold;

            // 최대 체력의 5%를 초과하는 피해에서만 보호막을 발동한다
            if (effectiveDamage <= threshold)
                return;

            ActivateShield();

            if (!lumenAegisActive)
                return;

            int initialShieldDamage =
                (int)Math.Ceiling(
                    info.Damage * (1f - ShieldDamageReduction));

            lumenAegisShield -= initialShieldDamage;

            // 최초 방어에서 성배 버퍼가 적용되기 전의 값을 저장한다
            double initialChaliceBufferBefore =
                GetChaliceTotalBuffer();

            // 최초 발동 공격의 성배 버퍼도 보호막이 부담한다
            ApplyChaliceBufferToShield();

            // 최초 방어에서 실제로 소모된 성배 버퍼를 계산한다
            double initialChaliceBufferAfter =
                GetChaliceTotalBuffer();

            int initialChaliceShieldDamage =
                (int)Math.Ceiling(
                    (initialChaliceBufferBefore -
                     initialChaliceBufferAfter) *
                    (1d - ShieldDamageReduction));

            int initialTotalShieldDamage =
                initialShieldDamage +
                initialChaliceShieldDamage;

            lumenAegisVisualHitTimer = VisualHitTime;

            int initialDustCount =
                initialTotalShieldDamage / 5;

            for (int k = 0; k < initialDustCount; k++)
            {
                Vector2 direction =
                    Main.rand.NextVector2Unit();

                Vector2 spawnPosition =
                    Player.Center + direction * 20f;

                Vector2 particleVelocity =
                    direction * Main.rand.NextFloat(2f, 6f);

                direction.Y *= 0.6f;

                Dust dust2 = Dust.NewDustPerfect(
                    spawnPosition,
                    DustID.Electric,
                    particleVelocity);

                dust2.scale = Main.rand.NextFloat(0.9f, 1.5f);
                dust2.noGravity = true;
            }

            SoundEngine.PlaySound(
                ShieldHitSound,
                Player.Center);

            lumenAegisImmuneTime += 40;

            Player.immune = true;
            Player.immuneTime = 40;

            CombatText.NewText(
                Player.Hitbox,
                Color.Gray,
                "-" + initialTotalShieldDamage);

            if (lumenAegisShield > 0f)
            {
                lumenAegisFreeDodgePending = true;
                info.Damage = 0;
                return;
            }

            if (lumenAegisShield == 0f)
            {
                info.Damage = 0;
                SoundEngine.PlaySound(
                    ShieldBreakSound,
                    Player.Center);

                EndShield();
                return;
            }

            // 최초 발동에서 보호막을 초과한 피해를 환산한다
            float initialOverflowDamage =
                -lumenAegisShield;

            lumenAegisShield = 0f;

            SoundEngine.PlaySound(
                ShieldBreakSound,
                Player.Center);

            int initialDamageToPlayer =
                (int)Math.Ceiling(
                    initialOverflowDamage /
                    (1f - ShieldDamageReduction));

            if (initialDamageToPlayer < 1)
                initialDamageToPlayer = 1;

            info.Damage = initialDamageToPlayer;

            EndShield();
        }


        // =========================
        // 실드 생성
        // =========================

        private void ActivateShield()
        {
            if (lumenAegisActive)
                return;

            if (lumenAegisCooldown > 0)
                return;

            if (lumenAegisEquipTimer > 0)
                return;

            lumenAegisActive = true;

            lumenAegisShieldMax = Player.statManaMax2 + 120f;

            lumenAegisShield =
                lumenAegisShieldMax;

            lumenAegisTimer =
                ShieldDuration;

            lumenAegisVisualHitTimer = 0;

            lumenAegisVisualActivateTimer =
    VisualActivateTime;



            lumenAegisVisualReadyTimer = 0;

            lumenAegisVisualEnding = false;
            lumenAegisVisualEndTimer = 0;

            lumenAegisVisualAlpha = 0.1f;
            lumenAegisVisualFresnelPower =
                NormalFresnelPower;

            SpawnLumenAegisProjectile();
        }

        // =========================
        // 실드 종료
        // =========================

        private void EndShield()
        {
            if (!lumenAegisActive &&
                lumenAegisShieldMax <= 0f)
            {
                return;
            }

            float damageRatio = 1f;

            if (lumenAegisShieldMax > 0f)
            {
                damageRatio =
                    1f -
                    lumenAegisShield /
                    lumenAegisShieldMax;
            }

            damageRatio =
                MathHelper.Clamp(
                    damageRatio,
                    0f,
                    1f);

            float cooldownSeconds =
    Math.Max(MinimumCooldown, damageRatio * MaximumCooldown);

            lumenAegisCooldownMax =
                (int)(cooldownSeconds * 60f);

            lumenAegisCooldown =
                lumenAegisCooldownMax;

            lumenAegisActive = false;
            lumenAegisShield = 0f;
            lumenAegisShieldMax = 0f;

            lumenAegisVisualHitTimer = 0;

            lumenAegisVisualEnding = true;

            lumenAegisVisualEndTimer =
                VisualEndTime;

            lumenAegisScaleTimer =
                VisualEndTime;

            lumenAegisScalingIn = false;
            lumenAegisScalingOut = true;
            lumenAegisVisualScale = 1f;

            lumenAegisVisualAlpha = 0.5f;
            lumenAegisVisualFresnelPower =
                NormalFresnelPower;
        }

        // =========================
        // 시각 효과
        // =========================

        private void UpdateLumenAegisVisual()
        {
            if (!lumenAegisEquipped)
            {
                lumenAegisVisualAlpha = 0f;
                lumenAegisVisualFresnelPower =
                    NormalFresnelPower;

                lumenAegisVisualReadyTimer = 0;
                lumenAegisVisualActivateTimer = 0;
                lumenAegisVisualEndTimer = 0;
                lumenAegisVisualEnding = false;

                lumenAegisScaleTimer = 0;
                lumenAegisScalingIn = false;
                lumenAegisScalingOut = false;
                lumenAegisVisualScale = 1f;

                return;
            }

            if (!lumenAegisWasEquipped)
                return;

            if (lumenAegisScalingIn)
            {
                if (lumenAegisScaleTimer > 0)
                {
                    lumenAegisScaleTimer--;

                    float progress =
                        1f -
                        lumenAegisScaleTimer /
                        (float)VisualActivateTime;

                    lumenAegisVisualScale =
                        MathHelper.Lerp(
                            2f,
                            1f,
                            progress);
                }
                else
                {
                    lumenAegisVisualScale = 1f;
                    lumenAegisScalingIn = false;
                }
            }
            else if (lumenAegisScalingOut)
            {
                if (lumenAegisScaleTimer > 0)
                {
                    lumenAegisScaleTimer--;

                    float progress =
                        1f -
                        lumenAegisScaleTimer /
                        (float)VisualEndTime;

                    lumenAegisVisualScale =
                        MathHelper.Lerp(
                            1f,
                            2f,
                            progress);
                }
                else
                {
                    lumenAegisVisualScale = 2f;
                    lumenAegisScalingOut = false;
                }
            }

            if (lumenAegisVisualEnding)
            {
                if (lumenAegisVisualEndTimer > 0)
                {
                    lumenAegisVisualEndTimer--;

                    float progress =
                        1f -
                        lumenAegisVisualEndTimer /
                        (float)VisualEndTime;

                    lumenAegisVisualAlpha =
                        MathHelper.Lerp(
                            0.5f,
                            0f,
                            progress);

                    lumenAegisVisualFresnelPower =
                        NormalFresnelPower;
                }
                else
                {
                    lumenAegisVisualAlpha = 0f;
                    lumenAegisVisualEnding = false;
                }

                return;
            }

            if (lumenAegisActive)
            {
                if (lumenAegisVisualActivateTimer > 0)
                {
                    lumenAegisVisualActivateTimer--;

                    float progress =
                        1f -
                        lumenAegisVisualActivateTimer /
                        (float)VisualActivateTime;

                    lumenAegisVisualAlpha =
                        MathHelper.Lerp(
                            0.1f,
                            0.5f,
                            progress);
                }
                else
                {
                    lumenAegisVisualAlpha = 0.5f;
                }

                if (lumenAegisVisualHitTimer > 0)
                {
                    lumenAegisVisualHitTimer--;

                    float progress =
                        1f -
                        lumenAegisVisualHitTimer /
                        (float)VisualHitTime;

                    float pulse =
                        progress < 0.5f
                            ? progress * 2f
                            : 2f - progress * 2f;

                    lumenAegisVisualAlpha =
                        MathHelper.Lerp(
                            0.5f,
                            1f,
                            pulse);

                    lumenAegisVisualFresnelPower =
                        MathHelper.Lerp(
                            NormalFresnelPower,
                            0.1f,
                            pulse);
                }
                else
                {
                    lumenAegisVisualAlpha = 0.5f;
                    lumenAegisVisualFresnelPower =
                        NormalFresnelPower;
                }

                return;
            }

            if (lumenAegisCooldown > 0)
            {
                lumenAegisVisualAlpha = 0f;
                lumenAegisVisualFresnelPower =
                    NormalFresnelPower;
                lumenAegisVisualReadyTimer = 0;

                return;
            }

            if (lumenAegisEquipTimer <= 0)
            {
                // 처음 준비 상태가 되었을 때 스케일 애니메이션을 시작한다
                if (lumenAegisVisualReadyTimer == 0)
                {
                    lumenAegisScaleTimer = VisualReadyTime;
                    lumenAegisScalingIn = true;
                    lumenAegisScalingOut = false;
                    lumenAegisVisualScale = 2f;
                }

                if (lumenAegisVisualReadyTimer <
                    VisualReadyTime)
                {
                    lumenAegisVisualReadyTimer++;
                }

                float progress =
                    lumenAegisVisualReadyTimer /
                    (float)VisualReadyTime;

                lumenAegisVisualAlpha =
                    MathHelper.Lerp(
                        0f,
                        0.15f,
                        progress);

                lumenAegisVisualFresnelPower =
                    NormalFresnelPower;

                // 사용할 준비가 된 동안 투사체를 유지한다
                SpawnLumenAegisProjectile();
            }
            else
            {
                lumenAegisVisualReadyTimer = 0;
                lumenAegisVisualAlpha = 0f;
                lumenAegisVisualFresnelPower =
                    NormalFresnelPower;
            }
        }

        // =========================
        // 투사체 생성
        // =========================

        private void SpawnLumenAegisProjectile()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            if (lumenAegisProjectileIndex >= 0 &&
                lumenAegisProjectileIndex < Main.maxProjectiles)
            {
                Projectile existing =
                    Main.projectile[
                        lumenAegisProjectileIndex];

                if (existing.active &&
                    existing.owner == Player.whoAmI &&
                    existing.type ==
                    ModContent.ProjectileType<LumenAegisProjectile>())
                {
                    return;
                }
            }

            int projectileType =
                ModContent.ProjectileType<LumenAegisProjectile>();

            int projectileIndex =
                Projectile.NewProjectile(
                    Player.GetSource_FromThis(),
                    Player.Center,
                    Vector2.Zero,
                    projectileType,
                    0,
                    0f,
                    Player.whoAmI);

            lumenAegisProjectileIndex =
                projectileIndex;
        }

        // =========================
        // 투사체 유지 판정
        // =========================

        public bool CanKeepLumenAegisProjectile()
        {
            if (!lumenAegisEquipped)
                return false;

            if (lumenAegisActive)
                return true;

            if (lumenAegisVisualEnding)
                return true;

            if (lumenAegisCooldown <= 0 &&
                lumenAegisEquipTimer <= 0)
            {
                return true;
            }

            return false;
        }

        // =========================
        // 투사체 인덱스 초기화
        // =========================

        public void ClearLumenAegisProjectileIndex(
            int projectileIndex)
        {
            if (lumenAegisProjectileIndex ==
                projectileIndex)
            {
                lumenAegisProjectileIndex = -1;
            }
        }
        public override void OnRespawn()
        {
            // 사망 후 리스폰하면 보호막 쿨타임을 다시 최대치로 설정한다

            lumenAegisCooldown = 0;
            lumenAegisCooldownMax = 0;
            lumenAegisEquipTimer = 0;
            lumenAegisActive = false;
            lumenAegisShield = 0f;
            lumenAegisShieldMax = 0f;
            lumenAegisImmuneTime = 0;
        }
        // =========================
        // 렌더링 데이터이터이터
        // =========================

        public bool TryGetLumenAegisRenderData(
            out float alpha,
            out float fresnelPower,
            out float scale)
        {
            alpha =
                lumenAegisVisualAlpha;

            fresnelPower =
                lumenAegisVisualFresnelPower;

            scale =
                lumenAegisVisualScale;

            if (!lumenAegisEquipped)
                return false;

            if (alpha <= 0.001f)
                return false;

            return true;
        }
    }
}