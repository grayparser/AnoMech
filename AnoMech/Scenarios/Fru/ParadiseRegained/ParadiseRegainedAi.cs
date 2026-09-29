using AnoMech.Core.Game.Ai;
using AnoMech.Core.Game.Party;
using AnoMech.Core.SimObjects;

namespace AnoMech.Scenarios.Fru.ParadiseRegained;

internal sealed class ParadiseRegainedAi : IScenarioAi<ParadiseRegainedState>
{
    public string Name => "H first / M1-R1 NW / M2-R2 NE; lines T-M-R-H";

    public void Run(ParadiseRegainedState pattern, SimWorld world)
    {
        var plan = new ParadiseRegainedAiPlan(pattern);
        // The off-tank bot Provokes during Wings; a player OT must press it.
        world.Events.Add(13f, () =>
        {
            if (pattern.Pandora is { IsActive: true } boss
                && world.Party.Get(PartyRole.OffTank) is SimPartyNpc bot && bot.IsAlive())
            {
                pattern.CurrentTank = PartyRole.OffTank;
                boss.SetTarget(bot, follow: false);
            }
        });
        world.Events.Add(2f, () => Move(0));
        world.Events.Add(10f, () => Move(1));
        world.Events.Add(16.5f, () => Move(2));

        void Move(int stage)
        {
            foreach (var member in world.Party.ActiveMembers())
                if (member is SimPartyNpc bot)
                    bot.MoveTo(plan.Position(bot.Role, stage), ParadiseRegainedState.RunSpeed);
        }
    }
}
