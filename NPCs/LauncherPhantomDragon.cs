using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using CalamityMod;
using Terraria.DataStructures;
using CAmod.Projectiles;
using System;
using System.Collections.Generic;
using System.IO;
using CAmod.Players;
using CAmod.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CAmod.NPCs
{
    public abstract class LauncherPhantomDragonPart : ModNPC
    {
        public abstract int NativeType { get; }
        private int spacingTicks;
        private const int DefenseTransitionTicks = 18;
        private static readonly float DefenseLayerOpacity = 1f - MathF.Pow(0.75f, 0.25f);
        private int defenseBlendTicks;
        internal float MapOpacity => MathHelper.Lerp(1f, 0.25f, defenseBlendTicks / (float)DefenseTransitionTicks);
        internal ulong InvulnerableUntil;
        public override string Texture => $"CAmod/Assets/PhantomDragon/NPC_{NativeType}";
        public LauncherPhantomDragonHead Head
        {
            get
            {
                if (this is LauncherPhantomDragonHead head) return head;
                int index = (int)NPC.ai[3];
                return index >= 0 && index < Main.maxNPCs && Main.npc[index].active
                    ? Main.npc[index].ModNPC as LauncherPhantomDragonHead : null;
            }
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = Main.npcFrameCount[NativeType];
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type,
                new NPCID.Sets.NPCBestiaryDrawModifiers { Hide = true });
        }

        public override void SetDefaults()
        {
            NPC.CloneDefaults(NativeType);
            NPC.aiStyle = -1; // The real worm AI is explicitly invoked through the scoped adapter.
            AIType = -1;
            AnimationType = NativeType == NPCID.CultistDragonHead ? -1 : NativeType;
            NPC.lifeMax = 1000;
            NPC.damage = NPC.defDamage = 0;
            NPC.defense = NPC.defDefense = 0;
            NPC.friendly = true;
            NPC.boss = false;
            NPC.chaseable = false;
            NPC.dontCountMe = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.knockBackResist = 0f;
            NPC.value = 0;
            NPC.netAlways = true;
            NPC.realLife = -1;
            // Vanilla segments start fully transparent and depend on native drawing state.
            NPC.hide = false;
            NPC.alpha = 0;
            NPC.dontTakeDamage = false;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            var head = Head;
            if (head == null || head.Owner < 0 || head.Owner >= Main.maxPlayers
                || NPC.frame.Width <= 0 || NPC.frame.Height <= 0)
                return true;
            int dye = Main.player[head.Owner].GetModPlayer<CreatureLauncherPlayer>().DragonDye;


            var texture = TextureAssets.Npc[Type].Value;
            var frame = NPC.frame;
            var position = NPC.Center - screenPos + new Vector2(0f, NPC.gfxOffY);
            var origin = new Vector2(frame.Width * 0.5f, frame.Height * 0.5f);
            var effects = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            float blend = defenseBlendTicks / (float)DefenseTransitionTicks;
            float diagonal = 3f * 0.70710678f * blend;
            Color color = NPC.GetAlpha(drawColor) * MathHelper.Lerp(1f, DefenseLayerOpacity, blend);
            if (dye == 0)
            {
                DrawLayers();
                return false;
            }
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            try
            {
                DrawLayers();
            }
            finally
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            return false;

            void DrawLayers()
            {
                for (int layer = 0; layer < 4; layer++)
                {
                    var offset = new Vector2(
                        (layer & 1) == 0 ? -diagonal : diagonal,
                        (layer & 2) == 0 ? -diagonal : diagonal);
                    var data = new DrawData(texture, position + offset, frame, color,
                        NPC.rotation, origin, NPC.scale, effects);
                    if (dye != 0) GameShaders.Armor.Apply(dye, NPC, data);
                    data.Draw(spriteBatch);
                }
            }
        }

        public override bool CheckActive() => false;

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
            => this is LauncherPhantomDragonHead ? null : false;
        public override bool? CanBeHitByItem(Player player, Item item) => false;
        public override bool? CanBeHitByProjectile(Projectile projectile) => false;
        public override bool CanBeHitByNPC(NPC attacker) => false;
        public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
        {
            if (NativeType != NPCID.CultistDragonHead)
                modifiers.FinalDamage *= 0.1f;
        }

        public override void AI()
        {
            var head = Head;
            if (head == null || !head.Initialized)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient) NPC.active = false;
                return;
            }
            if (head.Owner < 0 || head.Owner >= Main.maxPlayers || !Main.player[head.Owner].active
                || Main.player[head.Owner].dead
                || !Main.player[head.Owner].GetModPlayer<CreatureLauncherPlayer>().DragonEquipped)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient) head.Destroy();
                return;
            }
            if (this is LauncherPhantomDragonHead && !head.RefreshTarget())
            {
                if (Main.netMode != NetmodeID.MultiplayerClient) head.Destroy();
                return;
            }
            NPC.realLife = head.NPC.whoAmI;
            NPC.target = head.Owner;
            NPC.timeLeft = 600;
            if (this is LauncherPhantomDragonHead)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient) head.UpdateVitality();
                if (!head.DefenseMode && head.TargetIndex >= 0)
                {
                    head.MoveHead(Main.npc[head.TargetIndex].Center);
                    if (Math.Abs(NPC.velocity.X) > 0.001f)
                        NPC.spriteDirection = NPC.velocity.X < 0f ? 1 : -1;
                }
                else
                {
                    head.ResetAttackMotion();
                    if (head.DefenseMode) head.ResetAttackSpeed();
                    Vector2 followOffset = head.ProxyPlayers[head.Owner].Center - NPC.Center;
                    if (NPC.velocity.LengthSquared() <= 0.01f)
                        NPC.velocity = followOffset.SafeNormalize(-Vector2.UnitY) * 7f;

                    PhantomDragonAISystem.Run(NPC, head);
                    if (!(NPC.velocity.LengthSquared() > 0.01f))
                        NPC.velocity = followOffset.SafeNormalize(-Vector2.UnitY) * 7f;
                }
                if (NPC.velocity.LengthSquared() > 0.01f)
                    NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;
            }
            else
            {
                int index = (int)NPC.ai[1];
                if (index < 0 || index >= Main.maxNPCs) return;
                NPC previous = Main.npc[index];
                if (!previous.active || previous.ModNPC is not LauncherPhantomDragonPart part || part.Head != head)
                    return;

                Vector2 forward = (previous.Center - NPC.Center).SafeNormalize(
                    (previous.rotation - MathHelper.PiOver2).ToRotationVector2());
                float angle = forward.ToRotation() + MathHelper.PiOver2;
                float from = head.NPC.velocity.LengthSquared() < 0.01f ? NPC.rotation : angle;
                NPC.rotation = from + MathHelper.WrapAngle(previous.rotation - from) * 0.08f;
                float spacing = 28f * MathHelper.SmoothStep(0f, 1f, spacingTicks / 30f);
                if (spacingTicks < 30) spacingTicks++;
                NPC.Center = previous.Center - (NPC.rotation - MathHelper.PiOver2).ToRotationVector2() * spacing;
                NPC.velocity = Vector2.Zero;
                NPC.spriteDirection = MathF.Cos(NPC.rotation - MathHelper.PiOver2) >= 0f ? -1 : 1;
            }
            NPC.friendly = true;
            NPC.damage = 0;
            defenseBlendTicks = Math.Clamp(defenseBlendTicks + (head.DefenseMode ? 1 : -1), 0, DefenseTransitionTicks);
            NPC.alpha = 0;
            NPC.dontTakeDamage = defenseBlendTicks != 0;
            if (Main.netMode != NetmodeID.MultiplayerClient && NPC.active && !NPC.dontTakeDamage)
                head.ReceiveHostileHits(NPC);
            if (this is LauncherPhantomDragonHead && head.Owner == Main.myPlayer
                && NPC.active && !head.DefenseMode)
                head.AttackEnemies();
        }

        // Death visuals are emitted once for the entire chain by the head.
        public override void HitEffect(NPC.HitInfo hit) { }
    }

    public class LauncherPhantomDragonHead : LauncherPhantomDragonPart
    {
        public const byte DeathPacket = 5;
        public const byte ContactPacket = 7;
        private float attackMoveSpeed = 7f;
        private int ticksSinceContact;
        private bool biting;

        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Y = (biting ? (int)(Main.GameUpdateCount / 4 % 2) : 2) * frameHeight;
        }

        public void ResetAttackSpeed()
        {
            attackMoveSpeed = 7f;
            ticksSinceContact = 0;
        }

        public bool RegisterContact(int targetIndex, int hitType)
        {
            if (DefenseMode || targetIndex < 0 || targetIndex != TargetIndex || hitType != targetType) return false;
            ticksSinceContact = 0;
            if (Main.netMode == NetmodeID.Server) NPC.netUpdate = true;
            return true;
        }

        public static void HandleContactPacket(BinaryReader reader, int sender)
        {
            int index = reader.ReadInt16();
            if (Main.netMode != NetmodeID.Server || index < 0 || index >= Main.maxNPCs
                || !Main.npc[index].active || Main.npc[index].ModNPC is not LauncherPhantomDragonHead dragon
                || dragon.Owner != sender) return;
            dragon.RegisterContact(reader.ReadInt16(), reader.ReadInt32());
        }
        public override int NativeType => NPCID.CultistDragonHead;
        public int Owner = -1, TargetIndex = -1;
        private int targetType = -1;
        public bool Initialized;
        private bool destroyed;
        private bool dismissed;
        private int regenCredit;
        private float attackSeconds, brake;
        private float turnSpeed = 6.5f, dashBoost = 1f;
        public bool DefenseMode => Owner >= 0 && Owner < Main.maxPlayers
            && Main.player[Owner].GetModPlayer<CreatureLauncherPlayer>().DragonDefense;

        internal static int GetMaxHealth(Player owner)
        {
            int max = Math.Max(1, owner.statLifeMax2 + owner.statManaMax2 + owner.statDefense * 5);
            var calamity = owner.Calamity();
            if (calamity.auricSet || calamity.demonshadeSet)
                max += max / 2;

            return max;
        }

        public void UpdateVitality()
        {
            int max = GetMaxHealth(Main.player[Owner]);
            if (NPC.lifeMax != max)
            {
                NPC.life = Math.Clamp(NPC.life + max - NPC.lifeMax, 1, max);
                NPC.lifeMax = max;
                NPC.netUpdate = true;
            }
            if (NPC.life >= max) { regenCredit = 0; return; }
            regenCredit += (1 + (max - NPC.life) / 20) * (DefenseMode ? 3 : 2);
            if (regenCredit < 120) return;
            NPC.life = Math.Min(max, NPC.life + regenCredit / 120);
            regenCredit %= 120;
            NPC.netUpdate = true;
        }

        public void ResetAttackMotion()
        {
            biting = false;
            attackSeconds = brake = 0f;
            turnSpeed = 6.5f;
            dashBoost = 1f;
        }

        // DoGHead.MoveHead, converted from Unity seconds/units to Terraria ticks/pixels.
        public void MoveHead(Vector2 target)
        {
            const float dt = 1f / 60f;
            attackSeconds += dt;
            dashBoost = Math.Max(1f, dashBoost - 2.5f * dt);
            float anger = MathHelper.Clamp(1f - NPC.life / (float)NPC.lifeMax, 0f, 1f);
            ticksSinceContact = Math.Min(60, ticksSinceContact + 1);
            attackMoveSpeed = MathHelper.Clamp(attackMoveSpeed + (ticksSinceContact >= 60 ? 2f * dt : -3f * dt), 7f, 20f);
            float speed = attackMoveSpeed;
            float baseTurn = Math.Min(7.5f, 5f + anger * 1.25f + attackSeconds * 0.4f);
            Vector2 offset = target - NPC.Center;
            Vector2 toTarget = offset.SafeNormalize(NPC.velocity.SafeNormalize(Vector2.UnitX));
            Vector2 current = NPC.velocity.SafeNormalize(toTarget);
            float heading = current.ToRotation();
            float angle = MathHelper.WrapAngle(toTarget.ToRotation() - heading);
            if (Math.Abs(angle) > MathHelper.ToRadians(179.9f))
                angle = MathHelper.ToRadians(179f) * (NPC.whoAmI % 2 == 0 ? 1 : -1);
            float degrees = MathHelper.ToDegrees(Math.Abs(angle));
            float currentSpeed = NPC.velocity.Length();
            if (currentSpeed < 0.001f) currentSpeed = speed;
            if (degrees <= 60f)
            {
                brake = 0f;
                currentSpeed += speed * dt;
                turnSpeed -= 9f * dt;
            }
            else
            {
                brake += 2f * dt;
                currentSpeed -= currentSpeed * brake * dt;
                turnSpeed += 9f * dt;
            }

            bool close = offset.LengthSquared() < (9.36f * 16f) * (9.36f * 16f);
            if (close) turnSpeed = baseTurn * 0.55f;
            currentSpeed = MathHelper.Clamp(currentSpeed, speed * 0.55f, speed * 2.5f);
            turnSpeed = MathHelper.Clamp(turnSpeed, baseTurn * 0.55f, 10f);
            float slowFactor = MathHelper.Clamp((speed - currentSpeed) / (speed * 0.25f), 0f, 1f);
            float boostRange = MathHelper.Lerp(9.36f * 16f, 500f, slowFactor);
            float boostAngle = MathHelper.Lerp(64f, 16f, slowFactor);
            bool chomp = offset.LengthSquared() < boostRange * boostRange && degrees < boostAngle;
            biting = chomp;
            if (chomp) dashBoost = 1.75f;
            float turnBonus = 2f * (speed - 7f) / (20f - 7f);
            float step = chomp ? 0f : (turnSpeed + turnBonus) * dt;
            NPC.velocity = (heading + MathHelper.Clamp(angle, -step, step)).ToRotationVector2()
                * currentSpeed * dashBoost;
        }
        private readonly Player proxy = new Player();
        public Player[] ProxyPlayers { get; private set; }
        private readonly ulong[] contactUntil = new ulong[Main.maxNPCs];
        private readonly int[] contactType = new int[Main.maxNPCs];
        private const int BaseSegmentCount = 30;
        private const int SegmentsPerLoop = 4;
        private const int MaxSegmentCount = BaseSegmentCount + 4 * SegmentsPerLoop;
        private readonly List<int> parts = new List<int>(MaxSegmentCount);

        internal static int GetSegmentCount(Player owner) => BaseSegmentCount
            + owner.GetModPlayer<PhantomDragonProgressPlayer>().ExtraLoops * SegmentsPerLoop;

        public bool Initialize(int owner, int target)
        {
            int free = 0;
            for (int i = 0; i < Main.maxNPCs; i++) if (!Main.npc[i].active) free++;
            int segmentCount = GetSegmentCount(Main.player[owner]);
            if (free < segmentCount) return false;
            Owner = owner;
            TargetIndex = target;
            targetType = target >= 0 ? Main.npc[target].type : -1;
            NPC.life = NPC.lifeMax = GetMaxHealth(Main.player[Owner]);
            Array.Clear(NPC.ai, 0, NPC.ai.Length);
            NPC.realLife = NPC.whoAmI;
            NPC.ai[3] = NPC.whoAmI;
            Vector2 forward = target >= 0 ? (Main.npc[target].Center - NPC.Center).SafeNormalize(Vector2.UnitX) : new Vector2(Main.player[owner].direction, 0f);
            int previous = NPC.whoAmI;
            for (int i = 0; i < segmentCount; i++)
            {
                int type = i == segmentCount - 3 ? ModContent.NPCType<LauncherPhantomDragonBody3>()
                    : i == segmentCount - 2 ? ModContent.NPCType<LauncherPhantomDragonBody4>()
                    : i == segmentCount - 1 ? ModContent.NPCType<LauncherPhantomDragonTail>()
                    : (i - 2) % SegmentsPerLoop == 0 && i < segmentCount - 4 ? ModContent.NPCType<LauncherPhantomDragonBody1>()
                    : ModContent.NPCType<LauncherPhantomDragonBody2>();
                int index = Terraria.NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X,
                    (int)NPC.Center.Y, type, Start: NPC.whoAmI,
                    ai1: previous, ai3: NPC.whoAmI, Target: owner);
                if (index >= Main.maxNPCs) { Destroy(); return false; }
                NPC part = Main.npc[index];
                part.Center = NPC.Center;
                part.rotation = forward.ToRotation() + MathHelper.PiOver2;
                part.realLife = NPC.whoAmI;
                part.life = part.lifeMax = NPC.lifeMax;
                Main.npc[previous].ai[0] = index;
                Main.npc[previous].netUpdate = true;
                part.netUpdate = true;
                parts.Add(index);
                previous = index;
            }
            Initialized = true;
            RefreshTarget();
            NPC.netUpdate = true;
            return true;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write((short)Owner);
            writer.Write((short)TargetIndex);
            writer.Write(targetType);
            writer.Write(Initialized);
            writer.Write(NPC.lifeMax);
            writer.Write(NPC.life);
            writer.Write(attackMoveSpeed);
            writer.Write((byte)ticksSinceContact);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Owner = reader.ReadInt16();
            TargetIndex = reader.ReadInt16();
            targetType = reader.ReadInt32();
            Initialized = reader.ReadBoolean();
            NPC.lifeMax = reader.ReadInt32();
            NPC.life = reader.ReadInt32();
            attackMoveSpeed = reader.ReadSingle();
            ticksSinceContact = reader.ReadByte();
            RefreshTarget();
        }

        public bool RefreshTarget()
        {
            if (Owner < 0 || Owner >= Main.maxPlayers) return false;
            Player owner = Main.player[Owner];
            const float playerRangeSquared = 3000f * 3000f;
            const float dragonRangeSquared = 1500f * 1500f;
            bool defenseMode = DefenseMode;
            NPC enemy = null;
            if (!defenseMode && TargetIndex >= 0 && TargetIndex < Main.maxNPCs)
            {
                NPC current = Main.npc[TargetIndex];
                if (current.type == targetType && CanStrikeTarget(current)
                    && Vector2.DistanceSquared(current.Center, owner.Center) < playerRangeSquared
                    && Vector2.DistanceSquared(current.Center, NPC.Center) <= dragonRangeSquared)
                    enemy = current;
            }

            if (!defenseMode && enemy == null)
            {
                bool bossActive = false;
                foreach (NPC npc in Main.ActiveNPCs)
                {
                    if (!npc.boss || npc.friendly || npc.life <= 0) continue;
                    bossActive = true;
                    break;
                }

                float bestDamage = -1f;
                float closest = float.MaxValue;
                float nominalDamage = bossActive
                    ? Math.Max(1f, owner.GetTotalDamage(DamageClass.Magic).ApplyTo(MathF.Round(1000f * GetDamageMultiplier(owner))))
                    : 0f;
                float armorPenetration = bossActive ? owner.GetTotalArmorPenetration(DamageClass.Magic) : 0f;
                foreach (NPC candidate in Main.ActiveNPCs)
                {
                    if (!CanStrikeTarget(candidate) || (bossActive && candidate.lifeMax <= 10000)) continue;
                    float playerDistance = Vector2.DistanceSquared(candidate.Center, owner.Center);
                    float dragonDistance = Vector2.DistanceSquared(candidate.Center, NPC.Center);
                    if (playerDistance >= playerRangeSquared || dragonDistance > dragonRangeSquared) continue;

                    float distance = bossActive ? dragonDistance : playerDistance;
                    float damage = 0f;
                    if (bossActive)
                    {
                        float defense = Math.Max(0f, candidate.defense - armorPenetration);
                        damage = Math.Max(1f, nominalDamage - defense * 0.5f)
                            * (1f - MathHelper.Clamp(candidate.Calamity().DR, 0f, 1f));
                    }
                    if (damage < bestDamage || (damage == bestDamage && distance >= closest)) continue;
                    bestDamage = damage;
                    closest = distance;
                    enemy = candidate;
                }
            }

            int nextIndex = enemy?.whoAmI ?? -1;
            int nextType = enemy?.type ?? -1;
            if ((TargetIndex != nextIndex || targetType != nextType) && Main.netMode == NetmodeID.Server)
                NPC.netUpdate = true;
            TargetIndex = nextIndex;
            targetType = nextType;
            proxy.active = true;
            proxy.dead = false;
            proxy.width = enemy?.width ?? owner.width;
            proxy.height = enemy?.height ?? owner.height;
            proxy.Center = enemy?.Center ?? (owner.Center - Vector2.UnitY * 50f);
            proxy.velocity = enemy?.velocity ?? owner.velocity;
            ProxyPlayers ??= new Player[Main.player.Length];
            for (int i = 0; i < ProxyPlayers.Length; i++) ProxyPlayers[i] = proxy;
            return true;
        }

        public void ReceiveHostileHits(NPC struckPart)
        {
            if (struckPart.ModNPC is not LauncherPhantomDragonPart part
                || Main.GameUpdateCount < part.InvulnerableUntil) return;
            foreach (Projectile hostile in Main.ActiveProjectiles)
            {
                if (!hostile.hostile || hostile.damage <= 0) continue;
                if (hostile.type == PhantomDragonAISystem.NebulaSphereType
                    || hostile.type == ModContent.ProjectileType<CalamityMod.Projectiles.Boss.InfernadoRevenge>()) continue;
                if (!hostile.Colliding(hostile.Hitbox, struckPart.Hitbox)) continue;
                part.InvulnerableUntil = Main.GameUpdateCount + 40;
                var hit = struckPart.CalculateHitInfo(hostile.damage, hostile.velocity.X >= 0 ? 1 : -1,
                    crit: false, knockBack: 0f, damageType: DamageClass.Generic, damageVariation: false);
                // Body ModifyIncomingHit supplies 90% DR; realLife redirects damage to the head.
                struckPart.realLife = NPC.whoAmI;
                struckPart.StrikeNPC(hit);
                // The strike identifies the struck segment; the health snapshot belongs to the head.
                if (NPC.active) NPC.netUpdate = true;
                if (Main.netMode == NetmodeID.Server) NetMessage.SendStrikeNPC(struckPart, in hit);

                if (hostile.active && hostile.penetrate > 0 && --hostile.penetrate == 0)
                    hostile.Kill();
                return;
            }
        }

        public static bool CanStrikeTarget(NPC enemy)
        {
            if (!enemy.active || enemy.friendly || enemy.life <= 0 || enemy.dontTakeDamage || enemy.immortal)
                return false;

            // Worm segments can be damageable even when excluded from minion targeting.
            int rootIndex = enemy.realLife;
            return enemy.CanBeChasedBy() || (rootIndex >= 0 && rootIndex < Main.maxNPCs
                && rootIndex != enemy.whoAmI && Main.npc[rootIndex].active
                && !Main.npc[rootIndex].friendly && Main.npc[rootIndex].life > 0);
        }

        private static float GetDamageMultiplier(Player owner)
        {
            var calamity = owner.Calamity();
            if (calamity.auricSet || calamity.demonshadeSet) return 2f;
            return MathHelper.Lerp(2f / 3f, 2f, owner.GetModPlayer<PhantomDragonProgressPlayer>().DamageProgress);
        }

        internal static (int Head, int Body) GetAttackDamage(Player owner)
        {
            float multiplier = GetDamageMultiplier(owner);
            var magicDamage = owner.GetTotalDamage(DamageClass.Magic);
            int head = Math.Max(1, (int)magicDamage.ApplyTo(MathF.Round(1000f * multiplier)));
            int body = Math.Max(1, (int)magicDamage.ApplyTo(MathF.Round(250f * multiplier)));
            return (head, body);
        }

        public void AttackEnemies()
        {
            Player owner = Main.player[Owner];
            if (DefenseMode || NPC.dontTakeDamage || owner.GetModPlayer<DimGatePlayer>().gateTime > 0) return;
            var (headDamage, bodyDamage) = GetAttackDamage(owner);
            foreach (NPC enemy in Main.ActiveNPCs)
            {
                if (!CanStrikeTarget(enemy)) continue;
                // Each NPC (including individual worm segments) has its own dragon cooldown.
                int key = enemy.whoAmI;
                if (contactType[key] == enemy.type && Main.GameUpdateCount < contactUntil[key]) continue;
                int damage = NPC.Hitbox.Intersects(enemy.Hitbox) ? headDamage : 0;
                if (damage == 0)
                    foreach (NPC part in Main.ActiveNPCs)
                        if (part.ModNPC is LauncherPhantomDragonPart && part.realLife == NPC.whoAmI
                            && !part.dontTakeDamage && part.Hitbox.Intersects(enemy.Hitbox))
                        { damage = bodyDamage; break; }
                if (damage == 0) continue;
                contactType[key] = enemy.type;
                contactUntil[key] = Main.GameUpdateCount + 10;
                int index = Projectile.NewProjectile(owner.GetSource_FromThis(), enemy.Center,
                    Vector2.Zero, ModContent.ProjectileType<PhantomDragonContactHit>(),
                    damage, 0f, Owner, enemy.whoAmI, enemy.type, NPC.whoAmI);
                if (index < Main.maxProjectiles)
                {
                    Projectile projectile = Main.projectile[index];
                    projectile.CritChance = (int)owner.GetTotalCritChance(DamageClass.Magic);
                    projectile.ArmorPenetration = (int)owner.GetTotalArmorPenetration(DamageClass.Magic);
                    projectile.direction = NPC.velocity.X >= 0 ? 1 : -1;
                }
            }
        }

        public void Destroy()
        {
            if (destroyed || Main.netMode == NetmodeID.MultiplayerClient) return;
            dismissed = true;
            NPC.life = 0;
            NPC.checkDead();
            if (NPC.active) { OnKill(); NPC.active = false; }
            if (Main.netMode == NetmodeID.Server) NetMessage.SendData(MessageID.SyncNPC, number: NPC.whoAmI);
        }

        public override void OnKill()
        {
            if (destroyed || Main.netMode == NetmodeID.MultiplayerClient) return;
            destroyed = true;
            int fallenOwner = Initialized && !dismissed ? Owner : -1;
            if (fallenOwner >= 0 && fallenOwner < Main.maxPlayers)
                Main.player[fallenOwner].GetModPlayer<CreatureLauncherPlayer>().DragonDeathPending = true;
            var chain = new List<NPC> { NPC };
            foreach (NPC part in Main.ActiveNPCs)
                if (part.whoAmI != NPC.whoAmI && part.realLife == NPC.whoAmI
                    && part.ModNPC is LauncherPhantomDragonPart) chain.Add(part);
            if (Main.netMode == NetmodeID.Server)
            {
                ModPacket packet = Mod.GetPacket();
                packet.Write(DeathPacket);
                packet.Write((short)fallenOwner);
                packet.Write((byte)chain.Count);
                foreach (NPC part in chain) { packet.Write(part.Center.X); packet.Write(part.Center.Y); }
                packet.Send();
            }
            else
            {
                foreach (NPC part in chain) DeathDust(part.Center);
                KillOwner(fallenOwner);
            }
            foreach (NPC part in chain)
            {
                part.life = 0;
                part.active = false;
                if (Main.netMode == NetmodeID.Server)
                    NetMessage.SendData(MessageID.SyncNPC, number: part.whoAmI);
            }
        }

        public static void HandleDeathPacket(BinaryReader reader)
        {
            int owner = reader.ReadInt16();
            int count = reader.ReadByte();
            if (Main.netMode == NetmodeID.MultiplayerClient && owner == Main.myPlayer) KillOwner(owner);
            for (int i = 0; i < count; i++)
            {
                Vector2 center = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                if (Main.netMode == NetmodeID.MultiplayerClient && i < MaxSegmentCount + 1) DeathDust(center);
            }
        }
        private static void KillOwner(int index)
        {
            if (index < 0 || index >= Main.maxPlayers) return;
            Player owner = Main.player[index];
            if (!owner.active || owner.dead) return;
            PhantomDragonDeathSystem.KillOwner(owner);
        }

        private static void DeathDust(Vector2 center)
        {
            if (Main.dedServ) return;
            for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustPerfect(center + Main.rand.NextVector2Circular(10, 10), DustID.BlueTorch,
                    Main.rand.NextVector2Circular(3, 3), 80, new Color(120, 210, 255), 1.5f);
                dust.noGravity = true;
            }
        }
    }

    public class LauncherPhantomDragonBody1 : LauncherPhantomDragonPart { public override int NativeType => NPCID.CultistDragonBody1; }
    public class LauncherPhantomDragonBody2 : LauncherPhantomDragonPart { public override int NativeType => NPCID.CultistDragonBody2; }
    public class LauncherPhantomDragonBody3 : LauncherPhantomDragonPart { public override int NativeType => NPCID.CultistDragonBody3; }
    public class LauncherPhantomDragonBody4 : LauncherPhantomDragonPart { public override int NativeType => NPCID.CultistDragonBody4; }
    public class LauncherPhantomDragonTail : LauncherPhantomDragonPart { public override int NativeType => NPCID.CultistDragonTail; }
}
