## P4 Darklit Dragonsong (NA)

Run `dotnet test tests/AnoMech.Fru.Tests/AnoMech.Fru.Tests.csproj --filter Name~Darklit` for focused checks.
The scenario follows the supplied FRU-Sim `darklit_seq.gd`, `dd_positions.gd`
and the actual events in `p4_dd_main.tscn`, at revision
`2a77c857ce1bb6eb472a59a95c7544faba01c55a`.

It includes the opening Akh Rhai baits, Darklit raidwide, four-player bowtie,
water flex, two two-person towers, nearest-four proteans, Spirit Taker,
water stacks/Hallowed Wings, single-tank Somber Dance (both hits), and four
7+1 Akh Morn hits (seven with MT, OT solo). All eight party roles remain playable without AI moving the
human player. Sprint continues through the shared player-input implementation.

The tethered healer anchors NW. Tether DPS use R1 > R2 > M1 > M2 in the lineup;
non-tether DPS use R2 > R1 > M1 > M2 for west/east baits. Box and hourglass
chains resolve into a bowtie. When waters share a north/south group, only the
water-bearing non-tether and the other bait on that same east/west side swap.
Their support/DPS identities are retained for Spirit Taker spreads.

Native inspection used game `2026.09.15.0000.0000` and Dalamud `15.0.3.5`.
ContentDirectorManagedSG 181 slots 42/43 use two-person tower scenery `b1845`
at world (100,0,92/108). Slot 46 supplies the Fragment of Fate. Statuses
4157/4158 transition to active chains; 2257 is Lightsteeped, and 2461 is water.
ActionCastVFX 151/152 selects `m0640_cst_d_2lp_c0v/c1v` for the left/right wing
warning, with the boss facing north. Native casts own these effects.

Native radii are Akh Rhai 4, Spirit Taker 5, water 6, Somber Dance 8, and
Akh Morn/towers 4. Somber Dance's eight-yalm radius exceeds the reference sim's
scaled radius. By default MT takes both hits, or the OT bot does when the player
occupies MT. The assigned tank baits far, then moves to the Oracle for the near
hit while the other tank stays with the party. The optional "Player takes both
Somber Dance hits" setting assigns a tank player instead; for non-tank players
it retains the bot assignment. This setting is captured on Start.
Actual farthest/nearest targeting and overlap failures still apply. Survival of
both correct hits is assumed without requiring an invulnerability input.
Chain limits 22/2.358–61/2.358 and protean half-angle 30 degrees are reference
values (the latter from BossMod), not independently measured encounter geometry.
The actual scene moves to water at 36.9s and Akh Morn at 50s, and returns the
bosses at 52.2s. These events take precedence over differing script comments.
The reference's eleven Akh Rhai pulses and 0.6s Akh Morn hit spacing are retained.
Eight initial Akh Rhai casts release their visual before ten hit-only repeats;
the first pulse must not replace the pending cast on the same frame.

Checks execute 1,152 full assignment runs across all tether parties, three
chain shapes and sixteen water pairings, cycling 15/30/60 FPS. Another 768
runs combine every Spirit target, both wings and the chain/water patterns for
a fixed tether party. There are 192 manual-player runs covering both default and opt-in settings, plus failed puddles,
towers, proteans, chains, Spirit spreads, water, wings, fragment hits, both
Somber baits, Akh Morn, deathwall, seven resets, missing actors, and water-marker
handoffs. Native rendering, actor animation and actual player controls require
an in-game pass. These checks do not simulate mitigation or boss HP balancing.

Darklit Akh Morn uses a fixed 7+1 split independently of the Somber Dance option.
MT and all six non-tanks gather north; OT stays south and is assumed to survive
solo without a mitigation input. Every pulse enforces seven within four yalms
of MT and only OT within four yalms of OT. Successful bot/manual runs verify
all four damage events per player and the correct boss action; negative checks
reject the old 4+4 split, OT joining MT, and a later-pulse player joining OT.

Darklit begins with the hooded Usurper model 4375 (the native P4 BNpcBase
17833 default), not P2's ice form 4374. After baits snapshot at 6.6s, the full
Redress spin starts at 8.8s, half a second before the first Akh Rhai hit at
9.3s, using outgoing timeline 4574 (hide/mon_sp002)
and incoming timeline 4562 (show/mon_sp002) on dragon model 4376. Its native
animation, temporary model, VFX and sound remain owned by the timeline.
The incoming timeline waits for the skeleton to be ready. The outgoing actor
becomes untargetable/unlisted immediately and stays allocated another 5.2s,
covering the 154-frame sequence. Puddle damage/cleanup keeps its original
timing; the transformation finishes before the 16.4s Darklit cast.

The supplied video https://www.youtube.com/watch?v=T4X8gLzTdcg shows the spin
around 2:21–2:25. Native cbbm_show_sp02 was identified by comparing decoded
Havok n_hara translation samples with the reference simulator's spin_wings_out
animation (about 0.0012 RMS error after time-scale alignment, versus 0.088 or
more for show_sp03/04). This replaces the incorrect inferred 4580/7780 pair.
Checks enforce the hooded model until just before Akh Rhai releases, the specific Redress IDs,
completion before later casts, four reset timings and failed replacement
creation. Rendered fade/VFX synchronization still needs a live in-game check.
