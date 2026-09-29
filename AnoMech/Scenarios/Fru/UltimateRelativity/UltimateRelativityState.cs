using System;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using AnoMech.Core.Game.Party;

namespace AnoMech.Scenarios.Fru.UltimateRelativity;

internal enum RelativityAssignment
{
    ShortDpsWest, ShortDpsEast, ShortSupport, MediumDps, MediumSupport,
    LongSupportWest, LongSupportEast, LongDps,
}

// NA priorities and routes adapted from FRU-Sim, copyright 2025 William Craycroft (GPL-3.0):
// https://github.com/WCGH/FRU-Sim/tree/2a77c857ce1bb6eb472a59a95c7544faba01c55a/scenes/p3
internal sealed class UltimateRelativityState
{
    internal UltimateRelativityState ForRun() => (UltimateRelativityState)MemberwiseClone();

    public AnoMech.Core.SimObjects.SimEnemy? Boss { get; set; }

    public static readonly PartyRole[] Roles = Enum.GetValues<PartyRole>();
    public static readonly RelativityAssignment[] Assignments = Enum.GetValues<RelativityAssignment>();
    private static readonly int[] Clocks = [5, 3, 0, 2, 6, 7, 1, 4];
    private static readonly PartyRole[] DpsPriority = [Roles[7], Roles[6], Roles[4], Roles[5]];
    private static readonly PartyRole[] SupportPriority = [Roles[3], Roles[2], Roles[0], Roles[1]];
    private readonly PartyRole[] roles;
    private readonly RelativityAssignment[] darkness;
    public int North { get; }
    public int ClockwiseMask { get; }
    public bool DpsIce { get; }
    public PartyRole ShellTarget { get; }
    public PartyRole Role(RelativityAssignment assignment) => roles[(int)assignment];
    public RelativityAssignment Assignment(PartyRole role) => (RelativityAssignment)Array.IndexOf(roles, role);
    public RelativityAssignment Ice => DpsIce ? RelativityAssignment.LongDps : RelativityAssignment.ShortSupport;
    public RelativityAssignment Darkness(int wave) => darkness[wave];
    public int Clock(RelativityAssignment assignment) => Clocks[(int)assignment];
    public bool Clockwise(int clock) => (ClockwiseMask & (1 << clock)) != 0;
    public static int Wave(int clock) => clock switch { 7 or 1 or 4 => 0, 5 or 3 or 0 => 1, _ => 2 };
    public static int FireOrder(RelativityAssignment assignment) => assignment switch
    {
        RelativityAssignment.ShortDpsWest or RelativityAssignment.ShortDpsEast or RelativityAssignment.ShortSupport => 0,
        RelativityAssignment.MediumDps or RelativityAssignment.MediumSupport => 1,
        _ => 2
    };
    public bool Fire(RelativityAssignment assignment, int wave) => assignment != Ice && FireOrder(assignment) == wave;
    public static bool EarlyReturn(RelativityAssignment assignment) => (int)assignment < 5;
    public static bool Eye(RelativityAssignment assignment) => !EarlyReturn(assignment);
    public static bool Eruption(RelativityAssignment assignment) => EarlyReturn(assignment) && assignment != RelativityAssignment.MediumDps;
    public static RelativityAssignment[] ValidDarkness(int wave) => Assignments.Where(a => wave switch
    {
        0 => (int)a >= 3,
        1 => a is not RelativityAssignment.MediumDps and not RelativityAssignment.MediumSupport,
        _ => (int)a < 5,
    }).ToArray();

    // The two unique roles are [long DPS, medium DPS] / [short support, medium support].
    // Remaining pairs are ordered by the NA priority, not by their shuffled indices.
    public UltimateRelativityState(PartyRole longDps, PartyRole mediumDps, PartyRole shortSupport,
        PartyRole mediumSupport, bool dpsIce, int north, int clockwiseMask, int[] darknessChoices, PartyRole shellTarget)
    {
        if (!DpsPriority.Contains(longDps) || !DpsPriority.Contains(mediumDps) || longDps == mediumDps
            || !SupportPriority.Contains(shortSupport) || !SupportPriority.Contains(mediumSupport) || shortSupport == mediumSupport)
            throw new ArgumentException("Two distinct DPS and support roles are required.");
        if (north is < 0 or > 7 || clockwiseMask is < 0 or > 255) throw new ArgumentOutOfRangeException(nameof(north));
        if (darknessChoices.Length != 3 || !Roles.Contains(shellTarget)) throw new ArgumentException("Invalid mechanic targets.");
        var dps = DpsPriority.Where(r => r != longDps && r != mediumDps).ToArray();
        var supports = SupportPriority.Where(r => r != shortSupport && r != mediumSupport).ToArray();
        roles = [dps[0], dps[1], shortSupport, mediumDps, mediumSupport, supports[0], supports[1], longDps];
        darkness = new RelativityAssignment[3];
        for (var wave = 0; wave < 3; wave++)
        {
            var valid = ValidDarkness(wave).Except(darkness.Take(wave)).ToArray();
            darkness[wave] = valid[Math.Clamp(darknessChoices[wave], 0, valid.Length - 1)];
        }
        DpsIce = dpsIce; North = north; ClockwiseMask = clockwiseMask; ShellTarget = shellTarget;
    }
    public static UltimateRelativityState Random(Random random)
    {
        var dps = DpsPriority.OrderBy(_ => random.Next()).ToArray();
        var supports = SupportPriority.OrderBy(_ => random.Next()).ToArray();
        var choices = new int[3];
        var selected = new List<RelativityAssignment>();
        for (var wave = 0; wave < 3; wave++)
        {
            var valid = ValidDarkness(wave).Except(selected).ToArray();
            choices[wave] = random.Next(valid.Length);
            selected.Add(valid[choices[wave]]);
        }
        return new(dps[0], dps[1], supports[0], supports[1], random.Next(2) == 0, random.Next(8), random.Next(256),
            choices, Roles[random.Next(8)]);
    }
    public static Vector3 Rotate(Vector3 v, float radians) => new(v.X * MathF.Cos(radians) - v.Z * MathF.Sin(radians),
        v.Y, v.X * MathF.Sin(radians) + v.Z * MathF.Cos(radians));
    public Vector3 Direction(int clock) => new(MathF.Sin((clock + North) * MathF.PI / 4), 0, -MathF.Cos((clock + North) * MathF.PI / 4));
    public Vector3 Direction(RelativityAssignment a) => Direction(Clock(a));
    public Vector3 At(RelativityAssignment a, float radius) => Direction(a) * radius;
    public Vector3 Hourglass(int clock) => Direction(clock) * 9.5f;
    // Scenery origin lies 10.5 yalms on the opposite side; the authored model
    // has a -20 local Z offset. Slot 26 therefore renders the north hourglass.
    public byte HourglassSlot(int clock) => (byte)(26 + (clock + North) % 8);
    public static bool InsideBeam(Vector3 position, Vector3 origin, Vector3 direction, float length)
    {
        var offset = position - origin;
        var forward = Vector3.Dot(offset, direction);
        return forward >= 0 && forward <= length && MathF.Abs(offset.X * direction.Z - offset.Z * direction.X) <= 2.5f;
    }
}
