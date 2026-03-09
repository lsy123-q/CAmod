using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CAmod.Items.Weapons
{
    public class RuneBolt : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 48;

            Item.damage = 120; // 기본 데미지
            Item.DamageType = DamageClass.Magic;

            Item.mana = 10; // 마나 소모
            Item.useTime = 30;
            Item.useAnimation = 30;

            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;

            Item.knockBack = 2f;

            Item.autoReuse = true;
            Item.UseSound = SoundID.Item28; // 마법 발사 시전 소리를 낸다
            Item.shoot = ModContent.ProjectileType<Projectiles.RuneBoltProj>();
            // RuneBoltProj 투사체를 발사한다

            Item.shootSpeed = 20f;
            // 직선 발사 속도

            Item.rare = ItemRarityID.Pink;
            Item.value = Item.buyPrice(0, 10, 0, 0);
        }
        
        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 dir = Vector2.Normalize(Main.MouseWorld - player.Center); // 커서 방향을 구한다

            Vector2 spawnPos = player.Center + dir * 40f; // 플레이어 중심에서 커서 방향으로 20px 앞에서 생성한다

            Projectile.NewProjectile(source, spawnPos, velocity, type, damage, knockback, player.whoAmI);
            // 투사체를 새 위치에서 발사한다

            return false; // 기본 발사를 막는다
        }


    }
}