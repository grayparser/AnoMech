[TestFixture]
public sealed class FruRegressionTests
{
    [Test] public void DiamondDust() => DiamondDustChecks.Run();
    [Test] public void LightRampant() => LightRampantChecks.Run();
}
