using AnoMech.Core.Game.Party;
using AnoMech.Core.SimObjects;
using AnoMech.Scenarios;
using NUnit.Framework;

[TestFixture]
public sealed class DamageSolverMitigationTests
{
    [TestCase(false)]
    [TestCase(true)]
    public void PlayerSurvivalUsesStatusesAndConsumesShields(bool hasShield)
    {
        var party = new SimParty();
        var player = new SimPlayer { Role = PartyRole.MainTank };
        party.Members.Add(player);
        player.AddStatus(1191); // Rampart: 20% reduction is insufficient on its own.
        if (hasShield) player.AddStatus(1178); // The Blackest Night: 25% HP shield.
        var damage = new DamageSolver(party);

        var survives = damage.Survives(player, requiredMitigation: 0.25f);

        Assert.That(survives, Is.EqualTo(hasShield));
        Assert.That(player.HasStatus(1178), Is.False, "A checked shield is consumed by the hit");
        Assert.That(player.HasStatus(1191), Is.True, "Persistent reduction remains active");
    }

    [Test]
    public void BotSurvivalDoesNotSpendTheShield()
    {
        var party = new SimParty();
        var bot = new SimPartyNpc { Role = PartyRole.MainTank };
        party.Members.Add(bot);
        bot.AddStatus(1178);
        var damage = new DamageSolver(party);

        Assert.That(damage.Survives(bot, requiredMitigation: 0.9f), Is.True);
        Assert.That(bot.HasStatus(1178), Is.True, "Upstream exempts bot-driven actors from mitigation checks");
    }
}
