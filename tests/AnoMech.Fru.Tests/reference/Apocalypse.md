## Apocalypse (P3)

The **Futures Rewritten → P3 → Apocalypse** scenario uses NA water priority
(MT > OT > H1 > H2 and M1 > M2 > R1 > R2), static/Freepoc eruption spreads,
and a selectable MT/OT Darkest Dance bait (OT by default). It requires a full party; player movement stays
manual. Each run randomizes two of each water duration (including no water),
four initial axes, and clockwise/counterclockwise rotation.

Run only these checks with `dotnet test tests/AnoMech.Fru.Tests/AnoMech.Fru.Tests.csproj --filter Name~Apocalypse`.
The separate `dotnet test tests/AnoMech.FruCore.Tests` harness exercises the
production player movement code, including a gap-closer interrupting the end of
the knockback without the old slide pushing the player out again.
`dotnet test AnoMech.sln` runs these alongside the other FRU and upstream tests.
`ApocalypseChecks` exhausts all 2,520 water assignments, then executes 384 full
bot runs across eight rotations, 16 assignments, and 15/30/60 FPS. Another 64
runs replay externally recorded player inputs across every role and rotation,
checking that production AI never moves the player. Checks cover all three
four-person stacks, explosion/spread clearance, arena bounds, tank targeting,
knockback landing positions, native requests, cleanup, missing actors, reset,
and deliberate positioning failures.

The reference is [FRU-Sim at 2a77c85](https://github.com/WCGH/FRU-Sim/tree/2a77c857ce1bb6eb472a59a95c7544faba01c55a/scenes/p3):
`sequences/apoc_seq.gd`, `position_coords/apoc_pc_pos.gd`, `apoc_lights.gd`, and
`p3_apoc_main.tscn`. The animation tracks are authoritative when script comments
disagree: swaps at 16.9s, historical eruption at 35.7s, Apocalypse hits at 33.3/35.3/37.3/
39.3/41.3/43.3s, dance damage at 43.2s, and its visual jump at 44.2s. Water hits
remain 23.1/41.9/50.9s. The script samples the tank again for the later jump;
using the earlier damage position for both can put knockback landings outside
the arena. The selected bait tank leaves after the second water at 42.0s (0.2s earlier than the
reference track) to isolate the native eight-yalm splash at ordinary run speed.
At the same time, the other seven bots move to arena center so they clear the
baited tank buster. Checks assert their actual center arrival, splash clearance,
and selected-tank-only damage at the Dance snapshot across all 384 bot runs.
Both groups preposition two yalms behind Oracle at +/-30 degrees, then take the
native 21-yalm knockback. Bots return at 48.4s, after the full 0.7-second slide,
to their adjusted water groups' respective sides of the boss: two yalms inward
and four yalms left/right. The groups remain eight yalms apart at their final
positions, allowing long water to resolve safely during their approach at 50.9s.
Checks verify the full knockback distance, uninterrupted slide, successful final
water stacks, and actual return positions across all 384 bot runs.
These sides follow the flexed water assignments, including bots that swap out
of their original support/DPS group. An additional 96 runs use explicit
short-, medium-, and long-water flex cases plus a two-pair swap case, checking
the actual four occupants of each of the three stacks and the final boss sides
against independently specified groups. Static eruption spreads still use the
original roles; stack movements retain the flex assignments.
Final-stack sides are defined while facing the boss from arena center:
supports on the left, DPS on the right, with flexed roles joining the other
group. Both the knockback setup and return use this orientation. The previous
signs mirrored these sides and could put a correctly flexing human into the
wrong bot group. A PLD/OT with long water now has 24 additional manual-player
checks across all rotations and 15/30/60 FPS: its final route independently
uses the right side while the non-flexing WAR stays left. This reproduced a
final-water wipe before the sign fix; earlier player checks replayed bot routes
and therefore could not detect the shared orientation error.

Native action dimensions replace the simulator's visual dimensions: Apocalypse
radius 9 on a radius-14 ring, water/eruption radius 6, Spirit Taker radius 5,
and Dance radius 8. Godot north (+X) maps to native north (-Z); route coordinates
scale by `20 / 47.4`. P3 uses Oracle BNpc 17831, BGM 802, and the existing
P3/P4 stage controller at map slot 40. Weather 106 and live floor presentation
still need visual confirmation.

Installed build `2026.09.15.0000.0000` supplies action 40297's native
`n4gw_boss_gimmick18` timeline and `n4gw_b_g18_c0v.avfx` hit effect. EObj
`0x1EB0FF` resolves through ExportedSG 25372 to `sgvf_n4gc_b2164.sgb` and
`b2164kido1_o.avfx`. Its authored timelines provide the light's straight path,
right/left 45-degree arcs, and warning pulses (zero-based AVFX triggers 1/2/3/4).
Static VFX use stationary sources and unit scale; the resource already supplies
14-yalm movement over 60 authored frames. There are no custom-drawn warning
circles. Effect handles are owned by the world and expire through scenario events.
The opening water stack marker uses Lockon 62 (`com_share0c`) at 7.2s,
before debuff application at 12.8s. The countdown uses Lockon 184
(`m0581trg_dice0h`) at 18.0/36.8/45.8s, 5.1 seconds before each water hit,
only on that duration's two recipients. Each countdown also replays the stack
marker so the impending stack remains visible. These two resources were previously
assigned in reverse. Checks assert the literal native paths, opening recipients,
and each countdown window. Eruption uses Lockon 139 (`target_ae_s5f`).

At debuff application (12.8s), each water recipient also gets the native
overhead waiting clock `vfx/common/eff/d1049_stlp_b0k1.avfx`. It stays until
that recipient's countdown starts, then is removed before the finite countdown
is requested. The installed resource has an indefinite default timeline,
emitter/particle lifetimes and actor binder, so this waiting clock is owned
persistently and cleaned up on death, debuff loss, finish or world reset.
Checks cover all three handoffs, no-water exclusions, one spawn per recipient,
manual-player recipients and cleanup. Exact native appearance needs a live check.

The three opening/countdown/eruption lockons are finite native animations and use `persistent: false`.
The game owns their lifetimes, including while the simulation is paused; neither
mechanic resolution nor Reset manually destroys their handles. This avoids
destroying a handle the game may already have freed after a death/pause. Six
additional pause/reset checkpoints assert this ownership contract and repeated
cleanup. These are managed checks, not a reproduction of native heap corruption.
An early reset can leave a player lockon visible until its native animation ends.

Managed checks do not execute the native renderer, packets, or player input.
Verify light direction, pulse size/timing, cast/jump animation, floor/weather,
status clocks, player knockback, and reset in-game. As with existing scenarios,
native animations run in real time while the event-speed option scales scheduled
events; the validated choreography is at normal speed.
