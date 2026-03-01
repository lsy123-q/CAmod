using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using CAmod.Globals;
using CAmod.Players;
using Terraria.Audio;

namespace CAmod.Projectiles
{
    public class ArcaneHookCore : ModProjectile
    {
        public static Asset<Texture2D> ChainTex;
        public bool hookPulling;
        private const float AcquireRadius = 150f;
        private const float LaunchSpeed = 15f;
        private const float ChaseSpeed = 16f;
        private const float PullPlayerSpeed = 28f;
        private const float PullNpcSpeed = 20f;

        private int trackedNPC = -1;
        private int grabbedNPC = -1;
        private int stuckBoss = -1;
        private bool fixedState = false;
        private const int FlyTime = 60;
        private const int RecallTime = 60;
        private Vector2 stuckPos;
        private Vector2 bossOffset;
        private Vector2 grabbedOffset;
        private bool pendingTileSnap = false;
        private Point pendingTile;
        private bool recalling = false;
        private Vector2 launchStartPos;
        private Vector2 launchDir;
        private const float MaxRange = 880f;
        private bool playedFixSound = false;
        private bool jumpReleasedAfterFix = false;

        private long lastDamageSpawnTick = -1;

        public override void SetStaticDefaults()
        {
            ChainTex = ModContent.Request<Texture2D>("CAmod/Projectiles/ArcaneHookChain");
        }

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.netImportant = true;
            Projectile.extraUpdates = 1;
            Projectile.damage = 500;
        }

        public override void OnSpawn(IEntitySource source)
        {
            int rand = Main.rand.Next(3);
            SoundEngine.PlaySound(
                new SoundStyle($"CAmod/Sounds/ScorchedEarthShot{rand + 1}")
                {
                    Volume = 1f,
                    PitchVariance = 0.1f
                },
                Projectile.Center
            );

            Vector2 dir = Main.MouseWorld - Projectile.Center;
            if (dir.LengthSquared() < 0.001f)
                dir = Vector2.UnitX;

            dir.Normalize();
            launchStartPos = Projectile.Center;
            launchDir = dir;
            Projectile.velocity = dir * LaunchSpeed;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            float distFromPlayer = Vector2.Distance(player.MountedCenter, Projectile.Center);
            if (!fixedState && !recalling)
            {
                Projectile.timeLeft = 120;
            }
            if (!fixedState)
            {
                SpawnWaterDust();
            }

            if (!fixedState && !recalling && distFromPlayer >= MaxRange)
            {
                recalling = true;
                Projectile.tileCollide = false;
            }

            if (pendingTileSnap && !fixedState)
            {
                jumpReleasedAfterFix = false;
                fixedState = true;
                if (!playedFixSound)
                {
                    SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);
                    playedFixSound = true;
                }
                Projectile.velocity = Vector2.Zero;
                Projectile.tileCollide = false;

                Vector2 tileCenter = new Vector2(
                    pendingTile.X * 16f + 8f,
                    pendingTile.Y * 16f + 8f
                );

                Projectile.Center = tileCenter;
                stuckPos = tileCenter;
                pendingTileSnap = false;
                Projectile.netUpdate = true;
            }

            if (!fixedState && !pendingTileSnap && !recalling && grabbedNPC == -1 && stuckBoss == -1)
            {
                int centerTileX = (int)(Projectile.Center.X / 16f);
                int centerTileY = (int)(Projectile.Center.Y / 16f);
                float searchRadiusSq = 17f * 17f;

                for (int x = centerTileX - 2; x <= centerTileX + 2; x++)
                {
                    for (int y = centerTileY - 2; y <= centerTileY + 2; y++)
                    {
                        if (!WorldGen.InWorld(x, y))
                            continue;

                        Tile tile = Framing.GetTileSafely(x, y);

                        if (!tile.HasTile || !Main.tileSolid[tile.TileType])
                            continue;

                        Vector2 tileCenter = new Vector2(x * 16f + 8f, y * 16f + 8f);
                        float distSq = Vector2.DistanceSquared(tileCenter, Projectile.Center);

                        if (distSq <= searchRadiusSq)
                        {
                            pendingTileSnap = true;
                            pendingTile = new Point(x, y);
                            break;
                        }
                    }
                    if (pendingTileSnap)
                        break;
                }
            }

