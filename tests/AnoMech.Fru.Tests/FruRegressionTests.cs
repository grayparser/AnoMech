[TestFixture]
public sealed class FruRegressionTests
{
    [Test] public void DiamondDust() => DiamondDustChecks.Run();
    [Test] public void LightRampant() => LightRampantChecks.Run();
    [Test] public void UltimateRelativity() => UltimateRelativityChecks.Run();
    [Test] public void Apocalypse() => ApocalypseChecks.Run();
    [Test] public void CrystallizeTime() => CrystallizeTimeChecks.Run();
    [Test] public void Darklit() => DarklitChecks.Run();
}
