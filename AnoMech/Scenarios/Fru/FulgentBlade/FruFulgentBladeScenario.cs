using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AnoMech.Core.Game;
using AnoMech.Core.Game.Ai;
using AnoMech.Core.Game.Party;
using AnoMech.Core.SimObjects;
using static AnoMech.Scenarios.Fru.FruConstants;

namespace AnoMech.Scenarios.Fru.FulgentBlade;

public sealed class FruFulgentBladeScenario : IScenario
{
    public string Name => "Fulgent Blade";
    public float Duration => 43.5f;
    public IPhase Phase => FruZone.P5;
    public bool SupportsSolo => true;
    public IReadOnlyList<IScenarioAi> AiStrats { get; } = [new FulgentBladeAi()];

    private const float ArrowLeadTime = 2.3f;

    private SimWorld world = null!;
    private SimParty party = null!;
    private DamageSolver damage = null!;
    private SimCharacter? MainTank => party.Get(PartyRole.MainTank);
    private bool solo, facingLocked;
    private SimEnemy? pandora { get => pattern.Pandora; set => pattern.Pandora = value; }
    private FulgentBladeState pattern = null!;
    private readonly SimEnemy?[,] waveHelpers = new SimEnemy?[3, 4];
    private readonly SimOmen?[,] initialSeams = new SimOmen?[3, 2];
    private readonly SimOmen?[,] seamWarnings = new SimOmen?[3, 2];
    private readonly SimEnemy?[] stackHelpers = new SimEnemy?[2];

    public void Run(SimWorld worldParam, int? selectedAi)
    {
        world = worldParam;
        party = world.Party;
        damage = new DamageSolver(party);
        solo = selectedAi is null;
        facingLocked = false;
        pattern = new FulgentBladeState(Random.Shared.Next(4), Random.Shared.Next(4), Random.Shared.Next(2) == 0);
        Array.Clear(waveHelpers);
        Array.Clear(initialSeams);
        Array.Clear(seamWarnings);
        Array.Clear(stackHelpers);

        if (selectedAi is { } aiIndex && aiIndex >= 0 && aiIndex < AiStrats.Count)
            ((IScenarioAi<FulgentBladeState>)AiStrats[aiIndex]).Run(pattern, world);

        // FRU-Sim's FB sequence starts after the opening raidwide (here at 7.5s).
        world.Events.Add(0.5f, SpawnPandora);
        world.Events.Add(1.5f, () => pandora?.Cast(ActionId.FulgentBlade, castSeconds: 6f));
        world.Events.Add(7.5f, () =>
        {
            ResolveRaidwide();
            SpawnExalines(); // Reveal the seams when the opening cast resolves.
        });
        // Advance each randomized seam group into its native charging stage.
        world.Events.Add(13.5f, () => BeginExalineGroup(0));
        world.Events.Add(17.5f, () => BeginExalineGroup(1));
        // Start the native arrows 0.5s earlier than the reference's box warning
        // to allow for their fade-in; keep them visible until the first snapshot.
        world.Events.Add(20.5f - ArrowLeadTime, () => TelegraphGroup(0));
        world.Events.Add(21.5f, () => BeginExalineGroup(2));
        world.Events.Add(24.5f - ArrowLeadTime, () => TelegraphGroup(1));
        world.Events.Add(28.5f - ArrowLeadTime, () => TelegraphGroup(2));
        world.Events.Add(FulgentBladePartyPlan.AkhMornCastTime, CastAkhMorn);
        world.Events.Add(36f, ResolveAkhMorn);
        world.Events.Add(Duration, DespawnHelpers);

        // Schedule every snapshot up front so low frame rates cannot accumulate
        // drift between the three overlapping, two-second wave trains.
        for (var group = 0; group < FulgentBladeState.GroupCount; group++)
        {
            for (var hit = 0; hit < FulgentBladeState.HitCount; hit++)
            {
                var g = group;
                var h = hit;
                world.Events.Add(20.5f + group * 4f + hit * 2f, () => ResolveWave(g, h));
            }
        }
    }

