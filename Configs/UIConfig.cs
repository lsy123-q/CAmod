using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using System.ComponentModel;

namespace CAmod.Configs
{
    public class UIConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide; // 클라 전용 UI 설정이다

        [DefaultValue(35f)]
        [Range(0f, 1920f)] // 화면 가로 최대치까지 허용한다
        [Label("BloodMage UI X")]
        public float UIPosX;

        [DefaultValue(790f)]
        [Range(0f, 1080f)] // 화면 세로 최대치까지 허용한다
        [Label("BloodMage UI Y")]
        public float UIPosY;
    }
}
