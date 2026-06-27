using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CAmod.Players;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace CAmod.Items
{
    public class Aureola : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 10);
            if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
            {
                Item.rare = calamity.Find<ModRarity>("BurnishedAuric").Type;
            }
            else
            {
                Item.rare = ItemRarityID.Red;
            }
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
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Magic) += 0.20f; // 마법 데미지를 20% 증가시킨다
            player.GetCritChance(DamageClass.Magic) += 10; // 마법 치명타 확률을 10% 증가시킨다
            player.statManaMax2 += 150; // 최대 마나를 150 증가시킨다

            player.manaFlower = true; // 마나 꽃 기능을 활성화한다
            player.manaMagnet = true; // 마나 별 획득 범위를 증가시킨다

            player.manaCost -= 0.10f; // 마나 소모량을 10% 감소시킨다

            player.GetModPlayer<AureolaPlayer>().aureola = true; // Aureola 전용 마나 회복 효과를 활성화한다
        }
    }
}