using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Terraria;
using Terraria.ModLoader;
using CAmod.Players;
using CAmod.Players;
using System.Reflection;
using Terraria.ID;
using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework;
using CAmod.Projectiles;
namespace CAmod.Items.Weapons
{
    public class DivineRetribution : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.staff[Item.type] = true;

        }
        public override void SetDefaults()
        {
            Item.damage = 60;
            Item.DamageType = DamageClass.Magic;
            // 마법 무기로 설정한다

            Item.mana = 15;
            Item.width = 66;
            Item.height = 88;
            Item.useTime = 12;
            Item.useAnimation = 12;
            Item.value = Item.sellPrice(gold: 30);
            Item.noMelee = true;
            Item.knockBack = 3.5f;
            Item.useStyle = ItemUseStyleID.Shoot;
            if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
            {
                Item.rare = calamity.Find<ModRarity>("Turquoise").Type;

            }
            else
            {
                Item.rare = ItemRarityID.Red;

            }

            Item.UseSound = SoundID.Item73;
            Item.autoReuse = true;
            Item.shootSpeed = 19f;
            Item.shoot = ModContent.ProjectileType<Projectiles.DivineRetributionSpear>();
        }

        public override Vector2? HoldoutOrigin()
        {
            return new Vector2(5f, 5f);
            // 아이템을 쥐는 기준 위치다
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
            float num72 = Item.shootSpeed;
            // 투사체 기본 속도다

            Vector2 vector2 = player.RotatedRelativePoint(player.MountedCenter, true);
            // 플레이어 기준 좌표다

            float num78 = Main.mouseX + Main.screenPosition.X - vector2.X;
            float num79 = Main.mouseY + Main.screenPosition.Y - vector2.Y;

            if (player.gravDir == -1f)
            {
                num79 = Main.screenPosition.Y + Main.screenHeight - Main.mouseY - vector2.Y;
                // 중력 반전 상태를 보정한다
            }

            float num80 = (float)Math.Sqrt(num78 * num78 + num79 * num79);

            if ((float.IsNaN(num78) && float.IsNaN(num79)) || (num78 == 0f && num79 == 0f))
            {
                num78 = player.direction;
                // 마우스 좌표가 이상할 경우 기본 방향을 사용한다
            }
            else
            {
                num80 = num72 / num80;
                // 속도를 정규화한다
            }

            int numProjectiles = Main.rand.Next(5, 7);
            // 한 번에 5발을 소환한다

            for (int i = 0; i < numProjectiles; i++)
            {
                vector2 = new Vector2(
                    player.position.X + player.width * 0.5f
                    + Main.rand.Next(51) * -player.direction
                    + (Main.mouseX + Main.screenPosition.X - player.position.X),
                    player.MountedCenter.Y + 600f);
                // 플레이어 위쪽에서 소환 위치를 만든다

                vector2.X = (vector2.X + player.Center.X) / 2f + Main.rand.Next(-50, 51);
                vector2.Y += 100f * i;
                // 투사체 간 간격을 만든다

                num78 = Main.mouseX + Main.screenPosition.X - vector2.X;
                num79 = Main.mouseY + Main.screenPosition.Y - vector2.Y;

                if (num79 < 0f)
                    num79 *= -1f;

                if (num79 < 20f)
                    num79 = 20f;
                // 최소 낙하 각도를 보장한다

                num80 = (float)Math.Sqrt(num78 * num78 + num79 * num79);
                num80 = num72 / num80;

                num78 *= num80;
                num79 *= num80;

                float speedX6 = num78 + Main.rand.Next(-60, 61) * 0.02f;
                float speedY7 = num79 + Main.rand.Next(-60, 61) * 0.02f;
                // 랜덤 편차를 준다

                float ai1 = Main.rand.NextFloat() + 0.5f;

                Projectile.NewProjectile(
                    source,
                    vector2,
                    new Vector2(speedX6, -speedY7),
                    type,
                    damage,
                    knockback,
                    player.whoAmI,
                    0f,
                    ai1);
                // 투사체를 생성한다
            }

            return false;
            // 기본 발사 로직을 막는다
        }
        public override void AddRecipes()
        {
            try
            {
                if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
                {
                    Recipe recipe = CreateRecipe();

                    recipe.AddIngredient(
                        calamity.Find<ModItem>("UndinesRetribution").Type, 1);


                    recipe.AddIngredient(
                        calamity.Find<ModItem>("UnholyEssence").Type, 10);

                    recipe.AddIngredient(
                        calamity.Find<ModItem>("DivineGeode").Type, 10);


                    recipe.AddTile(TileID.LunarCraftingStation);

                    recipe.Register();
                }
            }
            catch
            {
                // 칼라미티 로드/문자열 탐색 실패 같은 예외를 삼킨다
            }
        }


    }
}
