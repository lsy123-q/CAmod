using System;
using System.Reflection;
using CAmod.NPCs;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CAmod.Systems
{
    // Reuse the installed vanilla worm AI rather than approximating its steering.
    // Adapters are active only inside our dragon's scoped AI call.
    public sealed class PhantomDragonAISystem : ModSystem
    {
        private ILHook wormHook;
        private static Action<NPC> runWormAI;
        [ThreadStatic] private static LauncherPhantomDragonHead activeHead;

        public override void Load()
        {
            var method = typeof(NPC).GetMethod("AI_006_Worms", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null) throw new MissingMethodException("NPC.AI_006_Worms");
            wormHook = new ILHook(method, AdaptDragonAI);
            runWormAI = (Action<NPC>)method.CreateDelegate(typeof(Action<NPC>));
        }

        internal static int NebulaSphereType { get; private set; } = -1;

        public override void PostSetupContent()
        {
            NebulaSphereType = -1;
            try
            {
                if (!ModLoader.TryGetMod("CatalystMod", out Mod catalyst)) return;

                Type expectedType = catalyst.Code?.GetType("CatalystMod.Projectiles.Enemy.NebulaSphere", throwOnError: false);
                if (expectedType != null && catalyst.TryFind<ModProjectile>("NebulaSphere", out var projectile)
                    && projectile.GetType() == expectedType)
                    NebulaSphereType = projectile.Type;
            }
            catch (Exception exception)
            {
                Mod.Logger.Warn("Could not resolve CatalystMod's NebulaSphere for Phantom Dragon.", exception);
            }
        }

        public override void Unload()
        {
            NebulaSphereType = -1;
            wormHook?.Dispose();
            wormHook = null;
            runWormAI = null;
            activeHead = null;
        }

        public static void Run(NPC part, LauncherPhantomDragonHead head)
        {
            var previous = activeHead;
            activeHead = head;
            try { runWormAI(part); }
            finally { activeHead = previous; }
        }

        private static int MapType(int type)
        {
            if (activeHead == null) return type;
            if (type == ModContent.NPCType<LauncherPhantomDragonHead>()) return NPCID.CultistDragonHead;
            if (type == ModContent.NPCType<LauncherPhantomDragonBody1>()) return NPCID.CultistDragonBody1;
            if (type == ModContent.NPCType<LauncherPhantomDragonBody2>()) return NPCID.CultistDragonBody2;
            if (type == ModContent.NPCType<LauncherPhantomDragonBody3>()) return NPCID.CultistDragonBody3;
            if (type == ModContent.NPCType<LauncherPhantomDragonBody4>()) return NPCID.CultistDragonBody4;
            if (type == ModContent.NPCType<LauncherPhantomDragonTail>()) return NPCID.CultistDragonTail;
            return type;
        }

        private static void AdaptDragonAI(ILContext il)
        {
            var cursor = new ILCursor(il);
            int types = 0, players = 0, targets = 0;
            while (cursor.TryGotoNext(MoveType.After, i => i.MatchLdfld<NPC>(nameof(NPC.type))))
            {
                cursor.EmitDelegate<Func<int, int>>(MapType);
                types++;
            }
            cursor.Index = 0;
            while (cursor.TryGotoNext(MoveType.After, i => i.MatchLdsfld<Main>(nameof(Main.player))))
            {
                cursor.EmitDelegate<Func<Player[], Player[]>>(
                    original => activeHead?.ProxyPlayers ?? original);
                players++;
            }
            cursor.Index = 0;
            while (cursor.TryGotoNext(MoveType.Before, i => i.MatchCall<NPC>(nameof(NPC.TargetClosest))))
            {
                cursor.Remove();
                cursor.EmitDelegate<Action<NPC, bool>>((npc, faceTarget) =>
                {
                    if (activeHead == null) { npc.TargetClosest(faceTarget); return; }
                    npc.target = activeHead.Owner;
                    if (!faceTarget) return;
                    var center = activeHead.ProxyPlayers[npc.target].Center;
                    npc.direction = center.X >= npc.Center.X ? 1 : -1;
                    npc.directionY = center.Y >= npc.Center.Y ? 1 : -1;
                });
                targets++;
            }
            if (types == 0 || players == 0 || targets == 0)
                throw new InvalidOperationException("Vanilla worm AI layout changed; dragon target adapters could not be installed.");
        }
    }
}
