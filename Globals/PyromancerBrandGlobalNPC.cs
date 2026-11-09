using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using ParticleLibrary.Core.V3.Particles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using CAmod.Particles;
using CAmod.Players;
using static CAmod.Items.Pyromancer;

namespace CAmod.Globals
{
    public class PyromancerBrandGlobalNPC : GlobalNPC
    {
        public const byte SyncPacket = 1;
        public override bool InstancePerEntity => true;
        // Allocated only when this NPC actually receives a brand.
        private Dictionary<int, BrandState> brands;
        private List<int> expiredOwners;

        public override void SetDefaults(NPC npc)
        {
            brands = null;
            expiredOwners = null;
        }

        public override void OnKill(NPC npc)
        {
            // Do not mutate an enumerated dictionary if our own tick just killed this NPC.
            brands = null;
        }

        public bool HasBrand(int owner) => brands != null && brands.ContainsKey(owner);

        public void AddBrand(NPC npc, int owner, int amount)
        {
            brands ??= new Dictionary<int, BrandState>();
            if (!brands.TryGetValue(owner, out BrandState state))
            {
                state = new BrandState(owner);
                brands.Add(owner, state);
            }
            state.Increase(amount);
            SendState(npc, state);
        }

        public override void PostAI(NPC npc)
        {
            if (brands == null)
                return;
            var activeBrands = brands;
            expiredOwners ??= new List<int>();
            expiredOwners.Clear();
            foreach (var entry in activeBrands)
            {
                if (!entry.Value.Update(npc))
                    expiredOwners.Add(entry.Key);
                if (!npc.active || npc.life <= 0)
                {
                    brands = null;
                    return;
                }
            }
            foreach (int owner in expiredOwners)
                activeBrands.Remove(owner);
            if (activeBrands.Count == 0)
                brands = null;
        }

        private static void SendState(NPC npc, BrandState state, int ignoreClient = -1)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
                return;
            ModPacket packet = ModContent.GetInstance<global::CAmod.CAmod>().GetPacket();
            packet.Write(SyncPacket);
            packet.Write((short)npc.whoAmI);
            packet.Write(npc.type);
            state.Write(packet);
            packet.Send(-1, ignoreClient);
        }

        public static void HandlePacket(BinaryReader reader, int sender)
        {
            int npcIndex = reader.ReadInt16();
            int npcType = reader.ReadInt32();
            BrandState state = BrandState.Read(reader);
            if (npcIndex < 0 || npcIndex >= Main.maxNPCs || state.Owner < 0 || state.Owner >= Main.maxPlayers
                || (Main.netMode == NetmodeID.Server && state.Owner != sender))
                return;
            NPC npc = Main.npc[npcIndex];
            if (!npc.active || npc.type != npcType || npc.life <= 0)
                return;
            var global = npc.GetGlobalNPC<PyromancerBrandGlobalNPC>();
            global.brands ??= new Dictionary<int, BrandState>();
            global.brands[state.Owner] = state;
            if (Main.netMode == NetmodeID.Server)
                SendState(npc, state, sender);
        }

