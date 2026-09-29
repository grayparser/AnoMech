using System.Numerics;
using AnoMech.Core.Game.Party;
using AnoMech.Core.SimObjects;
using AnoMech.Scenarios;
using AnoMech.Scenarios.Fru;
using AnoMech.Scenarios.Fru.Apocalypse;
using NUnit.Framework;

[TestFixture]
public sealed class ApocalypseReviewTests
{
    [TestCase(PartyRole.MainTank)]
    [TestCase(PartyRole.OffTank)]
    public void SelectedDarkestDanceTankCanBePlayedManually(PartyRole tank)
    {
        foreach (var clockwise in new[] { false, true })
            for (var rotation = 0; rotation < 4; rotation++)
            {
                var state = new ApocalypseState(rotation, clockwise, [3, 1, 1, 0, 2, 0, 2, 3]);
                var guide = Party();
                var manual = Party(tank);
                var guideScenario = new FruApocalypseScenario { SelectedBaitTank = tank };
                var manualScenario = new FruApocalypseScenario { SelectedBaitTank = tank };
                guideScenario.Run(guide, state);
                manualScenario.Run(manual, state);
                while (guide.Events.Elapsed < 57.1f)
                {
                    manual.Party.Player!.Position = guide.Party.Get(tank)!.Position;
                    Step(guide, guideScenario);
                    Step(manual, manualScenario);
                }
                Assert.That(manual.Party.Members.All(m => !m.Dead), Is.True,
                    $"Manual {tank}, rotation {rotation}, clockwise {clockwise}");
                Assert.That(manual.Party.Player!.Moves, Is.Empty);
                var hits = manual.Party.Members.Where(m => m.Damage.Any(d => d.Action == ApocalypseConstants.DanceHit)).ToArray();
                Assert.That(hits.Select(m => m.Role), Is.EqualTo(new[] { tank }));
            }
    }

    [TestCase(15)]
    [TestCase(30)]
    [TestCase(60)]
    [TestCase(144)]
    public void ApocalypseReleaseAndKnockbackUseTheAuthoredClock(int fps)
    {
        var world = Party();
        var scenario = new FruApocalypseScenario();
        scenario.Run(world, new ApocalypseState(0, true, [0, 1, 2, 3, 0, 1, 2, 3]));
        var eruption = false;
        var knockback = false;
        while (world.Events.Elapsed < 48)
        {
            Step(world, scenario, 1f / fps);
            var boss = world.Spawned.OfType<SimEnemy>().FirstOrDefault();
            if (boss == null) continue;
            if (!eruption && boss.NativeReleases.Any(c => c.Action == ApocalypseConstants.EruptionCast))
            {
                eruption = true;
                Assert.That(world.Events.Elapsed, Is.InRange(35.6f, 35.6f + 1.01f / fps));
                Assert.That(world.Party.Members.Sum(m => m.Damage.Count(d => d.Action == ApocalypseConstants.Eruption)), Is.EqualTo(8));
            }
            if (!knockback && boss.Casts.Any(c => c.Action == ApocalypseConstants.Knockback))
            {
                knockback = true;
                Assert.That(world.Events.Elapsed, Is.InRange(47.6f - 20f / 30, 47.6f - 20f / 30 + 1.01f / fps));
                Assert.That(world.Party.Members.All(m => m.Damage.All(d => d.Action != ApocalypseConstants.Knockback)), Is.True,
                    "The windup starts before forced movement");
            }
        }
        Assert.That(eruption && knockback, Is.True);
    }

    [Test]
    public void DamageSolverRejectsUnderfilledAndOverlappingWaterStacks()
    {
        var world = Party();
        var damage = new DamageSolver(world.Party);
        damage.SetStatuses(DamageType.Water, ApocalypseConstants.WaterVulnerability);
        foreach (var member in world.Party.Members) member.Position = new((int)member.Role < 4 ? -8 : 8, 0, 0);
        var left = IPositioned.From(new(-8, 0, 0));
        damage.Resolve(left, ApocalypseConstants.Water, [DamageType.Water], [(ApocalypseConstants.WaterVulnerability, 1)], stackMinTargets: 4);
        Assert.That(world.Party.Members.All(m => !m.Dead), Is.True);
        damage.Resolve(left, ApocalypseConstants.Water, [DamageType.Water], [], stackMinTargets: 4);
        Assert.That(world.Party.Members.Count(m => m.Dead), Is.EqualTo(4), "An overlapping second stack hits the water vulnerability");
        world.Party.Get(PartyRole.CasterDps)!.Position = Vector3.Zero;
        damage.Resolve(IPositioned.From(new(8, 0, 0)), ApocalypseConstants.Water, [DamageType.Water], [], stackMinTargets: 4);
        Assert.That(world.Party.Members.Count(m => m.Dead), Is.EqualTo(7), "The other underfilled stack kills its three occupants");
    }

    private static SimWorld Party(PartyRole? player = null)
    {
        var world = new SimWorld();
        foreach (var role in Enum.GetValues<PartyRole>())
            world.Party.Members.Add(role == player ? new SimPlayer { Role = role } : new SimPartyNpc { Role = role });
        return world;
    }
    private static void Step(SimWorld world, IScenario scenario, float delta = 1f / 60)
    {
        world.Events.Tick(delta);
        foreach (var member in world.Party.Members) member.Advance(delta);
        scenario.Tick(delta, world.Events.Elapsed);
    }
}
