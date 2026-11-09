using System;
using CAmod.Players;
using Mono.Cecil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace CAmod.Systems
{
    public sealed class PhantomDragonDeathSystem : ModSystem
    {
        [ThreadStatic] private static Player forcedOwner;
        private ILHook hook;
        private delegate bool PreKillCall(Player player, double damage, int direction, bool pvp,
            ref bool playSound, ref bool genGore, ref PlayerDeathReason reason);

        public override void Load()
        {
            hook = new ILHook(typeof(Player).GetMethod(nameof(Player.KillMe)),
                il =>
                {
                    var cursor = new ILCursor(il);
                    if (!cursor.TryGotoNext(MoveType.Before, i => i.Operand is MethodReference method
                        && method.Name == nameof(PlayerLoader.PreKill)
                        && method.DeclaringType.FullName == typeof(PlayerLoader).FullName))
                        throw new InvalidOperationException("Player.KillMe PreKill call not found.");
                    cursor.Remove();
                    cursor.EmitDelegate<PreKillCall>(CheckPreKill);
                });
        }

        public override void Unload()
        {
            hook?.Dispose();
            hook = null;
        }

        private static bool CheckPreKill(Player player, double damage, int direction, bool pvp,
            ref bool playSound, ref bool genGore, ref PlayerDeathReason reason)
        {
            if (ReferenceEquals(player, forcedOwner)) return true;
            return PlayerLoader.PreKill(player, damage, direction, pvp, ref playSound, ref genGore, ref reason);
        }

        public static void KillOwner(Player owner)
        {
            Player previous = forcedOwner;
            forcedOwner = owner;
            try
            {
                owner.KillMe(PlayerDeathReason.ByCustomReason(owner.name + "의 팬텀 드래곤이 쓰러졌습니다."),
                    Math.Max(1, owner.statLife), 0);
            }
            finally { forcedOwner = previous; }
        }
    }
}
