using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AnoMech.Core.Game.Party;
using static AnoMech.Scenarios.Fru.DiamondDust.DiamondDustConstants;

namespace AnoMech.Scenarios.Fru.DiamondDust;

// NA Partner Swap (Echo) presentation: retain P1 clocks, swap
// supports CCW / DPS CW only when needed, then G1 red/purple and G2 blue/yellow.
internal sealed class DiamondDustState(int firstIcicle, bool supportsMarked, bool axe, int reflectionOctant, bool stillness, int gazeOctant)
{
    internal DiamondDustState ForRun() => (DiamondDustState)MemberwiseClone();

    public AnoMech.Core.SimObjects.SimEnemy? Boss { get; set; }
    public IReadOnlyList<Vector3> Puddles { get; set; } = [];

    internal static readonly PartyRole[] Roles = Enum.GetValues<PartyRole>();
    public int FirstIcicle { get; } = firstIcicle % 4;
    public bool Axe { get; } = axe;
    public bool Stillness { get; } = stillness;
    public Vector3 ReflectionPosition => Point(reflectionOctant, 12);
    public Vector3 GazePosition => Point(gazeOctant, 19);
    public static DiamondDustState Randomize() => new(Random.Shared.Next(4), Random.Shared.Next(2) == 0,
        Random.Shared.Next(2) == 0, Random.Shared.Next(8), Random.Shared.Next(2) == 0, Random.Shared.Next(8));
    public bool Marked(PartyRole role) => ((int)role < 4) == supportsMarked;
    public static Vector3 Point(float octant, float radius) => new(MathF.Sin(octant * MathF.PI / 4) * radius, 0, -MathF.Cos(octant * MathF.PI / 4) * radius);
    public IEnumerable<Vector3> Icicles(int wave)
    {
        int[] offsets = wave == 0 ? [0, 4] : wave == 1 ? [1, 3, 5, 7] : [2, 6];
        return offsets.Select(o => Point(FirstIcicle + o, 20));
    }
    public bool Cursed => (reflectionOctant - FirstIcicle + 8) % 4 == 0;
    public bool BehindReflection(Vector3 position)
    {
        var offset = position - ReflectionPosition;
        return offset.LengthSquared() > 0.001f && Vector3.Dot(Vector3.Normalize(offset), Vector3.Normalize(ReflectionPosition)) > MathF.Sqrt(0.5f);
    }
    public static float DistanceToSegment(Vector3 point, Vector3 from, Vector3 to)
    {
        var line = to - from;
        var t = line.LengthSquared() > 0 ? Math.Clamp(Vector3.Dot(point - from, line) / line.LengthSquared(), 0, 1) : 0;
        return Vector3.Distance(point, from + t * line);
    }

}
