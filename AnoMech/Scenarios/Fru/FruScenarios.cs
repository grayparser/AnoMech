using System.Collections.Generic;
using AnoMech.Scenarios.Fru.DiamondDust;

namespace AnoMech.Scenarios.Fru;

internal static class FruScenarios
{
    public static IReadOnlyList<IScenario> CreateCatalog() =>
        [new FruDiamondDustScenario()];
}
