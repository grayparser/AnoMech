## P2 Light Rampant (NA conga)

Run `dotnet test tests/AnoMech.Fru.Tests/AnoMech.Fru.Tests.csproj --filter Name~LightRampant` for the focused
checks. The scenario follows FRU-Sim's NA sequence at the same pinned commit:
`scenes/p2/sequences/light_ramp_seq.gd`, `p2_lr_main.tscn`, and
`position_coords/light_ramp_player_pos.gd`. Convert its coordinates using
`(x, z) -> (z / 2.358, -x / 2.358)`; native towers sit exactly 16 yalms out.

Coverage includes 11,760 assignment combinations, 2,016 complete bot runs at
15/30/60 FPS, 64 runs with a manually positioned player, all 70 initial
Lightsteeped masks with changed debuffs before the center tower, lethal snapshot
fixtures, interrupted resets, and missing-boss handling. Both orb orders, both
Banish patterns, either Banish target role group, all six Weight-of-Light pairs,
and every puddle-target pairing are exercised. AI never moves the human player.

The same-role-group puddle flex removes players by role identity, then balances
the remaining conga to three supports-side and three DPS-side players. Only the
four players with two live Lightsteeped stacks take the center tower. The bots
start their post-tower movement at 19.1s: waiting until the source sim's 20s cue
leaves them in the fifth puddle's path at six yalms/second. There are five baits
per target, matching the encounter; the reference's self-timer and explicit
drop calls can produce extra baits and are not copied.

Native data inspected from game build `2026.09.15.0000.0000` identifies:

- Light Rampant actions 40212–40223 and House of Light 40188/40189.
- Statuses 4157–4159 and 2257; channeling tethers 110/111; Lockon 375.
- Tower map slots 9–14 and center slot 21 in ContentDirectorManagedSG 181.
- Holy Light actor 17826 and shared holy puddle EObj `0x1EBC4F`.

These are managed simulation checks, not native rendering/input tests. Live
verification is still needed for tower activation/occupancy VFX, halo and orb
cues, status/tether presentation, and snapshot timing. The 1.2-second puddle
exit grace is a simulator allowance. House of Light uses a 30-degree half-angle
from BossMod's provisional geometry; the native action sheet does not expose
an independently verified angle.
