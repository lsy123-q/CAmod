using System.Collections.Generic;
using Terraria.ModLoader;

namespace CAmod.Systems
{
    public class KeySystem : ModSystem
    {
        public static ModKeybind DimGate;

        public override void Load()
        {
            DimGate = KeybindLoader.RegisterKeybind(Mod, "Dimension Gate", "Q");
        }

        

        public override void Unload()
        {
            DimGate = null;
        }
    }
}
