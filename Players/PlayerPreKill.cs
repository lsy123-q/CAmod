using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace CAmod.Players
{
    public class PlayerPreKill : ModPlayer
    {
        public override bool PreKill(
            double damage,
            int hitDirection,
            bool pvp,
            ref bool playSound,
            ref bool genGore,
            ref PlayerDeathReason damageSource)
        {
           

            

            var blood = Player.GetModPlayer<BloodMagePlayer>();

            if (blood.bloodMageEquipped)
            {
                blood.allowBloodHeal = true;
                blood.lastLife = blood.Player.statLifeMax2;
            }

            return true;
        }

    }
}
