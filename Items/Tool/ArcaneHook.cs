using CAmod.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CAmod.Items.Tool
{
    public class ArcaneHook : ModItem
    {

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;

            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 15;
            Item.useAnimation = 15;

            Item.noUseGraphic = true;
            Item.noMelee = true;

            Item.value = Item.sellPrice(gold: 10);
            Item.rare = ItemRarityID.Yellow;

            Item.shoot = ModContent.ProjectileType<Projectiles.ArcaneHookProj>();
            Item.shootSpeed = 10f; // 기본 갈고리보다 빠르게 한다

            Item.mana = 20; // 🔥 갈고리 사용 시 마나를 소모한다
            Item.DamageType = DamageClass.Magic;

            Item.UseSound = SoundID.Item13;
        }

        public override bool CanUseItem(Player player)
        {
            // 마나 부족하면 사용 불가이다
            if (player.statMana < Item.mana)
                return false;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];

                if (p.active &&
                    p.owner == player.whoAmI &&
                    p.type == ModContent.ProjectileType<ArcaneHookCore>())
                {
                    return false; // 이미 존재하므로 새로 소환하지 않는다
                }
            }
            if (player.whoAmI == Main.myPlayer)
            {
                var source = player.GetSource_ItemUse(Item);

                Projectile.NewProjectile(
                    source,
                    player.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<ArcaneHookCore>(),
                    0,
                    0f,
                    player.whoAmI
                );
            }

            player.statMana -= Item.mana; // 직접 마나 차감한다
            player.manaRegenDelay = 60;   // 마나 재생 딜레이를 건다

            return false; // 기본 발사 로직을 막는다
        }
    }
}