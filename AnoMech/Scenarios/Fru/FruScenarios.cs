using System.Collections.Generic;
using AnoMech.Scenarios.Fru.DiamondDust;
using AnoMech.Scenarios.Fru.LightRampant;

namespace AnoMech.Scenarios.Fru;

internal static class FruScenarios
{
    public static IReadOnlyList<IScenario> CreateCatalog() =>
        [new FruDiamondDustScenario(), new FruLightRampantScenario()];
}
