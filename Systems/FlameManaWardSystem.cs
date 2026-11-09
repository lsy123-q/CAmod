using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CAmod.Players;
using CalamityMod.CalPlayer;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Terraria;
using Terraria.ModLoader;

namespace CAmod.Systems
{
    public sealed class FlameManaWardSystem : ModSystem
    {
        private ILHook calamityHook;
        private bool enabled;
        private static readonly HashSet<string> FireFlags = new()
        {
            "weakBrimstoneFlames", "brimstoneFlames", "searingLava", "demonicFlames",
            "daybroken", "holyFlames", "godSlayerInferno", "dragonFire", "vHex", "trueVHex", "banishingFire"
        };
        private static readonly HashSet<string> VanillaFireFlags = new() { "onFire", "onFire2", "onFire3", "burned" };

        public override void Load()
        {
            try
            {
                calamityHook = new ILHook(typeof(CalamityPlayer).GetMethod(nameof(CalamityPlayer.UpdateBadLifeRegen)), PatchRegen);
                IL_Player.UpdateLifeRegen += PatchVanilla;
                enabled = true;
            }
            catch (Exception exception)
            {
                IL_Player.UpdateLifeRegen -= PatchVanilla;
                calamityHook?.Dispose();
                calamityHook = null;
                Mod.Logger.Warn("Flame Mana Ward: regeneration layout changed; source conversion disabled.", exception);
            }
        }

        public override void Unload()
        {
            if (enabled) IL_Player.UpdateLifeRegen -= PatchVanilla;
            calamityHook?.Dispose();
            calamityHook = null;
            enabled = false;
        }

        private static FlameManaWardPlayer Ward(Player player) => player.GetModPlayer<FlameManaWardPlayer>();
        private static FlameManaWardPlayer Ward(CalamityPlayer cal) => Ward(cal.Player);
        private static bool Field(Instruction i, string name) => i.OpCode == OpCodes.Ldfld && i.Operand is FieldReference f && f.Name == name;
        private static bool Call(Instruction i, string name) => i.Operand is MethodReference m && m.Name == name;

        private static void PatchVanilla(ILContext il)
        {
            var instructions = il.Body.Instructions.ToArray();
            int badRegenIndex = Array.FindIndex(instructions, i => Call(i, "UpdateBadLifeRegen"));
            if (badRegenIndex < 0) throw new InvalidOperationException("Missing vanilla regeneration boundary.");
            var sources = new List<(Instruction flag, int amount, bool burning)>();
            for (int index = 0; index < badRegenIndex; index++)
            {
                Instruction flag = instructions[index];
                if (flag.OpCode != OpCodes.Ldfld || flag.Operand is not FieldReference field || !VanillaFireFlags.Contains(field.Name)) continue;
                // ILContext.Invoke turns branch operands into ILLabels at runtime.
                // A raw Cecil body still contains Instructions (e.g. offline tooling).
                Instruction end = flag.Next.Operand is ILLabel label ? label.Target : flag.Next.Operand as Instruction;
                if (end == null) throw new InvalidOperationException("Unexpected vanilla fire branch.");
                Instruction subtract = flag.Next;
                while (subtract != end && !(subtract.OpCode == OpCodes.Sub && subtract.Next.MatchStfld<Player>("lifeRegen"))) subtract = subtract.Next;
                if (subtract == end || !subtract.Previous.MatchLdcI4(out int amount)) throw new InvalidOperationException("Missing vanilla fire damage.");
                sources.Add((flag, amount, field.Name == "burned"));
            }
            if (sources.Count != 4) throw new InvalidOperationException("Missing vanilla heat sources.");
            var cursor = new ILCursor(il);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Action<Player>>(p => Ward(p).Begin());
            foreach (var source in sources)
            {
                cursor.Goto(source.flag, MoveType.After);
                cursor.Emit(OpCodes.Ldarg_0);
                int amount = source.amount; bool burning = source.burning;
                cursor.EmitDelegate<Func<bool, Player, bool>>((active, p) => Ward(p).DeferVanilla(active, amount, burning));
            }
            // Later vanilla checks must not block natural regeneration for paid-for fire.
            foreach (Instruction flag in instructions.Skip(badRegenIndex + 1).Where(i =>
                i.OpCode == OpCodes.Ldfld && i.Operand is FieldReference f && VanillaFireFlags.Contains(f.Name)))
            {
                cursor.Goto(flag, MoveType.After);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<bool, Player, bool>>((active, p) => Ward(p).RegenBlockingFire(active));
            }
        }

