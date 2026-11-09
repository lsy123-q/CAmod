using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.UI;
using ReLogic.Content;
using CAmod.Configs;

namespace CAmod.UI
{
    public abstract class ArcaneCooldownUI : UIState
    {
        private static readonly List<ArcaneCooldownUI> Widgets = new();
        private static ArcaneCooldownUI dragOwner;

        private Asset<Texture2D> baseTexture;
        private Asset<Texture2D> itemIcon;
        private Asset<Effect> donutShaderAsset;
        private Vector2 dragOffset;
        private bool isDragging;
        private Vector2 runtimePosition;
        private bool hasRuntimePosition;

        private const float BaseSize = 75f;
        private const float RenderTargetGap = -30f;

        protected abstract string ItemTexturePath { get; }
        protected abstract string TooltipTitle { get; }
        protected abstract float Cooldown { get; }
        protected abstract float CooldownMax { get; }
        protected virtual Vector2 IconDrawSize => new Vector2(19.8f, 25.08f);
        protected virtual string CooldownText => $"{Cooldown / 60f:F1}s";
        protected abstract Vector2 ItemPosition { get; set; }
        protected abstract Vector4 GaugeColor1 { get; }
        protected abstract Vector4 GaugeColor2 { get; }

        public static void ClearWidgets()
        {
            Widgets.Clear();
            dragOwner = null;
        }

        public override void OnInitialize()
        {
            if (!Widgets.Contains(this))
                Widgets.Add(this);

            baseTexture = ModContent.Request<Texture2D>("CAmod/UI/VampCooldown_Base");
            itemIcon = ModContent.Request<Texture2D>(ItemTexturePath);
            donutShaderAsset = ModContent.Request<Effect>("CAmod/Effects/DonutVFX");
        }

        public bool IsVisible() => Cooldown > 0f || (isDragging && Main.mouseMiddle);

        private float GetScale() => MathHelper.Clamp(
            ModContent.GetInstance<UIConfig>().ArcaneCooldownUIScale,
            0.5f,
            2f
        );

        private Rectangle GetDestination()
        {
            float scale = GetScale();
            Vector2 position = hasRuntimePosition ? runtimePosition : ItemPosition;
            int width = (int)(BaseSize * scale);
            int height = (int)(BaseSize * scale);
            return new Rectangle((int)position.X, (int)position.Y, width, height);
        }

        private Vector2 KeepOnScreen(Vector2 position, int width, int height)
        {
            position.X = MathHelper.Clamp(position.X, 0f, Main.screenWidth - width);
            position.Y = MathHelper.Clamp(position.Y, 0f, Main.screenHeight - height);
            return position;
        }

        private Rectangle GetCollisionBounds(Rectangle renderBounds)
        {
            if (RenderTargetGap >= 0f)
            {
                int margin = (int)RenderTargetGap;
                return new Rectangle(
                    renderBounds.X - margin,
                    renderBounds.Y - margin,
                    renderBounds.Width + margin * 2,
                    renderBounds.Height + margin * 2
                );
            }

            // A negative gap shrinks the collision area, allowing transparent render-target
            // edges to overlap by the requested amount before the UI is stopped or displaced.
            int inset = System.Math.Min(
                (int)-RenderTargetGap,
                (System.Math.Min(renderBounds.Width, renderBounds.Height) - 1) / 2
            );
            return new Rectangle(
                renderBounds.X + inset,
                renderBounds.Y + inset,
                renderBounds.Width - inset * 2,
                renderBounds.Height - inset * 2
            );
        }

        private Vector2 ResolveOverlap(Vector2 desired, int width, int height, float scale, bool stackToRight)
        {
            Vector2 position = KeepOnScreen(desired, width, height);

            for (int pass = 0; pass < Widgets.Count * 4; pass++)
            {
                Rectangle movingBounds = new Rectangle((int)position.X, (int)position.Y, width, height);
                bool displaced = false;

                for (int otherIndex = 0; otherIndex < Widgets.Count; otherIndex++)
                {
                    ArcaneCooldownUI other = Widgets[otherIndex];
                    if (other == this || other.Cooldown <= 0f)
                        continue;
                    Rectangle otherBounds = other.GetDestination();
                    Rectangle otherCollisionBounds = GetCollisionBounds(otherBounds);
                    if (!movingBounds.Intersects(otherCollisionBounds))
                        continue;

                    Vector2[] options = stackToRight
                        ? new[] { new Vector2(otherBounds.Right + RenderTargetGap, position.Y) }
                        : new[]
                        {
                            new Vector2(otherBounds.Right + RenderTargetGap, position.Y),
                            new Vector2(otherBounds.Left - width - RenderTargetGap, position.Y),
                            new Vector2(position.X, otherBounds.Bottom + RenderTargetGap),
                            new Vector2(position.X, otherBounds.Top - height - RenderTargetGap)
                        };

                    Vector2 best = position;
                    float bestDistance = float.MaxValue;
                    foreach (Vector2 option in options)
                    {
                        Vector2 candidate = KeepOnScreen(option, width, height);
                        Rectangle candidateBounds = new Rectangle((int)candidate.X, (int)candidate.Y, width, height);
                        if (candidateBounds.Intersects(otherCollisionBounds))
                            continue;

                        float distance = Vector2.DistanceSquared(candidate, desired);
                        if (distance < bestDistance)
                        {
                            best = candidate;
                            bestDistance = distance;
                        }
                    }

                    if (bestDistance < float.MaxValue)
                    {
                        position = best;
                        displaced = true;
                        break;
                    }
                }

                if (!displaced)
                    break;
            }

            return position;
        }

