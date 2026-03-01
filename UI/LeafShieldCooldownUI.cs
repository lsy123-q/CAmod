using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using ReLogic.Content;
using CAmod.Players;
using CAmod.Configs;
namespace CAmod.UI
{
    public class LeafShieldCooldownUI : UIState
    {
        private Asset<Texture2D> icon;
        public Vector2 PositionOffset; // 외부에서 밀어주는 위치값이다
        private Vector2 position; // 컨피그 값을 따라간다 // 기본 위치
        private bool dragging = false;
        private Vector2 dragOffset;
        private Asset<Texture2D> mask;
        public override void OnInitialize()
        {
            icon = ModContent.Request<Texture2D>("CAmod/UI/LeafShield_cool");
            mask = ModContent.Request<Texture2D>("CAmod/UI/LeafShield_cool_mask");


        }
        private Vector2 GetDrawPosition()
        {
            return position + PositionOffset; // 실제 화면에 그려지는 위치를 반환한다
        }
        public bool IsVisible()
        {
            Player player = Main.LocalPlayer;
            var mp = player.GetModPlayer<LeafWardPlayer>();

            // 지금 Draw에서 return하는 조건 그대로 복붙한다
            return mp.leafShieldEquipped || mp.leafShieldCooldown > 0f;
        }
        public override void Update(GameTime gameTime)
        {
            Player player = Main.LocalPlayer;
            var mp = player.GetModPlayer<LeafWardPlayer>();

            Texture2D tex = icon.Value;
            Vector2 drawPos = GetDrawPosition();

            Rectangle hitbox = new Rectangle(
                (int)drawPos.X,
                (int)drawPos.Y,
                tex.Width,
                tex.Height
            );

            // 좌클릭 시작
            // 좌클릭 시작
            if (Main.mouseLeft && Main.mouseLeftRelease && hitbox.Contains(Main.MouseScreen.ToPoint()))
            {
                dragging = true;
                dragOffset = drawPos - Main.MouseScreen; // 실제 그려진 위치 기준으로 잡는다
            }

            // 드래그 중
            if (dragging && Main.mouseLeft)
            {
                position = Main.MouseScreen + dragOffset - PositionOffset;
            }

            // 마우스 떼면 종료
            if (!Main.mouseLeft)
            {
                dragging = false;
            }

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Player player = Main.LocalPlayer;
            var mp = player.GetModPlayer<LeafWardPlayer>();

            if (mp.leafShieldCooldown <= 0f)
                return; // 쿨타임이 없으면 UI를 그리지 않는다


            var config = ModContent.GetInstance<UIConfig>();
            float convertedY = Main.screenHeight - config.UIPosY - icon.Value.Height;
            position = new Vector2(config.UIPosX, convertedY);
            Texture2D tex = icon.Value;
            Texture2D tex2 = mask.Value;
            // 항상 아이콘 기본 그림
            float alpha = 0.75f;
            spriteBatch.Draw(tex, position + PositionOffset, Color.White * alpha); ;

            Vector2 drawPos = GetDrawPosition();
            Rectangle hitbox = new Rectangle(
                (int)drawPos.X,
                (int)drawPos.Y,
                tex.Width,
                tex.Height
            );
            if (hitbox.Contains(Main.MouseScreen.ToPoint()))
            {
                Main.LocalPlayer.mouseInterface = true; // 아이템 클릭 방지

                Main.hoverItemName = "Arcane Evolving : The First Dryad's Blessing";


            }

            // 쿨이 있을 때만 마스크 표시
            if (mp.leafShieldCooldown > 0f)
            {
                float ratio = (float)mp.leafShieldCooldown / mp.leafShieldCooldownmax;


                int maskHeight = (int)(tex2.Height * ratio);

                Rectangle source = new Rectangle(
                                    0,
                                    0,
                                    tex2.Width,
                                    maskHeight
                                );

                Rectangle dest = new Rectangle(
                    (int)(position.X + PositionOffset.X),
                    (int)(position.Y + PositionOffset.Y),
                    tex2.Width,
                    maskHeight
                );
                spriteBatch.Draw(tex2, dest, source, Color.Black * 0.65f);



            }
        }
    }
}
