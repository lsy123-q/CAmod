using System;
using System.IO;
using CAmod.Globals;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CAmod.Systems
{
    public sealed class PyromancerBrandSoundSystem : ModSystem
    {
        public const byte StrikePacket = 2;
        private ILHook strikeHook;
        [ThreadStatic] private static NPC silentTarget;

        public override void Load()
        {
            var method = typeof(NPC).GetMethod(nameof(NPC.StrikeNPC),
                new[] { typeof(NPC.HitInfo), typeof(bool), typeof(bool) });
            if (method == null)
                throw new MissingMethodException("NPC.StrikeNPC(HitInfo, bool, bool)");
            strikeHook = new ILHook(method, SuppressBrandHitSound);
        }

        public override void Unload()
        {
            strikeHook?.Dispose();
            strikeHook = null;
            silentTarget = null;
        }

        private static void SuppressBrandHitSound(ILContext il)
        {
            var cursor = new ILCursor(il);
            // Only filter the sound style used by StrikeNPC's normal hurt sound.
            // Hit effects, death sounds, damage, and normal weapon hits are untouched.
            if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdfld<NPC>(nameof(NPC.HitSound))))
                throw new InvalidOperationException("Could not locate NPC.StrikeNPC's hit sound.");
            cursor.Emit(Mono.Cecil.Cil.OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<SoundStyle?, NPC, SoundStyle?>>(
                (sound, npc) => ReferenceEquals(npc, silentTarget) ? null : sound);
        }

        public static int StrikeSilently(NPC npc, NPC.HitInfo hit, bool fromNet = false)
        {
            NPC previous = silentTarget;
            silentTarget = npc;
            try
            {
                return npc.StrikeNPC(hit, fromNet: fromNet);
            }
            finally
            {
                silentTarget = previous;
            }
        }

        // A dedicated packet keeps the silence scoped to brand ticks on every peer.
        // Vanilla strike packets do not carry a per-hit sound suppression flag.
        public static void SendStrike(NPC npc, NPC.HitInfo hit, int owner, int ignoreClient = -1)
        {
            ModPacket packet = ModContent.GetInstance<global::CAmod.CAmod>().GetPacket();
            packet.Write(StrikePacket);
            packet.Write((short)npc.whoAmI);
            packet.Write(npc.type);
            packet.Write((byte)owner);
            packet.Write(hit.Damage);
            packet.Write(hit.SourceDamage);
            packet.Send(-1, ignoreClient);
        }

        public static void HandlePacket(BinaryReader reader, int sender)
        {
            int index = reader.ReadInt16();
            int type = reader.ReadInt32();
            int owner = reader.ReadByte();
            int damage = reader.ReadInt32();
            int sourceDamage = reader.ReadInt32();
            if (index < 0 || index >= Main.maxNPCs || owner < 0 || owner >= Main.maxPlayers
                || damage <= 0 || sourceDamage <= 0)
                return;
            NPC npc = Main.npc[index];
            if (!npc.active || npc.type != type || npc.life <= 0 || npc.immortal || npc.dontTakeDamage)
                return;
            if (Main.netMode == NetmodeID.Server && (owner != sender || !Main.player[owner].active
                || !npc.GetGlobalNPC<PyromancerBrandGlobalNPC>().HasBrand(owner)))
                return;

            var hit = new NPC.HitInfo
            {
                Damage = damage,
                SourceDamage = sourceDamage,
                DamageType = DamageClass.Generic,
                HitDirection = 0,
                Knockback = 0f,
                Crit = false,
                HideCombatText = true
            };
            npc.PlayerInteraction(owner);
            StrikeSilently(npc, hit, fromNet: true);
            if (Main.netMode == NetmodeID.Server)
                SendStrike(npc, hit, owner, sender);
        }
    }
}