            Player p = Main.player[Projectile.owner];

            if (!p.controlJump)
            {
                jumpReleasedAfterFix = true;
            }

            var mpCheck = p.GetModPlayer<ArcaneHookPlayer>();

            if (Projectile.owner == Main.myPlayer &&
                fixedState &&
                mpCheck.hookPulling &&
                jumpReleasedAfterFix &&
                p.controlJump)
            {
                mpCheck.hookPulling = false;
                Projectile.Kill();
                return;
            }

            if (fixedState)
            {
                Projectile.timeLeft = RecallTime;
            }

            // 회수 페이즈
            if (!fixedState && recalling)
            {
                Projectile.tileCollide = false;
                Projectile.timeLeft++;
                Vector2 toPlayer = player.MountedCenter - Projectile.Center;
                float distSq = toPlayer.LengthSquared();

                if (distSq > 4f)
                {
                    toPlayer.Normalize();
                    Projectile.velocity = toPlayer * (ChaseSpeed + 6f);
                }

                float killDist = 20f;
                if (distSq <= killDist * killDist)
                {
                    Projectile.Kill();
                    return;
                }
            }
            // 날아가는 페이즈
            else if (!fixedState)
            {
                if (trackedNPC == -1 || !Main.npc[trackedNPC].active || Main.npc[trackedNPC].friendly)
                {
                    trackedNPC = FindNPCEnteredRadius();
                }

                if (trackedNPC != -1)
                {
                    NPC t = Main.npc[trackedNPC];
                    Vector2 to = t.Center - Projectile.Center;

                    if (to.LengthSquared() > 4f)
                    {
                        to.Normalize();
                        Projectile.velocity = to * ChaseSpeed;
                    }
                }

                CheckNPCHitAndFix();
            }

