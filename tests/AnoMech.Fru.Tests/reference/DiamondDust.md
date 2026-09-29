## Diamond Dust (P2, NA Partner Swap)

Run just this scenario with `dotnet test tests/AnoMech.Fru.Tests/AnoMech.Fru.Tests.csproj --filter Name~DiamondDust`.
It is also included in the full check suite. Strategy references are the
[NA FRU resources](https://naurffxiv.com/ultimate/fru), their linked
[Partner Swap presentation](https://docs.google.com/presentation/d/1VqIifgNf8RzXIWb8EGGVdKvOtKk0HmhPcHIMpizYuig/edit),
and the [NA P2 mechanic guide](https://naurffxiv.com/ultimate/fru/guide/p2).

The production scenario and AI run in the headless harness. Checks cover:

- 256 combinations of first icicles, marked role group, kick, Reflection
  direction, and twin combo, at 15/30/60 FPS: 768 complete bot runs.
- Independent NA clock expectations: MT N, OT E, H1 W, H2 S, M1 SW,
  M2 SE, R1 NW, R2 NE; supports swap CCW and DPS CW only as necessary.
  G1 takes the red/purple knockback side and G2 blue/yellow.
- The cursed pattern starts clockwise immediately after the stars, clearing
  Shiva's axis before the first puddle. The near group slides across first;
  its landing must allow the following slide behind Shiva as well.
- 64 manual-player runs: every role, both twin combos, ordinary/cursed
  patterns, and 15/60 FPS. Native Thin Ice must receive parameter `0x140`
  (320, or 32 yalms) and preserve it on refresh. The harness models external
  client slide positions; the scenario never issues a simulated player slide
  or a movement lock that would compete with the native movement controller.
- Opening kick choices: Random, Axe, or Scythe. Every other mechanic stays
  randomized, including normal/cursed reflection patterns. Auto restores Random.
- Both combo voices play once at cast start: native voice `8205521` for
  Stillness and `8205522` for Silence. Reset while the voice is active releases
  the scenario-owned voice object, along with the remaining mechanic effects.
- Bad cone baits, overlapping marked spreads, under/overfilled healer stacks,
  missing healers, lingering puddles, gaze facing, twin cleaves, deathwall,
  missing actors, and resets during active effects.

`tests/AnoMech.FruCore.Tests` separately executes the real movement implementation:
The simulated 32-yalm bot slides use timeline 602 (`pc_contentsaction/icefloor`),
clear their forced-movement state on arrival/reset, and preserve the previous
Apocalypse gap-closer fix. Player ice movement belongs to the native client;
the headless harness does not execute or verify that native controller.

Native IDs come from the installed 2026.09.15 game data. P2 uses platform
slot 23, with its authored `stage2_ice_a` / `stage2_nomal_a` freeze/thaw
actions (`0x00010020` / `0x00010040`: mode 1, low action flags 0x20 / 0x40),
and BGM 759. The high word selects a mode, not a timeline index.
Markers are finite native Lockon 345 effects. Sinbound
Holy uses EObj 0x1EBC4F; Frigid Needles use two native crosses per marked
location. Ice/light body swaps retain the old actor slots briefly during
handoff. The scenario ends after Shiva returns, before Hallowed Ray.
Combo voices use installed `sound/voice/vo_line/8205521_<language>.scd` and
`8205522_<language>.scd` assets, selected by `CutsceneMovieVoice`. No audio
files are bundled. Voice sound slots remain owned until teardown, then stop
and return to native automatic cleanup.

These checks establish simulation behavior, not in-game visual fidelity.
The 1.25-second puddle exit allowance, inner/outer spawn radii, native form
handoff, weather, VFX/voice timing, and real Thin Ice input still need a live
game pass. Native rendering does not follow the simulator's speed scaling.
