using Microsoft.Xna.Framework;
using Terraria;
using CAmod.Players;
using CAmod.Configs;
using Terraria.ModLoader;

namespace CAmod.UI
{
    public class BloodMageCooldownUI : ArcaneCooldownUI
    {
        private static UIConfig Config => ModContent.GetInstance<UIConfig>();
        protected override string ItemTexturePath => "CAmod/Items/BloodMage";
        protected override string TooltipTitle => "Porphyria";
        protected override float Cooldown => Main.LocalPlayer.GetModPlayer<BloodMagePlayer>().vampCooldown;
        protected override float CooldownMax => Main.LocalPlayer.GetModPlayer<BloodMagePlayer>().vampCooldownMax;
        protected override string CooldownText => CooldownMax > 0f
            ? $"{Cooldown / CooldownMax * 100f:F0}%"
            : "0%";
        protected override Vector2 ItemPosition
        {
            get => new Vector2(Config.BloodMageUIPosX, Main.screenHeight - Config.BloodMageUIPosY - 75f * MathHelper.Clamp(Config.ArcaneCooldownUIScale, 0.5f, 2f));
            set { Config.BloodMageUIPosX = value.X; Config.BloodMageUIPosY = Main.screenHeight - value.Y - 75f * MathHelper.Clamp(Config.ArcaneCooldownUIScale, 0.5f, 2f); }
        }
        protected override Vector4 GaugeColor1 => new Vector4(1f, 146f / 255f, 169f / 255f, 1f);
        protected override Vector4 GaugeColor2 => new Vector4(1f, 0f, 54f / 255f, 1f);
    }
}

