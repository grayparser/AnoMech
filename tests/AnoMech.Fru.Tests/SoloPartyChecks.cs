using System.Numerics;
using AnoMech.Core.Game.Party;
using AnoMech.Core.SimObjects;
using AnoMech.Scenarios.Fru.FulgentBlade;
using static AnoMech.Scenarios.Fru.FruConstants;

internal static class SoloPartyChecks
{
    public static void Run()
    {
        foreach (var role in Enum.GetValues<PartyRole>())
        {
            var world = new SimWorld();
            var player = new SimPlayer { Role = role };
            world.Party.Members.Add(player);
            var scenario = new FruFulgentBladeScenario();
            scenario.Run(world, null);
            world.Events.Tick(44);
            Check(world.Party.Members.Count == 1 && ReferenceEquals(world.Party.Player, player), "Solo retains only the player");
            Check(player.Moves.Count == 0, "Solo never moves the player");
            Check(!world.Spawned.OfType<SimEnemy>().SelectMany(e => e.Casts).Any(c => c.Action == ActionId.AkhMornPandora), "Solo skips Akh Morn");
            Check(!player.Damage.Any(d => d.Action == ActionId.AkhMornPandoraAoe1 || d.Action == ActionId.AkhMornPandoraAoe2), "No solo stack damage");
        }
        var partyWorld = new SimWorld();
        var tank = new SimPlayer { Role = PartyRole.MainTank, Position = new(10, 0, 0) };
        partyWorld.Party.Members.Add(tank);
        var fight = new FruFulgentBladeScenario();
        fight.Run(partyWorld, 0);
        partyWorld.Events.Tick(1);
        var boss = partyWorld.Spawned.OfType<SimEnemy>().Single(e => e.Config.BNpcBaseId == BNpcBaseId.Pandora);
        fight.Tick(1, 1);
        Check(!boss.Following && boss.Position == Vector3.Zero && MathF.Abs(boss.Rotation - MathF.PI / 2) < .001f, "Centered Pandora faces MT without following");
        partyWorld.Events.Tick(FulgentBladePartyPlan.AkhMornCastTime - 1);
        var facing = boss.Rotation;
        tank.Position = new(-10, 0, 0);
        fight.Tick(.1f, 30);
        Check(boss.Rotation == facing && boss.Position == Vector3.Zero, "Akh Morn locks facing despite MT movement");
        Console.WriteLine("PASS: solo Fulgent skips stacks for all roles; centered Pandora tracks MT and locks Akh Morn facing.");
    }
    private static void Check(bool ok, string message) { if (!ok) throw new Exception(message); }
}
