## P3 Ultimate Relativity (NA)

Run `dotnet test tests/AnoMech.Fru.Tests/AnoMech.Fru.Tests.csproj --filter Name~UltimateRelativity` for
focused checks. The production scenario covers all three fire/Unholy Darkness
waves, the middle Dark Blizzard, eight rotating hourglasses, two Return
snapshots, the final gazes/eruptions/water, and Shell Crusher. It follows the
[FRU-Sim sequence](https://github.com/WCGH/FRU-Sim/blob/2a77c857ce1bb6eb472a59a95c7544faba01c55a/scenes/p3/sequences/ult_relativity_seq.gd),
its NA waypoint file, and the actual animation events in `p3_ur_main.tscn`.
The first Return snapshot uses 27.7s and the rewind starts at 53.3s, matching
the scene rather than the script's 27.6s/53.4s comments.

NA priorities are H2 > H1 > MT > OT for the long support pair and
R2 > R1 > M1 > M2 for the short DPS pair, with higher priority taking west.
The short support or long DPS can have ice. Each hourglass independently
randomizes clockwise/counterclockwise, and relative north has eight possible
orientations. Unholy Darkness selects distinct eligible targets per wave;
Shell Crusher selects a random party member. The player has normal movement
and Sprint, with the mechanic's input lock only during Return.

Source positions are converted from its 2.358-scale arena, with +X north
mapped to AnoMech's -Z north. Hourglass positions use the native scenery
instead: ContentDirectorManagedSG 181 slots 26–33 place their SGB roots at
radius 10.5, and the model's local Z=-20 offset produces a 9.5-yalm ring on
the opposite side. Slot 26 is therefore the north hourglass. The invisible
BNpc 17832 anchors tethers and casts at the visible model's horizontal position.
All eight hourglasses stay visible after their lasers finish and are removed
only by scenario completion or reset.

Every laser snapshots the nearest living player; the first hit may strike
that baiter alone, and any overlap or subsequent hit is lethal. Each hourglass
fires ten shots, with a 2.1s initial repeat delay then one-second intervals,
rotating fifteen degrees per shot. Native actions supply the 60/50-yalm
beam lengths and five-yalm width. Fire is radius eight; Darkness, Water,
Eruption and Shell Crusher are radius six. Ice uses the native outer radius
twelve and a provisional three-yalm inner safe radius, corroborated by the
reference sim and BossMod but not independently verified in a live encounter.

Return stores actual party positions and draws eight native world-space
traces. The rewind is a single forced movement preserving facing; it never
uses an assigned bot waypoint as the saved position. A 0.4-second rewind
and the 51.9–54.8s input lock follow the supplied reference. Native status
parameters select clockwise/right StatusLoopVFX 348 or counterclockwise/left
269 on hourglasses. These resolve to `m0489_stlp_right_c0d1` and
`m0489_stlp_left01f_c0d1`, respectively. The engine's status-gain path owns
those loops, while scenario time controls their release at the first shot.

Checks cover:

- 288 complete runs for every unique support/DPS assignment and ice role;
  independent NA priority, valid stack-target and native placement checks.
- 4,096 complete runs covering all 256 hourglass direction masks, eight arena
  orientations and two ice-role variants. Frame rate cycles through 15/30/60
  FPS across masks; this is not every mask at every frame rate.
- 128 manual-player runs at 15/60 FPS; no AI MoveTo calls, actual Return
  movement, preserved facing supplied by the harness, and input-lock release.
- Changed saved Return positions, bad fire and ice, insufficient Darkness,
  incorrect initial laser baits, rotating-beam hits, gaze facing, overlapping
  final eruptions, underfilled water/Shell Crusher, deathwall and missing targets.
- Seven interrupted resets, two player resets during the stun/rewind, failed
  native actor creation, eighty beam shots, all eight hourglass map slots,
  waiting-clock ownership and final cleanup.

Native resources were inspected from game build `2026.09.15.0000.0000` using
Dalamud reference build `15.0.3.5`. The plugin uses SDK `15.0.0`. Headless
checks establish managed simulation behavior only. Native hourglass animation,
rotation arrows, Return VFX, actual player facing/input, and visual timing need
an in-game pass. The reference cast durations (9.7/5.3/3.7–3.8/2.8 seconds)
are retained even where the action sheet rounds them to 10/5.5/4/3 seconds.
Native animation playback does not inherit event-speed scaling.
