using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AnoMech.Core.Game.Party;
using static AnoMech.Scenarios.Fru.Apocalypse.ApocalypseConstants;

namespace AnoMech.Scenarios.Fru.Apocalypse;

internal sealed class ApocalypseState
{
    internal ApocalypseState ForRun() => (ApocalypseState)MemberwiseClone();

    internal static readonly PartyRole[] Roles = Enum.GetValues<PartyRole>();
    private readonly int[] water;
    public int Rotation { get; }
    public bool Clockwise { get; }
    public AnoMech.Core.SimObjects.SimEnemy? Oracle { get; set; }
    public PartyRole BaitTank { get; set; } = PartyRole.OffTank;
    public ApocalypseState(int rotation, bool clockwise, IReadOnlyList<int> assignments)
    {
        if (rotation is < 0 or > 3) throw new ArgumentOutOfRangeException(nameof(rotation));
        if (assignments.Count != 8 || assignments.Any(x => x is < 0 or > 3)
            || Enumerable.Range(0, 4).Any(x => assignments.Count(a => a == x) != 2))
            throw new ArgumentException("Each water duration (including none) must occur twice.", nameof(assignments));
        Rotation = rotation; Clockwise = clockwise; water = assignments.ToArray();
    }
    public static ApocalypseState Randomize()
    {
        int[] durations = [0, 0, 1, 1, 2, 2, 3, 3];
        Random.Shared.Shuffle(durations);
        return new(Random.Shared.Next(4), Random.Shared.Next(2) == 0, durations);
    }
    public int Index(PartyRole role) => Array.IndexOf(Roles, role);
    public int Duration(PartyRole role) => water[Index(role)];
    public static float WaterTime(int duration) => duration switch { 1 => 23.1f, 2 => 41.9f, 3 => 50.9f, _ => throw new ArgumentOutOfRangeException(nameof(duration)) };
    public static float WaveTime(int wave) => 33.3f + 2 * wave;
    public IEnumerable<Vector3> Explosions(int wave)
    {
        if (wave < 2) yield return Vector3.Zero;
        for (var step = Math.Max(0, wave - 2); step <= wave; step++)
        {
            var angle = (Rotation * 45 + (Clockwise ? 45 : -45) * step) * MathF.PI / 180;
            var pos = new Vector3(MathF.Sin(angle), 0, -MathF.Cos(angle)) * RingRadius;
            yield return pos;
            yield return -pos;
        }
    }
    public Vector3 StartingDirection => new(MathF.Sin(Rotation * MathF.PI / 4), 0, -MathF.Cos(Rotation * MathF.PI / 4));
}
