using System;
using System.Linq;
using System.Numerics;
using AnoMech.Core.Game.Party;

using static AnoMech.Scenarios.Fru.LightRampant.LightRampantState;

namespace AnoMech.Scenarios.Fru.LightRampant;

internal sealed class LightRampantAiPlan(LightRampantState state)
{
    public Vector3 Preposition(PartyRole role) => (int)role switch
    {
        0 => FromSim(14.5f, 4.5f),
        1 => FromSim(11, 11),
        2 => FromSim(11, -11),
        3 => FromSim(14.5f, -4.5f),
        4 => FromSim(-14.5f, 4.5f),
        5 => FromSim(-11, 11),
        6 => FromSim(-11, -11),
        _ => FromSim(-14.5f, -4.5f),
    };
    public Vector3 Lineup(PartyRole role) => state.Index(role) switch
    {
        0 => FromSim(7, 13.7f),
        1 => FromSim(16.1f, 0),
        2 => FromSim(7, -13.7f),
        3 => FromSim(-7, -13.7f),
        4 => FromSim(-16.1f, 0),
        5 => FromSim(-7, 13.7f),
        6 => FromSim(0, -45),
        _ => FromSim(0, 45),
    };

    public Vector3 TowerSpot(PartyRole role) => state.Puddle(role) ? Lineup(role) : Tower(state.Index(role));
    public Vector3 PuddleSpot(PartyRole role, int dropped)
    {
        var p = dropped switch
        {
            0 => FromSim(0, -45),
            1 => FromSim(0, -30),
            2 => FromSim(0, -15),
            3 => FromSim(15, -15),
            4 => FromSim(30, -15),
            _ => FromSim(45, 0),
        };
        return state.Index(role) == 6 ? p : -p;
    }
    public Vector3 GroupSpot(PartyRole role) => FromSim(state.NorthGroup(role) ? 45 : -45, 0);
    public Vector3 Intermediate(PartyRole role) => FromSim(43, 10.8f) * (state.NorthGroup(role) ? 1 : -1);
    public Vector3 SafeSpot(PartyRole role, bool first)
    {
        var wideNorth = first == state.NorthOrbsFirst;
        return state.NorthGroup(role) ? FromSim(wideNorth ? 36.2f : 42, wideNorth ? 26.6f : 16.7f)
            : FromSim(wideNorth ? -42 : -36.2f, wideNorth ? -16.7f : -26.6f);
    }
    public Vector3 MiddleWait(PartyRole role) => state.NorthGroup(role)
        ? state.NorthOrbsFirst ? FromSim(16.1f, 0) : FromSim(7, 12)
        : state.NorthOrbsFirst ? FromSim(-7, -12) : FromSim(-16.1f, 0);
    public Vector3 BanishSpot(PartyRole role) => state.Pairs && (int)role < 4
        ? ClockSpot(Roles[(int)role switch { 0 => 6, 1 => 7, 2 => 4, _ => 5 }]) : ClockSpot(role);
    public static Vector3 ClockSpot(PartyRole role) => (int)role switch
    {
        0 => FromSim(18, 0),
        1 => FromSim(0, 18),
        2 => FromSim(0, -18),
        3 => FromSim(-18, 0),
        4 => FromSim(-12, -12),
        5 => FromSim(-12, 12),
        6 => FromSim(12, -12),
        _ => FromSim(12, 12),
    };

}
