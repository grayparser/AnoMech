using System;
using System.Numerics;
using AnoMech.Core.Game.Party;

using static AnoMech.Scenarios.Fru.ParadiseRegained.ParadiseRegainedState;

namespace AnoMech.Scenarios.Fru.ParadiseRegained;

internal sealed class ParadiseRegainedAiPlan(ParadiseRegainedState state)
{
    // Healers take the first tower. M1/R1 take relative NW, M2/R2 NE.
    // The native tower's smaller radius requires positions inside its actual
    // ring; the Godot scene's larger placeholder rings are not used here.
    public Vector3 Position(PartyRole role, int stage)
    {
        Vector3 position;
        if (stage == 0)
            return new(((int)role - 3.5f) * 0.35f, 0f, 7f);
        if (stage == 1)
        {
            position = role switch
            {
                PartyRole.MainTank => new(state.DarkFirst ? -6.0622f : 6.0622f, 0f, -3.5f),
                PartyRole.OffTank => state.DarkFirst ? new(0f, 0f, 1f) : new(10f, 0f, 10f),
                PartyRole.RegenHealer => new(-0.65f, 0f, 7f),
                PartyRole.ShieldHealer => new(0.65f, 0f, 7f),
                PartyRole.MeleeDpsA => new(-3.5f, 0f, 9.5f),
                PartyRole.PhysRangedDps => new(-3.8f, 0f, 9.7f),
                PartyRole.MeleeDpsB => new(3.5f, 0f, 9.5f),
                _ => new(3.8f, 0f, 9.7f),
            };
        }
        else
        {
            position = role switch
            {
                PartyRole.MainTank => new(0f, 0f, state.DarkFirst ? -14f : -1f),
                PartyRole.OffTank => new(state.DarkFirst ? -6.0622f : 6.0622f, 0f, 3.5f),
                PartyRole.RegenHealer => new(-0.65f, 0f, -7.5f),
                PartyRole.ShieldHealer => new(0.65f, 0f, -7.5f),
                PartyRole.MeleeDpsA => new(-5f, 0f, -4.5f),
                PartyRole.PhysRangedDps => new(-5.2f, 0f, -4.7f),
                PartyRole.MeleeDpsB => new(5f, 0f, -4.5f),
                _ => new(5.2f, 0f, -4.7f),
            };
        }
        return state.Rotate(position);
    }
}
