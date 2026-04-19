using System;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

public static class CalamityShakeExtension
{
    private static MethodInfo setShakeMethod;

    private static void Init()
    {
        if (setShakeMethod != null)
            return;
        // 이미 초기화됐으면 스킵한다

        Mod calamity = ModLoader.GetMod("CalamityMod");
        if (calamity == null)
            return;
        // 칼라미티 없으면 종료한다

        Type utils = calamity.Code.GetType("CalamityMod.CalamityUtils");
        if (utils == null)
            return;
        // 유틸 못 찾으면 종료한다

        setShakeMethod = utils.GetMethod("SetScreenshake", BindingFlags.Public | BindingFlags.Static);
        // SetScreenshake 메서드를 캐싱한다
    }

    public static void SetScreenshake(this Player player, float power)
    {
        Init();
        // 리플렉션 초기화한다

        if (setShakeMethod == null)
            return;
        // 실패하면 아무것도 안한다

        setShakeMethod.Invoke(null, new object[] { player, power });
        // 칼라미티 방식 지진을 건다
    }
}