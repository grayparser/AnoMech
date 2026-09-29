using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AnoMech.Core.Game.Party;

namespace AnoMech.Scenarios.Fru.CrystallizeTime;

internal enum CrystallizeTimeAssignment { AeroWest, IceWest, Eruption, Ice, Darkness, Water, AeroEast, IceEast }
internal enum CrystallizeTimeCorner { NW, NE, SE, SW }
internal enum CrystallizeTimePlayerPattern { Random, RedIce, RedAero, BlueDark, BlueStack }

internal sealed class CrystallizeTimeState
{
    internal CrystallizeTimeState ForRun()
    {
        var state = (CrystallizeTimeState)MemberwiseClone();
        state.Puddles = []; state.AeroSources = []; state.Cleansed = []; state.Intercepted = [];
        return state;
    }

    public AnoMech.Core.SimObjects.SimEnemy? Boss { get; set; }

    public Dictionary<CrystallizeTimeAssignment, (Vector3 Position, AnoMech.Core.SimObjects.SimEventObject? Object, float Created, PartyRole Creator)> Puddles { get; private set; } = [];
    public Dictionary<PartyRole, Vector3> AeroSources { get; private set; } = [];
    public Queue<PartyRole> Cleansed { get; private set; } = [];
    public Queue<PartyRole> Intercepted { get; private set; } = [];
    public const float ReferenceScale = 10f / 23.74f;
    public const float RunSpeed = 6f;
    public bool SlowNorthwest { get; }
    public CrystallizeTimeCorner Corner { get; }
    public bool East => Corner is CrystallizeTimeCorner.NE or CrystallizeTimeCorner.SE;
    public bool North => Corner is CrystallizeTimeCorner.NW or CrystallizeTimeCorner.NE;
    public PartyRole[] Roles { get; }
    public PartyRole[] Quietus { get; }
    public PartyRole JumpTarget { get; }

    public CrystallizeTimeState(bool slowNorthwest, CrystallizeTimeCorner corner, PartyRole[] roles, PartyRole[] quietus, PartyRole jumpTarget)
    {
        if (roles.Length != 8 || roles.Distinct().Count() != 8) throw new ArgumentException("CT needs all eight distinct roles.");
        SlowNorthwest = slowNorthwest;
        Corner = corner;
        Roles = (PartyRole[])roles.Clone();
        Quietus = (PartyRole[])quietus.Clone();
        JumpTarget = jumpTarget;
    }

    public static CrystallizeTimeState Randomize(Random random, bool mur, PartyRole? playerRole = null,
        CrystallizeTimePlayerPattern playerPattern = CrystallizeTimePlayerPattern.Random)
    {
        var roles = Enum.GetValues<PartyRole>();
        random.Shuffle(roles);
        if (playerRole is { } player && playerPattern != CrystallizeTimePlayerPattern.Random)
        {
            CrystallizeTimeAssignment[] choices = playerPattern switch
            {
                CrystallizeTimePlayerPattern.RedIce => [CrystallizeTimeAssignment.IceWest, CrystallizeTimeAssignment.IceEast],
                CrystallizeTimePlayerPattern.RedAero => [CrystallizeTimeAssignment.AeroWest, CrystallizeTimeAssignment.AeroEast],
                CrystallizeTimePlayerPattern.BlueDark => [CrystallizeTimeAssignment.Darkness],
                CrystallizeTimePlayerPattern.BlueStack => [CrystallizeTimeAssignment.Ice, CrystallizeTimeAssignment.Water, CrystallizeTimeAssignment.Eruption],
                _ => throw new ArgumentOutOfRangeException(nameof(playerPattern)),
            };
            var current = Array.IndexOf(roles, player);
            if (current < 0) throw new ArgumentOutOfRangeException(nameof(playerRole));
            var chosen = (int)choices[random.Next(choices.Length)];
            (roles[current], roles[chosen]) = (roles[chosen], roles[current]);
        }
        // Apply side priority after fixing the player's debuff category. A red
        // selection chooses Ice/Aero, not a side that could contradict NA priority.
        PartyRole[] priority = mur
            ? [PartyRole.RegenHealer, PartyRole.PhysRangedDps, PartyRole.MeleeDpsA, PartyRole.MainTank,
                PartyRole.OffTank, PartyRole.MeleeDpsB, PartyRole.CasterDps, PartyRole.ShieldHealer]
            : [PartyRole.ShieldHealer, PartyRole.RegenHealer, PartyRole.OffTank, PartyRole.MainTank,
                PartyRole.MeleeDpsA, PartyRole.MeleeDpsB, PartyRole.PhysRangedDps, PartyRole.CasterDps];
        foreach (var (west, east) in new[] { (0, 6), (1, 7) })
            if (Array.IndexOf(priority, roles[west]) > Array.IndexOf(priority, roles[east]))
                (roles[west], roles[east]) = (roles[east], roles[west]);
        var quietus = Enum.GetValues<PartyRole>();
        random.Shuffle(quietus);
        return new(random.Next(2) == 0, (CrystallizeTimeCorner)random.Next(4), roles, quietus[..3], (PartyRole)random.Next(8));
    }

    public PartyRole Role(CrystallizeTimeAssignment assignment) => Roles[(int)assignment];
    public CrystallizeTimeAssignment Assignment(PartyRole role) => (CrystallizeTimeAssignment)Array.IndexOf(Roles, role);
    public static Vector3 Ref(float north, float east) => new(east * ReferenceScale, 0, -north * ReferenceScale);
    // Native CT layout slots 34..39: N, NE, SE, S, SW, NW at radius eleven.
    public static Vector3 Hourglass(int index)
    {
        var angle = index * MathF.PI / 3f;
        return new(11f * MathF.Sin(angle), 0, -11f * MathF.Cos(angle));
    }
    public int[] HourglassPair(int wave) => wave == 0 ? [0, 3]
        : (wave == 2) == SlowNorthwest ? [5, 2] : [1, 4];

    public Vector3 WaveOrigin(bool second, int step)
        => second ? new(0, 0, (North ? -1 : 1) * (20 - 10 * step))
            : new((East ? 1 : -1) * (20 - 10 * step), 0, 0);
    public Vector3 WaveDirection(bool second)
        => second ? new(0, 0, North ? 1 : -1) : new(East ? -1 : 1, 0, 0);
    public static bool InWave(Vector3 point, Vector3 origin, Vector3 direction)
    {
        var offset = point - origin;
        var forward = Vector3.Dot(offset, direction);
        var side = offset.X * direction.Z - offset.Z * direction.X;
        return forward >= 0 && forward <= 10 && MathF.Abs(side) <= 20;
    }

}
