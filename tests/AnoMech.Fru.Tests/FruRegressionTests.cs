[TestFixture]
public sealed class FruRegressionTests
{
    [Test] public void DiamondDust() => DiamondDustChecks.Run();
    [Test] public void LightRampant() => LightRampantChecks.Run();
    [Test] public void UltimateRelativity() => UltimateRelativityChecks.Run();
    [Test] public void Apocalypse() => ApocalypseChecks.Run();
    [Test] public void CrystallizeTime() => CrystallizeTimeChecks.Run();
    [Test] public void Darklit() => DarklitChecks.Run();
    [Test] public void FulgentGeometry() => FulgentGeometryChecks.Run();
    [Test] public void ParadiseRegained() => ParadiseRegainedChecks.Run();
    [Test] public void CatalogAndFulgentEffects() => FruCatalogChecks.Run();
    [Test] public void FulgentPartyAi() => AiIntegrationChecks.Run();
    [Test] public void FulgentSoloAndFacing() => SoloPartyChecks.Run();
    [Test] public void PolarizingStrikes() => PolarizingStrikesChecks.Run();
}
