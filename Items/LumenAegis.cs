using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CAmod.Players;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CalamityMod;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Materials;
using CalamityMod.Tiles.Furniture.CraftingStations;
namespace CAmod.Items
{
    public class LumenAegis : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 10);
            Item.rare = ModContent.RarityType<CalamityMod.Rarities.BurnishedAuric>();
        }



        public override void UpdateAccessory(
            Player player,
            bool hideVisual)
        {
            LumenAegisPlayer aegis =
                player.GetModPlayer<LumenAegisPlayer>();

            // 보호막 쿨타임 중에만 엠퓰 효과를 활성화한다.
            if (aegis.lumenAegisCooldown > 0)
                player.Calamity().aAmpoule = true;

            // 루멘 이지스 효과를 활성화한다.
            aegis.lumenAegisEquipped = true;
        }

        public override bool PreDrawInInventory(
    SpriteBatch spriteBatch,
    Vector2 position,
    Rectangle frame,
    Color drawColor,
    Color itemColor,
    Vector2 origin,
    float scale
)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Item[Type].Value;

            float forcedScale = scale * 1.12f; // 인벤토리에서 보이는 아이템 크기를 1.35배로 키운다

            spriteBatch.Draw(
                texture,
                position,
                frame,
                drawColor,
                0f,
                origin,
                forcedScale,
                SpriteEffects.None,
                0f
            );

            return false; // 기본 인벤토리 그리기를 막고 직접 그린다
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<TheSponge>()
                .AddIngredient<AmbrosialAmpoule>()
                .AddIngredient<AuricBar>(5)
                .AddIngredient<AscendantSpiritEssence>(4)
                .AddTile<CosmicAnvil>()
                .Register();
        }
    }
}