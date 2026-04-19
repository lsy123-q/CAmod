using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
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
using Microsoft.Xna.Framework.Graphics;
namespace CAmod.Items.Weapons
{

    
    public class UltraCosmicDivide : ModItem
    {
        public Vector2 target;
        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 20;
            Item.damage = 5000;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 100;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 100f;
            Item.scale = 0.75f;
            if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
            {
                Item.rare = calamity.Find<ModRarity>("BurnishedAuric").Type;
            }
            else
            {
                Item.rare = ItemRarityID.Red;
            }
            Item.crit = 25;
            Item.autoReuse = true;
            Item.shootSpeed = 15f;
            Item.shoot = ModContent.ProjectileType<UltraCosmicBeam>();
            Item.value = Item.sellPrice(gold: 48);
            Item.channel = true;
        }

        public override Vector2? HoldoutOffset() => new Vector2(-20, 0);

        

        public override bool PreDrawInWorld(
    SpriteBatch spriteBatch,
    Color lightColor,
    Color alphaColor,
    ref float rotation,
    ref float scale,
    int whoAmI)
        {
            Texture2D tex = ModContent.Request<Texture2D>("CAmod/Items/Weapons/cosmic").Value;
            // 월드 드랍용 스프라이트 불러온다

            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;

            spriteBatch.Draw(
                tex,
                position,
                null,
                lightColor,
                rotation,
                origin,
                scale * 1.0f, // 크기 보정한다
                SpriteEffects.None,
                0f
            );

            return false;
            // 기존 드랍 스프라이트 그리지 않게 막는다
        }

        public override bool PreDrawInInventory(
    SpriteBatch spriteBatch,
    Vector2 position,
    Rectangle frame,
    Color drawColor,
    Color itemColor,
    Vector2 origin,
    float scale)
        {
            Texture2D tex = ModContent.Request<Texture2D>("CAmod/Items/Weapons/cosmic").Value;
            // 인벤토리에서만 사용할 별도 스프라이트 불러온다

            spriteBatch.Draw(
                tex,
                position,
                null,
                drawColor,
                0f,
                tex.Size() * 0.5f,
                scale*2.0f,
                SpriteEffects.None,
                0f
            );

            return false;
            // 기존 아이템 텍스처 그리지 않게 차단한다
        }
        public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
        {
            if (line.Mod == "Terraria" && line.Name == "ItemName")
            {
                Rarities.ArcaneCosmic.Draw(Item, line);
                return false; // 기본 드로우를 막는다
            }

            return true; // 다른 줄은 기본 드로우한다
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
{
            player.AddBuff(ModContent.BuffType<CosmicDivideCooltime>(), 60 * 25);
            SoundEngine.PlaySound(new SoundStyle("CAmod/Sounds/laser"), player.Center);

            Vector2 mouseWorld = Main.MouseWorld;

            Vector2 targetPos = target; 
    int proj = Projectile.NewProjectile(
        source,
        mouseWorld,
        Vector2.Zero,
        type,
        damage,
        knockback,
        player.whoAmI,
        mouseWorld.X,
        mouseWorld.Y
    );
    // ai에 시작점 저장한다

    Main.projectile[proj].localAI[0] = targetPos.X;
    Main.projectile[proj].localAI[1] = targetPos.Y;
    // localAI에 목표점 저장한다

    return false;
}

        public override bool CanUseItem(Player player)
        {
            // 이미 쿨타임 디버프 있으면 사용 불가능하다
            if (player.HasBuff(ModContent.BuffType<CosmicDivideCooltime>()))
                return false;

            return base.CanUseItem(player);
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            Vector2 mouseWorld = Main.MouseWorld;
            // 커서 월드 좌표 가져온다

         
            // 현재 화면의 최상단 Y좌표다

            // X축 기준 ±100px 랜덤 오차 만든다

            float topY = Main.screenPosition.Y - 100f;
            float randomOffsetX = Main.rand.NextFloat(-200f, 200f);

            Vector2 targetPos = new Vector2(mouseWorld.X + randomOffsetX, topY);
            target = targetPos;

            velocity = targetPos - position;
            velocity.Normalize();
            velocity *= Item.shootSpeed;
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.SpaceGun).
                AddIngredient(ItemID.HeatRay).
                AddIngredient(ItemID.FragmentSolar, 6).
                AddTile(TileID.LunarCraftingStation).
                Register();
        }
    }
    }