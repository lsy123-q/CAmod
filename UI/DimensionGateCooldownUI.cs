using Microsoft.Xna.Framework;
using Terraria;
using CAmod.Players;
using CAmod.Configs;
using Terraria.ModLoader;

namespace CAmod.UI
{
    public class DimensionGateCooldownUI : ArcaneCooldownUI
    {
        private static UIConfig Config => ModContent.GetInstance<UIConfig>();
        protected override string ItemTexturePath => "CAmod/Items/DimensionGate";
        protected override string TooltipTitle => "Dimension Gate";
        protected override float Cooldown => Main.LocalPlayer.GetModPlayer<DimGatePlayer>().gateCooldown;
        protected override float CooldownMax => Main.LocalPlayer.GetModPlayer<DimGatePlayer>().gateCooldownmax;
        protected override Vector2 ItemPosition
        {
            get => new Vector2(Config.DimensionGateUIPosX, Main.screenHeight - Config.DimensionGateUIPosY - 75f * MathHelper.Clamp(Config.ArcaneCooldownUIScale, 0.5f, 2f));
            set { Config.DimensionGateUIPosX = value.X; Config.DimensionGateUIPosY = Main.screenHeight - value.Y - 75f * MathHelper.Clamp(Config.ArcaneCooldownUIScale, 0.5f, 2f); }
        }
        protected override Vector4 GaugeColor1 => new Vector4(0.58f, 0.2f, 1f, 1f);
        protected override Vector4 GaugeColor2 => new Vector4(1f, 0.35f, 0.72f, 1f);
    }
}

