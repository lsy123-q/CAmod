using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace CAmod.Items.Weapons
{
    public class FlameEcho : ModItem
    {
        
        public override void SetStaticDefaults()
        {
            Item.staff[Item.type] = true;
            
        }

        public override void SetDefaults()
        {
            Item.width = 60;
            Item.height = 60;
            Item.damage = 600;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 50;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 0f;
            if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
            {
                Item.rare = calamity.Find<ModRarity>("Turquoise").Type;
                
            }
            else
            {
                Item.rare = ItemRarityID.Red;
               
            }
            Item.value = Item.sellPrice(gold: 75);
            Item.UseSound = SoundID.Item20;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Projectiles.FlameEcho_Controller>();
            Item.shootSpeed = 12f;

        }
        


        public override bool Shoot(
    Player player,
    EntitySource_ItemUse_WithAmmo source,
    Vector2 position,
    Vector2 velocity,
    int type,
    int damage,
    float knockback)
        {
            Vector2 muzzlePos =
                player.MountedCenter +
                Vector2.Normalize(Main.MouseWorld - player.MountedCenter) * 68f;
            // 총구 위치다

            float aimRot =
                (Main.MouseWorld - player.MountedCenter).ToRotation();
            // 커서 조준 각도다

            for (int i = 0; i < 75; i++)
            {
                float offsetAngle = MathHelper.TwoPi * i / 75f;
                int dustType = Main.rand.Next(2);
                if (dustType == 0)
                {
                    dustType = 244;
                }
                else
                {
                    dustType = 246;
                }
                // asteroid(둥근 4엽) 파라메트릭 곡선이다
                float unitOffsetX = (float)Math.Pow(Math.Cos(offsetAngle), 3d);
                float unitOffsetY = (float)Math.Pow(Math.Sin(offsetAngle), 3d);

                Vector2 puffDustVelocity =
                    new Vector2(unitOffsetX, unitOffsetY)
                    .RotatedBy(aimRot) * 5f;
                // 벡터 자체를 커서 방향으로 회전시킨다

                Dust gold = Dust.NewDustPerfect(
                    muzzlePos,
                    dustType,
                    puffDustVelocity
                );

                gold.scale = 1.6f;
                gold.fadeIn = 1.2f;
                gold.noGravity = true;
                
                gold.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                // 골드 코인 더스트로 회전된 아스테로이드 연출을 만든다
            }

            Projectile.NewProjectile(
    source,
    muzzlePos,
    Vector2.Zero,
    type,
    damage,
    knockback,
    player.whoAmI,
    player.altFunctionUse == 2 ? 1f : 0f
);
            // 실제 공격용 투사체다

          

            return false;
        }

        // ✅ 수정: player.itemRotation 사용
        public override void AddRecipes()
        {
            if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
            {
                Recipe recipe = CreateRecipe();

                recipe.AddIngredient(
                    calamity.Find<ModItem>("PurgeGuzzler").Type, 1);
                

                recipe.AddIngredient(
                    calamity.Find<ModItem>("Lazhar").Type, 1);

                recipe.AddIngredient(
                    calamity.Find<ModItem>("UelibloomBar").Type, 10);


                recipe.AddTile(TileID.LunarCraftingStation);

                recipe.Register();
            }
            else
            {
                CreateRecipe()
                    .AddIngredient(ItemID.LunarBar, 10)
                    // 루미나이트 주괴 10개를 요구한다
                    .AddTile(TileID.LunarCraftingStation)

                    // 고대 조작대에서 제작 가능하게 한다

                    .Register();
            }
        }
    }

}