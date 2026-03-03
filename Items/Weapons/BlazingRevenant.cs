using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CAmod.Projectiles;
using System;

namespace CAmod.Items.Weapons
{
    public class BlazingRevenant : ModItem
    {
        private static float globalRotationOffset = 0f;
        public override void SetStaticDefaults()
        {
            // 표시명/툴팁은 Localization 쓰면 됨
        }

        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;

            Item.damage = 450; 
            Item.DamageType = DamageClass.Magic; // 투사체가 Generic이라 맞춘다
            Item.knockBack = 3f;
            Item.crit = 0;
            Item.mana = 30;
            Item.useStyle = ItemUseStyleID.Shoot; // 발사 무기 스타일이다
            Item.noMelee = true; // 근접 판정 안쓴다
            Item.autoReuse = true; // 자동사용 가능하게 한다

            Item.useTime = 12; // 초기값이다 (실제로는 CanUseItem에서 매번 갱신한다)
            Item.useAnimation = 12; // useTime과 동일하게 맞춘다
            Item.UseSound = SoundID.Item73;

            Item.shoot = ModContent.ProjectileType<fire>(); // fire 투사체를 쏜다
            Item.shootSpeed = 10f; // 기본 속도다 (fire 내부에서 선회/호밍하니 너무 높게 안 준다)
           
            Item.value = Item.sellPrice(gold: 48);
            if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
            {
                Item.rare = calamity.Find<ModRarity>("BurnishedAuric").Type;

            }
            else
            {
                Item.rare = ItemRarityID.Red;

            }
        }

        public override bool CanUseItem(Player player)
        {
           

            return base.CanUseItem(player);
        }

        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
    Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.whoAmI != Main.myPlayer)
                return false; // 중복 소환 방지한다

          
            float speed = Item.shootSpeed;
            int max = player.statLifeMax2;
            int missing = max - player.statLife;
            float ratio = max > 0 ? (float)missing / max : 0f;
            

            // 발사 개수를 8 ~ 20으로 스케일한다
            int count = (int)MathHelper.Lerp(3f, 9f, ratio);
            count = Utils.Clamp(count, 3, 9);


            // 🔹 전체 투사체가 공유하는 선회값이다

            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + globalRotationOffset;
               
                Vector2 v = angle.ToRotationVector2() * speed;
                int spinValue = Main.rand.NextBool()
    ? Main.rand.Next(4, 9)     // 4 ~ 8
    : -Main.rand.Next(4, 9);
                float t = (i - (count - 1) * 0.5f) / ((count - 1) * 0.5f);
                // t는 -1 ~ 0 ~ +1 대칭값이다

                float spinOffset = t * 1.5f;

                Projectile.NewProjectile(
                    source,
                    player.Center,
                    v,
                    type,
                    damage,
                    knockback,
                    player.whoAmI,
                    1f,        // ai[0] : 스케일이다
                    0f,
                    spinValue // ai[2] : 선회각 보정값이다
                );
            }
            globalRotationOffset += MathHelper.Pi;
            return false; // 기본 발사 막는다
        }

        public override void AddRecipes()
{
    try
    {
        if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
        {
            Recipe recipe = CreateRecipe();

            recipe.AddIngredient(calamity.Find<ModItem>("RecitationoftheBeast").Type, 1); // 야수의 영창 1개를 요구한다
            recipe.AddIngredient(calamity.Find<ModItem>("AuricBar").Type, 5);            // 오릭 주괴 5개를 요구한다
            recipe.AddIngredient(calamity.Find<ModItem>("YharonSoulFragment").Type, 5);  // 야론 영혼조각 5개를 요구한다

            recipe.AddTile(calamity.Find<ModTile>("CosmicAnvil").Type); // 전우주의 모루에서 제작 가능하게 한다

            recipe.Register();
        }
    }
    catch
    {
        // 칼라미티 미설치/내부명 변경 등으로 등록이 실패하면 조합법을 생략한다
    }
}
    }
}
