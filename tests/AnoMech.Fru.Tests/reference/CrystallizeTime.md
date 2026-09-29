## Crystallize Time (P4)

The separate **Crystallize Time** scenario uses NA debuff priority and **7–1 Akh
Morn, with MT solo and the other seven on OT**. The solo tank receives AnoMech's
existing simulated invulnerability status during the four hits; this does not
press an actual player job action. Normal player movement remains manual.

Timing and initial routes follow [FRU-Sim's CT sequence at 2a77c85](https://github.com/WCGH/FRU-Sim/blob/2a77c857ce1bb6eb472a59a95c7544faba01c55a/scenes/p4/sequences/crystal_time_seq.gd)
and its `ct_positions.gd` / `exaline.tscn`. The encounter uses game dimensions:
12-yalm hourglasses/dragon explosions, 6-yalm water/darkness/eruption circles,
3–12-yalm Blizzard donuts, 15-yalm Aero circles with 30-yalm knockbacks,
40-by-10-yalm traveling strips, 5-yalm Spirit Taker, and 20-yalm Wings knockbacks.
The donut inner radius follows the encounter reference; it is not present in
the Action row. Reference coordinates map north/east to game east/south at
`10 / 23.74` scale. Bots leave dragon intercepts after actual contact to start
their next route, and only knocked-back bots defer
their next move until the Aero slide finishes.

Each cleansing puddle records the role that popped its dragon head. That member
cannot consume their own puddle for its entire lifetime, including after leaving
and returning. This applies to players and bots on either pop. Other members can
still cleanse Fang while the creator overlaps the puddle; an unrelated member
without Fang can still waste it. The one-second arming delay and 17.8-second
lifetime remain unchanged. Checks cover both heads/pops, player and bot creators,
15/30/60 FPS, lingering overlap, re-entry, and a simultaneous eligible recipient.

Spirit Taker spread starts at 40.6 seconds. Bots reserve nearby positions with
six-yalm clearance from the north crystal, the living player's actual position,
and other bots' destinations. Positions stay inside an 18.5-yalm arena radius.
Bots keep safe assignments and yield a spot if the player approaches it, updating
until the 43.4-second jump snapshot. Normal six-yalm/second movement still applies;
a last-moment player approach can leave too little time to escape. The player is
never moved by this spread logic. This is the CT jump, not Somber Dance.
Checks include 480 stationary-player cases across all corners, player roles and
15/30/60 FPS, including a player beside the crystal, plus 12 cases where the
player takes an assigned bot spot. They assert actual pre-jump positions clear
the five-yalm hit radius, bot separation, successful resolution and player control.

Native presentation uses the P4 stage controller (map slot 40), Fragment of
Fate crystal scenery (46), and CT hourglass scenery (34–39), with weather 106. Boss
BNpc rows are Usurper 17833 and Oracle 17835. The Usurper overrides her default
ModelChara 4375 (m0640 body 2) with 4376 (body 3), the native dragon-armored
form with the shoulder dragon and wing bones. Her BNpc identity and scale remain
unchanged. Heads use 17836 and the invisible
hourglass/fragment anchors use 17837/17841. Cleansing puddles are actual EObj
`0x1EBD41`, backed by `sgvf_w_btl_b3566.sgb`. Tethers use Channeling 133/134.
The crystal is `sgvf_n4gw_b3557.sgb` (layout 10885836) at world (100, 0, 84),
scenario-local (0, 0, -16). Its invisible combat anchor and protection checks
use that same north position. Slot 41 is a separate center floor circle, not the
crystal. Checks cover correct activation, normal/reset cleanup, a harmful spell
at the crystal, and no false crystal hit from that spell at arena center.
Native encounter actors Vision of Ryne (17844) and Vision of Gaia (17845) are
placed inside the crystal, facing each other. The fragment is a friendly,
targetable actor outside the enemy list, with a native HP bar (1,000,000 training
HP, consistent with other simulated actors). A fatal crystal hit also sets its
HP to zero. Exact retail HP and a separate healing/damage model are not simulated.
Both visions and the HP actor belong to scenario cleanup.

Dragon heads are absent until their 14.3-second appearance cue, separately from
the tethers at 10 seconds. They face their travel direction and begin moving at
15.3 seconds, one second after the native appearance request. Their spawn
configuration requests ActionTimeline 4562 only after the native model is ready;
for model m0253 this resolves to `show/mon_sp002.tmb` and the game's
`m0253_show_sp02c0k1.avfx`. This is a native animation/VFX request, with no custom
fade or overlay. Initial movement/contact cooldown still runs from 15.3 to 17.3.
The pulse is requested only at appearance, with native finite emitter windows
(frames 0–30, 4–34 and 17–47). The game owns its particle fade-out; no persistent
status loop is attached during travel and no pulse is replayed on interception.
No extra crystal effect is requested. Checks cover the appearance request,
absence of persistent/replayed head VFX, reset and unchanged crystal HP/effects.
Asynchronous native loading, the exact burst's visual match and its fade-out
relative to the first movement still need live-client verification.
Checks verify opening absence, both spawn requests, health/targetability,
Ryne/Gaia placement, fatal crystal HP, and cleanup. Native appearance timing,
poses, HP rendering and appearance animation need live-client verification.
All casts, elemental hits, Tidal Light, Return, Spirit Taker, Wings and Akh Morn
dispatch native actions. Tidal Light's warning is its own Action/Omen 589
(`exa_rz_o1v`), not a custom drawn strip. No overlay renderer, replacement
omen, hand-tinted effect, or copied game asset was added.

The scenario includes randomized debuffs, three Quietus targets, both slow
hourglass configurations and all four exaline corners; reactive head contacts
and single-use puddles; delayed spells, fragment protection, cleanse expiry,
rewind snapshots, spread, stun/forced return, two ordered Wings knockbacks,
and four Akh Morn hits. The player input lock is explicitly released on normal
completion and on `SimPlayer.Despawn`; all native objects belong to the world.

`CrystallizeTimeChecks` executes 384 complete bot runs (eight patterns, 16 role
assignments/jump targets, three frame rates) plus 320 complete manual-player
runs (eight arena patterns × eight roles × five debuff choices including Random). Negative checks cover hourglasses,
underfilled stacks, fireworks, exalines, incorrect rewind leading/order,
overlapping Spirit Taker, expired uncleansed Fang, and an incomplete seven-person
Akh Morn stack. Reset is exercised during tethers, heads/puddles, rewind, stun,
and Wings; missing boss allocation is also covered. The player run uses a
separate rehearsal to supply intended user inputs and verifies production
never issues normal movement to the selected player.

These are managed tests with recording engine stand-ins, not live client tests.
Verify native stage switching, Usurper's composite model, hourglass state/fill
animations, head scaling/path, puddle visibility/consumption, Return traces,
cast/telegraph alignment, player input release on early reset, and the complete
7–1 finish in game. P4 plays native BGM 801, Promises to Keep
(`music/ex3/BGM_EX3_Raid_11.scd`), respecting the existing music suppression setting.

The first dragon interception now resizes the same native actor/model instead
of deleting and immediately reusing its object-table slot. Checks require only
two dragon actors for the full run. Heads explicitly spawn at scale 2, shrink to
scale 1 after the first pop, and play the native disappear action and hide after
the second. A staged check verifies each pop advances one head stage, drops one
puddle, and never recreates an actor. Local event objects
default to the non-networked entity sentinel `0xE0000000` instead of zero.
These changes address the suspect transition in the reported Boss Mod 7.5.6.5
`WorldStateGameSync.UpdateActor` crash. The user reports that this error is gone.

Each dragon head now has a two-second initial and retrigger cooldown on the scenario clock.
Initial protection runs from movement activation at 15.3 seconds until 17.3,
so early overlap near the north side cannot pop either head. The appearance lead-in does not consume any of this movement-start protection.
Heads continue moving while protected; overlap causes no hit, shrink, puddle,
or Wyrmclaw consumption. Initial protection is reapplied on restart and checked
on both heads at 15/30/60 fps, including just before expiry and after expiry.
The previous interceptor must leave its original two-yalm contact area before
they can trigger the same head again. This prevents a continuous overlap from
consuming both pops when the timer expires or the model shrinks. A different
interceptor can take the second pop after cooldown; a genuine later contact
without Wyrmclaw still fails. Both heads track cooldown/contact state independently,
and restart clears that state. Checks at 15/30/60 fps cover lingering overlap,
shrink boundaries, early re-entry, a different interceptor, invalid later soaks,
independent heads, and restarting the same scenario instance.

The native actor-bound clock (`vfx/common/eff/d1049_stlp_b0k1.avfx`) is requested
with the initial debuffs at 6.5 seconds and remains until the 39.6-second snapshot.
It is also removed on death/reset; it is not limited to the final five seconds.
At snapshot, each living member leaves a world-space Return marker
(`vfx/common/eff/d1049_returnmark_c0k1.avfx`, VFX row 1013). This replaces the
enlarged boss variant `m0640_returnmark_c0v` (row 2269), whose authored root scale
is 2 instead of 1. The user confirmed using the original native player marker,
including its authored small rewind arrows. It stays fixed
when they move away and is removed at
42.7 seconds, matching the reference's Wings transition. This uses the native
StaticVfx owner with unit scale; no overlay or replacement artwork is involved.
Checks cover early/late timing, living recipients, actual saved positions,
stationary traces during movement, and cleanup. Asset identity is inferred from
the installed AVFX's clock textures, bindings and Return naming; the exact
countdown appearance and floor rendering still require live client verification.

CT's Scenario config panel has a **Your debuffs** selector: Random, RED+ICE,
RED+AERO, BLUE+DARK, and the grouped BLUE+ICE / BLUE / YELLOW stack option.
The grouped option selects blue ice, water, or eruption randomly. Auto restores
Random. The choice applies to the actual player slot (including role overrides)
on the next Start and stays selected for subsequent runs in the current session.
The remaining slots are filled by bots, with the red pairs sorted by NA priority
after the player's category is chosen. Hourglass/exaline patterns, Quietus and
Spirit Taker targets stay randomized. The selected group is captured at Start;
changing settings mid-run affects the next run only.

Selection checks cover 2,560 seeded role/choice combinations, uniqueness of all
eight roles, NA side priority, all three grouped-blue outcomes, and unchanged
Random behavior. The public scenario Start path is checked for all 32 fixed
choice/player-role pairs, including settings changes and restart. The settings
renderer is omitted from the headless test build through an optional partial
method; it is compiled by the full plugin build and needs a live UI check.

The dragon-armored Usurper uses native teleport animations around Wings.
The windup (136/137) precedes action 40229 at 42.7 seconds; its 4.8-second cast
restores the cast bar and releases `hide/mon_sp010`. First-edge arrival at 48
uses show/loop (7780/7781), followed by swing (4568) at 49.3 and impact at 49.9.
The first-edge departure uses hide (4582) at 53.0, retaining the earlier recovery
delay; second arrival, swing and impact remain at 53.1, 53.9 and 54.5 respectively.
The Usurper's draw flag stays enabled so native opacity animations can play;
there is no abrupt `SetVisible(false)` at these transitions or in the opening.
Presentation checks cover enabled drawing at 15/30/60 FPS, the retained dragon
model, cast/teleport ordering, and direct native wing timelines before both frozen-party
knockbacks in all four corner patterns. They check damage is not applied during
the animation lead-in and stun ends after the second knockback's recovery,
before Akh Morn movement. The two native visions
request persistent native draw elevation above their grounded actors through rewind and are
removed on reset. These checks record native API requests; actual animation,
crystal framing and model height require in-game confirmation.
