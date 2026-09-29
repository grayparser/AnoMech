using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AnoMech.Core.Game.Party;
using static AnoMech.Scenarios.Fru.DiamondDust.DiamondDustConstants;

using static AnoMech.Scenarios.Fru.DiamondDust.DiamondDustState;

namespace AnoMech.Scenarios.Fru.DiamondDust;

internal sealed class DiamondDustAiPlan(DiamondDustState state)
{
    private static readonly int[] Clocks = [0, 2, 6, 4, 5, 3, 7, 1];

    public static bool GroupOne(PartyRole role) => ((int)role & 1) == 0;

    public int Clock(PartyRole role)
    {
        var original = Clocks[(int)role];
        var requiredParity = (state.FirstIcicle + (state.Marked(role) ? 1 : 0)) & 1;
        return (original + ((original & 1) == requiredParity ? 0 : (int)role < 4 ? 7 : 1)) % 8;
    }
    public Vector3 KickSpot(PartyRole role) => Point(Clock(role), (state.Axe ? 16 : 0) + (state.Marked(role) ? 3 : 1));
    public Vector3 StoneSpot(PartyRole role) => Point(Clock(role), state.Marked(role) ? state.Axe ? 19 : 8 : state.Axe ? 4 : 1);
    public int KnockbackOctant(PartyRole role)
    {
        var g1 = state.FirstIcicle == 0 ? 0 : state.FirstIcicle + 4;
        return (g1 + (GroupOne(role) ? 0 : 4)) % 8;
    }
    public Vector3 KnockbackSpot(PartyRole role) => Point(KnockbackOctant(role), 6);

    private int Rotation(PartyRole role)
    {
        if (state.Cursed) return 1;
        var clockwiseDestination = Point(KnockbackOctant(role) + 2, 1);
        return Vector3.Dot(clockwiseDestination, state.ReflectionPosition) < 0 ? 1 : -1;
    }
    public Vector3 HolySpot(PartyRole role, int step) => Point(KnockbackOctant(role) + (state.Cursed ? 1f / 3 : 0) + Rotation(role) * step * 0.5f, 18);
    public Vector3 IceSpot(PartyRole role) => HolySpot(role, state.Cursed ? 5 : 4) * (17f / 18);

    // A fixed-length ice slide, checked against the entire swept path through
    // the actual puddles. Prefer a wide margin from the wall and cone edge.
    public Vector3? SlideDestination(Vector3 from, bool behind, IReadOnlyList<Vector3> puddles, bool prepareNextSlide = false)
    {
        Vector3? best = null;
        var bestMargin = float.NegativeInfinity;
        for (var degree = 0; degree < 720; degree++)
        {
            var end = from + Point(degree / 90f, SlideDistance);
            if (end.Length() > 19 || state.BehindReflection(end) != behind) continue;
            var clearance = puddles.Count == 0 ? 20 : puddles.Min(p => DistanceToSegment(p, from, end) - 6);
            if (clearance < 0.35f) continue;
            var relative = end - state.ReflectionPosition;
            var backDot = Vector3.Dot(Vector3.Normalize(relative), Vector3.Normalize(state.ReflectionPosition));
            var coneMargin = (behind ? backDot - MathF.Sqrt(0.5f) : MathF.Sqrt(0.5f) - backDot) * relative.Length();
            var margin = MathF.Min(MathF.Min(20 - end.Length(), clearance), coneMargin);
            if (margin > bestMargin && (!prepareNextSlide || SlideDestination(end, true, puddles) != null))
            { bestMargin = margin; best = end; }
        }
        return best;
    }
}
