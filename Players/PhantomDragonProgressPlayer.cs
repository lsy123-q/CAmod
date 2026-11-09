using System.IO;
using System.Numerics;
using CalamityMod.NPCs.Bumblebirb;
using CalamityMod.NPCs.CeaselessVoid;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.NPCs.OldDuke;
using CalamityMod.NPCs.Polterghast;
using CalamityMod.NPCs.ProfanedGuardians;
using CalamityMod.NPCs.Providence;
using CalamityMod.NPCs.Signus;
using CalamityMod.NPCs.StormWeaver;
using CalamityMod.NPCs.Yharon;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CAmod.Players
{
    public class PhantomDragonProgressPlayer : ModPlayer
    {
        internal const byte ProgressPacket = 8;
        private const string SaveKey = "phantomDragonBossKills";
        private const int MoonLord = 1 << 10;
        private const int ProvidenceBoss = 1 << 2;
        private const int Devourer = 1 << 8;
        private const int YharonBoss = 1 << 9;
        private const int DamageBosses = MoonLord - 1;
        private const int AllBosses = DamageBosses | MoonLord;
        private const int LengthBosses = MoonLord | ProvidenceBoss | Devourer | YharonBoss;

        private int defeatedBosses;
        private bool importedClientSave;
        internal bool IsReady => Main.netMode != NetmodeID.Server || importedClientSave;
        internal float DamageProgress
        {
            get
            {
                // Seven bosses grant 4/70 each, with the three major bosses granting 7/70, 14/70 and 21/70.
                int points = 4 * BitOperations.PopCount((uint)(defeatedBosses & DamageBosses));
                if ((defeatedBosses & ProvidenceBoss) != 0) points += 7 - 4;
                if ((defeatedBosses & Devourer) != 0) points += 14 - 4;
                if ((defeatedBosses & YharonBoss) != 0) points += 21 - 4;
                return points / 70f;
            }
        }
        internal int ExtraLoops => BitOperations.PopCount((uint)(defeatedBosses & LengthBosses));

        public override void SaveData(TagCompound tag) => tag[SaveKey] = defeatedBosses;

        public override void LoadData(TagCompound tag) => defeatedBosses = tag.GetInt(SaveKey) & AllBosses;

        public override void OnEnterWorld()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) SendProgress();
        }

        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            if (Main.netMode == NetmodeID.Server) SendProgress(toWho);
        }

        private void SendProgress(int toWho = -1)
        {
            ModPacket packet = Mod.GetPacket();
            packet.Write(ProgressPacket);
            packet.Write((byte)Player.whoAmI);
            packet.Write((ushort)defeatedBosses);
            packet.Send(toWho);
        }

        internal static void HandleProgress(BinaryReader reader, int sender)
        {
            int index = reader.ReadByte();
            int progress = reader.ReadUInt16() & AllBosses;
            if (index >= Main.maxPlayers) return;

            var player = Main.player[index].GetModPlayer<PhantomDragonProgressPlayer>();
            if (Main.netMode == NetmodeID.Server)
            {
                if (index != sender || player.importedClientSave) return;
                player.importedClientSave = true;
                player.defeatedBosses |= progress;
                player.SendProgress();
            }
            else if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                if (index == Main.myPlayer) player.defeatedBosses |= progress;
                else player.defeatedBosses = progress;
            }
        }

        internal static void RecordBossKill(NPC npc)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            int boss = npc.type == NPCID.MoonLordCore ? MoonLord : npc.ModNPC switch
            {
                ProfanedGuardianCommander _ => 1 << 0,
                Dragonfolly _ => 1 << 1,
                Providence _ => ProvidenceBoss,
                StormWeaverHead _ => 1 << 3,
                CeaselessVoid _ => 1 << 4,
                Signus _ => 1 << 5,
                Polterghast _ => 1 << 6,
                OldDuke _ => 1 << 7,
                DevourerofGodsHead _ => Devourer,
                Yharon _ => YharonBoss,
                _ => 0
            };
            if (boss == 0) return;

            foreach (Player participant in Main.ActivePlayers)
            {
                if (!npc.playerInteraction[participant.whoAmI]) continue;
                var progress = participant.GetModPlayer<PhantomDragonProgressPlayer>();
                if ((progress.defeatedBosses & boss) != 0) continue;

                progress.defeatedBosses |= boss;
                if (Main.netMode == NetmodeID.Server) progress.SendProgress();
            }
        }
    }

    public class PhantomDragonProgressGlobalNPC : GlobalNPC
    {
        public override void OnKill(NPC npc) => PhantomDragonProgressPlayer.RecordBossKill(npc);
    }
}
