using System;
using System.Numerics;
using AnoMech.Core.Game.Ai;
using AnoMech.Core.Game.Party;
using AnoMech.Core.SimObjects;

namespace AnoMech.Scenarios.Fru.DarklitDragonsong;

internal sealed class DarklitAi : IScenarioAi<DarklitState>
{
    public string Name => "NA";
    public string Group => "NA";
    public void Run(DarklitState pattern, SimWorld world)
    {
        var plan = new DarklitAiPlan(pattern);
        void Move(float time, Func<PartyRole, Vector3> position, Func<PartyRole, bool>? filter = null)
            => world.Events.Add(time, () =>
            {
                if (pattern.Boss is not { IsActive: true }) return;
                foreach (var role in DarklitState.Roles)
                    if ((filter == null || filter(role)) && world.Party.Get(role) is SimPartyNpc bot && bot.IsAlive()) bot.MoveTo(position(role), 6);
            });
        Move(2, DarklitAiPlan.Middle);
        Move(7.2f, DarklitAiPlan.OpeningSpread);
        Move(14.5f, DarklitAiPlan.Lineup);
        Move(25.2f, r => plan.Bowtie(r, false));
        Move(28.2f, r => plan.Bowtie(r));
        Move(33.3f, plan.Spirit);
        Move(36.9f, plan.Water);
        // The assigned tank takes both hits; everyone else stays clear.
        Move(40.6f, r => DarklitState.Convert(pattern.North(r) ? 17.3f : -17.3f, pattern.BaitEast ? 7.2f : -7.2f), r => r == pattern.SomberTank);
        Move(41.8f, _ => DarklitAiPlan.DanceTank(pattern.BaitEast), r => r == pattern.SomberTank);
        Move(42.4f, plan.DanceParty, r => r != pattern.SomberTank);
        // After the far jump finishes, become the nearest target for hit two.
        Move(45.5f, _ => pattern.Oracle?.Position ?? Vector3.Zero, r => r == pattern.SomberTank);
        Move(50, DarklitAiPlan.AkhMorn);
    }
}