            // 고정 상태 처리
            if (fixedState)
            {
                if (grabbedNPC != -1)
                {
                    NPC npc = Main.npc[grabbedNPC];

                    if (!npc.active || npc.life <= 0 || npc.dontTakeDamage || npc.immortal)
                    {
                        ReleaseGrab();
                        pendingTileSnap = false;
                        fixedState = false;
                        Projectile.tileCollide = false;
                        recalling = true;
                        trackedNPC = -1;
                        Projectile.timeLeft = RecallTime;
                        Vector2 toPlayer = player.MountedCenter - Projectile.Center;
                        if (toPlayer.LengthSquared() > 0.001f)
                        {
                            toPlayer.Normalize();
                            Projectile.velocity = toPlayer * (ChaseSpeed + 6f);
                        }
                        Projectile.netUpdate = true;
                        return;
                    }
                    // 일반 NPC를 납치한 상태에서 갈고리키를 누르면 회수한다
                    if (Projectile.owner == Main.myPlayer &&
                        grabbedNPC != -1 &&
                        Main.player[Projectile.owner].controlHook)
                    {
                        var mp = player.GetModPlayer<ArcaneHookPlayer>();

                        mp.hookPulling = false; // 플레이어 끌림 종료한다
                        ReleaseGrab();          // 제압 상태 해제한다
                        fixedState = false;     // 고정 상태 해제한다
                        recalling = true;       // 회수 페이즈로 전환한다
                        Projectile.tileCollide = false; // 타일 충돌 끈다
                        Projectile.netUpdate = true;
                        return;
                    }
                    else
                    {
                        Projectile.Center = npc.Center + grabbedOffset;

                        int projType = ModContent.ProjectileType<Projectiles.ArcaneHookDamage>();
                        int damage = (int)Math.Round((float)npc.lifeMax / 10f);
                        damage = Math.Clamp(damage, 10, 1000);
                        float knockBack = 15f;
                        float speed = 0.1f;
                        int count = 8;

                        float angle = MathHelper.TwoPi * (1 + 0.5f) / count;
                        Vector2 velocity = angle.ToRotationVector2() * speed;

                        if (Main.GameUpdateCount - lastDamageSpawnTick >= 12)
                        {
                            lastDamageSpawnTick = Main.GameUpdateCount;
                            Projectile.NewProjectile(
                                player.GetSource_FromThis(),
                                npc.Center,
                                velocity,
                                projType,
                                damage,
                                knockBack,
                                player.whoAmI,
                                npc.whoAmI
                            );
                        }

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            bool bossAlive = false;
                            for (int i = 0; i < Main.maxNPCs; i++)
                            {
                                NPC b = Main.npc[i];
                                if (b.active && b.boss && b.life > 0)
                                {
                                    bossAlive = true;
                                    break;
                                }
                            }

                            bool isWormPart = npc.realLife != -1;

                            if (bossAlive || isWormPart)
                            {
                                PullPlayerToCore(player);
                            }
                            else
                            {
                                PullGrabbedNpcToPlayer(player);
                            }
                        }
                    }
                }
                else
                {
                    // 타일 또는 보스 고정
                    if (stuckBoss != -1)
                    {
                        NPC boss = Main.npc[stuckBoss];
                        if (!boss.active || boss.life <= 0 || boss.dontTakeDamage || boss.immortal)
                        {
                            stuckBoss = -1;
                            fixedState = false;
                            recalling = true;

                            Vector2 toPlayer = player.MountedCenter - Projectile.Center;
                            if (toPlayer.LengthSquared() > 0.001f)
                            {
                                toPlayer.Normalize();
                                Projectile.velocity = toPlayer * (ChaseSpeed + 6f);
                            }

                            Projectile.netUpdate = true;
                            return;
                        }

                        if (boss.active && boss.life > 0 && !boss.dontTakeDamage && !boss.immortal)
                        {
                            int projType = ModContent.ProjectileType<Projectiles.ArcaneHookDamage>();
                            int damage = (int)Math.Round((float)boss.lifeMax / 10f);
                            damage = Math.Clamp(damage, 10, 1000);
                            float knockBack = 15f;
                            float speed = 0.1f;
                            int count = 8;

                            float angle = MathHelper.TwoPi * (1 + 0.5f) / count;
                            Vector2 velocity = angle.ToRotationVector2() * speed;

                            if (Main.GameUpdateCount % 12 == 0)
                            {
                                Projectile.NewProjectile(
                                    player.GetSource_FromThis(),
                                    boss.Center,
                                    velocity,
                                    projType,
                                    damage,
                                    knockBack,
                                    player.whoAmI,
                                    boss.whoAmI
                                );
                            }
                            boss.netUpdate = true;
                        }

                        if (boss.active)
                            Projectile.Center = boss.Center + bossOffset;
                    }
                    else
                    {
                        Projectile.Center = stuckPos;
                    }

                    if (Projectile.owner == Main.myPlayer)
                        PullPlayerToCore(player);
                }
            }

            // 머리 회전
            Vector2 faceDir = Projectile.Center - player.MountedCenter;
            if (faceDir.LengthSquared() > 0.001f)
                Projectile.rotation = faceDir.ToRotation() + MathHelper.PiOver2;
        }

        private int FindNPCEnteredRadius()
        {
            int best = -1;
            float bestDistSq = AcquireRadius * AcquireRadius;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (!n.active || n.friendly || n.dontTakeDamage || n.immortal)
                    continue;

                if (IsExcludedNPC(n))
                    continue; // 제외 대상이면 스킵한다

                Vector2 closest = n.Hitbox.ClosestPointInRect(Projectile.Center);
                float dSq = Vector2.DistanceSquared(closest, Projectile.Center);

                if (dSq <= bestDistSq)
                {
                    bestDistSq = dSq;
                    best = i;
                }
            }

            return best;
        }

        private void CheckNPCHitAndFix()
        {
            if (fixedState || recalling)
                return;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (!n.active || n.friendly || n.dontTakeDamage || n.immortal)
                    continue;

                // 보스는 거리 기반 체크 (CanHitPlayer 오버라이드 대응)
                if (IsExcludedNPC(n))
                    continue; // 제외 대상이면 스킵한다

                bool hit;
                if (n.boss)
                {
                    float dist = Vector2.Distance(Projectile.Center, n.Center);
                    hit = dist <= 5f; // Signus 등의 실제 충돌 범위 고려
                }
                else
                {
                    hit = Projectile.Hitbox.Intersects(n.Hitbox);
                }

                if (!hit)
                    continue;

                jumpReleasedAfterFix = false;
                fixedState = true;
                if (!playedFixSound)
                {
                    SoundEngine.PlaySound(
                        new SoundStyle("CAmod/Sounds/MeatySlash")
                        {
                            Volume = 1f,
                            PitchVariance = 0.1f
                        },
                        Projectile.Center
                    );
                    playedFixSound = true;
                }
                Projectile.velocity = Vector2.Zero;
                Projectile.tileCollide = false;

                if (n.boss)
                {
                    stuckBoss = i;
                    // 🔥 중요: 보스 위치를 덮어쓰지 않고, 상대적 오프셋만 저장
                    bossOffset = Projectile.Center - n.Center;
                    n.netUpdate = true; // 보스 위치 동기화
                }
                else
                {
                    grabbedNPC = i;
                    // 일반 몹은 위치 동기화 (제압 상태이므로 안전)
                    n.Center = Projectile.Center;
                    grabbedOffset = Vector2.Zero;
                    var g = n.GetGlobalNPC<ArcaneSuppressionGlobalNPC>();
                    g.SetSuppressed(n, true);
                }

                Projectile.netUpdate = true;
                break;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            fixedState = true;
            if (!playedFixSound)
            {
                SoundEngine.PlaySound(
                    new SoundStyle("CAmod/Sounds/MeatySlash")
                    {
                        Volume = 1f,
                        PitchVariance = 0.1f
                    },
                    Projectile.Center
                );
                playedFixSound = true;
            }
            Projectile.velocity = Vector2.Zero;
            Projectile.tileCollide = false;
            stuckPos = Projectile.Center;
            Projectile.netUpdate = true;
            return false;
        }

        private void PullPlayerToCore(Player player)
        {
            Vector2 toCore = Projectile.Center - player.Center;
            float dist = toCore.Length();
            var mp = player.GetModPlayer<ArcaneHookPlayer>();
            if (dist > 4f)
            {
                toCore.Normalize();
                mp.hookPulling = true;
                mp.hookTarget = Projectile.Center;
                mp.hookSpeed = PullPlayerSpeed;
            }
            else
            {
                player.velocity *= 0.5f;
            }
        }

        private void PullGrabbedNpcToPlayer(Player player)
        {
            NPC n = Main.npc[grabbedNPC];
            var g = n.GetGlobalNPC<ArcaneSuppressionGlobalNPC>();
            g.SetSuppressed(n, true);

            Vector2 start = n.Center;
            Vector2 target = player.Center;
            float dist = Vector2.Distance(start, target);

            if (dist <= 4f)
            {
                n.Center = target;
                n.velocity = Vector2.Zero;
                n.netUpdate = true;
                return;
            }

            float t = MathHelper.Clamp(dist / 4250f, 0f, 1f);
            float lerpFactor = (float)Math.Pow(t, 1.25f);
            n.Center = Vector2.Lerp(start, target, lerpFactor);
            n.velocity = Vector2.Zero;
            n.netUpdate = true;
        }

        private void ReleaseGrab()
        {
            if (grabbedNPC != -1 && grabbedNPC < Main.maxNPCs)
            {
                NPC n = Main.npc[grabbedNPC];
                if (n.active)
                {
                    var g = n.GetGlobalNPC<ArcaneSuppressionGlobalNPC>();
                    g.SetSuppressed(n, false);
                    n.netUpdate = true;
                }
            }
            grabbedNPC = -1;
        }

        public override void OnKill(int timeLeft)
        {
            Player player = Main.player[Projectile.owner];
            var mp = player.GetModPlayer<ArcaneHookPlayer>();
            mp.hookPulling = false;
            ReleaseGrab();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (ChainTex == null || !ChainTex.IsLoaded)
                return true;

            Texture2D coreTex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = coreTex.Size() / 2f;
            origin.Y -= 4f;
            Player player = Main.player[Projectile.owner];
            Texture2D chainTexture = ChainTex.Value;

            Vector2 start = player.MountedCenter;
            Vector2 center = Projectile.Center;
            float angle = center.AngleTo(start) - MathHelper.PiOver2;

            Vector2 dirToPlayer = Vector2.Normalize(start - Projectile.Center);
            Vector2 end = Projectile.Center + dirToPlayer * (Projectile.width * 0.5f);

            Vector2 diff = end - start;
            float length = diff.Length();

            if (length < 1f)
                return true;

            Vector2 direction = diff / length;
            float segmentLength = chainTexture.Height;
            int segments = (int)(length / segmentLength);
            float rotation = direction.ToRotation() - MathHelper.PiOver2;

            for (int i = 0; i <= segments; i++)
            {
                Vector2 startOffset = direction * 4f;
                Vector2 drawPos = (start - startOffset) + direction * segmentLength * i;

                Color light = Lighting.GetColor(
                    (int)drawPos.X / 16,
                    (int)drawPos.Y / 16
                );

                Main.EntitySpriteDraw(
                    chainTexture,
                    drawPos - Main.screenPosition,
                    null,
                    light,
                    rotation,
                    chainTexture.Size() / 2f,
                    1f,
                    SpriteEffects.None,
                    0
                );
            }

            Main.EntitySpriteDraw(
                coreTex,
                Projectile.Center - Main.screenPosition,
                null,
                lightColor,
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None,
                0
            );
            return false;
        }
        private bool IsExcludedNPC(NPC n)
        {
            // 모드 NPC면 실제 클래스 타입명을 가져온다 (RavagerClawLeft 같은거)
            string typeName = n.ModNPC?.GetType().Name ?? string.Empty;

            // 혹시 모를 대비로 FullName도 같이 본다 (표시명 기반인 경우가 많음)
            string fullName = n.FullName ?? string.Empty;

            // 포함 검사로 제외한다
            if (typeName.Contains("RavagerClaw") || typeName.Contains("RockPillar"))
                return true;

            if (fullName.Contains("RavagerClaw") || fullName.Contains("RockPillar"))
                return true;

            return false;
        }
        private void SpawnWaterDust()
        {
            for (int i = 0; i < 10; i++)
            {
                int d = Dust.NewDust(
                    Projectile.Center - new Vector2(10f),
                    20,
                    20,
                    DustID.Water,
                    Main.rand.NextFloat(-5f, 5f),
                    Main.rand.NextFloat(-5f, 5f),
                    150,
                    default,
                    1.2f
                );
                Main.dust[d].noGravity = false;
            }
        }

        private void SpawnBloodDust()
        {
            for (int i = 0; i < 40; i++)
            {
                int d = Dust.NewDust(
                    Projectile.Center,
                    20,
                    20,
                    DustID.Blood,
                    Main.rand.NextFloat(-5f, 5f),
                    Main.rand.NextFloat(-5f, 5f),
                    100,
                    default,
                    1.8f
                );
                Main.dust[d].noGravity = false;
            }
        }
    }
}