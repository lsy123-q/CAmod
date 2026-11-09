using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CAmod.Projectiles;
using CAmod.Particles;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using System;
using System;
using Terraria.Audio;

namespace CAmod.Players
{
    public class BloodMagePlayer : ModPlayer
    {
        public double manaHealBuffer; // 마나 회복 버퍼
        public double manaHealPartial; // 소수점 누적
        private const double ManaDecay = 0.0083333333333; // Chalice 동일값

        public double lifeHealBuffer; // 체력 회복 총량을 저장한다
        public double lifeHealPartial; // 소수점 누적용 변수다
        private const double LifeHealDecay = 0.0166666666666; // 초당 50% 지수감쇠

        private float vampAccelFrozen = 1f; // 쿨 시작 시점의 가속값을 저장한다
        public float vampCooldownMax; // UI용 최대 쿨타임을 저장한다
        public bool bloodMageEquipped;
        public int def;
        public float cachedRegenPerSecond;
        // 장신구로 재생을 0으로 만들기 전에 기존 초당 재생량을 캐싱한다
        public bool allowBloodHeal;
        public float vampCooldown;
        public bool wasBloodMageEquipped;
        public int lastLife;

        SoundStyle[] sounds =
        {
            SoundID.NPCHit9,
            SoundID.NPCDeath22,  // Drippler 사망음이다
            SoundID.NPCDeath11   // Creeper 사망음이다
        };

        public override void ResetEffects()
        {
            wasBloodMageEquipped = bloodMageEquipped;
            bloodMageEquipped = false;
            allowBloodHeal = false;
            // 기본적으로 모든 치유를 차단한다
        }

        public override void OnEnterWorld()
        {
            lastLife = Player.statLifeMax2;
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
            if (Player.creativeGodMode)
            {
                lastLife = Player.statLifeMax2;
            }

            if (bloodMageEquipped == true)
            {
                float regenPerSecond = Player.lifeRegen / 2f;

                cachedRegenPerSecond = (regenPerSecond > -2.5f)
                    ? regenPerSecond
                    : -2.5f;
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
                vampCooldown -= accel * (2.5f + cachedRegenPerSecond);
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

                    // 현재 프레임 기준 자연 체젠량을 계산한다
                    float regenPerSecond = Player.lifeRegen / 2f;
                    int naturalRegenInt = (int)Math.Floor(regenPerSecond / 60f);

                    // 최소 자연회복 허용량을 1로 보정한다
                    naturalRegenInt = Math.Max(naturalRegenInt, 1);

                    if (diff > naturalRegenInt)
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

                float bonus = MathHelper.Lerp(0f, 0.10f, missingRatio2);
                // 최대 10%까지 증가한다

                float bonus2 = MathHelper.Lerp(0f, 0.20f, missingRatio2);

                Player.GetCritChance(DamageClass.Magic) += bonus * 100f;
                Player.GetDamage(DamageClass.Magic) += bonus2;
                // 마법 피해를 잃은 체력 비례로 증가시킨다
            }

            if (Player.creativeGodMode)
            {
                lastLife = Player.statLifeMax2;
            }

            // 마나 지수 회복
            if (manaHealBuffer > 0)
            {
                double amountThisFrame = manaHealBuffer * ManaDecay;
                manaHealPartial += amountThisFrame;

                int manaToGain = (int)manaHealPartial;

                if (manaToGain > 0)
                {
                    int applied = manaToGain;

                    Player.statMana += applied;
                    manaHealPartial -= applied;
                    manaHealBuffer -= applied;

                    if (manaHealBuffer < 0)
                        manaHealBuffer = 0;
                }
            }

            if (lifeHealBuffer > 0 && Player.statLife >= Player.statLifeMax2)
            {
                int wasted = (int)lifeHealBuffer;
                // 남은 회복량을 정수 기준으로 계산한다

                lifeHealBuffer = 0;
                lifeHealPartial = 0;

                // 회복 1당 60프레임 쿨을 단축한다
                vampCooldown -= wasted * 60f;

                if (vampCooldown < 0f)
                    vampCooldown = 0f;
            }

            // 체력 지수 회복은 별도 처리해야 한다
            if (lifeHealBuffer > 0 && !Player.dead)
            {
                int space2 = Player.statLifeMax2 - Player.statLife;

                if (space2 > 0)
                {
                    double amountThisFrame = lifeHealBuffer * LifeHealDecay;
                    lifeHealPartial += amountThisFrame;

                    int lifeToGain = (int)lifeHealPartial;

                    if (lifeToGain > 0)
                    {
                        int applied = Math.Min(lifeToGain, space2);

                        Player.statLife += applied;
                        lifeHealPartial -= applied;
                        lifeHealBuffer -= applied;

                        lastLife = Player.statLife;

                        if (lifeHealBuffer < 0)
                            lifeHealBuffer = 0;
                    }
                }
            }
        }