    public void Tick(float delta, float elapsed)
    {
        if (facingLocked || pandora is not { IsActive: true } boss || !MainTank.IsAlive()) return;
        var offset = MainTank!.Position - boss.Position;
        if (offset.LengthSquared() > 0.001f) boss.SetRotation(MathF.Atan2(offset.X, offset.Z));
    }

    private void SpawnExalines()
    {
        for (var group = 0; group < FulgentBladeState.GroupCount; group++)
            for (var wave = 0; wave < FulgentBladeState.WavesPerGroup; wave++)
            {
                var definition = pattern.Wave(group, wave);
                var placement = new Placement(definition.Position, definition.Rotation);
                waveHelpers[group, wave] = SpawnHelper(placement);
                if (wave % 2 == 0)
                {
                    // Line and arrows share the scenery's axes: purple +X,
                    // gold -X. Keep the native palette on its matching wave side.
                    var line = pattern.ArrowWarning(group, wave / 2);
                    initialSeams[group, wave / 2] = world.SpawnOmen(Vfx.InitialSeam,
                        new Placement(line.Position, line.Rotation), Vector3.One,
                        startTrigger: Vfx.InitialSeamTrigger);
                }
            }
    }

    private SimEnemy? SpawnHelper(Placement placement) => world.SpawnEnemy(new EnemySpawnConfig(
        BNpcBaseId: BNpcBaseId.Helper,
        NameId: BNpcNameId.Pandora,
        Level: Level,
        Targetable: false,
        EnemyList: EnemyListMode.Never,
        IsVisible: true,
        Placement: placement));

    private void BeginExalineGroup(int group)
    {
        // Native slash_act_a sends queue trigger 4 (AVFX slot 3) to both
        // scenery instances 1 and 3. Instance 3's default timeline is only
        // the narrow seam; the trigger expands it into the broad side warning.
        for (var line = 0; line < FulgentBladeState.WavesPerGroup / 2; line++)
        {
            initialSeams[group, line]?.Trigger(Vfx.SeamChargeTrigger);
            var warning = pattern.ArrowWarning(group, line);
            seamWarnings[group, line] = world.SpawnOmen(Vfx.SeamWarning,
                new Placement(warning.Position, warning.Rotation), Vector3.One,
                startTrigger: Vfx.SeamChargeTrigger);
        }
    }

    private void TelegraphGroup(int group)
    {
        // The arrows belong to the line's scenery object, not action 40115.
        // Spawn its native world-space resource separately from the cast omens.
        for (var line = 0; line < FulgentBladeState.WavesPerGroup / 2; line++)
        {
            var warning = pattern.ArrowWarning(group, line);
            // The AVFX already contains the full line of arrows at game size.
            // Do not apply the width/depth scaling used by action-backed omens.
            // SimWorld owns this looping StaticVfx and cleans it on expiry/reset.
            world.SpawnOmen(Vfx.PathArrows, new Placement(warning.Position, warning.Rotation),
                Vector3.One, durationSeconds: ArrowLeadTime);
        }
    }

    private void ResolveWave(int group, int hit)
    {
        // End the persistent seam warning as the first AOE visual starts,
        // before casting the hit effects or applying their damage.
        if (hit == 0)
        {
            for (var line = 0; line < FulgentBladeState.WavesPerGroup / 2; line++)
            {
                seamWarnings[group, line]?.Despawn();
                seamWarnings[group, line] = null;
            }
        }
        // Each native action supplies its strip width and length.
        for (var wave = 0; wave < FulgentBladeState.WavesPerGroup; wave++)
        {
            var definition = pattern.Wave(group, wave, hit);
            var action = definition.IsLight ? ActionId.PathOfLightRest : ActionId.PathOfDarknessRest;
            var helper = waveHelpers[group, wave];
            if (helper is { IsActive: true })
            {
                helper.SetPosition(definition.Position);
                helper.Cast(action, castSeconds: 0f, animationLock: 0f);
            }
            damage.Resolve(helper, action, [DamageType.Lethal], []);
        }
        if (hit == 0)
        {
            for (var line = 0; line < FulgentBladeState.WavesPerGroup / 2; line++)
            {
                initialSeams[group, line]?.Despawn();
                initialSeams[group, line] = null;
            }
        }
    }

