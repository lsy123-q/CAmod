using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using System.Collections.Generic;
using Terraria.ModLoader.Config;
using CAmod.Configs;

namespace CAmod.UI
{
    public class UISystem : ModSystem
    {
        private UserInterface bloodUI;
        private UserInterface dimGateUI;
        private UserInterface leafUI;
        private UserInterface glassUI;
        private UserInterface lumenUI;
        private UserInterface bloodGodHeartUI;

        private BloodMageCooldownUI bloodState;
        private DimensionGateCooldownUI dimGateState;
        private GlassCannonCooldownUI glassState;
        private LeafShieldCooldownUI leafState;
        private LumenAegisCooldownUI lumenState;
        private BloodGodHeartCooldownUI bloodGodHeartState;

        public override void Load()
        {
            if (Main.dedServ)
                return;

            NormalizePreviousDefaultPositions();

            bloodState = new BloodMageCooldownUI();
            bloodState.Activate();
            bloodUI = new UserInterface();
            bloodUI.SetState(bloodState);

            dimGateState = new DimensionGateCooldownUI();
            dimGateState.Activate();
            dimGateUI = new UserInterface();
            dimGateUI.SetState(dimGateState);

            leafState = new LeafShieldCooldownUI();
            leafState.Activate();
            leafUI = new UserInterface();
            leafUI.SetState(leafState);

            lumenState = new LumenAegisCooldownUI();
            lumenState.Activate();
            lumenUI = new UserInterface();
            lumenUI.SetState(lumenState);

            bloodGodHeartState = new BloodGodHeartCooldownUI();
            bloodGodHeartState.Activate();
            bloodGodHeartUI = new UserInterface();
            bloodGodHeartUI.SetState(bloodGodHeartState);


            glassState = new GlassCannonCooldownUI();
            glassState.Activate();
            glassUI = new UserInterface();
            glassUI.SetState(glassState);
        }

        private static void NormalizePreviousDefaultPositions()
        {
            UIConfig config = ModContent.GetInstance<UIConfig>();
            if (config.BloodMageUIPosX == 35f && config.DimensionGateUIPosX == 85f &&
                config.LeafShieldUIPosX == 135f && config.BloodMageUIPosY == 650f &&
                config.DimensionGateUIPosY == 650f && config.LeafShieldUIPosY == 650f)
            {
                config.DimensionGateUIPosX = config.BloodMageUIPosX;
                config.LeafShieldUIPosX = config.BloodMageUIPosX;

                try
                {
                    config.SaveChanges(silent: true);
                }
                catch
                {
                    // The adjusted defaults are still active for this session.
                }
            }
        }

        public override void UpdateUI(GameTime gameTime)
        {
            bloodUI?.Update(gameTime);
            dimGateUI?.Update(gameTime);
            leafUI?.Update(gameTime);
            lumenUI?.Update(gameTime);
            bloodGodHeartUI?.Update(gameTime);
            glassUI?.Update(gameTime);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
            if (index < 0)
                return;

            layers.Insert(index, new LegacyGameInterfaceLayer(
                "CAmod: ArcaneCooldownUI",
                () =>
                {
                    GameTime gameTime = new GameTime();
                    if (bloodUI != null && bloodState.IsVisible())
                        bloodUI.Draw(Main.spriteBatch, gameTime);
                    if (dimGateUI != null && dimGateState.IsVisible())
                        dimGateUI.Draw(Main.spriteBatch, gameTime);
                    if (leafUI != null && leafState.IsVisible())
                        leafUI.Draw(Main.spriteBatch, gameTime);
                    if (lumenUI != null && lumenState.IsVisible())
                        lumenUI.Draw(Main.spriteBatch, gameTime);
                    if (bloodGodHeartUI != null && bloodGodHeartState.IsVisible())
                        bloodGodHeartUI.Draw(Main.spriteBatch, gameTime);
                    return true;
                },
                InterfaceScaleType.UI
            ));
        }

        public override void Unload()
        {
            ArcaneCooldownUI.ClearWidgets();
            bloodUI = null;
            dimGateUI = null;
            leafUI = null;
            glassUI = null;
            lumenUI = null;
            bloodGodHeartUI = null;
            bloodState = null;
            dimGateState = null;
            leafState = null;
            glassState = null;
            lumenState = null;
            bloodGodHeartState = null;
        }
    }
}
