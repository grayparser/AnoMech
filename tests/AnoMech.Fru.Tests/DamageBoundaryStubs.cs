using System.Numerics;
using AnoMech.Core.Game;
using AnoMech.Core.SimObjects;

// Only the native action-sheet/query boundary is substituted. DamageSolver is
// production code. ActionGeometry.json was exported from the installed action sheet.
namespace AnoMech.Core.Game
{
    internal sealed record ActionGeometry(uint RowId, string Name, int CastType, float EffectRange, float XAxisModifier)
    {
        internal static readonly ActionGeometry[] Rows = System.Text.Json.JsonSerializer.Deserialize<ActionGeometry[]>(
            File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "ActionGeometry.json")))!;
    }
    public sealed class AoeQuery(uint actionId, Placement source, float? size = null)
    {
        public IReadOnlyList<SimCharacter> Run(PartyFinder find)
        {
            var row = ActionGeometry.Rows.Single(r => r.RowId == actionId);
            var members = row.CastType switch
            {
                2 or 5 or 6 => find.InsideCircle(source.Position, row.EffectRange),
                10 => find.InsideCircle(source.Position, row.EffectRange)
                    .Where(m => Vector3.Distance(m.Position, source.Position) >= (size ?? 0)).ToList(),
                4 or 12 => find.InsideRect(source, row.XAxisModifier / 2f, row.EffectRange),
                _ => throw new InvalidOperationException($"Unsupported native action geometry for {actionId}")
            };
            foreach (var member in members) member.Damage.Add((actionId, "resolved AoE", false));
            return members;
        }
    }
}
namespace AnoMech.Core
{
    public static class ActionLookup { public static string Name(uint id) => $"{id} {ActionGeometry.Rows.FirstOrDefault(r => r.RowId == id)?.Name}"; }
}
namespace AnoMech.Core.Native
{
    public static class DamageNumbers
    {
        public static void ShowFraction(SimCharacter member, float fraction, string name)
            => member.Damage.Add((uint.Parse(name.Split(' ')[0]), "damage feedback", false));
    }
}
namespace AnoMech
{
    public static class Plugin
    {
        public static TestLog Log { get; } = new();
        public static TestConfiguration Config { get; } = new();
        public static TestUserActions UserActions { get; } = new();
    }
    // Exercise production mitigation when requested; never bypass it through disabled settings.
    public sealed class TestConfiguration { public bool EnableTankMitigation => true; }
    public sealed class TestUserActions { public bool Enabled => true; }
    public sealed class TestLog { public void Info(string message) { } }
}
namespace AnoMech.Windows
{
    public sealed class DamageDebugWindow
    {
        public static DamageDebugWindow? Instance => null;
        public void Record(AoeQuery query) { }
    }
}
namespace AnoMech.Core.SimObjects
{
    public partial class SimCharacter
    {
        public void Die(string cause)
        {
            Dead = true;
            if (Damage.Count > 0) Damage[^1] = (Damage[^1].Action, cause, true);
            else Damage.Add((0, cause, true));
        }
    }
}
