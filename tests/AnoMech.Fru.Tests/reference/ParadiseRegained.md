## Paradise Regained

`ParadiseRegainedChecks` covers all 12 patterns (three tower rotations, either
remaining-tower order, and dark-first/light-first). The standalone P5 scenario
uses a full party. Healers take the first tower; M1/R1 take relative northwest,
and M2/R2 relative northeast. MT takes the first cleave and OT the first distance
bait, then they swap. Player movement remains manual in every role.

Timing follows [FRU-Sim's Paradise Regained sequence at 2a77c85](https://github.com/WCGH/FRU-Sim/blob/2a77c857ce1bb6eb472a59a95c7544faba01c55a/scenes/p5/sequences/p5_pr_seq.gd):
cast at 1.3s, towers at 6/9.5/13s, Wings at 8.3s, and tower snapshots at
15.7/19.2/22.7s. Wing cleaves and tank baits accompany the first two snapshots.
At 25s Pandora faces north, the tower/buster helpers are cleaned up, and
Polarizing Strikes begins with the same boss and party in this scenario.

Native visuals use these installed game resources:

- Map slots 51/52/53, `sgvf_n4gw_b3559.sgb`, activation `0x00020001`.
- Actions 40233/40313, ActionCastVFX 586/587, VFX 2265/2266:
  `vfx/common/eff/m0914_cst_b2lp_c0v.avfx` and `m0914_cst_b2lp_c1v.avfx`.
  The native cast supplies the ordered wing warnings automatically.
- Cleaves 40314/40315, tank baits 39879/39880, tower hit/failure 40320/40321.

There are no added overlay shapes, generic substitute omens, or custom-painted
effects. Geometry is used only for damage checks. Tower centers come from the
native layout at radius 7; towers use the action's radius 3 and baits radius 4.
The 240-degree cleaves use +/-60-degree offsets consistent with the
[BossMod encounter definitions at 0d922f7](https://github.com/awgil/ffxiv_bossmod/tree/0d922f7ab1149a10b091993124c7a165928175b3/BossMod.Ultimate/Dawntrail/Ultimate/FRU).
Native action rows give their range as 100. Bot positions account for the actual
tower radius instead of using the reference simulator's larger visual rings.

The checks simulate 288 pattern/role/frame-rate runs at 15/30/60 FPS and execute
96 full production-scenario runs with recording stand-ins for the native engine.
They cover two-person occupancy, isolated tank hits, nearest/farthest selection,
player control, native action/target/map requests, helper cleanup, over/under-soak
failures, missing actors, and early reset. `SimMapEffect` itself is linked into
the checks so hide-on-reset and idempotent teardown run production code.

Rendering and native packet handling still need an in-game run: verify both wing
orders, the tower activation/dismissal states, cleave alignment, tank-buster
placement, and reset during the cast. The harness validates requests and managed
behavior; it does not render or execute the native client.

### Polarizing Strikes continuation

The initial Polarizing Strikes cast and Polarizing Paths follow-ups pair
`NativeCast` with a scheduled `NativeActionEffect`
at cast completion. The initial 6.7-second bar starts at 28.5s and releases at
35.2s, before the 35.3s line hit. The three 2.4-second bars start at 37.2/41.9/46.6s and
release Pandora's swing at 39.6/44.3/49.0s. The normal `SimCast` auto-release is
not started for those follow-ups, so it cannot replay the swing later. The
separate stack hits remain at 40.0/44.7/49.4s. Native action 40234 selects end
timeline 3205 (`m0914/mon_sp005.tmb`); its C010 animation starts at frame 0.
`--filter Name~PolarizingStrikes` runs the combined 288-case suite plus explicit release checks at
15/30/60/144 FPS, reset during a cast, and boss despawn before release. These
checks record native calls; they do not render the animation in a live client.

The [reference Polarizing Strikes script](https://github.com/WCGH/FRU-Sim/blob/2a77c857ce1bb6eb472a59a95c7544faba01c55a/scenes/p5/sequences/p5_ps_seq.gd)
and `p5_main.tscn` provide four stack hits at scenario times
35.3/40.0/44.7/49.4s, followed by echoes at 37.4/42.1/46.8/51.7s.
The first cast starts at 28.5s; three Polarizing Paths casts start at
37.2/41.9/46.6s. This is an internal sequence, not another scenario entry.

Living players closest to Pandora on each boss-relative side bait the native
100-by-6-yalm lines. This uses retail targeting rather than the Godot script's
random-side approximation. Each line needs exactly four members; overlapping
lines, underfilled stacks, and repeated exposure to a color's resistance-down
status fail. The front bait receives native Light/Dark Resistance Down
(4164/3323). Pairs lead in tank, melee, ranged, healer order, crossing to the
other side after their own bait. Bots move normally and never move the player.
Echoes query the original line placements; they do not retarget moving players.

Native resources, verified in the installed game data:

- Boss casts 40316/40234 share timeline 3205, `mon_sp/m0914/mon_sp005`.
- Light/dark strikes 40317/40318 use timelines 11290/11291,
  `n4gw_boss_gimmick06/07`, and the authored
  `vfx/monster/gimmick5/eff/n4gw_b_g06_c0v.avfx` / `n4gw_b_g07_c0v.avfx`.
- Echo actions 40119/40120 use the native hit-only timeline 1222. Their visible
  trail belongs to the original strike AVFX; the implementation does not replay
  the first-hit visual or add a substitute rectangle for the echo.

Each round has its own pair of native helpers, allocated at the handoff and kept
stationary through the echo. Cleanup at 54s removes all eight helpers and the
resistance statuses. Reset uses normal world/party ownership and clears all
pending events. The statuses last until cleanup, independent of event time scale.

`PolarizingStrikesChecks` executes the complete combined scenario in 288
pattern/role/frame-rate runs. It checks four 4+4 stacks, every role leading once,
all four echo dodges, native action counts/order, actor cleanup, and player
control. Negative checks cover stolen baits, a missing side, underfilled stacks,
same-color resistance, standing in an echo, missing Pandora, and early reset.
Native trail timing, colors, status appearance, and alignment still need live
verification. The reference scene accidentally calls the long initial cast for
two follow-ups; the implementation uses the documented/native Polarizing Paths
cast for all three follow-ups while retaining the reference hit schedule.
