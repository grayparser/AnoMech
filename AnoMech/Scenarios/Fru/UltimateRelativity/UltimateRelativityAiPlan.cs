using System;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using AnoMech.Core.Game.Party;

using static AnoMech.Scenarios.Fru.UltimateRelativity.UltimateRelativityState;

namespace AnoMech.Scenarios.Fru.UltimateRelativity;

internal sealed class UltimateRelativityAiPlan(UltimateRelativityState state)
{
    public Vector3 Bait(RelativityAssignment a)
    {
        var dir = state.Direction(a);
        var tangent = Rotate(dir, MathF.PI / 2);
        return dir * (23 / 2.358f) + tangent * (state.Clockwise(state.Clock(a)) ? -1 : 1) * (4.1f / 2.358f);
    }
    public Vector3 FireSpot(PartyRole role, int wave, bool intermediate = false)
    {
        var a = state.Assignment(role);
        if (wave == 0 && a == RelativityAssignment.ShortSupport && !state.DpsIce) return state.At(a, 20.5f / 2.358f);
        return state.At(a, FireOrder(a) == wave ? (intermediate ? 20.5f : 32.5f) / 2.358f : 3 / 2.358f);
    }
    public Vector3 BaitSpot(PartyRole role, int wave)
    {
        var a = state.Assignment(role);
        if (Wave(state.Clock(a)) == wave) return Bait(a);
        if (wave == 0) return state.At(a, Eruption(a) ? 20.5f / 2.358f : 3 / 2.358f);
        if (wave == 1 && a is RelativityAssignment.MediumDps or RelativityAssignment.MediumSupport) return state.At(a, 10.25f / 2.358f);
        return state.At(a, 3 / 2.358f);
    }
    public Vector3 CenterSpot(PartyRole role) => state.At(state.Assignment(role), 3 / 2.358f);
    public Vector3 FinalSpot(PartyRole role) => state.At(state.Assignment(role), 0.5f);

}
