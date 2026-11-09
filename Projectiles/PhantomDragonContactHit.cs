using CalamityMod.NPCs.StormWeaver;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;

namespace CAmod.Projectiles
{
    public class PhantomDragonContactHit : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_1";

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 2;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 2;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hide = true;
            Projectile.usesLocalNPCImmunity = true;
            // -2 skips both local timer writes and clearing NPC.immune in Projectile.Damage.
            // The one-hit projectile and the dragon's per-NPC timer handle repeat hits.
            Projectile.localNPCHitCooldown = -2;
        }

        public override bool? CanHitNPC(NPC target)
        {
            int index = (int)Projectile.ai[2];
            if (index < 0 || index >= Main.maxNPCs || !Main.npc[index].active
                || Main.npc[index].ModNPC is not NPCs.LauncherPhantomDragonHead dragon
                || dragon.Owner != Projectile.owner || dragon.DefenseMode || dragon.NPC.dontTakeDamage
                || target.whoAmI != (int)Projectile.ai[0] || target.type != (int)Projectile.ai[1]
                || !NPCs.LauncherPhantomDragonHead.CanStrikeTarget(target))
                return false;
            return null;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (target.ModNPC is (StormWeaverHead or StormWeaverBody or StormWeaverTail)
                && ModLoader.TryGetMod("InfernumMode", out Mod infernum)
                && infernum.Call("GetInfernumActive") is true)
                modifiers.FinalDamage *= 1f / 3f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            int index = (int)Projectile.ai[2];
            if (damageDone <= 0 || Projectile.owner != Main.myPlayer || index < 0 || index >= Main.maxNPCs
                || !Main.npc[index].active || Main.npc[index].ModNPC is not NPCs.LauncherPhantomDragonHead dragon
                || dragon.Owner != Projectile.owner) return;
            Player owner = Main.player[Projectile.owner];
            owner.GetModPlayer<Players.BossFightTimerPlayer>().RecordPhantomDragonDamage(damageDone);
            owner.GetModPlayer<Players.CreatureLauncherPlayer>()
                .AddDragonDamage(Projectile, target, damageDone);
            if (!dragon.RegisterContact(target.whoAmI, (int)Projectile.ai[1])) return;
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                var packet = Mod.GetPacket();
                packet.Write(NPCs.LauncherPhantomDragonHead.ContactPacket);
                packet.Write((short)index);
                packet.Write((short)target.whoAmI);
                packet.Write((int)Projectile.ai[1]);
                packet.Send();
            }
        }

        public override void AI()
        {
            int index = (int)Projectile.ai[0];
            if (index < 0 || index >= Main.maxNPCs || !Main.npc[index].active
                || Main.npc[index].type != (int)Projectile.ai[1])
            {
                Projectile.Kill();
                return;
            }
            Projectile.Center = Main.npc[index].Center;
        }
    }
}
