using System;
using CalamityMod.CalPlayer;
using CalamityMod.CalPlayer.Dashes;
using CalamityMod.Items.Accessories;
using CalamityMod.World;
using Terraria;
using Terraria.ModLoader;

namespace CAmod.Players
{
    public sealed class FlameManaWardPlayer : ModPlayer
    {
        public const int MaximumRegenConverted = 40;
        public bool Equipped;
        private double prepaidMana;
        private double vanillaFire, calamityFire, holyFire;
        private int crownRemaining, infectedRemaining, radianceRemaining;
        private int residualFire;
        private bool prepared;

        public override void ResetEffects() => Equipped = false;
        public override void UpdateDead() { prepaidMana = 0d; prepared = false; }

        internal void Begin()
        {
            vanillaFire = calamityFire = holyFire = 0d;
            residualFire = 0;
            prepared = false;
            if (!Equipped) prepaidMana = 0d;
        }

        internal bool DeferVanilla(bool active, int amount, bool burningBlock)
        {
            if (!Equipped || !active) return active;
            vanillaFire += amount;
            // Burning also slows movement. Only its regeneration damage is deferred.
            if (burningBlock) Player.moveSpeed *= 0.5f;
            return false;
        }

        internal void DeferCalamity(bool active, int amount, bool immune)
        {
            if (active && !immune)
                calamityFire += amount * (CalamityWorld.death ? 1.25d : 1d);
        }

        internal float RemoveDeathFire(float total)
        {
            if (!Equipped) return total;
            double fire = (Player.onFire ? 2d : 0d) + (Player.onFire2 ? 3d : 0d)
                + (Player.burned ? 15d : 0d);
            calamityFire += fire;
            return total - (float)fire;
        }

        internal float DeferManaBurn(float signedRegen)
        {
            if (!Equipped || signedRegen >= 0f) return signedRegen;
            calamityFire -= signedRegen;
            return 0f;
        }

        internal float ApplyModifier(float amount, bool subtract)
        {
            if (Equipped)
                calamityFire = subtract ? calamityFire - (int)(calamityFire * amount)
                    : (int)(calamityFire * amount);
            return amount;
        }

        internal void Prepare(CalamityPlayer cal)
        {
            if (!Equipped) return;
            prepared = true;
            // Include Holy Inferno before assigning the shared free mitigation budget.
            holyFire = cal.holyInferno
                ? Math.Min(200, (int)((cal.hInfernoBoost + 1d) * (CalamityWorld.death ? 1.25d : 1d))) : 0;
            bool regularFireImmune = (cal.silvaCountdown > 0 && cal.hasSilvaEffect && cal.silvaSet)
                || (cal.LastUsedDashID == GodslayerArmorDash.ID && Player.dashDelay < 0);
            double fire = (regularFireImmune ? 0d : vanillaFire + calamityFire) + holyFire;
            crownRemaining = Reserve(cal.crownJewel ? CrownJewel.ReducedDoTAmount : 0, ref fire);
            infectedRemaining = Reserve(cal.infectedJewel ? InfectedJewel.ReducedDoTAmount : 0, ref fire);
            radianceRemaining = Reserve(cal.purity ? Radiance.ReducedDoTAmount : 0, ref fire);
            residualFire = Math.Max(0, (int)fire);
        }

        private static int Reserve(int protection, ref double fire)
        {
            int used = Math.Min(protection, (int)Math.Ceiling(fire));
            fire = Math.Max(0d, fire - used);
            return protection - used;
        }

        internal int RemainingProtection(int original, int source) => !Equipped || !prepared ? original
            : source == 0 ? crownRemaining : source == 1 ? infectedRemaining : radianceRemaining;

        internal bool DeferInferno(CalamityPlayer cal)
        {
            if (!Equipped || !prepared) return false;
            if (cal.holyInferno) cal.hInfernoBoost++;
            else cal.hInfernoBoost = 0;
            return true;
        }

        internal void ApplyFire()
        {
            if (!Equipped || !prepared) return;
            int eligible = Math.Min(MaximumRegenConverted, residualFire);
            // Use vanilla potion handling so inventory selection, sickness, and mod hooks
            // behave exactly like Mana Flower. Only the owning client consumes items.
            int neededMana = (int)Math.Ceiling(Math.Max(0d, eligible / 12d - prepaidMana) - 1e-9d);
            if (neededMana > 0 && Player.manaFlower && Player.statMana < neededMana
                && Player.whoAmI == Main.myPlayer)
                Player.QuickMana();
            double available = Math.Max(0, Player.statMana) + prepaidMana;
            int converted = Math.Min(eligible, (int)Math.Floor(Math.Min(eligible, available * 12d) + 1e-6d));
            if (converted > 0)
            {
                // 2 regeneration units = 1 HP/sec; 1 unit costs 1/12 mana per tick.
                double cost = converted / 12d;
                int payment = (int)Math.Ceiling(Math.Max(0d, cost - prepaidMana) - 1e-9d);
                Player.statMana -= payment;
                prepaidMana = Math.Max(0d, prepaidMana + payment - cost);
                Player.manaRegenDelay = Math.Max(Player.manaRegenDelay, (int)Player.maxRegenDelay);
                residualFire -= converted;
            }
            // Fully blocked fire never zeros existing regeneration or its recovery timer.
            if (residualFire > 0)
            {
                if (holyFire > 0d) Player.nebulaLevelLife = 0;
                if (Player.lifeRegen > 0) Player.lifeRegen = 0;
                Player.lifeRegenTime = 0f;
                Player.lifeRegen -= residualFire;
            }
        }

        internal bool RegenBlockingFire(bool active) =>
            active && (!Equipped || !prepared || residualFire > 0);
    }
}
