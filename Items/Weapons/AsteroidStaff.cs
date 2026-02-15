using CAmod.Buffs;
using CAmod.Players;
using CAmod.Projectiles;
using Microsoft.Xna.Framework;
using rail;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using YourModName;
namespace CAmod.Items.Weapons
{


    public class AsteroidStaff : ModItem
    {
        public const int CooldownTime = 15 * 60;
        public override void SetStaticDefaults()
        {
            Item.staff[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 50;
            Item.height = 50;
            Item.damage = 180 ;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 90;
            Item.useTime = 5;
            Item.useAnimation = 180;
            Item.useStyle = ItemUseStyleID.Shoot;
            
            Item.noMelee = true;
            Item.knockBack = 6.75f;
            
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Asteroid>();
            Item.shootSpeed = 20f;
            
          
            Item.rare = ItemRarityID.Purple;
        }
        public override bool CanUseItem(Player player)
        {
            // 플레이어의 커스텀 쿨타임 필드를 가져옴

            int debuffType = ModContent.BuffType<UltimateCool>();
            if (!player.HasBuff(debuffType) && base.CanUseItem(player))
            {
                player.AddBuff(debuffType, 15 * 60);
                return base.CanUseItem(player);
            }
            else
            {
                return false;
            }


           
          
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float meteorSpeed = Item.shootSpeed;
            int asteroidAmt = 3;
            SoundEngine.PlaySound(SoundID.Item88, player.Center);
            for (int i = 0; i < asteroidAmt; i++)
            {
                // 1. 소환 위치 설정
                // X축: 마우스 월드 좌표 + 약간의 랜덤 분산
                // Y축: 플레이어의 위치보다 600~800픽셀 위 (기존 방식 유지)
                float spawnX = Main.MouseWorld.X + (float)Main.rand.Next(-250, 251);
                float spawnY = player.MountedCenter.Y - 600f - (float)(100 * i);

                Vector2 spawnPos = new Vector2(spawnX, spawnY);

                // 2. 방향 계산 (마우스 위치를 향해 날아가도록 설정)
                Vector2 targetPos = Main.MouseWorld;
                // 약간의 오차를 주어 탄착군 형성
                targetPos.X += (float)Main.rand.Next(-40, 41);

                Vector2 heading = targetPos - spawnPos;

                // 3. 속도 정규화 및 적용
                if (heading.Y < 0f) heading.Y *= -1f; // 아래로 향하게 강제
                if (heading.Y < 20f) heading.Y = 20f; // 최소 하강 속도 보정

                float dist = heading.Length();
                dist = meteorSpeed / dist;
                heading *= dist;

                // 4. 투사체 생성 (기존 ai1 설정값 유지)
                Projectile.NewProjectile(
                    source,
                    spawnPos.X,
                    spawnPos.Y,
                    heading.X * 0.75f,
                    heading.Y * 0.75f,
                    type,
                    damage,
                    knockback,
                    player.whoAmI,
                    0f,
                    0.5f + (float)Main.rand.NextDouble() * 0.3f
                );
            }
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.MeteorStaff).
                AddIngredient(ItemID.LunarBar, 5).
                AddTile(TileID.LunarCraftingStation).
                Register();
        }
    }
}
