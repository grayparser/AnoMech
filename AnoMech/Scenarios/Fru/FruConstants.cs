namespace AnoMech.Scenarios.Fru;

// All IDs and tunables for Futures Rewritten (Ultimate), in one flat class.
// One name per value. Values in decimal.
public static class FruConstants
{
    public const byte Level = 100;

    public static class BgmId
    {
        // Return to Oblivion, music/ex3/BGM_EX3_Raid_08.scd.
        public const ushort Usurper = 759;
        // The Extreme (Shadowbringers), music/ex3/BGM_EX3_Raid_12.scd.
        public const ushort Oracle = 802;
        // Promises to Keep, music/ex3/BGM_EX3_Raid_11.scd (native BGM sheet).
        public const ushort OracleAndUsurper = 801;
    }

    public static class MapEffect
    {
        // ContentDirectorManagedSG row 181: slots 0..53 include the earlier
        // platforms, ice, hourglasses, and mechanic VFX.
        public const byte SlotCount = 54;
        public const uint Show = 0x00010001;
        public const uint Hide = 0x00040004;
    }

    public static class BNpcBaseId
    {
        public const uint Helper = 9020; // 0x233C — canonical FRU helper
    }

    public static class Geometry
    {
        public const float ArenaRadius = 20f;
    }
}
