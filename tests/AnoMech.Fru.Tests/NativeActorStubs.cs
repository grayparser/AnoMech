using System.Numerics;
using AnoMech.Core.Game;
using AnoMech.Core.Map;
using AnoMech.Core.SimObjects;

// Record native calls without allocating client objects. These tests execute
// the production scenario and effect owner, but cannot verify rendered VFX.
namespace AnoMech.Core.Map
{
    public sealed class MapController
    {
        public List<(uint State, byte Index)> Effects { get; } = [];
        public void AddEffect(uint state, byte index) => Effects.Add((state, index));
    }
}

namespace AnoMech.Core.SimObjects
{
    public static class SimCharacterDeathExtensions
    {
        public static bool IsAlive(this SimCharacter? member) => member is { IsActive: true, Dead: false };
    }

    public partial class SimCharacter
    {
        public uint GameObjectId => 100u + (uint)Role;
        public List<(uint Action, string Cause, bool Lethal)> Damage { get; } = [];
        public HashSet<ushort> Statuses { get; } = [];
        public IReadOnlyList<(ushort StatusId, float Remaining)> ActiveStatusSnapshot
            => Statuses.Select(id => (id, StatusRemaining.GetValueOrDefault(id))).ToArray();
        public Dictionary<ushort, float> StatusRemaining { get; } = [];
        public Dictionary<ushort, int> StatusParams { get; } = [];
        public List<ushort> StatusHistory { get; } = [];
        public bool HasStatus(ushort status) => Statuses.Contains(status);
        public void AddStatus(ushort status, float duration = 0f, int stacks = 1, bool overrideStacks = false)
        {
            StatusParams[status] = overrideStacks ? stacks : StatusParams.GetValueOrDefault(status) + stacks;
            Statuses.Add(status);
            if (duration > 0) StatusRemaining[status] = duration;
            StatusHistory.Add(status);
        }
        public void RemoveStatus(ushort status) { Statuses.Remove(status); StatusParams.Remove(status); StatusRemaining.Remove(status); }
    }

    public enum EnemyListMode { Always, Never, OnlyWhenVisible, Manual }
    public record struct EnemySpawnConfig(uint BNpcBaseId, uint NameId = 0, byte Level = 0,
        bool Targetable = false, EnemyListMode EnemyList = EnemyListMode.Always, Placement Placement = default, float Scale = 0,
        bool IsHostile = true, ushort SpawnTimeline = 0, uint ModelCharaId = 0, bool IsVisible = true);

    public sealed partial class SimEnemy : ISimObject
    {
        public EnemySpawnConfig Config { get; set; }
        public bool ManualEnemyListVisible { get; private set; }
        public void SetVisibleInEnemyList(bool visible) => ManualEnemyListVisible = visible;
        public List<(uint Action, float? CastSeconds, uint? Target)> Casts { get; } = [];
        public bool Cast(uint action, float? castSeconds = null, uint? targetId = null, Vector3? targetLocation = null, float animationLock = 0.6f)
        {
            Casts.Add((action, castSeconds, targetId));
            return true;
        }
        public List<(uint Action, float CastSeconds, uint? Target)> NativeCasts { get; } = [];
        public List<(uint Action, ushort SpellId, uint? AnimationTarget)> NativeReleases { get; } = [];
        public void NativeCast(uint action, FFXIVClientStructs.FFXIV.Client.Game.ActionType type,
            float omenDelay, float castTime, bool interruptible, uint? targetId = null)
            => NativeCasts.Add((action, castTime, targetId));
        public void NativeActionEffect(uint action, float animationLock, ushort spellId, byte variation,
            FFXIVClientStructs.FFXIV.Client.Game.ActionType type, byte flags, uint? animationTargetId = null)
            => NativeReleases.Add((action, spellId, animationTargetId));
        public void SetPosition(Vector3 position) => Position = position;
        public void SetRotation(float rotation) => Rotation = rotation;
        public void Despawn() { ActiveActorVfx.Clear(); IsActive = false; }
        public void Tick(float deltaSeconds) { }
    }

    public sealed partial class SimWorld
    {
        public MapController Map { get; } = new();
        public List<ISimObject> Spawned { get; } = [];
        public bool FailEnemySpawns { get; set; }
        public SimEnemy? SpawnEnemy(EnemySpawnConfig config)
        {
            if (FailEnemySpawns) return null;
            var enemy = new SimEnemy { Config = config, Position = config.Placement.Position, Rotation = config.Placement.Rotation };
            if (config.Scale > 0) enemy.SetScale(config.Scale);
            Spawned.Add(enemy);
            return enemy;
        }
        public SimMapEffect SpawnMapEffect(byte index, uint show, uint hide)
        {
            var effect = new SimMapEffect(Map, index, show, hide);
            Spawned.Add(effect);
            return effect;
        }
        public void Despawn()
        {
            foreach (var spawned in Spawned) spawned.Despawn();
            Spawned.Clear();
            foreach (var member in Party.Members) { member.Statuses.Clear(); member.StatusParams.Clear(); member.StatusRemaining.Clear(); member.ActiveActorVfx.Clear(); member.StopMoving(); }
            Party.Player?.SetMechanicInputLock(false);
        }
    }
}

namespace AnoMech.Scenarios
{
    public interface IPhase { }

}

namespace AnoMech.Scenarios.Fru
{
    public static class FruZone
    {
        private sealed class StubPhase : IPhase { }
        public static IPhase P5 { get; } = new StubPhase();
        public static IPhase P4 { get; } = new StubPhase();
        public static IPhase P3 { get; } = new StubPhase();
        public static IPhase P2 { get; } = new StubPhase();
    }
}

namespace FFXIVClientStructs.FFXIV.Client.Game
{
    public enum ActionType : byte { Action = 1 }
}