        private static Action<object, bool, int, bool> OriginalHelper(MethodReference method)
        {
            Type type = typeof(CalamityPlayer).Assembly.GetType(method.DeclaringType.FullName.Replace('/', '+'), true);
            MethodInfo original = type.GetMethod(method.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var owner = Expression.Parameter(typeof(object));
            var active = Expression.Parameter(typeof(bool));
            var amount = Expression.Parameter(typeof(int));
            var immune = Expression.Parameter(typeof(bool));
            return Expression.Lambda<Action<object, bool, int, bool>>(
                Expression.Call(Expression.Convert(owner, type), original, active, amount, immune), owner, active, amount, immune).Compile();
        }

        private static void PatchRegen(ILContext il)
        {
            var instructions = il.Body.Instructions.ToArray();
            var sources = new List<(Instruction call, Action<object, bool, int, bool> original)>();
            foreach (Instruction call in instructions.Where(i => i.Operand is MethodReference m && m.Name.Contains("g__ApplyDoTDebuff")))
            {
                for (Instruction scan = call.Previous; scan != null; scan = scan.Previous)
                {
                    if (scan.Operand is MethodReference previous && previous.Name.Contains("g__ApplyDoTDebuff")) break;
                    if (scan.OpCode != OpCodes.Ldfld || scan.Operand is not FieldReference flag || !FireFlags.Contains(flag.Name)) continue;
                    sources.Add((call, OriginalHelper((MethodReference)call.Operand)));
                    break;
                }
            }
            if (sources.Count != FireFlags.Count) throw new InvalidOperationException("Missing Calamity heat sources.");
            Instruction manaAdd = instructions.Single(i => Call(i, "get_BaseLostRegen") && i.Previous.OpCode == OpCodes.Mul).Next.Next;
            if (manaAdd.OpCode != OpCodes.Add || manaAdd.Next.OpCode != OpCodes.Sub) throw new InvalidOperationException("Changed mana burn formula.");
            var totalStores = instructions.Where(i => i.OpCode == OpCodes.Stfld && i.Operand is FieldReference f && f.Name == "totalNegativeLifeRegen").ToArray();
            Instruction deathTotalStore = totalStores[1];
            if (deathTotalStore.Previous.OpCode != OpCodes.Add) throw new InvalidOperationException("Changed Death Mode scaling.");
            Instruction finalTotal = instructions.Single(i => Field(i, "totalNegativeLifeRegen") && i.Next.OpCode == OpCodes.Conv_I4
                && i.Next.Next.OpCode == OpCodes.Sub && i.Next.Next.Next.MatchStfld<Player>("lifeRegen"));
            Instruction beforeJewels = finalTotal.Next.Next.Next;
            Instruction beforeInferno = instructions.Single(i => Field(i, "holyInferno")).Previous;
            Instruction afterInferno = instructions.Single(i => Call(i, "get_ZoneAbyss")).Previous;
            var modifiers = instructions.Where(i => i.OpCode == OpCodes.Ldsfld && i.Operand is FieldReference f
                && (f.Name == "DoTMultiplier" || f.Name == "SetBonusDebuffDamageReduction")).ToArray();
            var protections = instructions.Where(i =>
                (i.OpCode == OpCodes.Ldsfld && i.Operand is FieldReference f && f.Name == "ReducedDoTAmount")
                || Call(i, "get_ReducedDoTAmount")).ToArray();
            if (protections.Length != 3 || modifiers.Length != 3) throw new InvalidOperationException("Changed mitigation layout.");

            var cursor = new ILCursor(il);
            foreach (var source in sources)
            {
                cursor.Goto(source.call, MoveType.Before);
                cursor.MoveAfterLabels();
                cursor.Emit(OpCodes.Ldarg_0);
                var original = source.original;
                cursor.EmitDelegate<Action<object, bool, int, bool, CalamityPlayer>>((owner, active, amount, immune, cal) =>
                {
                    var ward = Ward(cal);
                    if (ward.Equipped) ward.DeferCalamity(active, amount, immune);
                    else original(owner, active, amount, immune);
                });
                cursor.Remove();
            }
            cursor.Goto(deathTotalStore, MoveType.Before);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<float, CalamityPlayer, float>>((total, cal) => Ward(cal).RemoveDeathFire(total));
            cursor.Goto(manaAdd, MoveType.After);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<float, CalamityPlayer, float>>((value, cal) => Ward(cal).DeferManaBurn(value));
            foreach (Instruction modifier in modifiers)
            {
                bool subtract = ((FieldReference)modifier.Operand).Name == "SetBonusDebuffDamageReduction";
                cursor.Goto(modifier, MoveType.After);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, CalamityPlayer, float>>((value, cal) => Ward(cal).ApplyModifier(value, subtract));
            }
            cursor.Goto(beforeJewels, MoveType.After);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Action<CalamityPlayer>>(cal => Ward(cal).Prepare(cal));
            for (int index = 0; index < protections.Length; index++)
            {
                int source = index;
                cursor.Goto(protections[index], MoveType.After);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<int, CalamityPlayer, int>>((value, cal) => Ward(cal).RemainingProtection(value, source));
            }
            cursor.Goto(beforeInferno, MoveType.Before);
            cursor.MoveAfterLabels();
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<CalamityPlayer, bool>>(cal => Ward(cal).DeferInferno(cal));
            cursor.Emit(OpCodes.Brtrue, il.DefineLabel(afterInferno));
            cursor.Goto(afterInferno, MoveType.Before);
            cursor.MoveAfterLabels();
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Action<CalamityPlayer>>(cal => Ward(cal).ApplyFire());
        }
    }
}
