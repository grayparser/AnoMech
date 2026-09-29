using System.Collections.Generic;
using AnoMech.Scenarios.Fru.DiamondDust;
using AnoMech.Scenarios.Fru.LightRampant;
using AnoMech.Scenarios.Fru.UltimateRelativity;
using AnoMech.Scenarios.Fru.Apocalypse;

namespace AnoMech.Scenarios.Fru;

internal static class FruScenarios
{
    public static IReadOnlyList<IScenario> CreateCatalog() =>
        [new FruDiamondDustScenario(), new FruLightRampantScenario(), new FruUltimateRelativityScenario(), new FruApocalypseScenario()];
}
