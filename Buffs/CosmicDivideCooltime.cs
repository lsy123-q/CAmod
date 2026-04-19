using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;
using System;
namespace CAmod.Buffs
{
    public class CosmicDivideCooltime : ModBuff
    {
        public override void SetStaticDefaults()
        {
            

            Main.buffNoTimeDisplay[Type] = false;
            // 남은시간 표시 활성화한다

            Main.debuff[Type] = true;
            // 디버프로 취급한다

            Main.buffNoSave[Type] = true;
            // 저장 안된다 (재접시 초기화)

            Main.buffNoTimeDisplay[Type] = false;
            // 시간 표시 확실히 켠다
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.buffTime[buffIndex] == 1)
            {
                Terraria.Audio.SoundEngine.PlaySound(
                    new Terraria.Audio.SoundStyle("CAmod/Sounds/OmegaBlueRecharge"),
                    player.Center
                );


                
            
               

                    for (int i = 0; i < 75; i++)
                    {
                        float t = MathHelper.TwoPi * i / 60;
                        // 현재 각도다
                        Vector2 length = new Vector2(
                            (float)Math.Cos(t) * 10f + (float)Math.Cos(t)*15f, 
                            (float)Math.Sin(t)*5f);

                    Vector2 length2 = new Vector2(
                        (float)Math.Cos(t) * 5f,
                        (float)Math.Sin(t) * 10f + (float)Math.Sin(t) * 15f);

                    length = length.RotatedBy(MathHelper.ToRadians(45f));
                  
                    length2 = length2.RotatedBy(MathHelper.ToRadians(45f));



                    Dust dog = Dust.NewDustPerfect(
                                            player.Center,
                                            DustID.ShadowbeamStaff,
                                            length
                                        );

                    dog.scale *= 1.3f;
                    dog.fadeIn = 1.5f;
                    dog.noGravity = true;
                    dog.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                    Dust dog2 = Dust.NewDustPerfect(
                                           player.Center,
                                           DustID.ShadowbeamStaff,
                                           length2
                                       );
                    dog2.scale *= 1.3f;
                    dog2.fadeIn = 1.5f;
                    dog2.noGravity = true;
                    dog2.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                }



                // 남은시간 1초일때 사운드 재생한다
            }
        }
    }
}