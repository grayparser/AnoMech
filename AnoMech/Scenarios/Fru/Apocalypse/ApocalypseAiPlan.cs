using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AnoMech.Core.Game.Party;
using static AnoMech.Scenarios.Fru.Apocalypse.ApocalypseConstants;

namespace AnoMech.Scenarios.Fru.Apocalypse;

// NA priority: MT > OT > H1 > H2 / M1 > M2 > R1 > R2.
// Static spread routes adapted from WCGH/FRU-Sim (GPL-3.0),
// commit 2a77c857ce1bb6eb472a59a95c7544faba01c55a, scenes/p3.
internal sealed class ApocalypseAiPlan
{
    private readonly ApocalypseState state;
    private static PartyRole[] Roles => ApocalypseState.Roles;
    private readonly int[] slots = Enumerable.Range(0, 8).ToArray();
    private int Rotation => state.Rotation;
    private bool Clockwise => state.Clockwise;
    public const float Scale = 20f / 47.4f;
    public ApocalypseAiPlan(ApocalypseState state)
    {
        this.state = state;
        var support = Adjusters(0);
        var dps = Adjusters(4);
        for (var i = 0; i < support.Length; i++)
            (slots[support[i]], slots[dps[i]]) = (slots[dps[i]], slots[support[i]]);
    }
    private int[] Adjusters(int start) => Enumerable.Range(0, 4)
        .Where(d => Enumerable.Range(start, 4).Count(i => state.Duration(Roles[i]) == d) == 2)
        .Select(d => Enumerable.Range(start, 4).First(i => state.Duration(Roles[i]) == d)).ToArray();
    public int Index(PartyRole role) => Array.IndexOf(Roles, role);
    public int Slot(PartyRole role, bool adjusted = true) => adjusted ? slots[Index(role)] : Index(role);
    public bool SupportGroup(PartyRole role) => Slot(role) < 4;
    private float SpreadRotation => (Clockwise ? new[] { -45, 0, -135, -90 } : new[] { -135, -90, -45, 0 })[Rotation];
    // Godot's north is +X; convert to native north (-Z), preserving CW rotation.
    internal static Vector3 Reference(float x, float z, float degrees = 0)
    {
        var angle = degrees * MathF.PI / 180;
        return new Vector3(x * MathF.Sin(angle) + z * MathF.Cos(angle), 0,
            -x * MathF.Cos(angle) + z * MathF.Sin(angle)) * Scale;
    }
    public Vector3 Setup(PartyRole role, bool adjusted = true, bool spread = false)
    {
        var slot = Slot(role, adjusted);
        var support = slot < 4;
        var local = slot % 4;
        var x = (local % 2 == 0 ? 1 : -1) * (support ? 1 : -1) * (spread ? 8 : 4);
        var z = (support ? -1 : 1) * (spread ? (local < 2 ? 9 : 27) : (local < 2 ? 8 : 16));
        return Reference(x, z);
    }
    public Vector3 FirstStack(PartyRole role) => Reference(0, SupportGroup(role) ? -12 : 12);
    public Vector3 Spread(PartyRole role)
    {
        var slot = Slot(role, false);
        var side = slot < 4 ? 1 : -1;
        var local = slot % 4;
        var (x, z) = local switch
        {
            0 => Clockwise ? (23f, 0f) : (16.23f, 16.23f),
            1 => Clockwise ? (16.23f, -16.23f) : (23f, 0f),
            2 => (44f, 8f),
            _ => (44f, -8f)
        };
        return Reference(x * side, z * side, SpreadRotation);
    }
    public Vector3 PostEruption(PartyRole role) => Reference((Slot(role, false) < 4 ? 1 : -1)
        * (Slot(role, false) % 4 < 2 ? 10 : 33), 0, SpreadRotation);
    public Vector3 SecondStack(PartyRole role) => Reference(SupportGroup(role) ? 10 : -10, 0, SpreadRotation);
    public Vector3 TankBait(bool near)
    {
        var angle = (Clockwise ? new[] { -45, 0, -135, -90 } : new[] { -45, 0, 45, 90 })[Rotation];
        if (Slot(state.BaitTank) >= 4) angle += 180;
        return Reference(near ? 10 : 30, near ? -10 : -30, angle);
    }
    public Vector3 ReturnStack(PartyRole role, Vector3 boss)
    {
        var inward = boss.LengthSquared() > 0.01f ? Vector3.Normalize(-boss) : Vector3.UnitZ;
        var right = new Vector3(inward.Z, 0, -inward.X);
        // Keep the adjusted water groups eight yalms apart, on the same sides
        // as their knockback rays: supports left, DPS right when facing the boss
        // from arena center. Flexed roles follow their assigned water group.
        return boss + inward * 2 + right * (SupportGroup(role) ? -4 : 4);
    }
    public Vector3 KnockbackStack(PartyRole role, Vector3 boss)
    {
        var inward = boss.LengthSquared() > 0.01f ? Vector3.Normalize(-boss) : Vector3.UnitZ;
        var angle = (SupportGroup(role) ? 30 : -30) * MathF.PI / 180;
        var dir = new Vector3(inward.X * MathF.Cos(angle) - inward.Z * MathF.Sin(angle), 0,
            inward.X * MathF.Sin(angle) + inward.Z * MathF.Cos(angle));
        return boss + dir * 2;
    }
}
