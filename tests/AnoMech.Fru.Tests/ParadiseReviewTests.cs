using System.Numerics;
using AnoMech.Core.Game.Party;
using AnoMech.Core.SimObjects;
using AnoMech.Scenarios;
using AnoMech.Scenarios.Fru;
using AnoMech.Scenarios.Fru.ParadiseRegained;
using NUnit.Framework;

[TestFixture]
public sealed class ParadiseReviewTests
{
    [TestCase(true)]
    [TestCase(false)]
    public void PlayerOffTankMustProvokePandora(bool darkFirst)
    {
        foreach (var correctTarget in new[] { true, false })
        {
            var world = Party(PartyRole.OffTank);
            var state = new ParadiseRegainedState(0, true, darkFirst);
            var plan = new ParadiseRegainedAiPlan(state);
            var scenario = new FruParadiseRegainedScenario();
            scenario.Run(world, state);
            world.Events.Add(13, () =>
            {
                var boss = world.Spawned.OfType<SimEnemy>().Single(e => e.Config.BNpcBaseId == FruConstants.BNpcBaseId.Pandora);
                scenario.OnPlayerAction(ParadiseRegainedState.ProvokeAction, correctTarget ? boss.GameObjectId : 999999);
            });
            while (world.Events.Elapsed < 23)
            {
                world.Events.Tick(1f / 60);
                foreach (var member in world.Party.Members) member.Advance(1f / 60);
                var stage = world.Events.Elapsed >= 16.5f ? 2 : world.Events.Elapsed >= 10 ? 1 : 0;
                world.Party.Player!.Position = plan.Position(PartyRole.OffTank, stage);
            }
            Assert.That(world.Party.Members.All(m => !m.Dead), Is.EqualTo(correctTarget),
                "A wrong-target Provoke must not silently perform the tank swap");
            Assert.That(world.Party.Player!.Moves, Is.Empty);
        }
    }

    private static SimWorld Party(PartyRole? player = null)
    {
        var world = new SimWorld();
        foreach (var role in Enum.GetValues<PartyRole>())
            world.Party.Members.Add(role == player ? new SimPlayer { Role = role } : new SimPartyNpc { Role = role });
        return world;
    }
}
