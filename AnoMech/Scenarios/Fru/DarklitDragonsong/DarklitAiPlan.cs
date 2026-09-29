using System;
using System.Linq;
using System.Numerics;
using AnoMech.Core.Game.Party;

using static AnoMech.Scenarios.Fru.DarklitDragonsong.DarklitState;

namespace AnoMech.Scenarios.Fru.DarklitDragonsong;

internal sealed class DarklitAiPlan(DarklitState state)
{
    private static Vector3 Quadrant(int i, float north, float east) => Convert(i % 4 < 2 ? north : -north, i % 4 is 1 or 2 ? east : -east);
    public static Vector3 Middle(PartyRole role) => Convert((int)role % 2 * 0.3f, (int)role * 0.08f);
    public static Vector3 OpeningSpread(PartyRole role) => (int)role switch
    {
        0 => Convert(16.4f, 10.1f),
        1 => Convert(10.1f, 16.4f),
        2 => Convert(10.1f, -16.4f),
        3 => Convert(16.4f, -10.1f),
        4 => Convert(-16.4f, 10.1f),
        5 => Convert(-10.1f, 16.4f),
        6 => Convert(-10.1f, -16.4f),
        _ => Convert(-16.4f, -10.1f),
    };
    public static Vector3 Lineup(PartyRole role) => (int)role switch
    {
        0 => Convert(4.5f, 5),
        1 => Convert(0.2f, 11.3f),
        2 => Convert(0.2f, -11.3f),
        3 => Convert(4.5f, -5),
        4 => Convert(-25.5f, 5),
        5 => Convert(-21.2f, 11.3f),
        6 => Convert(-21.2f, -11.3f),
        _ => Convert(-25.5f, -5),
    };
    public Vector3 Bowtie(PartyRole role, bool swapped = true)
    {
        var i = state.Index(role, swapped);
        return i < 4 ? Quadrant(i, 19, 5.2f) : Quadrant(i, 6.7f, 14.6f);
    }
    public Vector3 Spirit(PartyRole role)
    {
        var i = state.Index(role);
        if (i < 4) return Quadrant(i, 19, 17.2f);
        // Supports/DPS keep their spread identities even after a water swap.
        var west = i is 4 or 7;
        return Convert(0, west ? ((int)role < 4 ? -38 : -19) : ((int)role < 4 ? 19 : 0));
    }
    public Vector3 Water(PartyRole role) => Convert(state.North(role) ? 17.3f : -17.3f, state.EastWing ? -15.5f : 15.5f) + Middle(role) * 0.3f;
    public Vector3 DanceParty(PartyRole role) => Convert(state.North(role) ? 15 : -15, 0) + Middle(role) * 0.3f;
    public static Vector3 DanceTank(bool east) => Convert(0, east ? 36 : -36);
    public static Vector3 AkhMorn(PartyRole role) => Convert(role == PartyRole.OffTank ? -13 : 13, 0) + Middle(role) * 0.3f;

}
