using System.Collections.Generic;
using Terraria.ModLoader;

namespace CAmod.Systems
{
    public class KeySystem : ModSystem
    {
        public static ModKeybind DimGate;
        public static ModKeybind leafshield;
        public static ModKeybind HarmonyCycleToggle;
        public override void Load()
        {
            DimGate = KeybindLoader.RegisterKeybind(Mod, "Dimension Gate", "Q");
            leafshield = KeybindLoader.RegisterKeybind(Mod, "Leaf Shield", "G");
            HarmonyCycleToggle = KeybindLoader.RegisterKeybind(Mod, "Harmony Cycle Toggle", "V");
            // 하모니 구조 ON/OFF 토글 키다
            // 몬스터 조종술 단축키를 등록한다
        }

        public override void Unload()
        {
            DimGate = null;
            leafshield = null;
            HarmonyCycleToggle = null;
        }
    }
}
