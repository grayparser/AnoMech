using System;
using System.Numerics;
using AnoMech.Core.Game.Ai;
using AnoMech.Core.Game.Party;
using AnoMech.Core.SimObjects;

namespace AnoMech.Scenarios.Fru.LightRampant;

internal sealed class LightRampantAi : IScenarioAi<LightRampantState>
{
    public string Name => "NA · Conga";
    public string Group => "NA";
    public void Run(LightRampantState pattern, SimWorld world)
    {
        var plan = new LightRampantAiPlan(pattern);
        void Move(float time, Func<PartyRole, Vector3> position, Func<PartyRole, bool>? include = null)
            => world.Events.Add(time, () =>
            {
                if (pattern.Boss is not { IsActive: true }) return;
                foreach (var role in LightRampantState.Roles)
                    if ((include == null || include(role)) && world.Party.Get(role) is SimPartyNpc bot && bot.IsAlive())
                        bot.MoveTo(position(role), 6);
            });
        Move(3, plan.Preposition);
        Move(9.75f, plan.Lineup, pattern.Puddle);
        Move(10.75f, plan.Lineup);
        Move(12.3f, plan.TowerSpot, r => !pattern.Puddle(r));
        for (var wave = 0; wave < 5; wave++)
        {
            var dropped = wave + 1;
            Move(15.7f + 1.6f * wave, r => plan.PuddleSpot(r, dropped), pattern.Puddle);
        }
        // Leave as soon as towers resolve: the reference delay clips the fifth bait at six yalms/s.
        Move(19.1f, plan.GroupSpot, r => !pattern.Puddle(r));
        Move(23.3f, plan.Intermediate);
        Move(24.5f, r => plan.SafeSpot(r, true));
        Move(27, r => plan.SafeSpot(r, false));
        Move(29.7f, r => (world.Party.Get(r)?.FindStatus(LightRampantConstants.Lightsteeped)?.Stacks ?? 0) == 2 ? Vector3.Zero : plan.MiddleWait(r));
        Move(34.4f, plan.BanishSpot);
        Move(38, LightRampantAiPlan.ClockSpot);
    }
}