    private void SpawnPandora()
    {
        pandora = world.SpawnEnemy(new EnemySpawnConfig(
            BNpcBaseId: BNpcBaseId.Pandora,
            NameId: BNpcNameId.Pandora,
            Level: Level,
            Targetable: true,
            EnemyList: EnemyListMode.Always,
            IsVisible: true,
            Placement: new Placement(Vector3.Zero, 0f)));
        pandora?.SetTarget(MainTank, follow: false);
    }

    private void ResolveRaidwide()
    {
        foreach (var member in party.ActiveMembers())
            damage.ApplyDamage(member, 0.8f, ActionId.FulgentBlade, "raidwide", lethal: false);
    }

    private void CastAkhMorn()
    {
        if (solo) return;
        facingLocked = true;
        if (pandora is not { IsActive: true } boss) return;
        var mainTank = MainTank;
        if (mainTank.IsAlive())
        {
            var offset = mainTank!.Position - boss.Position;
            if (offset.LengthSquared() > 0.001f)
                boss.SetRotation(MathF.Atan2(offset.X, offset.Z));
        }
        boss.Cast(ActionId.AkhMornPandora, castSeconds: 7.5f);
        for (var i = 0; i < stackHelpers.Length; i++)
            stackHelpers[i] = SpawnHelper(new Placement(boss.Position, 0f));
    }

    private void ResolveAkhMorn()
    {
        if (solo) return;
        if (pandora is not { IsActive: true } boss) return;
        var members = party.ActiveMembers().ToArray();
        if (members.Length == 0) return;
        var right = new Vector3(MathF.Cos(boss.Rotation), 0f, -MathF.Sin(boss.Rotation));
        var leftSide = members.Where(m => Vector3.Dot(m.Position - boss.Position, right) <= 0f).ToArray();
        var rightSide = members.Where(m => Vector3.Dot(m.Position - boss.Position, right) > 0f).ToArray();
        var targets = new List<SimCharacter>();
        if (leftSide.Length > 0) targets.Add(leftSide[Random.Shared.Next(leftSide.Length)]);
        if (rightSide.Length > 0) targets.Add(rightSide[Random.Shared.Next(rightSide.Length)]);

        if (targets.Count != 2)
        {
            party.WipeAllPlayers("Akh Morn: no target on one side of Pandora");
            return;
        }
        var stacks = targets.Select(t => (Target: t, Members: party.Find.InsideCircle(t.Position, Geometry.AkhMornRadius))).ToArray();
        for (var i = 0; i < stacks.Length; i++)
        {
            var stack = stacks[i];
            var action = i == 0 ? ActionId.AkhMornPandoraAoe1 : ActionId.AkhMornPandoraAoe2;
            var helper = stackHelpers[i];
            if (helper is { IsActive: true })
            {
                helper.SetPosition(stack.Target.Position);
                helper.Cast(action, targetId: stack.Target.GameObjectId, castSeconds: 0f);
            }
            foreach (var member in stack.Members)
            {
                var overlapping = stacks.Count(s => s.Members.Contains(member)) > 1;
                var failed = stack.Members.Count != 4 || overlapping;
                damage.ApplyDamage(member, failed ? 1f : 0.5f, action,
                    overlapping ? "overlapping Akh Morn stacks" : $"{stack.Members.Count}/4 players in stack", failed);
            }
        }
    }

    private void DespawnHelpers()
    {
        foreach (var helper in waveHelpers) helper?.Despawn();
        foreach (var helper in stackHelpers) helper?.Despawn();
        foreach (var seam in initialSeams) seam?.Despawn();
        foreach (var warning in seamWarnings) warning?.Despawn();
        Array.Clear(waveHelpers);
        Array.Clear(stackHelpers);
        Array.Clear(initialSeams);
        Array.Clear(seamWarnings);
    }
}
