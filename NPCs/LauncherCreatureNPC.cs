using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CAmod.NPCs
{
    public abstract class LauncherCreatureNPC : ModNPC
    {
        public const byte DeathPacket = 4;
        protected abstract int Kind { get; }
        protected virtual int VisualType => Kind == 1 ? NPCID.BeeSmall
            : Kind == 2 ? NPCID.PlanterasTentacle : NPCID.Sharkron;
        private bool finished;
        private bool playedDeathEffects;
        private int Owner => (int)NPC.ai[1];
        private int PayloadDamage => Kind == 1 ? 25 : Kind == 2 ? 625 : 1250;
        private float Speed => Kind == 0 ? 12f : Kind == 1 ? 14f : Kind == 2 ? 16f : 18f;
        public override string Texture => Kind == 0 ? $"Terraria/Images/Projectile_{ProjectileID.BeeHive}"
            : $"Terraria/Images/NPC_{VisualType}";

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = Kind == 0 ? 1 : Main.npcFrameCount[VisualType];
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type,
                new NPCID.Sets.NPCBestiaryDrawModifiers { Hide = true });
        }

        public override void SetDefaults()
        {
            NPC.width = Kind == 1 ? 16 : Kind == 2 ? 24 : Kind == 3 ? 60 : 26;
            NPC.height = Kind == 1 ? 14 : Kind == 3 ? 32 : 28;
            NPC.lifeMax = 1;
            NPC.damage = 0;
            NPC.defense = 0;
            NPC.friendly = true;
            NPC.chaseable = false;
            NPC.dontCountMe = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.knockBackResist = 0f;
            NPC.aiStyle = -1;
            NPC.netAlways = true;
            NPC.value = 0f;
        }

        // Intercept manually below: any damaging hostile projectile kills one unit.
        // Friendly attacks must not kill their own launcher's creatures.
        public override bool? CanBeHitByProjectile(Projectile projectile) => false;
        public override bool? CanBeHitByItem(Player player, Item item) => false;
        public override bool CanBeHitByNPC(NPC attacker) => false;
        public override bool CheckActive() => false;

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += 0.25;
            NPC.frame.Y = (int)NPC.frameCounter % Main.npcFrameCount[Type] * frameHeight;
        }

        private NPC GetTarget()
        {
            int index = (int)NPC.ai[0];
            if (index >= 0 && index < Main.maxNPCs && Main.npc[index].CanBeChasedBy())
                return Main.npc[index];
            NPC best = null;
            float distance = 1200f * 1200f;
            foreach (NPC candidate in Main.ActiveNPCs)
            {
                if (!candidate.CanBeChasedBy()) continue;
                float current = Vector2.DistanceSquared(candidate.Center, NPC.Center);
                if (current < distance) { distance = current; best = candidate; }
            }
            if (best != null) NPC.ai[0] = best.whoAmI;
            return best;
        }

        public override void AI()
        {
            NPC.timeLeft = 60;
            NPC target = GetTarget();
            if (target != null)
            {
                Vector2 desired = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX) * Speed;
                NPC.velocity = NPC.ai[3] == 0f && Kind != 1 ? desired
                    : (NPC.velocity * (Kind == 1 ? 12f : 6f) + desired) / (Kind == 1 ? 13f : 7f);
            }
            NPC.spriteDirection = NPC.velocity.X >= 0f ? 1 : -1;
            NPC.rotation = Kind == 0 ? NPC.rotation + 0.12f
                : NPC.velocity.ToRotation() + (NPC.spriteDirection == -1 ? MathHelper.Pi : 0f);
            NPC.ai[3]++;
            if (Main.netMode == NetmodeID.MultiplayerClient || finished) return;
            if (Owner < 0 || Owner >= Main.maxPlayers || !Main.player[Owner].active || Main.player[Owner].dead
                || NPC.ai[3] >= (Kind == 1 ? 180 : 300)) { Die(); return; }

            foreach (Projectile hostile in Main.ActiveProjectiles)
            {
                if (hostile.hostile && hostile.damage > 0 && hostile.Colliding(hostile.Hitbox, NPC.Hitbox))
                {
                    Die();
                    return;
                }
            }
            // Check all enemies, so a different monster can intercept a homing unit.
            foreach (NPC enemy in Main.ActiveNPCs)
            {
                if (!enemy.CanBeChasedBy() || !NPC.Hitbox.Intersects(enemy.Hitbox)) continue;
                if (Kind != 0)
                {
                    var hit = enemy.CalculateHitInfo(PayloadDamage, NPC.velocity.X >= 0 ? 1 : -1,
                        crit: false, knockBack: 0f, damageType: DamageClass.Generic, damageVariation: false);
                    enemy.PlayerInteraction(Owner);
                    int damage = enemy.StrikeNPC(hit);
                    if (Main.netMode == NetmodeID.Server) NetMessage.SendStrikeNPC(enemy, ref hit);
                    else if (damage > 0) Main.player[Owner].addDPS(damage);
                }
                Die();
                return;
            }
        }

        private void Die()
        {
            if (finished) return;
            finished = true;
            if (Main.netMode == NetmodeID.Server)
            {
                ModPacket packet = Mod.GetPacket();
                packet.Write(DeathPacket);
                packet.Write((byte)Kind);
                packet.Write(NPC.Center.X); packet.Write(NPC.Center.Y);
                packet.Write(NPC.velocity.X); packet.Write(NPC.velocity.Y);
                packet.Send();
            }
            else PlayDeathEffects(Kind, NPC.Center, NPC.velocity);
            NPC.life = 0;
            NPC.checkDead();
            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.SyncNPC, number: NPC.whoAmI);
        }

        public override void OnKill()
        {
            if (Kind != 0 || Main.netMode == NetmodeID.MultiplayerClient || Owner < 0 || Owner >= Main.maxPlayers
                || !Main.player[Owner].active || Main.player[Owner].dead) return;
            for (int i = 0; i < 10; i++)
            {
                int index = Terraria.NPC.NewNPC(NPC.GetSource_Death(), (int)NPC.Center.X,
                    (int)NPC.Center.Y, ModContent.NPCType<LauncherBeeNPC>(), ai0: NPC.ai[0], ai1: Owner);
                if (index >= Main.maxNPCs) break;
                Main.npc[index].Center = NPC.Center;
                Main.npc[index].velocity = (MathHelper.TwoPi * i / 10f).ToRotationVector2() * 6f;
                Main.npc[index].netUpdate = true;
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0 && !finished && !playedDeathEffects && !Main.dedServ)
            {
                playedDeathEffects = true;
                PlayDeathEffects(Kind, NPC.Center, NPC.velocity);
            }
        }

        public static void HandleDeathPacket(BinaryReader reader)
        {
            int kind = reader.ReadByte();
            Vector2 center = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            Vector2 velocity = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            if (Main.netMode == NetmodeID.MultiplayerClient && kind <= 3)
                PlayDeathEffects(kind, center, velocity);
        }

        private static void PlayDeathEffects(int kind, Vector2 center, Vector2 velocity)
        {
            if (Main.dedServ) return;
            SoundEngine.PlaySound(SoundID.NPCDeath1, center);
            int dustType = kind <= 1 ? DustID.Honey : kind == 2 ? DustID.Grass : DustID.Blood;
            for (int i = 0; i < (kind == 1 ? 5 : 16); i++)
            {
                Dust dust = Dust.NewDustPerfect(center + Main.rand.NextVector2Circular(8f, 8f), dustType,
                    velocity * 0.15f + Main.rand.NextVector2Circular(3f, 3f));
                dust.scale = kind == 1 ? 0.7f : 1f;
            }
            // Vanilla Plantera tentacle and Sharkron gore IDs, read from NPC.VanillaHitEffect.
            int[] gores = kind == 2 ? new[] { 388, 389 }
                : kind == 3 ? new[] { 583, 577, 578, 579 }
                : kind == 1 ? new[] { ModContent.GoreType<LauncherBeeGore>() }
                : new[] { GoreID.HoneyDrip, GoreID.HoneyDrip, GoreID.HoneyDrip };
            foreach (int gore in gores)
                Gore.NewGore(new EntitySource_Misc("CreatureLauncherDeath"), center,
                    velocity * 0.25f + Main.rand.NextVector2Circular(2f, 2f), gore, kind == 1 ? 0.65f : 1f);
        }
    }

    public class LauncherHiveNPC : LauncherCreatureNPC { protected override int Kind => 0; }
    public class LauncherBeeNPC : LauncherCreatureNPC { protected override int Kind => 1; }
    public class LauncherTentacleNPC : LauncherCreatureNPC { protected override int Kind => 2; }
    public class LauncherSharkNPC : LauncherCreatureNPC { protected override int Kind => 3; }

    public class LauncherBeeGore : ModGore
    {
        public override string Texture => $"Terraria/Images/NPC_{NPCID.BeeSmall}";
        public override void OnSpawn(Gore gore, IEntitySource source)
        {
            gore.Frame = new SpriteFrame(1, (byte)Main.npcFrameCount[NPCID.BeeSmall], 0, 0);
            gore.timeLeft = 45;
        }
    }
}
