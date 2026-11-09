using Terraria;

public static class CalamityShakeExtension
{
    public static void SetScreenshake(this Player player, float power)
    {
        global::CalamityMod.CalamityUtils.SetScreenshake(player, power);
    }
}
