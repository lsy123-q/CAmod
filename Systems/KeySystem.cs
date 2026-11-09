using System.Collections.Generic;
using Terraria.ModLoader;

namespace CAmod.Systems
{
    public class KeySystem : ModSystem
    {
        public static ModKeybind DimGate;
        public static ModKeybind leafshield;
        public static ModKeybind HarmonyCycleToggle;
        public static ModKeybind PhantomDragonMode;
        public override void Load()
        {
            PhantomDragonMode = KeybindLoader.RegisterKeybind(Mod, "Phantom Dragon Mode", "B");
            DimGate = KeybindLoader.RegisterKeybind(Mod, "Dimension Gate", "Q");
            leafshield = KeybindLoader.RegisterKeybind(Mod, "Leaf Shield", "G");
            HarmonyCycleToggle = KeybindLoader.RegisterKeybind(Mod, "Harmony Cycle Toggle", "V");
            // �ϸ�� ���� ON/OFF ��� Ű��
            // ���� ������ ����Ű�� ����Ѵ�
        }

        public override void Unload()
        {
            PhantomDragonMode = null;
            DimGate = null;
            leafshield = null;
            HarmonyCycleToggle = null;
        }
    }
}