        public override void UpdateLifeRegen()
        {
            if (!bloodMageEquipped)
                return;
        }

        public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
        {
            if (bloodMageEquipped)
            {
                int lastdamage = hurtInfo.Damage;

                lastdamage += Player.GetModPlayer<global::CalamityMod.CalPlayer.CalamityPlayer>()
                    .chaliceBleedoutToApplyOnHurt;

                float damageRatio = MathHelper.Clamp(
                    lastdamage / (float)Player.statLifeMax2,
                    0f,
                    1f
                );
                // 받은 피해가 최대 체력 대비 몇 %인지다

                float dustScale =
                    MathHelper.Lerp(0.8f, 1.5f, damageRatio);
                // 데미지가 클수록 더 크게 튄다

                Vector2 hitDir = Player.Center - npc.Center;
                // 공격자가 있는 쪽에서 맞은 방향을 구한다

                if (hitDir.LengthSquared() < 0.0001f)
                    hitDir = new Vector2(hurtInfo.HitDirection, 0f);
                // 예외 상황에서는 피격 방향을 사용한다

                hitDir.Normalize();

                for (int i = 0; i < 25; i++)
                {
                    Vector2 vel =
                        hitDir * Main.rand.NextFloat(2.5f, 5.0f) +
                        Main.rand.NextVector2Circular(0.6f, 0.6f);
                    // 맞은 방향을 중심으로 피가 퍼지게 한다

                    BloodParticleSystem.Buffer.Create(
                        new ParticleInfo(
                            position: Player.Center.ToNumerics(),
                            velocity: vel.ToNumerics(),
                            rotation: 0f,
                            scale: new System.Numerics.Vector2(
                                7f * dustScale,
                                7f * dustScale
                            ),
                            depth: 1f,
                            color: Color.White,
                            duration: 100
                        )
                    );
                }
            }
        }

        public override void OnHitByProjectile(
            Projectile proj,
            Player.HurtInfo hurtInfo)
        {
            if (bloodMageEquipped)
            {
                int lastdamage = hurtInfo.Damage;

                lastdamage += Player.GetModPlayer<global::CalamityMod.CalPlayer.CalamityPlayer>()
                    .chaliceBleedoutToApplyOnHurt;

                float damageRatio = MathHelper.Clamp(
                    lastdamage / (float)Player.statLifeMax2,
                    0f,
                    1f
                );
                // 받은 피해가 최대 체력 대비 몇 %인지다

                float dustScale =
                    MathHelper.Lerp(0.8f, 1.5f, damageRatio);
                // 데미지가 클수록 더 크게 튄다

                Vector2 hitDir = Player.Center - proj.Center;
                // 공격자가 있는 쪽에서 맞은 방향을 구한다

                if (hitDir.LengthSquared() < 0.0001f)
                    hitDir = new Vector2(hurtInfo.HitDirection, 0f);
                // 예외 상황에서는 피격 방향을 사용한다

                hitDir.Normalize();

                for (int i = 0; i < 25; i++)
                {
                    Vector2 vel =
                        hitDir * Main.rand.NextFloat(2.5f, 5.0f) +
                        Main.rand.NextVector2Circular(0.6f, 0.6f);
                    // 맞은 방향을 중심으로 피가 퍼지게 한다

                    BloodParticleSystem.Buffer.Create(
                        new ParticleInfo(
                            position: Player.Center.ToNumerics(),
                            velocity: vel.ToNumerics(),
                            rotation: 0f,
                            scale: new System.Numerics.Vector2(
                                7f * dustScale,
                                7f * dustScale
                            ),
                            depth: 1f,
                            color: Color.White,
                            duration: 100
                        )
                    );
                }
            }
        }

