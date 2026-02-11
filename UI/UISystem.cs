using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using System.Collections.Generic;

namespace CAmod.UI
{
    public class UISystem : ModSystem
    {
        private UserInterface bloodUI;
        private UserInterface dimGateUI;

        private BloodMageCooldownUI bloodState;
        private DimensionGateCooldownUI dimGateState;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                // BloodMage UI 초기화한다
                bloodState = new BloodMageCooldownUI();
                bloodState.Activate();

                bloodUI = new UserInterface();
                bloodUI.SetState(bloodState);

                // DimensionGate UI 초기화한다
                dimGateState = new DimensionGateCooldownUI();
                dimGateState.Activate();

                dimGateUI = new UserInterface();
                dimGateUI.SetState(dimGateState);
            }
        }


        public override void UpdateUI(GameTime gameTime)
        {
            bloodUI?.Update(gameTime);
            dimGateUI?.Update(gameTime);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int index = layers.FindIndex(layer =>
                layer.Name.Equals("Vanilla: Inventory"));

            if (index != -1)
            {
                layers.Insert(index, new LegacyGameInterfaceLayer(
    "CAmod: ArcaneCooldownUI",
    delegate
    {
        bloodUI?.Draw(Main.spriteBatch, new GameTime());
        dimGateUI?.Draw(Main.spriteBatch, new GameTime());
        return true;
    },
    InterfaceScaleType.UI));

            }
        }
    }
}
