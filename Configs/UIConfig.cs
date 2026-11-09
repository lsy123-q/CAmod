using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using System.ComponentModel;

namespace CAmod.Configs
{
    public class UIConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [Header("BloodMageCooldownUI")]
        [DefaultValue(16f)] [Range(0f, 1920f)] [Label("Blood Mage UI X")]
        public float BloodMageUIPosX = 16f;
        [DefaultValue(794f)] [Range(0f, 1080f)] [Label("Blood Mage UI Y")]
        public float BloodMageUIPosY = 794f;

        [Header("DimensionGateCooldownUI")]
        [DefaultValue(16f)] [Range(0f, 1920f)] [Label("Dimension Gate UI X")]
        public float DimensionGateUIPosX = 16f;
        [DefaultValue(794f)] [Range(0f, 1080f)] [Label("Dimension Gate UI Y")]
        public float DimensionGateUIPosY = 794f;

        [Header("LeafShieldCooldownUI")]
        [DefaultValue(16f)] [Range(0f, 1920f)] [Label("Leaf Shield UI X")]
        public float LeafShieldUIPosX = 16f;
        [DefaultValue(794f)] [Range(0f, 1080f)] [Label("Leaf Shield UI Y")]
        public float LeafShieldUIPosY = 794f;

        [Header("LumenAegisCooldownUI")]
        [DefaultValue(16f)] [Range(0f, 1920f)] [Label("Lumen Aegis UI X")]
        public float LumenAegisUIPosX = 16f;
        [DefaultValue(794f)] [Range(0f, 1080f)] [Label("Lumen Aegis UI Y")]
        public float LumenAegisUIPosY = 794f;

        [Header("BloodGodHeartCooldownUI")]
        [DefaultValue(16f)] [Range(0f, 1920f)] [Label("Blood God Heart UI X")]
        public float BloodGodHeartUIPosX = 16f;
        [DefaultValue(794f)] [Range(0f, 1080f)] [Label("Blood God Heart UI Y")]
        public float BloodGodHeartUIPosY = 794f;

        [Header("ArcaneCooldownUI")]
        [DefaultValue(1f)] [Range(0.5f, 2f)] [Increment(0.05f)] [Label("Arcane Cooldown UI Scale")]
        public float ArcaneCooldownUIScale = 1f;

        [DefaultValue(0.95f)] [Range(0f, 1f)] [Increment(0.05f)] [Label("Cooldown UI Icon Alpha")]
        public float CooldownUIIconAlpha = 0.95f;

        [DefaultValue(0.8f)] [Range(0f, 1f)] [Increment(0.05f)] [Label("Cooldown UI Overall Alpha")]
        public float CooldownUIOverallAlpha = 0.8f;
    }
}
