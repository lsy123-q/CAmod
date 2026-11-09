using Microsoft.Xna.Framework;
using Terraria;
using CAmod.Players;
using CAmod.Configs;
using Terraria.ModLoader;

namespace CAmod.UI
{
    public class LeafShieldCooldownUI : ArcaneCooldownUI
    {
        private static UIConfig Config => ModContent.GetInstance<UIConfig>();
        protected override string ItemTexturePath => "CAmod/Items/LeafShield";
        protected override string TooltipTitle => "Leaf Shield";
        protected override float Cooldown => Main.LocalPlayer.GetModPlayer<LeafWardPlayer>().leafShieldCooldown;
        protected override float CooldownMax => Main.LocalPlayer.GetModPlayer<LeafWardPlayer>().leafShieldCooldownmax;
        protected override Vector2 ItemPosition
        {
            get => new Vector2(Config.LeafShieldUIPosX, Main.screenHeight - Config.LeafShieldUIPosY - 75f * MathHelper.Clamp(Config.ArcaneCooldownUIScale, 0.5f, 2f));
            set { Config.LeafShieldUIPosX = value.X; Config.LeafShieldUIPosY = Main.screenHeight - value.Y - 75f * MathHelper.Clamp(Config.ArcaneCooldownUIScale, 0.5f, 2f); }
        }
        protected override Vector4 GaugeColor1 => new Vector4(0.65f, 1f, 0.35f, 1f);
        protected override Vector4 GaugeColor2 => new Vector4(0.75f, 1f, 0f, 1f);
    }
}