        private void EnsureInitialPosition()
        {
            if (hasRuntimePosition || Cooldown <= 0f)
                return;

            Rectangle destination = GetDestination();
            runtimePosition = ResolveOverlap(
                ItemPosition,
                destination.Width,
                destination.Height,
                GetScale(),
                stackToRight: true
            );
            hasRuntimePosition = true;
        }

        public override void Update(GameTime gameTime)
        {
            if (baseTexture == null || Main.gameMenu)
                return;

            if (Cooldown <= 0f)
            {
                if (isDragging && Main.mouseMiddle)
                {
                    base.Update(gameTime);
                    return;
                }

                hasRuntimePosition = false;
                if (isDragging || dragOwner == this)
                {
                    isDragging = false;
                    dragOwner = null;
                    TrySaveConfig();
                }
                base.Update(gameTime);
                return;
            }

            if (dragOwner == null)
                EnsureInitialPosition();

            base.Update(gameTime);
        }

        private void TrySaveConfig()
        {
            try
            {
                ModContent.GetInstance<UIConfig>().SaveChanges(silent: true);
            }
            catch
            {
                // Keep the current position if saving fails.
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            float cooldown = Cooldown;
            float cooldownMax = CooldownMax;
            bool heldAfterCooldown = isDragging && Main.mouseMiddle;
            if ((cooldown <= 0f || cooldownMax <= 0f) && !heldAfterCooldown)
                return;

            EnsureInitialPosition();

            Effect shader = donutShaderAsset?.Value;
            if (shader == null || baseTexture == null || itemIcon == null)
                return;

            Rectangle destination = GetDestination();
            HandlePointerInput(ref destination);
            float progress = cooldownMax > 0f
                ? MathHelper.Clamp(1f - cooldown / cooldownMax, 0f, 1f)
                : 1f;
            Vector4 color1 = GaugeColor1;
            Vector4 color2 = GaugeColor2;

            shader.Parameters["BackgroundColor"]?.SetValue(new Vector4(0.12f, 0.12f, 0.12f, 1f));
            shader.Parameters["Progress"]?.SetValue(progress);
            Vector2 iconDrawSize = IconDrawSize;
            shader.Parameters["ImageScale"]?.SetValue(iconDrawSize / BaseSize);
            UIConfig config = ModContent.GetInstance<UIConfig>();
            shader.Parameters["ImageAlpha"]?.SetValue(MathHelper.Clamp(config.CooldownUIIconAlpha, 0f, 1f));
            shader.Parameters["GlobalAlpha"]?.SetValue(MathHelper.Clamp(config.CooldownUIOverallAlpha, 0f, 1f));
            shader.Parameters["Color1"]?.SetValue(color1);
            shader.Parameters["Color2"]?.SetValue(color2);
            shader.Parameters["Color3"]?.SetValue(color2);
            shader.Parameters["Color1Position"]?.SetValue(0f);
            shader.Parameters["Color2Position"]?.SetValue(1f);
            shader.Parameters["Color3Position"]?.SetValue(1f);
            shader.Parameters["uImage1"]?.SetValue(itemIcon.Value);

            spriteBatch.End();
            try
            {
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, RasterizerState.CullCounterClockwise, shader, Main.UIScaleMatrix);
                Main.graphics.GraphicsDevice.Textures[1] = itemIcon.Value;
                Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;
                shader.CurrentTechnique.Passes[0].Apply();
                spriteBatch.Draw(baseTexture.Value, destination, null, Color.White, 0f, Vector2.Zero,
                    SpriteEffects.None, 0f);
            }
            finally
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
            }
        }

        private void HandlePointerInput(ref Rectangle destination)
        {
            Vector2 mousePosition = Main.MouseScreen;
            Point mousePoint = mousePosition.ToPoint();

            if (!isDragging && dragOwner == null && Main.mouseMiddle && Main.mouseMiddleRelease && destination.Contains(mousePoint))
            {
                isDragging = true;
                dragOwner = this;
                dragOffset = mousePosition - new Vector2(destination.X, destination.Y);
            }

            if (isDragging && dragOwner == this)
            {
                if (Main.mouseMiddle)
                {
                    Vector2 desired = mousePosition - dragOffset;
                    Vector2 userPosition = KeepOnScreen(desired, destination.Width, destination.Height);
                    Vector2 resolved = ResolveOverlap(userPosition, destination.Width, destination.Height, GetScale(), false);
                    ItemPosition = resolved;
                    runtimePosition = resolved;
                    hasRuntimePosition = true;
                    destination = GetDestination();
                    destination = GetDestination();
                }
                else
                {
                    isDragging = false;
                    dragOwner = null;
                    TrySaveConfig();
                }
            }

            if (!isDragging && destination.Contains(mousePoint))
                Main.hoverItemName = $"{TooltipTitle}: {CooldownText}";
        }
    }
}

