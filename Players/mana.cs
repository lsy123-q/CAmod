using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace CAmod.Players
{
   
    public class mana : ModPlayer
    {
        public override void GetHealMana(Item item, bool quickHeal, ref int healValue)
        {
            if (item.healMana <= 0)
                return;
            // 체력 회복 포션이 아닐 경우 무시한다

            healValue = (int)(healValue * 1.5f);
        }
      

    }

}
