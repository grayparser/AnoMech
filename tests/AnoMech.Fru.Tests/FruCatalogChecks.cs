using AnoMech.Core.Game;
using AnoMech.Core.Game.Ai;
using AnoMech.Core.SimObjects;
using AnoMech.Scenarios;
using AnoMech.Scenarios.Fru;

internal static class FruCatalogChecks
{
    public static void Run()
    {
        var catalog = FruScenarios.CreateCatalog();
        string[] names = ["Diamond Dust", "Light Rampant", "Ultimate Relativity", "Apocalypse", "Darklit Dragonsong", "Crystallize Time", "Fulgent Blade"];
        Check(catalog.Select(s => s.Name).SequenceEqual(names), "Only individual FRU scenarios are registered");
        var world = new SimWorld();
        var fulgent = catalog.Single(s => s.Name == "Fulgent Blade");
        fulgent.Run(world, null);
        world.Events.Tick(7.49f);
        Check(!world.Spawned.OfType<SimOmen>().Any(o => o.Path == FruConstants.Vfx.InitialSeam), "No exawave seams before cast completion");
        world.Events.Tick(.02f);
        var seams = world.Spawned.OfType<SimOmen>().Where(o => o.Path == FruConstants.Vfx.InitialSeam).ToArray();
        Check(seams.Length == 6 && seams.All(o => o.StartTrigger == FruConstants.Vfx.InitialSeamTrigger), "All seams start as Fulgent Blade resolves");
        world.Events.Clear(); world.Despawn();
        Check(seams.All(o => !o.IsActive), "Seams clear on reset");
        CheckFulgentWarnings(fulgent);
        Console.WriteLine("PASS: FRU individual catalog and Fulgent seam/warning timing, arrow overlap and reset.");
    }
    private static void CheckFulgentWarnings(IScenario scenario)
    {
        foreach (var fps in new[] { 15, 30, 60, 144 })
        {
            var world = new SimWorld();
            scenario.Run(world, null);
            while (world.Events.Elapsed < 43.6f)
            {
                world.Events.Tick(1f / fps);
                var time = world.Events.Elapsed;
                var warnings = world.Spawned.OfType<SimOmen>()
                    .Where(o => o.Path == FruConstants.Vfx.SeamWarning).ToArray();
                var expectedGroups = Enumerable.Range(0, 3).Count(g => time >= 13.5f + 4 * g);
                Check(warnings.Length == expectedGroups * 2, "Two intermediate warnings spawn per charging group");
                for (var group = 0; group < expectedGroups; group++)
                    for (var line = 0; line < 2; line++)
                    {
                        var warning = warnings[group * 2 + line];
                        Check(warning.StartTrigger == 3,
                            "Native slash_act_a triggers slot 3 on the broad warning; default playback only gives a narrow line");
                        Check(warning.Scale == System.Numerics.Vector3.One, "Intermediate warning retains native scale");
                        var seam = world.Spawned.OfType<SimOmen>().Single(o =>
                            o.Path == FruConstants.Vfx.InitialSeam && o.Placement == warning.Placement);
                        Check(seam.StartTrigger == FruConstants.Vfx.SeamChargeTrigger,
                            "Warning follows the corresponding randomized seam's placement and charging stage");
                        Check(warning.IsActive == (time < 20.5f + 4 * group),
                            "Intermediate warning persists until its group's first AOE, without ending another group");
                        if (time >= 18.2f + 4 * group && time < 20.5f + 4 * group)
                            Check(world.Spawned.OfType<SimOmen>().Any(o => o.IsActive &&
                                o.Path == FruConstants.Vfx.PathArrows && o.Placement == warning.Placement),
                                "Intermediate seam warning overlaps its bouncing arrows");
                    }
                // All existing wave visuals still start at their original snapshots.
                var expectedHits = Enumerable.Range(0, 3).Sum(g => Enumerable.Range(0, 7)
                    .Count(h => time >= 20.5f + 4 * g + 2 * h)) * 4;
                var actualHits = world.Spawned.OfType<SimEnemy>().Sum(e => e.Casts.Count(c =>
                    c.Action == FruConstants.ActionId.PathOfLightRest || c.Action == FruConstants.ActionId.PathOfDarknessRest));
                Check(actualHits == expectedHits, "Intermediate VFX does not move the wave cast schedule");
            }
            world.Events.Clear();
            world.Despawn();
        }
        foreach (var resetTime in new[] { 14f, 18.5f, 22.5f, 27f, 29f })
        {
            var world = new SimWorld();
            scenario.Run(world, null);
            world.Events.Tick(resetTime);
            var warnings = world.Spawned.OfType<SimOmen>()
                .Where(o => o.Path == FruConstants.Vfx.SeamWarning).ToArray();
            Check(warnings.Any(o => o.IsActive) || resetTime > 28.5f, "Exercise active warnings during reset");
            world.Events.Clear();
            world.Despawn();
            Check(warnings.All(o => !o.IsActive), "Reset clears every intermediate warning");
            scenario.Run(world, null);
            world.Events.Tick(13.51f);
            Check(world.Spawned.OfType<SimOmen>().Count(o => o.Path == FruConstants.Vfx.SeamWarning && o.IsActive) == 2,
                "Reusing the scenario after reset creates a fresh warning pair");
            world.Events.Clear();
            world.Despawn();
        }
    }
    private static void Check(bool ok, string message) { if (!ok) throw new Exception(message); }
}
