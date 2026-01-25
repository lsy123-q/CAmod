using System.Collections.Generic;
using Terraria.ModLoader;

namespace CAmod.Systems
{
    public class KeySystem : ModSystem
    {
        public static ModKeybind DimGate;
        public static ModKeybind leafshield;
        public override void Load()
        {
            DimGate = KeybindLoader.RegisterKeybind(Mod, "Dimension Gate", "Q");
            leafshield = KeybindLoader.RegisterKeybind(Mod, "Leaf Shield.", "G");
            // 몬스터 조종술 단축키를 등록한다
        }

        public override void Unload()
        {
            DimGate = null;
            leafshield = null;
            // 키바인드 참조를 해제한다
        }
    }
}
