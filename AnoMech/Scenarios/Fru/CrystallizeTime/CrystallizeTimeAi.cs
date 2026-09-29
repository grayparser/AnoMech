using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AnoMech.Core.Game.Ai;
using AnoMech.Core.Game.Party;
using AnoMech.Core.SimObjects;

namespace AnoMech.Scenarios.Fru.CrystallizeTime;

internal sealed partial class CrystallizeTimeAi : IScenarioAi<CrystallizeTimeState>
{
    public string Name => "NA priority / 7-1 Akh Morn (MT solo)";
    public string Group => "NA";
    private CrystallizeTimeState pattern = null!;
    private CrystallizeTimeAiPlan plan = null!;
    private SimWorld world = null!;
    private Dictionary<CrystallizeTimeAssignment, (Vector3 Position, SimEventObject? Object, float Created, PartyRole Creator)> puddles => pattern.Puddles;
    private Dictionary<PartyRole, Vector3> aeroSources => pattern.AeroSources;
    private readonly Dictionary<PartyRole, CrystallizeTimeAssignment> puddleDestinations = [];
    private IEnumerable<SimCharacter> Members() => world.Party.ActiveMembers().Where(m => m.IsAlive());

    public void Run(CrystallizeTimeState state, SimWorld worldParam)
    {
        pattern = state; world = worldParam; plan = new(state);
        puddleDestinations.Clear(); spreadDestinations.Clear();
        foreach (var (time, route) in plan.Routes()) At(time, () => Move(route));
        At(35.8f, () => MoveRoles(plan.Rewind));
        At(40.6f, StartSpiritSpread);
        At(56.4f, () => MoveRoles(CrystallizeTimeAiPlan.AkhMorn));
    }
    private void At(float time, Action action) => world.Events.Add(time, () =>
    {
        if (pattern.Boss is { IsActive: true }) action();
    });
    public void Tick(CrystallizeTimeState state, SimWorld worldParam, float delta)
    {
        if (state.Boss is not { IsActive: true }) return;
        var time = world.Events.Elapsed;
        if (time is >= 40.6f and < 43.4f) UpdateSpiritSpread();
        while (state.Cleansed.TryDequeue(out var role))
            if (time < 35.8f && world.Party.Get(role) is SimPartyNpc bot && bot.IsAlive()
                && plan.AfterCleanse.TryGetValue(pattern.Assignment(role), out var next))
            {
                puddleDestinations.Remove(role);
                bot.MoveTo(next.Position, CrystallizeTimeState.RunSpeed);
            }
        while (state.Intercepted.TryDequeue(out var role))
            if (world.Party.Get(role) is SimPartyNpc bot && bot.IsAlive()
                && pattern.Assignment(role) is CrystallizeTimeAssignment.AeroWest or CrystallizeTimeAssignment.AeroEast)
            {
                var escape = pattern.East ? CrystallizeTimeRoutes.POST_EARLY_SOAK_E : CrystallizeTimeRoutes.POST_EARLY_SOAK_W;
                bot.MoveTo(escape[pattern.Assignment(role)].Position, CrystallizeTimeState.RunSpeed);
            }
        foreach (var (role, source) in puddleDestinations)
            if (world.Party.Get(role) is SimPartyNpc bot && bot.IsAlive() && puddles.TryGetValue(source, out var puddle)
                && puddle.Object is { IsActive: true }) bot.MoveTo(puddle.Position, CrystallizeTimeState.RunSpeed);
    }
    private void Move(IReadOnlyDictionary<CrystallizeTimeAssignment, CrystallizeTimeDestination> route)
    {
        foreach (var (assignment, destination) in route)
        {
            var role = pattern.Role(assignment);
            puddleDestinations.Remove(role);
            if (world.Party.Get(role) is not SimPartyNpc bot || !bot.IsAlive()) continue;
            if (destination.Puddle is { } source)
            {
                puddleDestinations[role] = source;
                if (puddles.TryGetValue(source, out var puddle)) bot.MoveTo(puddle.Position, CrystallizeTimeState.RunSpeed);
            }
            else if (aeroSources.ContainsKey(role) && world.Events.Elapsed is >= 21.1f and < 22.15f)
                world.Events.Add(22.15f - world.Events.Elapsed, () =>
                {
                    if (pattern.Boss is { IsActive: true } && bot.IsAlive()) bot.MoveTo(destination.Position, CrystallizeTimeState.RunSpeed);
                });
            else bot.MoveTo(destination.Position, CrystallizeTimeState.RunSpeed);
        }
    }
    private void MoveRoles(Func<PartyRole, Vector3> positions)
    {
        puddleDestinations.Clear();

        foreach (var member in Members())
            if (member is SimPartyNpc bot) bot.MoveTo(positions(bot.Role), CrystallizeTimeState.RunSpeed);
    }
}
