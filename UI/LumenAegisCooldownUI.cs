using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using CAmod.Configs;
using CAmod.Players;

namespace CAmod.UI
{
    public class LumenAegisCooldownUI : ArcaneCooldownUI
    {
        private const float EquipDelayTicks = 15f * 60f;
        private static UIConfig Config => ModContent.GetInstance<UIConfig>();
        private static LumenAegisPlayer Player => Main.LocalPlayer.GetModPlayer<LumenAegisPlayer>();

        protected override string ItemTexturePath => "CAmod/Items/LumenAegis";
        protected override string TooltipTitle => "Lumen Aegis";
        protected override float Cooldown => MathHelper.Max(Player.lumenAegisCooldown, Player.lumenAegisEquipTimer);
        protected override float CooldownMax => MathHelper.Max(Player.lumenAegisCooldownMax, EquipDelayTicks);

        // Keep the 42x51 icon's height aligned with the other 30x38 icons at 66% size.
        protected override Vector2 IconDrawSize => new Vector2(25.08f * 42f / 51f, 25.08f);

        protected override Vector2 ItemPosition
        {
            get => new Vector2(Config.LumenAegisUIPosX, Main.screenHeight - Config.LumenAegisUIPosY - 75f * MathHelper.Clamp(Config.ArcaneCooldownUIScale, 0.5f, 2f));
            set { Config.LumenAegisUIPosX = value.X; Config.LumenAegisUIPosY = Main.screenHeight - value.Y - 75f * MathHelper.Clamp(Config.ArcaneCooldownUIScale, 0.5f, 2f); }
        }

        protected override Vector4 GaugeColor1 => new Vector4(0.25f, 0.78f, 1f, 1f);
        protected override Vector4 GaugeColor2 => new Vector4(1f, 1f, 1f, 1f);
    }
}
