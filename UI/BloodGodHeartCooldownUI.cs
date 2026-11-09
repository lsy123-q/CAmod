using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using CAmod.Configs;
using CAmod.Players;

namespace CAmod.UI
{
    public class BloodGodHeartCooldownUI : ArcaneCooldownUI
    {
        private const float BloodHeartCooldownMax = 3600f;
        private static UIConfig Config => ModContent.GetInstance<UIConfig>();

        protected override string ItemTexturePath => "CAmod/Items/BloodGodHeart";
        protected override string TooltipTitle => "Blood God Heart";
        protected override float Cooldown => Main.LocalPlayer.GetModPlayer<BloodGodHeartPlayer>().bloodHeartCooldown;
        protected override float CooldownMax => BloodHeartCooldownMax;
        protected override Vector2 IconDrawSize => new Vector2(19.8f * 42f / 30f, 25.08f);

        protected override Vector2 ItemPosition
        {
            get => new Vector2(Config.BloodGodHeartUIPosX, Main.screenHeight - Config.BloodGodHeartUIPosY - 75f * MathHelper.Clamp(Config.ArcaneCooldownUIScale, 0.5f, 2f));
            set { Config.BloodGodHeartUIPosX = value.X; Config.BloodGodHeartUIPosY = Main.screenHeight - value.Y - 75f * MathHelper.Clamp(Config.ArcaneCooldownUIScale, 0.5f, 2f); }
        }

        protected override Vector4 GaugeColor2 => new Vector4(154f / 255f, 42f / 255f, 71f / 255f, 1f);
        protected override Vector4 GaugeColor1 => new Vector4(96f / 255f, 57f / 255f, 63f / 255f, 1f);
    }
}
