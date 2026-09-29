using System.Collections.Generic;
using System.Numerics;
using AnoMech.Core.Game;
using AnoMech.Core.SimObjects;
using static AnoMech.Scenarios.Fru.FruConstants;

namespace AnoMech.Scenarios.Fru;

public sealed class FruZone : IZone
{
    public static readonly FruZone Instance = new();
    public static readonly Phase P2 = new(Instance, "P2", 105, BgmId.Usurper, InitP2Arena);

    public string Name => "Futures Rewritten";
    public uint TerritoryId => 1238;
    public Vector3 Origin => new(100f, 0f, 100f);
    public byte Level => FruConstants.Level;
    public ushort ItemLevel => 735;

    public IReadOnlyList<WaymarkLayout> WaymarkPresets { get; } =
        [new WaymarkLayout("FRU PF - Inner Circle", FruUtils.FruWaymarks)];

    public void Run(SimWorld world)
    {
        world.EnforceArenaBoundary(Geometry.ArenaRadius);
    }

    private static void InitP2Arena(SimWorld world) => world.Events.Add(1f, () =>
    {
        // Native P2 platform: ContentDirectorManagedSG 181, slot 23.
        for (byte slot = 0; slot < MapEffect.SlotCount; slot++)
            world.Map.AddEffect(slot == 23 ? MapEffect.Show : MapEffect.Hide, slot);
    });

}