        // Include existing brands in NPC synchronization, including clients joining mid-fight.
        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter writer)
        {
            writer.Write((ushort)(brands?.Count ?? 0));
            if (brands != null)
                foreach (BrandState state in brands.Values)
                    state.Write(writer);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader reader)
        {
            int count = reader.ReadUInt16();
            // Incremental owner updates are authoritative for that owner's current damage state.
            for (int i = 0; i < count; i++)
            {
                BrandState state = BrandState.Read(reader);
                if (state.Owner < 0 || state.Owner >= Main.maxPlayers || state.Owner == Main.myPlayer)
                    continue;
                brands ??= new Dictionary<int, BrandState>();
                brands[state.Owner] = state;
            }
        }

        private sealed class BrandState
        {
            public readonly int Owner;
            private const int HoldDurationTicks = 5 * 60;
            private int timeLeft = HoldDurationTicks;
            private int tickTimer;
            private Vector2? previousParticleCenter;
            private float particleDistanceRemainder;
            private int damagePerTick = BASE_TICK_DAMAGE;
            private int now;
            private float max;
            private float fadeBuffer;

            public BrandState(int owner) { Owner = owner; }

            public void Increase(int amount)
            {
                damagePerTick = (int)Math.Min(int.MaxValue, (long)damagePerTick + Math.Max(0, amount));
                if (!DEV_MODE)
                    damagePerTick = Math.Min(damagePerTick, BALANCED_TICK_DAMAGE_CAP);
                timeLeft = HoldDurationTicks;
            }

            public bool Update(NPC target)
            {
                if (Owner < 0 || Owner >= Main.maxPlayers || !Main.player[Owner].active
                    || !target.active || target.life <= 0)
                    return false;

                if (timeLeft <= 1)
                {
                    if (now <= 0)
                        return false;
                    float ratio = max > 0f ? MathHelper.Clamp(now / max, 0f, 1f) : 0f;
                    float fadeFrames = 900f * ratio;
                    if (fadeFrames <= 1f)
                        return false;
                    timeLeft = 2;
                    fadeBuffer += now / fadeFrames;
                    int decrease = (int)fadeBuffer;
                    now -= decrease;
                    fadeBuffer -= decrease;
                    if (now <= 1)
                        return false;
                    damagePerTick = now;
                }
                timeLeft--;

                if (!Main.dedServ && target.Opacity > 0f && Main.player[Owner].GetModPlayer<PyromancerPlayer>().visualEnabled)
                    EmitParticles(target);
                else
                {
                    previousParticleCenter = null;
                    particleDistanceRemainder = 0f;
                }

                tickTimer++;
                if (tickTimer < 20 || target.immortal || target.dontTakeDamage || target.immune[Owner] > 0)
                    return true;
                tickTimer = 0;
                Player owner = Main.player[Owner];
                float magicAdd = owner.GetDamage(DamageClass.Magic).Additive - 1f;
                float genericAdd = owner.GetDamage(DamageClass.Generic).Additive - 1f;
                float crit = (owner.GetCritChance(DamageClass.Magic)
                    + owner.GetCritChance(DamageClass.Generic)) / 100f + 1f;
                max = (float)Math.Round((magicAdd + genericAdd) * crit * 1000f);
                int rawDamage = Math.Max(1, Math.Min(damagePerTick, (int)max));
                now = rawDamage;
                if (Owner != Main.myPlayer)
                    return true;

                int boostedDamage = (int)Math.Min(int.MaxValue,
                    Math.Max(1d, Math.Round(rawDamage * (1d + owner.GetModPlayer<PyromancerPlayer>().BrandDamageBonus))));
                NPC.HitInfo hit = target.CalculateHitInfo(boostedDamage, hitDirection: 0, crit: false,
                    knockBack: 0f, damageType: DamageClass.Generic, damageVariation: false);
                hit.HideCombatText = true;
                target.PlayerInteraction(Owner);
                int actualDamage = global::CAmod.Systems.PyromancerBrandSoundSystem.StrikeSilently(target, hit);
                if (Main.netMode == NetmodeID.MultiplayerClient)
                    global::CAmod.Systems.PyromancerBrandSoundSystem.SendStrike(target, hit, Owner);
                if (actualDamage > 0)
                {
                    // The dot flag changes only text animation; damage stays a direct strike.
                    CombatText.NewText(target.Hitbox, new Color(255, 189, 25), actualDamage, dramatic: false, dot: true);
                    owner.addDPS(actualDamage);
                    owner.GetModPlayer<BossFightTimerPlayer>().RecordPyromancerDamage(actualDamage);
                }
                if (target.active && target.life > 0)
                    SendState(target, this);
                return true;
            }

            private void EmitParticles(NPC target)
            {
                // Original local Fire1/Fire2/Fire3 particle effect.
                Player owner1 = Main.player[Owner];
                float magicAdd1 = owner1.GetDamage(DamageClass.Magic).Additive - 1f;
                float genericAdd1 = owner1.GetDamage(DamageClass.Generic).Additive - 1f;
                float TotalMagicCrit1 = owner1.GetCritChance(DamageClass.Magic) / 100f + 1f;
                int magicPowerValue1 = (int)Math.Round(
                    (magicAdd1 + genericAdd1) * TotalMagicCrit1 * 1000f);
                float intensity = MathHelper.Clamp(damagePerTick / (float)magicPowerValue1, 0f, 1f);
                int fireCount = 1 + (int)((target.width + target.height) / 10 * intensity / 2);
                const float particleSpacing = 2.5f;
                Vector2 center = target.Center;
                Vector2 previous = previousParticleCenter ?? center;
                previousParticleCenter = center;
                float totalDistance = Vector2.Distance(previous, center) + particleDistanceRemainder;
                int movementCount = (int)(totalDistance / particleSpacing);
                particleDistanceRemainder = totalDistance - movementCount * particleSpacing;
                fireCount = Math.Min(50, Math.Max(fireCount, movementCount));
    
                float stackRatio = max > 0f
                    ? MathHelper.Clamp(damagePerTick / max, 0f, 1f)
                    : 0f;
    
                // A bounded size range instead of clipping random samples at 25 or 40.
                // Hitbox growth approaches 40 smoothly; every sample remains at least 25.
                float hitboxAverage = (target.width + target.height) * 0.5f;
                float sizeRange = 15f * (hitboxAverage / (hitboxAverage + 13f));
                float minSize = 25f + sizeRange * (0.6f * intensity);
                float maxSize = 25f + sizeRange * (0.2f + 0.8f * intensity);

                for (int i = 0; i < fireCount; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    float radius = MathF.Sqrt(Main.rand.NextFloat());
                    float radiusX = target.width * 0.35f * stackRatio;
                    float radiusY = target.height * 0.35f * stackRatio;
                    Vector2 offset = new Vector2(
                        MathF.Cos(angle) * radius * radiusX,
                        MathF.Sin(angle) * radius * radiusY);
                    Vector2 spawnPos = Vector2.Lerp(previous, center, (i + 1f) / fireCount) + offset;
                    Vector2 velocity = new Vector2(
                        Main.rand.NextFloat(-0.6f, 0.6f),
                        Main.rand.NextFloat(-5.0f, -0.5f));
                    float scaleValue = Main.rand.NextFloat(minSize, maxSize);
                    ParticleInfo info = new ParticleInfo(
                        new System.Numerics.Vector2(spawnPos.X, spawnPos.Y),
                        new System.Numerics.Vector2(velocity.X, velocity.Y),
                        0f,
                        new System.Numerics.Vector2(scaleValue, scaleValue),
                        Color.White,
                        Main.rand.Next(40, 80));

                    // The sampled radius is normalized to the actual elliptical spawn area.
                    // When the spawn area collapses to the center, all particles use the center odds.
                    float spawnRadius = stackRatio > 0f ? radius : 0f;
                    float additiveChance = 0.25f * (1f - spawnRadius);
                    bool additive = Main.rand.NextFloat() < additiveChance;
    
                    switch (Main.rand.Next(3))
                    {
                        case 0:
                            if (additive) FireParticleSystem.AdditiveBuffer1.Create(info);
                            else FireParticleSystem.Buffer1.Create(info);
                            break;
                        case 1:
                            if (additive) FireParticleSystem.AdditiveBuffer2.Create(info);
                            else FireParticleSystem.Buffer2.Create(info);
                            break;
                        case 2:
                            if (additive) FireParticleSystem.AdditiveBuffer3.Create(info);
                            else FireParticleSystem.Buffer3.Create(info);
                            break;
                    }
                }
    
            }

            public void Write(BinaryWriter writer)
            {
                writer.Write((byte)Owner);
                writer.Write((short)timeLeft);
                writer.Write((byte)tickTimer);
                writer.Write(damagePerTick);
                writer.Write(now);
                writer.Write(max);
                writer.Write(fadeBuffer);
            }

            public static BrandState Read(BinaryReader reader)
            {
                int owner = reader.ReadByte();
                var state = new BrandState(owner);
                state.timeLeft = Math.Clamp((int)reader.ReadInt16(), 1, HoldDurationTicks);
                state.tickTimer = Math.Clamp((int)reader.ReadByte(), 0, 20);
                state.damagePerTick = Math.Max(1, reader.ReadInt32());
                state.now = Math.Max(0, reader.ReadInt32());
                float maxValue = reader.ReadSingle();
                float remainder = reader.ReadSingle();
                state.max = float.IsFinite(maxValue) ? Math.Max(0f, maxValue) : 0f;
                state.fadeBuffer = float.IsFinite(remainder) ? MathHelper.Clamp(remainder, 0f, 1f) : 0f;
                return state;
            }
        }
    }
}