        public override void OnHitNPC(
            NPC target,
            NPC.HitInfo hit,
            int damageDone)
        {
            if (!bloodMageEquipped ||
                target.friendly ||
                damageDone <= 0)
                return;

            if (cachedRegenPerSecond <= -2.5f)
            {
                return;
            }

            if (Player.statLife + lifeHealBuffer >= Player.statLifeMax2)
            {
                return;
            }

            if (hit.DamageType != DamageClass.Magic)
            {
                return;
            }

            if (vampCooldown > 0)
                return;

            // 이미 쿨타임이면 흡혈 자체를 허용하지 않는다
            float lostLifeValue = 10f;

            float healPotential =
                (float)damageDone / lostLifeValue;

            if ((int)healPotential <= 0)
                return;

            int missingLife =
                Player.statLifeMax2 - Player.statLife;

            if (missingLife <= 0)
                return;

            // 실질적으로 회복 가능한 양만 사용한다
            float effectiveHeal =
                Math.Min(healPotential, missingLife);

            // 여기서 실질 회복량 기준으로 쿨타임 확정
            float cooldown = (float)effectiveHeal * 60;

            vampCooldown = cooldown;
            vampCooldownMax = cooldown;

            float missing =
                Player.statLifeMax2 - Player.statLife;

            float missingRatio = MathHelper.Clamp(
                (float)missing / Player.statLifeMax2,
                0f,
                1f
            );

            // 구체가 날아가기 전에 이미 쿨타임이 예약된다

            // 실제 회복량이 최대 체력의 몇 %인지다
            float healRatio = MathHelper.Clamp(
                (float)effectiveHeal / Player.statLifeMax2,
                0f,
                1f
            );

            float scale =
                MathHelper.Lerp(0.5f, 1.75f, healRatio);
            // 회복량이 클수록 더 크게 튄다

            Vector2 origin = new Vector2(
                Main.rand.NextFloat(
                    target.position.X,
                    target.position.X + target.width
                ),
                Main.rand.NextFloat(
                    target.position.Y,
                    target.position.Y + target.height
                )
            );
            float boomscale = MathHelper.Clamp(effectiveHeal, 0.5f, 10f);
            for (int i = 0; i < 15; i++)
            {
                Vector2 vel =
                    Main.rand.NextVector2Circular(1f, 1f) * scale;

                BloodParticleSystem.Buffer.Create(
                    new ParticleInfo(
                        position: origin.ToNumerics(),
                        velocity: vel.ToNumerics(),
                        rotation: 0f,
                        scale: new System.Numerics.Vector2(
                            boomscale,
                            boomscale
                        ),
                        depth: 1f,
                        color: Color.White,
                        duration: 45
                    )
                );
            }

            SoundStyle style =
                sounds[Main.rand.Next(sounds.Length)];

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

            float framesPerHeal =
                60f / (5f + regen * at);

            int cooldown =
                (int)(healPotential * framesPerHeal);

            if (cooldown < 1)
                cooldown = 1;

            return cooldown;
        }
        // 예상 회복량 기준으로 쿨타임을 산출한다
    }
}