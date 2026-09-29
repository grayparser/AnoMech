using System;
using System.Numerics;
using AnoMech.Core.Game.Ai;
using AnoMech.Core.Game.Party;
using AnoMech.Core.SimObjects;

namespace AnoMech.Scenarios.Fru.UltimateRelativity;

internal sealed class UltimateRelativityAi : IScenarioAi<UltimateRelativityState>
{
    public string Name => "NA";
    public string Group => "NA";
    public void Run(UltimateRelativityState pattern, SimWorld world)
    {
        var plan = new UltimateRelativityAiPlan(pattern);
        void Each(Action<SimPartyNpc> action)
        {
            if (pattern.Boss is not { IsActive: true }) return;
            foreach (var role in UltimateRelativityState.Roles)
                if (world.Party.Get(role) is SimPartyNpc bot && bot.IsAlive()) action(bot);
        }
        void Move(float time, Func<PartyRole, Vector3> position)
            => world.Events.Add(time, () => Each(bot => bot.MoveTo(position(bot.Role), 6)));
        Move(1, plan.CenterSpot);
        Move(19.2f, r => plan.FireSpot(r, 0));
        Move(23.9f, r => plan.BaitSpot(r, 0));
        Move(28.6f, r => plan.FireSpot(r, 1, true));
        Move(30.6f, r => plan.FireSpot(r, 1));
        Move(34, r => plan.BaitSpot(r, 1));
        Move(38.7f, r => plan.FireSpot(r, 2));
        Move(45, r => plan.BaitSpot(r, 2));
        Move(48.8f, plan.CenterSpot);
        world.Events.Add(51, () => Each(bot =>
        {
            bot.StopMoving();
            bot.Face(bot.Position + pattern.Direction(pattern.Assignment(bot.Role)) * 100);
        }));
        Move(55.8f, plan.FinalSpot);
    }
}
