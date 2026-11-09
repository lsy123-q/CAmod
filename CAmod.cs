using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CAmod.Players;

namespace CAmod
{
	// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
	public class CAmod : Mod
	{

		public override void HandlePacket(System.IO.BinaryReader reader, int whoAmI)
		{
			byte messageType = reader.ReadByte();
            if (messageType == global::CAmod.NPCs.LauncherPhantomDragonHead.ContactPacket)
            {
                global::CAmod.NPCs.LauncherPhantomDragonHead.HandleContactPacket(reader, whoAmI);
                return;
            }
            if (messageType == PhantomDragonProgressPlayer.ProgressPacket)
            {
                PhantomDragonProgressPlayer.HandleProgress(reader, whoAmI);
                return;
            }
            if (messageType == CreatureLauncherPlayer.DragonModePacket)
            {
                CreatureLauncherPlayer.HandleDragonMode(reader, whoAmI);
                return;
            }
            if (messageType == global::CAmod.NPCs.LauncherPhantomDragonHead.DeathPacket)
            {
                global::CAmod.NPCs.LauncherPhantomDragonHead.HandleDeathPacket(reader);
                return;
            }
            if (messageType == global::CAmod.Players.CreatureLauncherPlayer.TargetPacket)
            {
                global::CAmod.Players.CreatureLauncherPlayer.HandlePacket(reader, whoAmI);
                return;
            }
            if (messageType == global::CAmod.NPCs.LauncherCreatureNPC.DeathPacket)
            {
                global::CAmod.NPCs.LauncherCreatureNPC.HandleDeathPacket(reader);
                return;
            }
            if (messageType == global::CAmod.Systems.PyromancerBrandSoundSystem.StrikePacket)
            {
                global::CAmod.Systems.PyromancerBrandSoundSystem.HandlePacket(reader, whoAmI);
                return;
            }
            if (messageType == global::CAmod.Globals.PyromancerBrandGlobalNPC.SyncPacket)
            {
                global::CAmod.Globals.PyromancerBrandGlobalNPC.HandlePacket(reader, whoAmI);
                return;
            }

		}
	}
}
