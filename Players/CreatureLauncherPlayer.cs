using Microsoft.Xna.Framework;
using CAmod.Projectiles;
using System.IO;
using CAmod.Systems;
using Terraria.GameInput;
using CAmod.NPCs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CAmod.Players
{
    public class CreatureLauncherPlayer : ModPlayer
    {
        public const byte TargetPacket = 3;
        public const byte DragonModePacket = 6;
        public bool DragonDefense { get; private set; }
        public bool DragonDeathPending;
        public int DragonDye;
        public bool HiveEquipped, TentacleEquipped, SharkEquipped, DragonEquipped;
        private readonly int[] charges = new int[3];
        private readonly int[] chargeTimers = new int[3];
        private readonly int[] shotTimers = new int[3];
        private readonly int[] targets = { -1, -1, -1 };
        private int requestCooldown;
        private int dragonDamageRemainder;
        private readonly ulong[] dragonOrbReadyAt = new ulong[5];

        public override void ResetEffects()
        {
            HiveEquipped = TentacleEquipped = SharkEquipped = DragonEquipped = false;
            DragonDye = 0;
        }

        private bool Equipped(int kind) => kind == 0 ? HiveEquipped : kind == 1 ? TentacleEquipped : SharkEquipped;

        public override void UpdateDead()
        {
            DragonDeathPending = false;
            dragonDamageRemainder = 0;
            DragonDefense = false;
            for (int i = 0; i < 3; i++) Clear(i);
        }

        private void Clear(int i)
        {
            charges[i] = chargeTimers[i] = shotTimers[i] = 0;
            targets[i] = -1;
        }

        public override void PostUpdate()
        {
            if (requestCooldown > 0) requestCooldown--;
            if (!DragonEquipped)
            {
                DragonDefense = false;
                dragonDamageRemainder = 0;
            }
            // NPC creation and all charges are authoritative on the server.
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            if (DragonEquipped && !Player.dead && !DragonDeathPending
                && Player.GetModPlayer<PhantomDragonProgressPlayer>().IsReady && !HasActiveDragon()) SpawnDragon();
            for (int i = 0; i < 3; i++)
            {
                if (!Equipped(i) || Player.dead) { Clear(i); continue; }
                if (++chargeTimers[i] >= 300)
                {
                    chargeTimers[i] = 0;
                    charges[i] = System.Math.Min(5, charges[i] + 1);
                }
                if (targets[i] < 0) continue;
                if (charges[i] == 0 || !CanTarget(i, Main.npc[targets[i]]))
                {
                    targets[i] = -1;
                    shotTimers[i] = 0;
                    continue;
                }
                if (++shotTimers[i] < 15) continue;
                shotTimers[i] = 0;
                int type = i == 0 ? ModContent.NPCType<LauncherHiveNPC>()
                    : i == 1 ? ModContent.NPCType<LauncherTentacleNPC>()
                    : ModContent.NPCType<LauncherSharkNPC>();
                int index = NPC.NewNPC(Player.GetSource_FromThis(), (int)Player.Center.X,
                    (int)Player.Center.Y, type, ai0: targets[i], ai1: Player.whoAmI);
                if (index < Main.maxNPCs)
                {
                    Main.npc[index].Center = Player.Center;
                    Main.npc[index].netUpdate = true;
                    charges[i]--;
                }
                if (charges[i] == 0) targets[i] = -1;
            }
        }

        internal void AddDragonDamage(Projectile contact, NPC target, int damageDone)
        {
            const int damagePerOrb = 10000;
            long totalDamage = (long)dragonDamageRemainder + damageDone;
            int earned = (int)(totalDamage / damagePerOrb);
            dragonDamageRemainder = (int)(totalDamage % damagePerOrb);
            if (Player.statLife >= Player.statLifeMax2) return;
            ulong now = Main.GameUpdateCount;

            for (int i = 0; i < dragonOrbReadyAt.Length && earned > 0; i++)
            {
                if (dragonOrbReadyAt[i] > now) continue;

                Vector2 position = target.position + new Vector2(
                    Main.rand.NextFloat(target.width), Main.rand.NextFloat(target.height));
                int index = Projectile.NewProjectile(contact.GetSource_OnHit(target), position, Vector2.Zero,
                    ModContent.ProjectileType<PhantomDragonHealOrb>(), 0, 0f, Player.whoAmI);
                if (index >= Main.maxProjectiles) break;

                dragonOrbReadyAt[i] = now + 60;
                earned--;
            }
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (!DragonEquipped || Player.dead || KeySystem.PhantomDragonMode?.JustPressed != true) return;
            DragonDefense = !DragonDefense;
            if (Main.netMode == NetmodeID.MultiplayerClient) SendDragonMode();
        }

        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            if (Main.netMode == NetmodeID.Server) SendDragonMode(toWho, fromWho);
        }

        private void SendDragonMode(int toWho = -1, int ignore = -1)
        {
            var packet = Mod.GetPacket();
            packet.Write(DragonModePacket);
            packet.Write((byte)Player.whoAmI);
            packet.Write(DragonDefense);
            packet.Send(toWho, ignore);
        }

        public static void HandleDragonMode(BinaryReader reader, int sender)
        {
            int index = reader.ReadByte();
            bool defense = reader.ReadBoolean();
            if (index >= Main.maxPlayers || (Main.netMode == NetmodeID.Server && index != sender)) return;
            var state = Main.player[index].GetModPlayer<CreatureLauncherPlayer>();
            if (Main.netMode == NetmodeID.Server && (!state.DragonEquipped || state.Player.dead)) return;
            state.DragonDefense = defense;
            if (Main.netMode == NetmodeID.Server) state.SendDragonMode();
        }

        private void SpawnDragon()
        {
            int free = 0;
            for (int i = 0; i < Main.maxNPCs; i++)
                if (!Main.npc[i].active) free++;
            if (free < LauncherPhantomDragonHead.GetSegmentCount(Player) + 1) return;
            int index = NPC.NewNPC(Player.GetSource_FromThis(), (int)Player.Center.X, (int)Player.Center.Y,
                ModContent.NPCType<LauncherPhantomDragonHead>());
            if (index >= Main.maxNPCs) return;
            Main.npc[index].Center = Player.Center;
            if (Main.npc[index].ModNPC is LauncherPhantomDragonHead dragon && dragon.Initialize(Player.whoAmI, -1))
                Main.npc[index].netUpdate = true;
            else
                Main.npc[index].active = false;
        }

        private bool HasActiveDragon()
        {
            foreach (NPC npc in Main.ActiveNPCs)
                if (npc.ModNPC is LauncherPhantomDragonHead dragon && dragon.Owner == Player.whoAmI)
                    return true;
            return false;
        }

        private void RequestTarget(NPC target)
        {
            if (Player.whoAmI != Main.myPlayer || requestCooldown > 0 || !target.CanBeChasedBy()
                || !(HiveEquipped || TentacleEquipped || SharkEquipped)) return;
            requestCooldown = 15;
            if (Main.netMode != NetmodeID.MultiplayerClient) { QueueTarget(target.whoAmI); return; }
            ModPacket packet = Mod.GetPacket();
            packet.Write(TargetPacket);
            packet.Write((short)target.whoAmI);
            packet.Write(target.type);
            packet.Send();
        }

        private static bool CanTarget(int kind, NPC npc) =>
            npc.CanBeChasedBy();

        private void QueueTarget(int index)
        {
            if (!Player.active || Player.dead || index < 0 || index >= Main.maxNPCs) return;
            for (int i = 0; i < 3; i++)
                if (Equipped(i) && charges[i] > 0 && targets[i] < 0 && CanTarget(i, Main.npc[index]))
                {
                    targets[i] = index;
                    shotTimers[i] = 15;
                }
        }

        public static void HandlePacket(BinaryReader reader, int sender)
        {
            int index = reader.ReadInt16();
            int type = reader.ReadInt32();
            if (Main.netMode != NetmodeID.Server || sender < 0 || sender >= Main.maxPlayers
                || index < 0 || index >= Main.maxNPCs || Main.npc[index].type != type) return;
            Main.player[sender].GetModPlayer<CreatureLauncherPlayer>().QueueTarget(index);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (damageDone > 0 && proj.DamageType.CountsAsClass(DamageClass.Magic)) RequestTarget(target);
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (damageDone > 0 && hit.DamageType.CountsAsClass(DamageClass.Magic)) RequestTarget(target);
        }
    }
}
