using AnoMech.Core.Game.Party;
using Dalamud.Bindings.ImGui;

namespace AnoMech.Scenarios.Fru.Apocalypse;

public sealed partial class FruApocalypseScenario
{
    partial void DrawBaitSettings()
    {
        var selected = SelectedBaitTank == PartyRole.MainTank ? 0 : 1;
        if (ImGui.Combo("Darkest Dance bait", ref selected, new[] { "Main tank", "Off tank" }, 2))
            SelectedBaitTank = selected == 0 ? PartyRole.MainTank : PartyRole.OffTank;
        ImGui.TextDisabled("The selected tank baits the farthest-player hit. Applies on the next Start.");
    }
}
