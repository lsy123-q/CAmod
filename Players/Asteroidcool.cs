using Terraria;
using Terraria.ModLoader;

namespace YourModName
{
    public class Asteroidcool : ModPlayer
    {
        // 쿨타임을 저장할 변수 (프레임 단위)
        public int customCooldown = 0;

        public override void PostUpdate()
        {
            // 매 프레임마다 0이 될 때까지 1씩 감소
            if (customCooldown > 0)
            {
                customCooldown--;
            }



        }
    }
}