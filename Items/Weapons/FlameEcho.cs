using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;
using System;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using CAmod.Particles;

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
                Item.rare =
                    calamity.Find<ModRarity>("Turquoise").Type;
            }
            else
            {
                Item.rare = ItemRarityID.Red;
            }

            Item.value = Item.sellPrice(gold: 30);

            Item.UseSound = SoundID.Item73;
            Item.autoReuse = true;

            Item.shoot =
                ModContent.ProjectileType<
                    Projectiles.FlameEcho_Controller>();

            Item.shootSpeed = 12f;
        }

        public override Vector2? HoldoutOrigin()
        {
            return new Vector2(2f, 2f);
            // 아이템을 쥐는 기준 위치다
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
                Vector2.Normalize(
                    Main.MouseWorld -
                    player.MountedCenter
                ) * 68f;
            // 총구 위치다

            float aimRot =
                (
                    Main.MouseWorld -
                    player.MountedCenter
                ).ToRotation();
            // 커서 조준 각도다

            // =========================================
            // V3 파티클 발사 연출
            // =========================================

            for (int i = 0; i < 75; i++)
            {
                float offsetAngle =
                    MathHelper.TwoPi * i / 75f;

                // asteroid(둥근 4엽) 파라메트릭 곡선
                float unitOffsetX =
                    (float)Math.Pow(
                        Math.Cos(offsetAngle),
                        3d
                    );

                float unitOffsetY =
                    (float)Math.Pow(
                        Math.Sin(offsetAngle),
                        3d
                    );

                Vector2 puffVelocity =
                    new Vector2(
                        unitOffsetX,
                        unitOffsetY
                    ).RotatedBy(aimRot) * 5f;

                // 기존 Dust의 속도 감쇠
                puffVelocity *= 0.65f;

                Color color =
                    Main.rand.NextBool()
                    ? new Color(255, 210, 80)
                    : new Color(255, 230, 130);

                float particleRotation =
                    Main.rand.NextFloat(
                        MathHelper.TwoPi
                    );

                // 기존 scale = 1.6f 대응
                float particleScale = 1.6f;

                CosmicParticleSystem.Buffer.Create(
                    new ParticleInfo(
                        position: muzzlePos.ToNumerics(),
                        velocity: puffVelocity.ToNumerics(),
                        rotation: particleRotation,
                        scale: new System.Numerics.Vector2(
                            particleScale * 10f,
                            particleScale * 10f
                        ),
                        depth: 1f,
                        color: color,
                        duration: 60
                    )
                );
               
            }

            // =========================================
            // 실제 공격용 투사체
            // =========================================

            Projectile.NewProjectile(
                source,
                muzzlePos,
                Vector2.Zero,
                type,
                damage,
                knockback,
                player.whoAmI,
                player.altFunctionUse == 2
                    ? 1f
                    : 0f
            );

            return false;
        }

        public override void AddRecipes()
        {
            if (ModLoader.TryGetMod(
                "CalamityMod",
                out Mod calamity))
            {
                Recipe recipe =
                    CreateRecipe();

                recipe.AddIngredient(
                    calamity.Find<ModItem>(
                        "Photosynthesis"
                    ).Type,
                    1
                );

                recipe.AddIngredient(
                    calamity.Find<ModItem>(
                        "UelibloomBar"
                    ).Type,
                    10
                );

                recipe.AddIngredient(
                    calamity.Find<ModItem>(
                        "UnholyEssence"
                    ).Type,
                    10
                );

                recipe.AddIngredient(
                    calamity.Find<ModItem>(
                        "DivineGeode"
                    ).Type,
                    10
                );

                recipe.AddTile(
                    TileID.LunarCraftingStation
                );

                recipe.Register();
            }
            else
            {
                CreateRecipe()
                    .AddIngredient(
                        ItemID.LunarBar,
                        10
                    )
                    .AddTile(
                        TileID.LunarCraftingStation
                    )
                    .Register();
            }
        }
    }
}