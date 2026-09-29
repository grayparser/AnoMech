using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AnoMech.Core.Game.Party;

using static AnoMech.Scenarios.Fru.CrystallizeTime.CrystallizeTimeState;

namespace AnoMech.Scenarios.Fru.CrystallizeTime;

internal sealed class CrystallizeTimeAiPlan(CrystallizeTimeState state)
{
    public Vector3 Rewind(PartyRole role)
    {
        var group1 = role is PartyRole.MainTank or PartyRole.RegenHealer or PartyRole.MeleeDpsA or PartyRole.PhysRangedDps;
        var tank = role is PartyRole.MainTank or PartyRole.OffTank;
        // The leading axis alternates around the corners. Exactly one tank and
        // its light party are ahead on each axis, so the two first-four sets differ.
        var leadEast = state.Corner is CrystallizeTimeCorner.NW or CrystallizeTimeCorner.SE ? group1 : !group1;
        var front = tank ? 22.7f : 20.5f;
        var back = tank ? 18.2f : 15.4f;
        return Ref((state.North ? 1 : -1) * (leadEast ? back : front), (state.East ? 1 : -1) * (leadEast ? front : back));
    }

    public static Vector3 AkhMorn(PartyRole role)
        => role == PartyRole.MainTank ? new(-9.3f, 0, 0) : Vector3.Zero;

    public IEnumerable<(float Time, IReadOnlyDictionary<CrystallizeTimeAssignment, CrystallizeTimeDestination> Route)> Routes()
    {
        yield return (14, state.SlowNorthwest ? CrystallizeTimeRoutes.PRE_HG_1_NW : CrystallizeTimeRoutes.PRE_HG_1_NE);
        yield return (18, state.SlowNorthwest ? CrystallizeTimeRoutes.POST_HG_1_NW : CrystallizeTimeRoutes.POST_HG_1_NE);
        yield return (20.7f, CrystallizeTimeRoutes.PUDDLE_DODGE);
        yield return (21.4f, state.SlowNorthwest ? CrystallizeTimeRoutes.POST_KB_NW : CrystallizeTimeRoutes.POST_KB_NE);
        yield return (23.2f, state.SlowNorthwest ? CrystallizeTimeRoutes.POST_HG_2_NW : CrystallizeTimeRoutes.POST_HG_2_NE);
        yield return (24.5f, state.East ? CrystallizeTimeRoutes.POST_UD_E : CrystallizeTimeRoutes.POST_UD_W);
        yield return (26.2f, state.East ? CrystallizeTimeRoutes.POST_EARLY_SOAK_E : CrystallizeTimeRoutes.POST_EARLY_SOAK_W);
        yield return (28.3f, state.East ? CrystallizeTimeRoutes.POST_HG_3_E : CrystallizeTimeRoutes.POST_HG_3_W);
        yield return (29.3f, state.East ? CrystallizeTimeRoutes.POST_EXA_2_E : CrystallizeTimeRoutes.POST_EXA_2_W);
        yield return (31.2f, state.Corner switch
        {
            CrystallizeTimeCorner.NW => CrystallizeTimeRoutes.POST_EXA_3_NW,
            CrystallizeTimeCorner.NE => CrystallizeTimeRoutes.POST_EXA_3_NE,
            CrystallizeTimeCorner.SE => CrystallizeTimeRoutes.POST_EXA_3_SE,
            _ => CrystallizeTimeRoutes.POST_EXA_3_SW
        });
        yield return (33.6f, state.Corner switch
        {
            CrystallizeTimeCorner.NW => CrystallizeTimeRoutes.POST_EXA_4_NW,
            CrystallizeTimeCorner.NE => CrystallizeTimeRoutes.POST_EXA_4_NE,
            CrystallizeTimeCorner.SE => CrystallizeTimeRoutes.POST_EXA_4_SE,
            _ => CrystallizeTimeRoutes.POST_EXA_4_SW
        });
    }
    public IReadOnlyDictionary<CrystallizeTimeAssignment, CrystallizeTimeDestination> AfterCleanse => state.Corner switch
    {
        CrystallizeTimeCorner.NW => CrystallizeTimeRoutes.POST_SOAK_TARGET_NW,
        CrystallizeTimeCorner.NE => CrystallizeTimeRoutes.POST_SOAK_TARGET_NE,
        CrystallizeTimeCorner.SE => CrystallizeTimeRoutes.POST_SOAK_TARGET_SE,
        _ => CrystallizeTimeRoutes.POST_SOAK_TARGET_SW
    };
}
