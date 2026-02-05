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
        public override void SetStaticDefaults()
        {
            // 표시명/툴팁은 Localization 쓰면 됨
        }

        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;

            Item.damage = 450; // 데미지 500이다
            Item.DamageType = DamageClass.Magic; // 투사체가 Generic이라 맞춘다
            Item.knockBack = 3f;
            Item.crit = 0;
            Item.mana = 15;
            Item.useStyle = ItemUseStyleID.Shoot; // 발사 무기 스타일이다
            Item.noMelee = true; // 근접 판정 안쓴다
            Item.autoReuse = true; // 자동사용 가능하게 한다

            Item.useTime = 36; // 초기값이다 (실제로는 CanUseItem에서 매번 갱신한다)
            Item.useAnimation = 36; // useTime과 동일하게 맞춘다
            Item.UseSound = SoundID.Item73;

            Item.shoot = ModContent.ProjectileType<fire>(); // fire 투사체를 쏜다
            Item.shootSpeed = 10f; // 기본 속도다 (fire 내부에서 선회/호밍하니 너무 높게 안 준다)
           
            Item.value = Item.buyPrice(gold: 32);
            if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
            {
                Item.rare = calamity.Find<ModRarity>("Violet").Type;

            }
            else
            {
                Item.rare = ItemRarityID.Red;

            }
        }

        public override bool CanUseItem(Player player)
        {
            // 잃은 체력 비례로 useTime을 36~12로 만든다
            int max = player.statLifeMax2;
            int missing = max - player.statLife;
            
            float ratio = max > 0 ? (float)missing / max : 0f; // 잃은 체력 비율이다 (0~1)
ratio = MathF.Sqrt(MathHelper.Clamp(ratio, 0f, 1f)); // 초반 급가속, 후반 완만하게 만든다
            int use = (int)MathHelper.Lerp(42f, 12f, MathHelper.Clamp(ratio, 0f, 1f));
            use = Utils.Clamp(use, 12, 42);

            Item.useTime = use; // 사용시간을 갱신한다
            Item.useAnimation = use; // 애니메이션도 같이 갱신한다

            return base.CanUseItem(player);
        }

        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
    Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.whoAmI != Main.myPlayer)
                return false; // 중복 소환 방지한다

            int count = 8; // 8개 소환한다
            float speed = Item.shootSpeed;
      
        

            // 🔹 전체 투사체가 공유하는 선회값이다
            
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count; // 원형으로 균등 분배한다
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
