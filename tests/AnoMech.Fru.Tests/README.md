# FRU scenario regression tests

Open `AnoMech.sln` to discover these NUnit tests, or run:

```sh
dotnet test tests/AnoMech.Fru.Tests
```

Available scenario/reference notes:

- [DiamondDust](reference/DiamondDust.md)
- [LightRampant](reference/LightRampant.md)
- [UltimateRelativity](reference/UltimateRelativity.md)
- [Apocalypse](reference/Apocalypse.md)
- [CrystallizeTime](reference/CrystallizeTime.md)

The project links the current production scenarios, scheduler, AI and DamageSolver.
Native actors, action-sheet lookups and rendering are boundary substitutes.
ActionGeometry.json was exported from game build 2026.09.15.0000.0000.
Diagnostic warnings fail the fixtures so scheduler-caught exceptions cannot pass silently.
Native audio, animation timing, actual Provoke execution and client reset behavior
still require live-game verification. Core movement/VFX tests are in AnoMech.FruCore.Tests.
