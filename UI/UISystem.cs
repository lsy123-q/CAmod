using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using System.Collections.Generic;
using CAmod.Players;
namespace CAmod.UI
{
    public class UISystem : ModSystem
    {
        private UserInterface bloodUI;
        private UserInterface dimGateUI;
        private UserInterface leafUI;
        private UserInterface glassUI;
    
      
        private BloodMageCooldownUI bloodState;
        private DimensionGateCooldownUI dimGateState;
        private GlassCannonCooldownUI glassState;
        private LeafShieldCooldownUI leafState;
        
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

                leafState = new LeafShieldCooldownUI();
                leafState.Activate();

                leafUI = new UserInterface();
                leafUI.SetState(leafState);

                glassState = new GlassCannonCooldownUI();
                glassState.Activate();

                glassUI = new UserInterface();
                glassUI.SetState(glassState);

            }
        }


        public override void UpdateUI(GameTime gameTime)
        {
            bloodUI?.Update(gameTime);
            dimGateUI?.Update(gameTime);
            leafUI?.Update(gameTime);
            glassUI?.Update(gameTime);
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
       int offsetX = 0;
       GameTime gt = new GameTime();

       if (bloodUI != null && bloodState.IsVisible())
       {
           bloodState.PositionOffset = new Vector2(offsetX, 0);
           bloodUI.Draw(Main.spriteBatch, gt);
           offsetX += 40;
       }

       if (dimGateUI != null && dimGateState.IsVisible())
       {
           dimGateState.PositionOffset = new Vector2(offsetX, 0);
           dimGateUI.Draw(Main.spriteBatch, gt);
           offsetX += 40;
       }

       if (leafUI != null && leafState.IsVisible())
       {
           leafState.PositionOffset = new Vector2(offsetX, 0);
           leafUI.Draw(Main.spriteBatch, gt);
           offsetX += 40;
       }
       /*
       if (glassUI != null && glassState.IsVisible())
       {
           glassState.PositionOffset = new Vector2(offsetX, 0);
           glassUI.Draw(Main.spriteBatch, gt);
       }*/

       return true;
   }

,
    InterfaceScaleType.UI));

            }
        }
    }
}
