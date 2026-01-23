using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace CAmod.Items.Weapons
{
    public class BrimstoneBombTerror : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 60;
            Item.height = 60;

            Item.damage = 250;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 40;

            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;

            Item.noMelee = true;
            Item.knockBack = 4f;

            Item.shoot = ModContent.ProjectileType<Projectiles.bomb>();
            Item.shootSpeed = 2f;

            Item.rare = ItemRarityID.Red;
            Item.value = Item.sellPrice(0, 20, 0, 0);

            Item.autoReuse = true;
        }

        public override bool Shoot(
    Player player,
    Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
    Vector2 position,
    Vector2 velocity,
    int type,
    int damage,
    float knockback)
        {
            int count = 10;
            // 원형으로 10개 배치한다
            float commonDir = Main.rand.NextBool() ? 1f : -1f;
            float step = MathHelper.TwoPi / count;

            for (int i = 0; i < count; i++)
            {
                float rot = step * i;

                Vector2 vel = rot.ToRotationVector2() * 1f;
                // 일단 바깥으로 날린다

                Projectile.NewProjectile(
    source,
    player.Center,
    vel,
    type,
    0,
    0f,
    player.whoAmI,
    Main.rand.NextFloat(), // ai[0] : 시드
    commonDir              // ai[1] : 공통 회전 방향
);
            }

            return false;
            // 기본 발사 차단한다
        }

    }
}
